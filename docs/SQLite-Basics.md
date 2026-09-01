# SQLite basics for ConferenceRoomBooking

## What SQLite is

SQLite is a relational database stored in one local file. It does not need a separate database server or service. That makes it convenient for learning, local development, automated tests, and a small demonstration project.

SQLite still uses SQL and supports tables, primary keys, foreign keys, indexes, transactions, and joins. The main difference from systems such as PostgreSQL or SQL Server is that the application reads and writes a local database file directly.

For this project, a file such as `conference-room-booking.db` could contain rooms, available services, bookings, and the services selected for each booking.

## Two ways to work with SQLite

You can write SQL directly:

```sql
CREATE TABLE Rooms (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Capacity INTEGER NOT NULL,
    HourlyRate INTEGER NOT NULL
);

INSERT INTO Rooms (Name, Capacity, HourlyRate)
VALUES ('Hall A', 50, 2000);

SELECT Id, Name, Capacity, HourlyRate
FROM Rooms
WHERE Capacity >= 40;

UPDATE Rooms
SET HourlyRate = 2500
WHERE Id = 1;

DELETE FROM Rooms
WHERE Id = 1;
```

In an ASP.NET Core application, Entity Framework Core (EF Core) is usually more convenient. You work with C# objects and LINQ, and EF Core translates those operations into SQL.

## Recommended database model

Do not store services in `Room.Options` as a string array. A relational design makes services searchable and gives each service its own price.

Suggested tables:

```text
Room 1 ─── many RoomService many ─── 1 AdditionalService
Room 1 ─── many Booking
Booking 1 ─── many BookingService many ─── 1 AdditionalService
```

- `Room`: ID, name, capacity, and hourly rate.
- `AdditionalService`: ID, name, and price (projector, Wi-Fi, sound).
- `RoomService`: which services a room supports.
- `Booking`: room ID, start time, end time, and calculated total price.
- `BookingService`: which services were selected for a booking.

Money should normally use `decimal`, not `int`, `float`, or `double`. A booking should use full date-and-time values (`DateTime` or `DateTimeOffset`) rather than only `TimeOnly`, because availability depends on the date too.

## 1. Install EF Core's SQLite provider

Run these commands from the project directory:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.11
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.11
dotnet tool install --global dotnet-ef
```

Keep all EF Core packages on the same major and minor version. If `dotnet-ef` is already installed, update it instead:

```bash
dotnet tool update --global dotnet-ef
```

These commands are propositions only; they have not been run for this project.

## 2. Make the room entity persistable

The current `Room` properties are private because they do not have an access modifier. EF Core and API serialization should receive public properties.

An improved starting point would be:

```csharp
namespace ConferenceRoomBooking.Models;

public class Room
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Capacity { get; set; }
    public decimal HourlyRate { get; set; }

    public List<Booking> Bookings { get; set; } = [];
}
```

Example booking entity:

```csharp
namespace ConferenceRoomBooking.Models;

public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public decimal TotalPrice { get; set; }

    public Room Room { get; set; } = null!;
}
```

Validation such as positive capacity, positive price, and `EndsAt > StartsAt` should happen before saving.

## 3. Create a DbContext

The `DbContext` represents a session with the database. A possible `Data/BookingDbContext.cs` is:

```csharp
using ConferenceRoomBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Data;

public class BookingDbContext(DbContextOptions<BookingDbContext> options)
    : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>()
            .HasIndex(room => room.Name)
            .IsUnique();

        modelBuilder.Entity<Booking>()
            .HasCheckConstraint(
                "CK_Booking_TimeRange",
                "EndsAt > StartsAt");
    }
}
```

## 4. Add the connection string

Example `appsettings.json` entry:

```json
{
  "ConnectionStrings": {
    "BookingDatabase": "Data Source=conference-room-booking.db"
  }
}
```

This tells SQLite to create or open `conference-room-booking.db` in the application's working directory. Do not commit real secrets for server databases. SQLite connection strings normally contain a path rather than a password.

## 5. Register the context in Program.cs

```csharp
using ConferenceRoomBooking.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("BookingDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'BookingDatabase' was not found.");

builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlite(connectionString));
```

ASP.NET Core will then create one context per HTTP request by default.

## 6. Create the schema with migrations

A migration is a versioned description of a database schema change.

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The first command creates C# migration files. The second applies them and creates the SQLite database file. Later model changes should get new, clearly named migrations:

```bash
dotnet ef migrations add AddAdditionalServices
dotnet ef database update
```

Do not manually edit the generated database file. Change the entity/configuration, create a migration, inspect it, and then apply it.

## 7. Basic EF Core operations

Create:

```csharp
var room = new Room
{
    Name = "Hall A",
    Capacity = 50,
    HourlyRate = 2000m
};

db.Rooms.Add(room);
await db.SaveChangesAsync();
```

Read:

```csharp
var rooms = await db.Rooms
    .AsNoTracking()
    .Where(room => room.Capacity >= requestedCapacity)
    .ToListAsync();
```

Update:

```csharp
var room = await db.Rooms.FindAsync(roomId);
if (room is null)
{
    return Results.NotFound();
}

room.HourlyRate = 2500m;
await db.SaveChangesAsync();
```

Delete:

```csharp
var room = await db.Rooms.FindAsync(roomId);
if (room is null)
{
    return Results.NotFound();
}

db.Rooms.Remove(room);
await db.SaveChangesAsync();
```

Use asynchronous EF Core methods in API handlers so database I/O does not block request-processing threads.

## 8. Availability and overlapping bookings

Two time ranges overlap when an existing booking starts before the requested end and ends after the requested start:

```csharp
var rooms = await db.Rooms
    .AsNoTracking()
    .Where(room => room.Capacity >= requestedCapacity)
    .Where(room => !room.Bookings.Any(booking =>
        booking.StartsAt < requestedEnd &&
        booking.EndsAt > requestedStart))
    .ToListAsync();
```

Notice the strict `<` and `>` comparisons. They allow one booking to begin exactly when another finishes.

Checking availability and inserting a booking must be treated as one operation. Otherwise two simultaneous requests could both observe an available room and create overlapping reservations. SQLite permits only one writer at a time, but application-level validation and a transaction are still important. For a production system with heavy concurrent booking traffic, PostgreSQL would be a stronger long-term choice.

## 9. Seed the task's initial data

The PDF requires these initial rooms:

- Hall A: capacity 50, 2000 UAH/hour.
- Hall B: capacity 100, 3500 UAH/hour.
- Hall C: capacity 30, 1500 UAH/hour.

It also defines projector (500 UAH), Wi-Fi (300 UAH), and sound (700 UAH). These can be inserted through EF Core model seeding or an application startup initializer. Use stable IDs or another deterministic key so the same records are not inserted every time the application starts.

## 10. Useful development commands

```bash
dotnet build
dotnet run
dotnet ef migrations list
dotnet ef database update
sqlite3 conference-room-booking.db
```

Inside the optional `sqlite3` command-line program:

```sql
.tables
.schema Rooms
SELECT * FROM Rooms;
.quit
```

The `sqlite3` executable is separate from the EF Core NuGet provider and might need to be installed on your operating system. The application itself does not require that executable.

## Suggested implementation order

1. Correct and expand the `Room` model.
2. Add `AdditionalService`, `Booking`, and join entities.
3. Add EF Core packages and `BookingDbContext`.
4. Add the SQLite connection string and dependency registration.
5. Create and inspect the initial migration.
6. Seed the initial rooms and services.
7. Implement room CRUD endpoints.
8. Implement availability search.
9. Implement booking and price calculation.
10. Add tests, especially for overlapping bookings and pricing boundaries.

## Important limitations

- SQLite is excellent for this assignment but is not designed for many simultaneous writers.
- SQLite has fewer built-in data types than PostgreSQL or SQL Server. EF Core maps .NET types onto SQLite's storage classes.
- Enable and model foreign-key relationships correctly; do not represent relationships as comma-separated text.
- Do not expose EF entities directly as every API request and response. Request/response DTOs make validation and future changes safer.
- Do not trust a price supplied by the client. Calculate the final price on the server and save the calculated result.

## Official references

- [SQLite EF Core provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
- [EF Core migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SQLite SQL language](https://www.sqlite.org/lang.html)
- [SQLite foreign keys](https://www.sqlite.org/foreignkeys.html)
