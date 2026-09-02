using ConferenceRoomBooking.Models;
using Microsoft.Data.Sqlite;


class Meethods
{
    
    public int AddRoom(string name, int capacity, int[] serviceIds, int hourlyRate) 
    {
        // value - базова плата за оренду залу
        using var connection = new SqliteConnection("Data Source=database/booking.db"); 
        connection.Open(); 
        using var transaction = connection.BeginTransaction(); 
        using var command = connection.CreateCommand(); 
        command.Transaction = transaction; 
        //
        command.CommandText = 
        """
        insert into rooms ( Name, Capacity, HourlyRate)
        Values ($Name, $Capacity, $HourlyRate);
        Select last_insert_rowid(); 

        """;
        command.Parameters.AddWithValue("$Name", name); 
        command.Parameters.AddWithValue("$Capacity", capacity); 
        command.Parameters.AddWithValue("$Hourlyrate", hourlyRate);
        
        int roomId = Convert.ToInt32 (command.ExecuteScalar());
        //
        foreach ( var serviceId in serviceIds)
        {
            using var serviceCommand = connection.CreateCommand();  
            serviceCommand.Transaction = transaction; 
            serviceCommand.CommandText = 
            """
                Insert into RoomServices (RoomId, ServiceId)
                Values ($RoomId, $ServiceId);
            """;
            serviceCommand.Parameters.AddWithValue("$RoomId", roomId); 
            serviceCommand.Parameters.AddWithValue("$ServiceId", serviceId); 

            serviceCommand.ExecuteNonQuery();
        }
        transaction.Commit(); 

        return roomId; 

    }

    public bool EditRoom (int id, string ? name = null, int ?  capacity = null, int [] ? serviceIds = null, int ? hourlyRate = null)
    {
       using var connection = new SqliteConnection("Data Source = database/booking.db"); 
       connection.Open(); 
       using var transaction = connection.BeginTransaction(); 
       using var command = connection.CreateCommand(); 
       command.Transaction = transaction; 
       command.CommandText = 
       """
       Update Rooms
       set  
            Name = Coalesce ($Name, name), 
            Capacity = Coalesce ($Capacity, capacity),
            HourlyRate = Coalesce ($HourlyRate, HourlyRate)
        Where Id = $id;

       """;
        command.Parameters.AddWithValue ("$Id", id); 
        command.Parameters.AddWithValue ("$Name", name is null ? DBNull.Value : name); 
        command.Parameters.AddWithValue ("$Capacity", capacity is null ? DBNull.Value : capacity); 
        command.Parameters.AddWithValue ("$HourlyRate", hourlyRate is null ? DBNull. Value : hourlyRate); 

        int changedRows = command.ExecuteNonQuery();
        
        if ( changedRows == 0)
        {
            transaction.Rollback(); 
            return false; 
        }
        
        if (serviceIds is not null){
            
            using var deleteCommand = connection.CreateCommand(); 
            deleteCommand.Transaction = transaction; 
            deleteCommand.CommandText = 
            """
            Delete From RoomServices
            Where RoomId = $RoomId; 
            """; 

            deleteCommand.Parameters.AddWithValue("$RoomId", id); 
            deleteCommand.ExecuteNonQuery();

            foreach (int serviceId in serviceIds)
            {
                using var insert = connection.CreateCommand();
                insert.Transaction = transaction; 
                insert.CommandText = 
                """
                Insert into RoomServices (RoomId, ServiceId)
                Values ($RoomId, $ServiceId);
                """; 

                insert.Parameters.AddWithValue("$RoomId", id); 
                insert.Parameters.AddWithValue("$ServiceId", serviceId); 
                insert.ExecuteNonQuery(); 

            }
        }
        transaction.Commit(); 
        return true;  
        
    }

    public bool DeleteRoom(int id)
    {
        using var connection = new SqliteConnection ("Data Source = database/booking.db"); 
        connection.Open(); 
        using  var command = connection.CreateCommand(); 
        command.CommandText = 
        """
        Delete From Rooms
        Where Id = $id;
        """; 

        command.Parameters.AddWithValue("$Id", id); 
        int changedRows = command.ExecuteNonQuery(); 
        return changedRows >  0;
    }

    public Room [] FindEmptyRooms (DateTimeOffset time, int duration ,int capacity)
    {
        if (duration <= 0)
        {
            throw new ArgumentException (" duration must be greater then zero"); 
        }
        DateTimeOffset endAt = time.AddHours(duration);

        using var connection = new SqliteConnection ("Data Source = database/booking.db"); 
        connection.Open(); 

    using var command = connection.CreateCommand(); 
    command.CommandText = 
    """
    Select 
        room.Id,
        room.Name,
        room.Capacity,
        room.HourlyRate
        From Rooms As room
        Where room.capacity >= $capacity
        And NOt exists (
        Select 1
        From Bookings AS Booking
        Where booking.RoomId = room.Id
            And booking.StartAt < $endAt
            And booking.EndAt > $startAt 
    ); 
    """; 
    command.Parameters.AddWithValue("$capacity", capacity); 
    command.Parameters.AddWithValue("$startAt", time.UtcDateTime.ToString("O")); 
    command.Parameters.AddWithValue("$endAt", endAt.UtcDateTime.ToString("O")); 
    using var reader = command.ExecuteReader(); 
    var rooms = new List<Room>(); 
    while (reader.Read())
        {
            rooms.Add (new Room
            {
                Id = reader.GetInt32(0), 
                Name = reader.GetString(1),
                Capacity = reader.GetInt32(2),
                HourlyRate = reader.GetInt32(3)
            }); 
        }
        return rooms.ToArray();
    }



    //що мені там треба було зробити ? переписати резервацію кімнат і додати таблицю для обраних опцій
    /*
        тоді можна почати з початку ай гес, додати таблицю, там просто буде ід кімнати і ід для послуги, в мене вже мала б бути функція для зчитування резервацвії тому візьмемо її

    */
    
    public int ReserveRoom (int id, DateTimeOffset time, int duration, int [] serviceIds ) 
    {
        if (duration <= 0)
        {
            throw new ArgumentException ( "Duration must be more then zero"); 
        }
        DateTimeOffset endAt = time.AddHours(duration); 

        using var connection = new SqliteConnection ("Data Source = database/booking.db"); 
        connection.Open(); 



        // переробити бо вона просто зчитує всі можливості, а мені треба зчитувати те, що ми резервуємо

        foreach ( var serviceId in serviceIds)
        {
            using var add = connection.CreateCommand();
            add.CommandText =
            """
            Insert into BookingServices (RoomId, ServiceId)
            Values ($RoomId, $ServiceId);
            """;
            add.Parameters.AddWithValue("$RoomId", id);
            add.Parameters.AddWithValue("$ServiceId", serviceId);
            add.ExecuteNonQuery();
        }
        using var priceCommand = connection.CreateCommand(); 
        priceCommand.CommandText = 
        """ 
        Select (room.HourlyRate * $duration) + Coalesce(
            Sum(service.Price) , 0)
            From Rooms as room
            Left Join BookingServices as bookingService 
                on bookingService.RoomId = room.Id
            left join services as service
                on service.Id = bookingService.ServiceId
            Where room.Id = $Id
            Group by room.Id, room.HourlyRate; 
        """; 

        priceCommand.Parameters.AddWithValue("$Id", id);
        priceCommand.Parameters.AddWithValue("$duration", duration);

        var totalPrice = priceCommand.ExecuteScalar(); 

         
        if (totalPrice is null)
        {
            throw new ArgumentException ("room does not exist");
        }
        
        using var bookingCommand = connection.CreateCommand(); 
        bookingCommand.CommandText = 
        """
        Insert Into Bookings (
            RoomId,
            StartAt,
            EndAt,
            TotalPrice
            
        )
        Values ($Roomid, $StartAt, $endAt, $TotalPrice) 
        returning Id;
        """; 

        bookingCommand.Parameters.AddWithValue ("$roomId", id); 
        bookingCommand.Parameters.AddWithValue ("$startAt", time); 
        bookingCommand.Parameters.AddWithValue ("$endAt", endAt); 
        bookingCommand.Parameters.AddWithValue ("$totalPrice", totalPrice); 

        var result = bookingCommand.ExecuteScalar(); 
        return Convert.ToInt32(result);

    }

}
