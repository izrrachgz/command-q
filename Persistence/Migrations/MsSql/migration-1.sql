-- SEQUENCE equivalent in SQL Server: Use IDENTITY instead of SEQUENCE

-- Table: HealthCheckLog
IF OBJECT_ID('dbo.HealthCheckLog', 'U') IS NOT NULL
    DROP TABLE dbo.HealthCheckLog;

CREATE TABLE dbo.HealthCheckLog
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE(),
    [Deleted] DATETIME NULL,
    CONSTRAINT PK_HealthCheckLog_Id PRIMARY KEY (Id)
);
--For faster performance
CREATE INDEX IX_HealthCheckLog_Created ON dbo.HealthCheckLog (Created) WHERE Deleted IS NULL;

-- Table: Account
IF OBJECT_ID('dbo.Account', 'U') IS NOT NULL
    DROP TABLE dbo.Account;

CREATE TABLE dbo.Account
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Username] VARCHAR(256) NOT NULL,
    [Email] VARCHAR(256) NOT NULL,
    [Password] VARCHAR(256) NOT NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE(),
    [Deleted] DATETIME NULL,
    CONSTRAINT PK_Account_Id PRIMARY KEY (Id)
);
--Login lookup performance
CREATE UNIQUE INDEX UX_Account_Username ON dbo.Account (Username) WHERE Deleted IS NULL;
CREATE UNIQUE INDEX UX_Account_Email ON dbo.Account (Email) WHERE Deleted IS NULL;
--Active Account
CREATE INDEX IX_Account_Active ON dbo.Account (Deleted) INCLUDE (Username, Email, Created);

-- Table: Address
IF OBJECT_ID('dbo.Address', 'U') IS NOT NULL
    DROP TABLE dbo.Address;

CREATE TABLE dbo.Address
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Name] VARCHAR(256) NOT NULL,
    [Description] VARCHAR(MAX) NULL,
    [Line1] VARCHAR(256) NOT NULL,
    [Line2] VARCHAR(256) NULL,
    [ExternalNumber] VARCHAR(16) NULL,
    [InternalNumber] VARCHAR(16) NULL,
    [References] VARCHAR(512) NULL,
    [City] VARCHAR(64) NOT NULL,
    [PostalCode] VARCHAR(16) NOT NULL,
    [Country] VARCHAR(128) NOT NULL,
    [Phone] VARCHAR(24) NULL,
    [EmailAddress] VARCHAR(256) NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE(),
    [Deleted] DATETIME NULL,
    CONSTRAINT PK_Address_Id PRIMARY KEY (Id),
    CONSTRAINT FK_Address_Account FOREIGN KEY (AccountId) REFERENCES Account(Id)
);
--For faster performance
CREATE INDEX IX_Address_AccountId ON dbo.Address (AccountId) WHERE Deleted IS NULL;
--For lookups
CREATE INDEX IX_Address_Location ON dbo.Address (Country, City, PostalCode) WHERE Deleted IS NULL;
--For listing
CREATE INDEX IX_Address_Created ON dbo.Address (Created) WHERE Deleted IS NULL;

-- Table: Contact
IF OBJECT_ID('dbo.Contact', 'U') IS NOT NULL
    DROP TABLE dbo.Contact;

CREATE TABLE dbo.Contact
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Name] VARCHAR(256) NOT NULL,
    [Description] VARCHAR(MAX) NULL,
    [EmailAddress] VARCHAR(256) NULL,
    [PhoneNumber] VARCHAR(256) NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE(),
    [Deleted] DATETIME NULL,
    CONSTRAINT PK_Contact_Id PRIMARY KEY (Id),
    CONSTRAINT FK_Contact_Account FOREIGN KEY (AccountId) REFERENCES Account(Id)
);
--For faster performance
CREATE INDEX IX_Contact_AccountId ON dbo.Contact (AccountId) WHERE Deleted IS NULL;
--For lookups
CREATE INDEX IX_Contact_Email ON dbo.Contact (EmailAddress) WHERE Deleted IS NULL;

-- Table: Event
IF OBJECT_ID('dbo.Event', 'U') IS NOT NULL
    DROP TABLE dbo.Event;

CREATE TABLE dbo.Event
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Name] VARCHAR(256) NOT NULL,
    [Description] VARCHAR(MAX) NULL,
    [Start] DATETIME NOT NULL,
    [End] DATETIME NOT NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE(),
    [Deleted] DATETIME NULL,
    CONSTRAINT PK_Event_Id PRIMARY KEY (Id)
);
--For faster performance
CREATE INDEX IX_Event_Start_End ON dbo.Event (Start, [End]) WHERE Deleted IS NULL;
--For lookups
CREATE INDEX IX_Event_Name ON dbo.Event (Name) WHERE Deleted IS NULL;

-- Table: Place
IF OBJECT_ID('dbo.Place', 'U') IS NOT NULL
    DROP TABLE dbo.Place;

CREATE TABLE dbo.Place
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Name] VARCHAR(256) NOT NULL,
    [Description] VARCHAR(MAX) NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE(),
    [Deleted] DATETIME NULL,
    CONSTRAINT PK_Place_Id PRIMARY KEY (Id)
);
--For faster performance
CREATE INDEX IX_Place_Name ON dbo.Place (Name) WHERE Deleted IS NULL;

-- Table: Register
IF OBJECT_ID('dbo.Register', 'U') IS NOT NULL
    DROP TABLE dbo.Register;

CREATE TABLE dbo.Register
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Action] SMALLINT NOT NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE(),
    [Deleted] DATETIME NULL,
    CONSTRAINT PK_Register_Id PRIMARY KEY (Id),
    CONSTRAINT FK_Register_Account FOREIGN KEY (AccountId) REFERENCES Account(Id)
);
--For faster performance
CREATE INDEX IX_Register_AccountId ON dbo.Register (AccountId) WHERE Deleted IS NULL;
--For audit/history
CREATE INDEX IX_Register_Created ON dbo.Register (Created) WHERE Deleted IS NULL;
--For filter by action
CREATE INDEX IX_Register_Action ON dbo.Register (Action, Created) WHERE Deleted IS NULL;

-- Table: RegisterEvent
IF OBJECT_ID('dbo.RegisterEvent', 'U') IS NOT NULL
    DROP TABLE dbo.RegisterEvent;

CREATE TABLE dbo.RegisterEvent
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [RegisterId] BIGINT NOT NULL,
    [EventId] BIGINT NOT NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE(),
    [Deleted] DATETIME NULL,
    CONSTRAINT PK_RegisterEvent_Id PRIMARY KEY (Id),
    CONSTRAINT FK_RegisterEvent_Event FOREIGN KEY (EventId) REFERENCES Event(Id),
    CONSTRAINT FK_RegisterEvent_Register FOREIGN KEY (RegisterId) REFERENCES Register(Id)
);
--For faster performance
CREATE INDEX IX_RegisterEvent_RegisterId ON dbo.RegisterEvent (RegisterId) WHERE Deleted IS NULL;
--For faster performance
CREATE INDEX IX_RegisterEvent_EventId ON dbo.RegisterEvent (EventId) WHERE Deleted IS NULL;
--Prevent duplicate entries
CREATE UNIQUE INDEX UX_RegisterEvent_Unique ON dbo.RegisterEvent (RegisterId, EventId) WHERE Deleted IS NULL;

-- Table: RegisterPlace
IF OBJECT_ID('dbo.RegisterPlace', 'U') IS NOT NULL
    DROP TABLE dbo.RegisterPlace;

CREATE TABLE dbo.RegisterPlace
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [RegisterId] BIGINT NOT NULL,
    [PlaceId] BIGINT NOT NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Modified] DATETIME NOT NULL DEFAULT GETDATE(),
    [Deleted] DATETIME NULL,
    CONSTRAINT PK_RegisterPlace_Id PRIMARY KEY (Id),
    CONSTRAINT FK_RegisterPlace_Place FOREIGN KEY (PlaceId) REFERENCES Place(Id),
    CONSTRAINT FK_RegisterPlace_Register FOREIGN KEY (RegisterId) REFERENCES Register(Id)
);
--For faster performance
CREATE INDEX IX_RegisterPlace_RegisterId ON dbo.RegisterPlace (RegisterId) WHERE Deleted IS NULL;
--For faster performance
CREATE INDEX IX_RegisterPlace_PlaceId ON dbo.RegisterPlace (PlaceId) WHERE Deleted IS NULL;
--Prevent duplicate entries
CREATE UNIQUE INDEX UX_RegisterPlace_Unique ON dbo.RegisterPlace (RegisterId, PlaceId) WHERE Deleted IS NULL;

-- Table: Migrations
IF OBJECT_ID('dbo.Migrations', 'U') IS NOT NULL
    DROP TABLE dbo.Migrations;

CREATE TABLE dbo.Migrations
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Created] DATETIME NOT NULL DEFAULT GETDATE(),
    [Deleted] DATETIME NULL,
    CONSTRAINT PK_Migrations_Id PRIMARY KEY (Id)
);

-- Always insert a new record when what is considered a migration ends
INSERT INTO dbo.Migrations (Deleted) VALUES (NULL);
