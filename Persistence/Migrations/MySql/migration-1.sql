-- MySQL does not support sequences, use AUTO_INCREMENT instead

-- Table: HealthCheckLog
DROP TABLE IF EXISTS HealthCheckLog;

CREATE TABLE HealthCheckLog (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
    PRIMARY KEY (Id)
);

-- Table: Migrations
DROP TABLE IF EXISTS Migrations;

CREATE TABLE Migrations (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
    PRIMARY KEY (Id)
);

-- Always insert a new record when what is considered a migration ends
INSERT INTO Migrations (Deleted) VALUES (NULL);
