using ConferenceRoomBooking.Models;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton<Methods>();

using (var connection = new SqliteConnection("Data Source=Data/booking.db"))
{
    connection.Open();

    // Old local databases used RoomId instead of BookingId in this table.
    bool oldBookingServices = false;
    using (var checkCommand = connection.CreateCommand())
    {
        checkCommand.CommandText = "PRAGMA table_info(BookingServices);";
        using var reader = checkCommand.ExecuteReader();
        while (reader.Read())
        {
            if (reader.GetString(1) == "RoomId")
            {
                oldBookingServices = true;
            }
        }
    }

    if (oldBookingServices)
    {
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "Select Count(*) from BookingServices;";
        if (Convert.ToInt32(countCommand.ExecuteScalar()) > 0)
        {
            throw new InvalidOperationException("Old BookingServices table contains data and cannot be migrated automatically");
        }

        using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = "Drop table BookingServices;";
        dropCommand.ExecuteNonQuery();
    }

    using var command = connection.CreateCommand();
    command.CommandText = File.ReadAllText("Data/booking.sql");
    command.ExecuteNonQuery();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/rooms", (CreateRoomRequest request, Methods methods) => Run(() =>
{
    int id = methods.AddRoom(request.Name, request.Capacity, request.ServiceIds, request.HourlyRate);
    return Results.Created($"/rooms/{id}", new { Id = id });
}))
.WithName("AddRoom");

app.MapPut("/rooms/{id:int}", (int id, EditRoomRequest request, Methods methods) => Run(() =>
{
    bool changed = methods.EditRoom(id, request.Name, request.Capacity, request.ServiceIds, request.HourlyRate);
    return changed ? Results.NoContent() : Results.NotFound();
}))
.WithName("EditRoom");

app.MapDelete("/rooms/{id:int}", (int id, Methods methods) =>
    methods.DeleteRoom(id) ? Results.NoContent() : Results.NotFound())
.WithName("DeleteRoom");

app.MapGet("/rooms/available", (DateTimeOffset startAt, int duration, int capacity, Methods methods) => Run(() =>
{
    return Results.Ok(methods.FindEmptyRooms(startAt, duration, capacity));
}))
.WithName("FindAvailableRooms");

app.MapPost("/bookings", (CreateBookingRequest request, Methods methods) => Run(() =>
{
    var booking = methods.ReserveRoom(request.RoomId, request.StartAt, request.Duration, request.ServiceIds);
    return Results.Created($"/bookings/{booking.Id}", booking);
}))
.WithName("ReserveRoom");

app.MapGet("/reports/rooms", (DateTimeOffset from, DateTimeOffset to, Methods methods) => Run(() =>
{
    return Results.Ok(methods.GetRoomReport(from, to));
}))
.WithName("GetRoomReport");

app.Run();

static IResult Run(Func<IResult> action)
{
    try
    {
        return action();
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new { Error = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { Error = exception.Message });
    }
    catch (Exception exception) when (exception is ArgumentException or SqliteException)
    {
        return Results.BadRequest(new { Error = exception.Message });
    }
}
