PRAGMA foreign_keys = ON;


CREATE TABLE IF NOT EXISTS Rooms ( 
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Capacity INTEGER NOT NULL CHECK (Capacity > 0),
    HourlyRate INTEGER NOT NULL CHECK (HourlyRate >= 0)
);



CREATE TABLE IF NOT EXISTS Services ( 
    Id integer Primary key Autoincrement,
    name string not null Unique, 
    Price int not null check (Price >  0)
);

Create table if not Exists RoomServices (
    RoomID integer not null, 
    ServiceId integer not null, 

    PRIMARY KEY (RoomId, ServiceId),

    FOREIGN KEY (Roomid) 
    References Rooms(Id)
    on delete cascade, 

    FOREIGN KEY (ServiceId)
    References  Services(Id)
    On Delete Cascade
);

Create Table if not exists Bookings ( 
    Id Integer Primary Key Autoincrement, 
    RoomId integer not null, 
    StartAt text not null, 
    EndAt text not null, 
    TotalPrice integer not null, 
    FOREIGN KEY (RoomId) REFERENCES Rooms (Id)
);