namespace ConferenceRoomBooking.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int HourlyRate { get; set; }
}

public class CreateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int[] ServiceIds { get; set; } = [];
    public int HourlyRate { get; set; }
}

public class EditRoomRequest
{
    public string? Name { get; set; }
    public int? Capacity { get; set; }
    public int[]? ServiceIds { get; set; }
    public int? HourlyRate { get; set; }
}

public class CreateBookingRequest
{
    public int RoomId { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public int Duration { get; set; }
    public int[] ServiceIds { get; set; } = [];
}

public class BookingResult
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public int TotalPrice { get; set; }
}

public class RoomReport
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public int Revenue { get; set; }
}
