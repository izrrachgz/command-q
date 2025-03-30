-- SEQUENCE equivalent in SQL Server: Use IDENTITY instead of SEQUENCE

-- Table: HealthCheckLog
IF OBJECT_ID('dbo.HealthCheckLog', 'U') IS NOT NULL
    DROP TABLE dbo.HealthCheckLog;

CREATE TABLE dbo.HealthCheckLog
(
    Id BIGINT IDENTITY(1,1) NOT NULL,
    Created DATETIME NOT NULL DEFAULT GETDATE(),
    Modified DATETIME NOT NULL DEFAULT GETDATE(),
    Deleted DATETIME NULL,
    CONSTRAINT PK_HealthCheckLog_Id PRIMARY KEY (Id)
);

-- Table: Migrations
IF OBJECT_ID('dbo.Migrations', 'U') IS NOT NULL
    DROP TABLE dbo.Migrations;

CREATE TABLE dbo.Migrations
(
    Id BIGINT IDENTITY(1,1) NOT NULL,
    Created DATETIME NOT NULL DEFAULT GETDATE(),
    Deleted DATETIME NULL,
    CONSTRAINT PK_Migrations_Id PRIMARY KEY (Id)
);

-- Always insert a new record when what is considered a migration ends
INSERT INTO dbo.Migrations (Deleted) VALUES (NULL);
