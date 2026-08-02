-- =========================
-- HealthCheckLog
-- =========================
CREATE TABLE HealthCheckLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL
);

CREATE INDEX IX_HealthCheckLog_Created
ON HealthCheckLog (Created)
WHERE Deleted IS NULL;

-- =========================
-- Account
-- =========================
CREATE TABLE Account (
    Id TEXT NOT NULL,
    Username TEXT NOT NULL,
    Email TEXT NOT NULL,
    Password TEXT NOT NULL,
    Roles TEXT NOT NULL,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL,
    PRIMARY KEY (Id)
);

CREATE UNIQUE INDEX UX_Account_Username
ON Account (Username)
WHERE Deleted IS NULL;

CREATE UNIQUE INDEX UX_Account_Email
ON Account (Email)
WHERE Deleted IS NULL;

CREATE INDEX IX_Account_Active
ON Account (Deleted, Username, Email, Created);

INSERT INTO Account
(Id, Username, Email, Password, Roles)
VALUES
('3D6A4E33-5B5F-40BA-9E31-F21815B9D09F',
 'TurboDecoder',
 'admin@local.com',
 'e8424ea5339b85a750ab5f4c63bde5d5c62975792a3bed6ccb37c045207c6771',
 'User');

-- =========================
-- AccountSession
-- =========================
CREATE TABLE AccountSession (
    AccountId TEXT NOT NULL,
    TokenVersion INTEGER NOT NULL DEFAULT 0,
    FailedLoginAttempts INTEGER NOT NULL DEFAULT 0,
    LockedUntilUtc TEXT NULL,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_AccountSession_Active
ON AccountSession (AccountId)
WHERE Deleted IS NULL;

CREATE INDEX IX_AccountSession_LockedUntilUtc
ON AccountSession (LockedUntilUtc)
WHERE LockedUntilUtc IS NOT NULL AND Deleted IS NULL;

CREATE INDEX IX_AccountSession_FailedLoginAttempts
ON AccountSession (FailedLoginAttempts)
WHERE FailedLoginAttempts > 0 AND Deleted IS NULL;

INSERT INTO AccountSession
(AccountId, TokenVersion, FailedLoginAttempts, LockedUntilUtc)
VALUES
('3D6A4E33-5B5F-40BA-9E31-F21815B9D09F', 0, 0, NULL);

-- =========================
-- AccountActivity
-- =========================
CREATE TABLE AccountActivity (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId TEXT NOT NULL,
    Module TEXT NOT NULL,
    Action TEXT NOT NULL,
    EntityType TEXT NULL,
    EntityId TEXT NULL,
    Message TEXT NOT NULL,
    MetadataJson TEXT NULL,
    IpAddress TEXT NULL,
    UserAgent TEXT NULL,
    CreatedUtc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_AccountActivity_AccountId_CreatedUtc
ON AccountActivity (AccountId, CreatedUtc)
WHERE Deleted IS NULL;

CREATE INDEX IX_AccountActivity_Module_Action
ON AccountActivity (Module, Action, CreatedUtc)
WHERE Deleted IS NULL;

CREATE INDEX IX_AccountActivity_Entity
ON AccountActivity (EntityType, EntityId)
WHERE Deleted IS NULL AND EntityType IS NOT NULL AND EntityId IS NOT NULL;

-- =========================
-- Address
-- =========================
CREATE TABLE Address (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId TEXT NOT NULL,
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
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_Address_AccountId
ON Address (AccountId)
WHERE Deleted IS NULL;

CREATE INDEX IX_Address_Location
ON Address (Country, City, PostalCode)
WHERE Deleted IS NULL;

CREATE INDEX IX_Address_Created
ON Address (Created)
WHERE Deleted IS NULL;

-- =========================
-- Contact
-- =========================
CREATE TABLE Contact (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT NULL,
    EmailAddress TEXT NULL,
    PhoneNumber TEXT NULL,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_Contact_AccountId
ON Contact (AccountId)
WHERE Deleted IS NULL;

CREATE INDEX IX_Contact_Email
ON Contact (EmailAddress)
WHERE Deleted IS NULL;

-- =========================
-- AccountFile
-- =========================
CREATE TABLE AccountFile (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId TEXT NOT NULL,
    FileName TEXT NOT NULL,
    Path TEXT NOT NULL,
    ContentType TEXT NULL,
    Size INTEGER NOT NULL DEFAULT 0,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_AccountFile_AccountId
ON AccountFile (AccountId)
WHERE Deleted IS NULL;

CREATE INDEX IX_AccountFile_Created
ON AccountFile (Created)
WHERE Deleted IS NULL;

CREATE INDEX IX_AccountFile_FileName
ON AccountFile (FileName)
WHERE Deleted IS NULL;

-- =========================
-- Event
-- =========================
CREATE TABLE Event (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT NULL,
    Start TEXT NOT NULL,
    End TEXT NOT NULL,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL
);

CREATE INDEX IX_Event_Start_End
ON Event (Start, End)
WHERE Deleted IS NULL;

CREATE INDEX IX_Event_Name
ON Event (Name)
WHERE Deleted IS NULL;

-- =========================
-- Place
-- =========================
CREATE TABLE Place (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT NULL,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL
);

CREATE INDEX IX_Place_Name
ON Place (Name)
WHERE Deleted IS NULL;

-- =========================
-- Register
-- =========================
CREATE TABLE "Register" (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountId TEXT NOT NULL,
    Action INTEGER NOT NULL,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL,
    FOREIGN KEY (AccountId) REFERENCES Account(Id)
);

CREATE INDEX IX_Register_AccountId
ON "Register" (AccountId)
WHERE Deleted IS NULL;

CREATE INDEX IX_Register_Created
ON "Register" (Created)
WHERE Deleted IS NULL;

CREATE INDEX IX_Register_Action
ON "Register" (Action, Created)
WHERE Deleted IS NULL;

-- =========================
-- RegisterEvent
-- =========================
CREATE TABLE RegisterEvent (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RegisterId INTEGER NOT NULL,
    EventId INTEGER NOT NULL,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL,
    FOREIGN KEY (RegisterId) REFERENCES "Register"(Id),
    FOREIGN KEY (EventId) REFERENCES Event(Id)
);

CREATE UNIQUE INDEX UX_RegisterEvent_Unique
ON RegisterEvent (RegisterId, EventId)
WHERE Deleted IS NULL;

CREATE INDEX IX_RegisterEvent_RegisterId
ON RegisterEvent (RegisterId)
WHERE Deleted IS NULL;

CREATE INDEX IX_RegisterEvent_EventId
ON RegisterEvent (EventId)
WHERE Deleted IS NULL;

-- =========================
-- RegisterPlace
-- =========================
CREATE TABLE RegisterPlace (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RegisterId INTEGER NOT NULL,
    PlaceId INTEGER NOT NULL,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL,
    FOREIGN KEY (RegisterId) REFERENCES "Register"(Id),
    FOREIGN KEY (PlaceId) REFERENCES Place(Id)
);

CREATE UNIQUE INDEX UX_RegisterPlace_Unique
ON RegisterPlace (RegisterId, PlaceId)
WHERE Deleted IS NULL;

CREATE INDEX IX_RegisterPlace_RegisterId
ON RegisterPlace (RegisterId)
WHERE Deleted IS NULL;

CREATE INDEX IX_RegisterPlace_PlaceId
ON RegisterPlace (PlaceId)
WHERE Deleted IS NULL;

-- =========================
-- MigrationRecord
-- =========================
CREATE TABLE MigrationRecord (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MigrationId INTEGER NOT NULL,
    Module TEXT NOT NULL,
    Name TEXT NOT NULL,
    Checksum TEXT NOT NULL,
    AppliedUtc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Created TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted TEXT NULL
);

CREATE UNIQUE INDEX UX_MigrationRecord_MigrationId
ON MigrationRecord (MigrationId)
WHERE Deleted IS NULL;

INSERT INTO MigrationRecord (MigrationId, Module, Name, Checksum, AppliedUtc, Deleted)
VALUES (1, 'Core', 'migration-1', 'seeded-by-migration-1', CURRENT_TIMESTAMP, NULL);

