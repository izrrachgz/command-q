-- SEQUENCE equivalent in SQL Server: Use IDENTITY instead of SEQUENCE

-- Table: HealthCheckLog

CREATE TABLE dbo.HealthCheckLog
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_HealthCheckLog_Id PRIMARY KEY (Id)
);
--For faster performance
CREATE INDEX IX_HealthCheckLog_Created ON dbo.HealthCheckLog (Created) WHERE Deleted IS NULL;

-- Table: Account

CREATE TABLE dbo.Account
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Username] VARCHAR(256) NOT NULL,
    [Email] VARCHAR(256) NOT NULL,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_Account_Id PRIMARY KEY (Id)
);
--Profile lookup performance
CREATE UNIQUE INDEX UX_Account_Username ON dbo.Account (Username) WHERE Deleted IS NULL;
CREATE UNIQUE INDEX UX_Account_Email ON dbo.Account (Email) WHERE Deleted IS NULL;
--Active Account
CREATE INDEX IX_Account_Active ON dbo.Account (Deleted) INCLUDE (Username, Email, Created);

-- Table: AccountIdentity

CREATE TABLE dbo.AccountIdentity
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Issuer] NVARCHAR(500) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [Subject] NVARCHAR(256) COLLATE Latin1_General_100_BIN2 NOT NULL,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_AccountIdentity_Id PRIMARY KEY (Id),
    CONSTRAINT FK_AccountIdentity_Account FOREIGN KEY ([AccountId]) REFERENCES Account(Id)
);
CREATE INDEX IX_AccountIdentity_AccountId ON dbo.AccountIdentity (AccountId) WHERE Deleted IS NULL;
CREATE UNIQUE INDEX UX_AccountIdentity_Issuer_Subject ON dbo.AccountIdentity (Issuer, Subject) WHERE Deleted IS NULL;

-- Table: AccountActivity
CREATE TABLE dbo.AccountActivity
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Module] VARCHAR(128) NOT NULL,
    [Action] VARCHAR(128) NOT NULL,
    [EntityType] VARCHAR(128) NULL,
    [EntityId] VARCHAR(128) NULL,
    [Message] VARCHAR(1024) NOT NULL,
    [MetadataJson] VARCHAR(MAX) NULL,
    [IpAddress] VARCHAR(64) NULL,
    [UserAgent] VARCHAR(512) NULL,
    [CreatedUtc] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_AccountActivity_Id PRIMARY KEY (Id),
    CONSTRAINT FK_AccountActivity_Account FOREIGN KEY (AccountId) REFERENCES Account(Id)
);
CREATE INDEX IX_AccountActivity_AccountId_CreatedUtc ON dbo.AccountActivity (AccountId, CreatedUtc DESC) WHERE Deleted IS NULL;
CREATE INDEX IX_AccountActivity_Module_Action ON dbo.AccountActivity (Module, Action, CreatedUtc DESC) WHERE Deleted IS NULL;
CREATE INDEX IX_AccountActivity_Entity ON dbo.AccountActivity (EntityType, EntityId) WHERE Deleted IS NULL AND EntityType IS NOT NULL AND EntityId IS NOT NULL;

-- Table: AccountPreference

CREATE TABLE dbo.AccountPreference
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Culture] VARCHAR(16) NOT NULL DEFAULT 'en-US',
    [Theme] SMALLINT NOT NULL DEFAULT 0,
    [CompactNavigation] BIT NOT NULL DEFAULT 0,
    [EnhancedFocus] BIT NOT NULL DEFAULT 0,
    [ReduceMotion] BIT NOT NULL DEFAULT 0,
    [ShowDenseTables] BIT NOT NULL DEFAULT 0,
    [TimeZone] VARCHAR(64) NOT NULL DEFAULT 'America/Hermosillo',
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_AccountPreference_Id PRIMARY KEY (Id),
    CONSTRAINT FK_AccountPreference_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account(Id),
    CONSTRAINT CK_AccountPreference_Theme CHECK (Theme IN (0, 1))
);
CREATE UNIQUE INDEX UX_AccountPreference_AccountId ON dbo.AccountPreference (AccountId) WHERE Deleted IS NULL;
CREATE INDEX IX_AccountPreference_TimeZone ON dbo.AccountPreference (TimeZone) WHERE Deleted IS NULL;

-- Table: Address

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
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
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

CREATE TABLE dbo.Contact
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Name] VARCHAR(256) NOT NULL,
    [Description] VARCHAR(MAX) NULL,
    [EmailAddress] VARCHAR(256) NULL,
    [PhoneNumber] VARCHAR(256) NULL,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_Contact_Id PRIMARY KEY (Id),
    CONSTRAINT FK_Contact_Account FOREIGN KEY (AccountId) REFERENCES Account(Id)
);
--For faster performance
CREATE INDEX IX_Contact_AccountId ON dbo.Contact (AccountId) WHERE Deleted IS NULL;
--For lookups
CREATE INDEX IX_Contact_Email ON dbo.Contact (EmailAddress) WHERE Deleted IS NULL;

-- Table: AccountFile

CREATE TABLE dbo.AccountFile
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [FileName] VARCHAR(256) NOT NULL,
    [Path] VARCHAR(1024) NOT NULL,
    [ContentType] VARCHAR(256) NULL,
    [Size] BIGINT NOT NULL DEFAULT 0,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_AccountFile_Id PRIMARY KEY (Id),
    CONSTRAINT FK_AccountFile_Account FOREIGN KEY (AccountId) REFERENCES Account(Id)
);
--For faster performance
CREATE INDEX IX_AccountFile_AccountId ON dbo.AccountFile (AccountId) WHERE Deleted IS NULL;
--For listing
CREATE INDEX IX_AccountFile_Created ON dbo.AccountFile (Created) WHERE Deleted IS NULL;
--For file searches
CREATE INDEX IX_AccountFile_FileName ON dbo.AccountFile (FileName) WHERE Deleted IS NULL;

-- Table: Event

CREATE TABLE dbo.Event
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Name] VARCHAR(256) NOT NULL,
    [Description] VARCHAR(MAX) NULL,
    [Start] DATETIME2(7) NOT NULL,
    [End] DATETIME2(7) NOT NULL,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_Event_Id PRIMARY KEY (Id)
);
--For faster performance
CREATE INDEX IX_Event_Start_End ON dbo.Event (Start, [End]) WHERE Deleted IS NULL;
--For lookups
CREATE INDEX IX_Event_Name ON dbo.Event (Name) WHERE Deleted IS NULL;

-- Table: Place

CREATE TABLE dbo.Place
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Name] VARCHAR(256) NOT NULL,
    [Description] VARCHAR(MAX) NULL,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_Place_Id PRIMARY KEY (Id)
);
--For faster performance
CREATE INDEX IX_Place_Name ON dbo.Place (Name) WHERE Deleted IS NULL;

-- Table: Register

CREATE TABLE dbo.Register
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Action] SMALLINT NOT NULL,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
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

CREATE TABLE dbo.RegisterEvent
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [RegisterId] BIGINT NOT NULL,
    [EventId] BIGINT NOT NULL,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
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

CREATE TABLE dbo.RegisterPlace
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [RegisterId] BIGINT NOT NULL,
    [PlaceId] BIGINT NOT NULL,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
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

-- Table: ApplicationConfiguration

CREATE TABLE dbo.ApplicationConfiguration
(
    [Id] SMALLINT IDENTITY(1,1) NOT NULL,
    [ServerTimeZone] VARCHAR(64) NOT NULL DEFAULT 'America/Hermosillo',
    [ApplicationVersion] VARCHAR(64) NOT NULL,
    [Language] VARCHAR(16) NOT NULL DEFAULT 'en-US',
    [Environment] VARCHAR(64) NOT NULL,
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_ApplicationConfiguration_Id PRIMARY KEY (Id)
);
CREATE INDEX IX_ApplicationConfiguration_Created ON dbo.ApplicationConfiguration (Created DESC) WHERE Deleted IS NULL;
CREATE INDEX IX_ApplicationConfiguration_Version ON dbo.ApplicationConfiguration (ApplicationVersion, Environment) WHERE Deleted IS NULL;

-- Table: MigrationRecord

CREATE TABLE dbo.MigrationRecord
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [MigrationId] BIGINT NOT NULL,
    [Module] VARCHAR(128) NOT NULL,
    [Name] VARCHAR(256) NOT NULL,
    [Checksum] VARCHAR(128) NOT NULL,
    [AppliedUtc] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Created] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Modified] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    [Deleted] DATETIME2(7) NULL,
    CONSTRAINT PK_MigrationRecord_Id PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX UX_MigrationRecord_MigrationId ON dbo.MigrationRecord (MigrationId) WHERE Deleted IS NULL;

-- Always insert a new record when what is considered a migration ends
INSERT INTO dbo.MigrationRecord (MigrationId, [Module], [Name], [Checksum], AppliedUtc, Deleted)
VALUES (1, 'Core', 'migration-1', 'seeded-by-migration-1', SYSUTCDATETIME(), NULL);

