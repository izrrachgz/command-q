-- Drop existing tables if they exist
DROP TABLE IF EXISTS HealthCheckLog;
DROP TABLE IF EXISTS Migrations;

-- Table: HealthCheckLog
CREATE TABLE HealthCheckLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Created DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    Modified DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    Deleted DATETIME NULL
);

-- Table: Migrations
CREATE TABLE Migrations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Created DATETIME NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    Deleted DATETIME NULL
);

-- Always insert a new record when what is considered a migration ends
INSERT INTO Migrations (Deleted) VALUES (NULL);
