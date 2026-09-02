PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Rooms (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Capacity INTEGER NOT NULL CHECK (Capacity > 0),
    HourlyRate INTEGER NOT NULL CHECK (HourlyRate >= 0)
);

CREATE TABLE IF NOT EXISTS Services (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Price INTEGER NOT NULL CHECK (Price > 0)
);

CREATE TABLE IF NOT EXISTS RoomServices (
    RoomId INTEGER NOT NULL,
    ServiceId INTEGER NOT NULL,
    PRIMARY KEY (RoomId, ServiceId),
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Bookings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RoomId INTEGER NOT NULL,
    StartAt TEXT NOT NULL,
    EndAt TEXT NOT NULL,
    TotalPrice INTEGER NOT NULL CHECK (TotalPrice >= 0),
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Bookings_RoomId_StartAt_EndAt
ON Bookings (RoomId, StartAt, EndAt);

CREATE TABLE IF NOT EXISTS BookingServices (
    BookingId INTEGER NOT NULL,
    ServiceId INTEGER NOT NULL,
    PRIMARY KEY (BookingId, ServiceId),
    FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
);

INSERT INTO Services (Name, Price)
SELECT 'Projector', 500
WHERE NOT EXISTS (SELECT 1 FROM Services WHERE Name = 'Projector');

INSERT INTO Services (Name, Price)
SELECT 'Wi-Fi', 300
WHERE NOT EXISTS (SELECT 1 FROM Services WHERE Name = 'Wi-Fi');

INSERT INTO Services (Name, Price)
SELECT 'Sound', 700
WHERE NOT EXISTS (SELECT 1 FROM Services WHERE Name = 'Sound');

INSERT INTO Rooms (Name, Capacity, HourlyRate)
SELECT 'Hall A', 50, 2000
WHERE NOT EXISTS (SELECT 1 FROM Rooms WHERE Name = 'Hall A');

INSERT INTO Rooms (Name, Capacity, HourlyRate)
SELECT 'Hall B', 100, 3500
WHERE NOT EXISTS (SELECT 1 FROM Rooms WHERE Name = 'Hall B');

INSERT INTO Rooms (Name, Capacity, HourlyRate)
SELECT 'Hall C', 30, 1500
WHERE NOT EXISTS (SELECT 1 FROM Rooms WHERE Name = 'Hall C');

INSERT OR IGNORE INTO RoomServices (RoomId, ServiceId)
SELECT room.Id, service.Id
FROM Rooms as room
CROSS JOIN Services as service
WHERE room.Name IN ('Hall A', 'Hall B', 'Hall C');
