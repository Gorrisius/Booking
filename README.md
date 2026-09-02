# Conference Room Booking

Simple ASP.NET Core API for managing conference rooms and their bookings. Data is stored in SQLite.

## Run

```bash
dotnet restore
dotnet run
```

The database is created in `Data/booking.db`. On first start it is filled with Hall A, Hall B, Hall C and the Projector, Wi-Fi and Sound services.

## API

- `POST /rooms` - add a room
- `PUT /rooms/{id}` - edit a room
- `DELETE /rooms/{id}` - delete a room
- `GET /rooms/available` - find rooms by start time, duration and capacity
- `POST /bookings` - reserve a room and calculate its total price
- `GET /reports/rooms` - booking count and revenue for every room

The OpenAPI document is available at `/openapi/v1.json` in the Development environment.

## Price calculation

- 06:00-09:00 - 10% discount
- 09:00-12:00 - standard hourly rate
- 12:00-14:00 - 15% extra
- 14:00-18:00 - standard hourly rate
- 18:00-23:00 - 20% discount

The selected services are added once to the room price. A room cannot be booked if another booking overlaps the requested time.
