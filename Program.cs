using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Open the Data and run the SQL file once when the application starts.
using (var connection = new SqliteConnection("Data Source=Data/booking.db"))
{
    connection.Open();

    var sql = File.ReadAllText("Data/booking.sql");
    using var command = connection.CreateCommand();
    command.CommandText = sql;
    command.ExecuteNonQuery();
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
