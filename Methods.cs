
using System.Data;
using System.Data.Common;
using System.IO.Pipelines;
using System.Net.Quic;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;
using ConferenceRoomBooking.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;


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
    
    public int ReserveRoom (int id, DateTimeOffset time, int duration, int [] serviceIds ) 
    {
        if (duration <= 0)
        {
            throw new ArgumentException ( "Duration must be more then zero"); 
        }
        DateTimeOffset endAt = time.AddHours(duration); 

        using var connection = new SqliteConnection ("Data Source = database/booking.db"); 
        connection.Open(); 

        using var priceCommand = connection.CreateCommand(); 
        priceCommand.CommandText = 
        """ 
        Select ( (select HourlyRate from Rooms where Id = id) + Coalesce(sum(service.Price), 0)
        From RoomServices as roomservice
        inner join service as service
            on service.Id = roomService.ServiceId 
        Where roomService.RoomId = $Id)
        ; 
        """; 
        priceCommand.Parameters.AddWithValue("$Id", id); 
        var totalPrice = priceCommand.ExecuteScalar(); 
         
        if (totalPrice is null)
        {
            throw new ArgumentException ("room does not exist");
        }
        
        using var bookingCommand = connection.CreateCommand(); 
        bookingCommand.CommandText = 
        """
        Isert Into Bookings (
            RoomId,
            StartAt,
            EndAt,
            TotalPrice
            
        )
        Values ($Roomid, $StartAt, $endAt, TotalPrice); 
        """; 

        bookingCommand.Parameters.AddWithValue ("$roomId", id); 
        bookingCommand.Parameters.AddWithValue ("$startAt", time); 
        bookingCommand.Parameters.AddWithValue ("$endAt", endAt); 
        bookingCommand.Parameters.AddWithValue ("$totalPrice", totalPrice); 

        var result = bookingCommand.ExecuteScalar(); 
        return Convert.ToInt32(result);

    }

}