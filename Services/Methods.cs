using ConferenceRoomBooking.Models;
using Microsoft.Data.Sqlite;


public class Methods
{
    private const string connectionString = "Data Source=Data/booking.db";

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();

        return connection;
    }

    public int AddRoom(string name, int capacity, int[] serviceIds, int hourlyRate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required");
        }
        if (capacity <= 0 || hourlyRate < 0)
        {
            throw new ArgumentException("Capacity and hourly rate are invalid");
        }

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
        """
        Insert into Rooms (Name, Capacity, HourlyRate)
        Values ($Name, $Capacity, $HourlyRate)
        Returning Id;
        """;
        command.Parameters.AddWithValue("$Name", name);
        command.Parameters.AddWithValue("$Capacity", capacity);
        command.Parameters.AddWithValue("$HourlyRate", hourlyRate);

        int roomId = Convert.ToInt32(command.ExecuteScalar());
        AddRoomServices(connection, transaction, roomId, serviceIds);
        transaction.Commit();

        return roomId;
    }

    public bool EditRoom(int id, string? name = null, int? capacity = null, int[]? serviceIds = null, int? hourlyRate = null)
    {
        if (name is not null && string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty");
        }
        if (capacity is <= 0 || hourlyRate is < 0)
        {
            throw new ArgumentException("Capacity or hourly rate is invalid");
        }

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
        """
        Update Rooms
        Set Name = Coalesce($Name, Name),
            Capacity = Coalesce($Capacity, Capacity),
            HourlyRate = Coalesce($HourlyRate, HourlyRate)
        Where Id = $Id;
        """;
        command.Parameters.AddWithValue("$Id", id);
        command.Parameters.AddWithValue("$Name", name is null ? DBNull.Value : name);
        command.Parameters.AddWithValue("$Capacity", capacity is null ? DBNull.Value : capacity);
        command.Parameters.AddWithValue("$HourlyRate", hourlyRate is null ? DBNull.Value : hourlyRate);

        if (command.ExecuteNonQuery() == 0)
        {
            transaction.Rollback();
            return false;
        }

        if (serviceIds is not null)
        {
            using var deleteCommand = connection.CreateCommand();
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "Delete from RoomServices Where RoomId = $RoomId;";
            deleteCommand.Parameters.AddWithValue("$RoomId", id);
            deleteCommand.ExecuteNonQuery();

            AddRoomServices(connection, transaction, id, serviceIds);
        }

        transaction.Commit();
        return true;
    }

    public bool DeleteRoom(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "Delete from Rooms Where Id = $Id;";
        command.Parameters.AddWithValue("$Id", id);

        return command.ExecuteNonQuery() > 0;
    }

    public Room[] FindEmptyRooms(DateTimeOffset time, int duration, int capacity)
    {
        if (duration <= 0 || capacity <= 0)
        {
            throw new ArgumentException("Duration and capacity must be greater than zero");
        }

        DateTimeOffset endAt = time.AddHours(duration);
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        Select room.Id, room.Name, room.Capacity, room.HourlyRate
        From Rooms as room
        Where room.Capacity >= $Capacity
        And Not Exists (
            Select 1
            From Bookings as booking
            Where booking.RoomId = room.Id
            And booking.StartAt < $EndAt
            And booking.EndAt > $StartAt
        )
        Order by room.Capacity;
        """;
        command.Parameters.AddWithValue("$Capacity", capacity);
        command.Parameters.AddWithValue("$StartAt", ToDatabaseTime(time));
        command.Parameters.AddWithValue("$EndAt", ToDatabaseTime(endAt));

        using var reader = command.ExecuteReader();
        var rooms = new List<Room>();
        while (reader.Read())
        {
            rooms.Add(new Room
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Capacity = reader.GetInt32(2),
                HourlyRate = reader.GetInt32(3)
            });
        }

        return rooms.ToArray();
    }

    public BookingResult ReserveRoom(int id, DateTimeOffset time, int duration, int[] serviceIds)
    {
        if (duration <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero");
        }

        DateTimeOffset endAt = time.AddHours(duration);
        if (time.Date != endAt.Date || time.Hour < 6 || endAt.TimeOfDay > TimeSpan.FromHours(23))
        {
            throw new ArgumentException("Booking must be between 06:00 and 23:00 on one day");
        }

        serviceIds = serviceIds.Distinct().ToArray();
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction(deferred: false);

        int hourlyRate = GetHourlyRate(connection, transaction, id);
        CheckAvailability(connection, transaction, id, time, endAt);

        int servicesPrice = 0;
        foreach (int serviceId in serviceIds)
        {
            servicesPrice += GetServicePrice(connection, transaction, id, serviceId);
        }

        int roomPrice = CalculateRoomPrice(hourlyRate, time, duration);
        int totalPrice = roomPrice + servicesPrice;

        using var bookingCommand = connection.CreateCommand();
        bookingCommand.Transaction = transaction;
        bookingCommand.CommandText =
        """
        Insert into Bookings (RoomId, StartAt, EndAt, TotalPrice)
        Values ($RoomId, $StartAt, $EndAt, $TotalPrice)
        Returning Id;
        """;
        bookingCommand.Parameters.AddWithValue("$RoomId", id);
        bookingCommand.Parameters.AddWithValue("$StartAt", ToDatabaseTime(time));
        bookingCommand.Parameters.AddWithValue("$EndAt", ToDatabaseTime(endAt));
        bookingCommand.Parameters.AddWithValue("$TotalPrice", totalPrice);

        int bookingId = Convert.ToInt32(bookingCommand.ExecuteScalar());
        foreach (int serviceId in serviceIds)
        {
            using var serviceCommand = connection.CreateCommand();
            serviceCommand.Transaction = transaction;
            serviceCommand.CommandText =
            """
            Insert into BookingServices (BookingId, ServiceId)
            Values ($BookingId, $ServiceId);
            """;
            serviceCommand.Parameters.AddWithValue("$BookingId", bookingId);
            serviceCommand.Parameters.AddWithValue("$ServiceId", serviceId);
            serviceCommand.ExecuteNonQuery();
        }

        transaction.Commit();
        return new BookingResult
        {
            Id = bookingId,
            RoomId = id,
            StartAt = time,
            EndAt = endAt,
            TotalPrice = totalPrice
        };
    }

    public RoomReport[] GetRoomReport(DateTimeOffset from, DateTimeOffset to)
    {
        if (from >= to)
        {
            throw new ArgumentException("From must be earlier than to");
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        Select room.Id, room.Name, Count(booking.Id), Coalesce(Sum(booking.TotalPrice), 0)
        From Rooms as room
        Left Join Bookings as booking on booking.RoomId = room.Id
            And booking.StartAt >= $From
            And booking.StartAt < $To
        Group by room.Id, room.Name
        Order by room.Id;
        """;
        command.Parameters.AddWithValue("$From", ToDatabaseTime(from));
        command.Parameters.AddWithValue("$To", ToDatabaseTime(to));

        using var reader = command.ExecuteReader();
        var report = new List<RoomReport>();
        while (reader.Read())
        {
            report.Add(new RoomReport
            {
                RoomId = reader.GetInt32(0),
                RoomName = reader.GetString(1),
                BookingCount = reader.GetInt32(2),
                Revenue = reader.GetInt32(3)
            });
        }

        return report.ToArray();
    }

    private void AddRoomServices(SqliteConnection connection, SqliteTransaction transaction, int roomId, int[] serviceIds)
    {
        foreach (int serviceId in serviceIds.Distinct())
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
            """
            Insert into RoomServices (RoomId, ServiceId)
            Values ($RoomId, $ServiceId);
            """;
            command.Parameters.AddWithValue("$RoomId", roomId);
            command.Parameters.AddWithValue("$ServiceId", serviceId);
            command.ExecuteNonQuery();
        }
    }

    private int GetHourlyRate(SqliteConnection connection, SqliteTransaction transaction, int roomId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "Select HourlyRate from Rooms Where Id = $RoomId;";
        command.Parameters.AddWithValue("$RoomId", roomId);
        var result = command.ExecuteScalar();

        if (result is null)
        {
            throw new KeyNotFoundException("Room does not exist");
        }

        return Convert.ToInt32(result);
    }

    private int GetServicePrice(SqliteConnection connection, SqliteTransaction transaction, int roomId, int serviceId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
        """
        Select service.Price
        From Services as service
        Inner Join RoomServices as roomService on roomService.ServiceId = service.Id
        Where roomService.RoomId = $RoomId and service.Id = $ServiceId;
        """;
        command.Parameters.AddWithValue("$RoomId", roomId);
        command.Parameters.AddWithValue("$ServiceId", serviceId);
        var result = command.ExecuteScalar();

        if (result is null)
        {
            throw new ArgumentException("Service is not available for this room");
        }

        return Convert.ToInt32(result);
    }

    private void CheckAvailability(SqliteConnection connection, SqliteTransaction transaction, int roomId, DateTimeOffset time, DateTimeOffset endAt)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
        """
        Select Count(*)
        From Bookings
        Where RoomId = $RoomId
        And StartAt < $EndAt
        And EndAt > $StartAt;
        """;
        command.Parameters.AddWithValue("$RoomId", roomId);
        command.Parameters.AddWithValue("$StartAt", ToDatabaseTime(time));
        command.Parameters.AddWithValue("$EndAt", ToDatabaseTime(endAt));

        if (Convert.ToInt32(command.ExecuteScalar()) > 0)
        {
            throw new InvalidOperationException("Room is already booked for this time");
        }
    }

    private int CalculateRoomPrice(int hourlyRate, DateTimeOffset time, int duration)
    {
        double total = 0;
        for (int hour = 0; hour < duration; hour++)
        {
            int currentHour = time.AddHours(hour).Hour;
            double multiplier = 1;

            if (currentHour >= 12 && currentHour < 14)
            {
                multiplier = 1.15;
            }
            else if (currentHour >= 6 && currentHour < 9)
            {
                multiplier = 0.9;
            }
            else if (currentHour >= 18 && currentHour < 23)
            {
                multiplier = 0.8;
            }

            total += hourlyRate * multiplier;
        }

        return Convert.ToInt32(Math.Round(total));
    }

    private string ToDatabaseTime(DateTimeOffset time)
    {
        return time.UtcDateTime.ToString("O");
    }
}
