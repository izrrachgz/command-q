-- =========================
-- HealthCheckLog
-- =========================
CREATE TABLE HealthCheckLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL
);

CREATE INDEX IX_HealthCheckLog_CreatedUtc
ON HealthCheckLog (CreatedUtc)
WHERE DeletedUtc IS NULL;

-- =========================
-- Account
-- =========================
CREATE TABLE Account (
    Id BLOB NOT NULL,
    Username TEXT NOT NULL,
    Email TEXT NOT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL,
    PRIMARY KEY (Id)
);

CREATE UNIQUE INDEX UX_Account_Username
ON Account (Username)
WHERE DeletedUtc IS NULL;

CREATE UNIQUE INDEX UX_Account_Email
ON Account (Email)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_Account_Active
ON Account (DeletedUtc, Username, Email, CreatedUtc);

-- =========================
-- AccountIdentity
-- =========================
CREATE TABLE AccountIdentity (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId BLOB NOT NULL,
    Issuer TEXT NOT NULL,
    Subject TEXT NOT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_AccountIdentity_AccountId
ON AccountIdentity (AccountId)
WHERE DeletedUtc IS NULL;

CREATE UNIQUE INDEX UX_AccountIdentity_Issuer_Subject
ON AccountIdentity (Issuer, Subject)
WHERE DeletedUtc IS NULL;

-- =========================
-- AccountActivity
-- =========================
CREATE TABLE AccountActivity (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId BLOB NOT NULL,
    Module TEXT NOT NULL,
    Action TEXT NOT NULL,
    EntityType TEXT NULL,
    EntityId TEXT NULL,
    Message TEXT NOT NULL,
    MetadataJson TEXT NULL,
    IpAddress TEXT NULL,
    UserAgent TEXT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_AccountActivity_AccountId_CreatedUtc
ON AccountActivity (AccountId, CreatedUtc)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_AccountActivity_Module_Action
ON AccountActivity (Module, Action, CreatedUtc)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_AccountActivity_Entity
ON AccountActivity (EntityType, EntityId)
WHERE DeletedUtc IS NULL AND EntityType IS NOT NULL AND EntityId IS NOT NULL;

-- =========================
-- AccountPreference
-- =========================
CREATE TABLE AccountPreference (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId BLOB NOT NULL,
    Culture TEXT NOT NULL DEFAULT 'en-US' CHECK (length(Culture) <= 16),
    Theme INTEGER NOT NULL DEFAULT 0 CHECK (Theme IN (0, 1)),
    CompactNavigation INTEGER NOT NULL DEFAULT 0 CHECK (CompactNavigation IN (0, 1)),
    EnhancedFocus INTEGER NOT NULL DEFAULT 0 CHECK (EnhancedFocus IN (0, 1)),
    ReduceMotion INTEGER NOT NULL DEFAULT 0 CHECK (ReduceMotion IN (0, 1)),
    ShowDenseTables INTEGER NOT NULL DEFAULT 0 CHECK (ShowDenseTables IN (0, 1)),
    TimeZone TEXT NOT NULL DEFAULT 'America/Hermosillo' CHECK (length(TimeZone) <= 64),
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);
CREATE UNIQUE INDEX UX_AccountPreference_AccountId ON AccountPreference (AccountId) WHERE DeletedUtc IS NULL;

-- =========================
-- Address
-- =========================
CREATE TABLE Address (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId BLOB NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT NULL,
    Line1 TEXT NOT NULL,
    Line2 TEXT NULL,
    ExternalNumber TEXT NULL,
    InternalNumber TEXT NULL,
    "References" TEXT NULL,
    City TEXT NOT NULL,
    PostalCode TEXT NOT NULL,
    Country TEXT NOT NULL,
    Phone TEXT NULL,
    EmailAddress TEXT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_Address_AccountId
ON Address (AccountId)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_Address_Location
ON Address (Country, City, PostalCode)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_Address_CreatedUtc
ON Address (CreatedUtc)
WHERE DeletedUtc IS NULL;

-- =========================
-- Contact
-- =========================
CREATE TABLE Contact (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId BLOB NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT NULL,
    EmailAddress TEXT NULL,
    PhoneNumber TEXT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_Contact_AccountId
ON Contact (AccountId)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_Contact_Email
ON Contact (EmailAddress)
WHERE DeletedUtc IS NULL;

-- =========================
-- AccountFile
-- =========================
CREATE TABLE AccountFile (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId BLOB NOT NULL,
    FileName TEXT NOT NULL,
    Path TEXT NOT NULL,
    ContentType TEXT NULL,
    Size INTEGER NOT NULL DEFAULT 0,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_AccountFile_AccountId
ON AccountFile (AccountId)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_AccountFile_CreatedUtc
ON AccountFile (CreatedUtc)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_AccountFile_FileName
ON AccountFile (FileName)
WHERE DeletedUtc IS NULL;

-- =========================
-- Event
-- =========================
CREATE TABLE Event (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT NULL,
    StartUtc TEXT NOT NULL,
    EndUtc TEXT NOT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL
);

CREATE INDEX IX_Event_StartUtc_EndUtc
ON Event (StartUtc, EndUtc)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_Event_Name
ON Event (Name)
WHERE DeletedUtc IS NULL;

-- =========================
-- Place
-- =========================
CREATE TABLE Place (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL
);

CREATE INDEX IX_Place_Name
ON Place (Name)
WHERE DeletedUtc IS NULL;

-- =========================
-- Register
-- =========================
CREATE TABLE "Register" (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId BLOB NOT NULL,
    Action INTEGER NOT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_Register_AccountId
ON "Register" (AccountId)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_Register_CreatedUtc
ON "Register" (CreatedUtc)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_Register_Action
ON "Register" (Action, CreatedUtc)
WHERE DeletedUtc IS NULL;

-- =========================
-- RegisterEvent
-- =========================
CREATE TABLE RegisterEvent (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RegisterId INTEGER NOT NULL,
    EventId INTEGER NOT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL,
    FOREIGN KEY (RegisterId) REFERENCES "Register"(Id),
    FOREIGN KEY (EventId) REFERENCES Event(Id)
);

CREATE UNIQUE INDEX UX_RegisterEvent_Unique
ON RegisterEvent (RegisterId, EventId)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_RegisterEvent_RegisterId
ON RegisterEvent (RegisterId)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_RegisterEvent_EventId
ON RegisterEvent (EventId)
WHERE DeletedUtc IS NULL;

-- =========================
-- RegisterPlace
-- =========================
CREATE TABLE RegisterPlace (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RegisterId INTEGER NOT NULL,
    PlaceId INTEGER NOT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL,
    FOREIGN KEY (RegisterId) REFERENCES "Register"(Id),
    FOREIGN KEY (PlaceId) REFERENCES Place(Id)
);

CREATE UNIQUE INDEX UX_RegisterPlace_Unique
ON RegisterPlace (RegisterId, PlaceId)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_RegisterPlace_RegisterId
ON RegisterPlace (RegisterId)
WHERE DeletedUtc IS NULL;

CREATE INDEX IX_RegisterPlace_PlaceId
ON RegisterPlace (PlaceId)
WHERE DeletedUtc IS NULL;

-- =========================
-- ApplicationConfiguration
-- =========================
CREATE TABLE ApplicationConfiguration (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ServerTimeZone TEXT NOT NULL DEFAULT 'America/Hermosillo' CHECK (length(ServerTimeZone) <= 64),
    ApplicationVersion TEXT NOT NULL CHECK (length(ApplicationVersion) <= 64),
    Language TEXT NOT NULL DEFAULT 'en-US' CHECK (length(Language) <= 16),
    Environment TEXT NOT NULL CHECK (length(Environment) <= 64),
    InstanceName TEXT NOT NULL CHECK (length(InstanceName) <= 128),
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL
);
CREATE INDEX IX_ApplicationConfiguration_CreatedUtc ON ApplicationConfiguration (CreatedUtc DESC) WHERE DeletedUtc IS NULL;

-- =========================
-- MigrationRecord
-- =========================
CREATE TABLE MigrationRecord (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MigrationId INTEGER NOT NULL,
    Module TEXT NOT NULL,
    Name TEXT NOT NULL,
    Checksum TEXT NOT NULL,
    AppliedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    CreatedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    ModifiedUtc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%d %H:%M:%f000Z', 'now')),
    DeletedUtc TEXT NULL
);

CREATE UNIQUE INDEX UX_MigrationRecord_MigrationId
ON MigrationRecord (MigrationId)
WHERE DeletedUtc IS NULL;

INSERT INTO MigrationRecord (MigrationId, Module, Name, Checksum, AppliedUtc, DeletedUtc)
VALUES (1, 'Core', 'migration-1', 'seeded-by-migration-1', strftime('%Y-%m-%d %H:%M:%f000Z', 'now'), NULL);

