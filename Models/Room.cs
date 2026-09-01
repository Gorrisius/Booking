namespace ConferenceRoomBooking.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int HourlyRate { get; set; }
}

