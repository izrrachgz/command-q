SET time_zone = '+00:00';

-- =========================
-- HealthCheckLog
-- =========================
CREATE TABLE HealthCheckLog (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    INDEX IX_HealthCheckLog_Created (Deleted, Created)
) ENGINE=InnoDB;

-- =========================
-- Account
-- =========================
CREATE TABLE Account (
    Id CHAR(36) NOT NULL,
    Username VARCHAR(256) NOT NULL,
    Email VARCHAR(256) NOT NULL,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    ActiveRecord TINYINT GENERATED ALWAYS AS (CASE WHEN Deleted IS NULL THEN 1 ELSE NULL END) STORED,
    PRIMARY KEY (Id),
    UNIQUE KEY UX_Account_Username (Username, ActiveRecord),
    UNIQUE KEY UX_Account_Email (Email, ActiveRecord),
    INDEX IX_Account_Active (Deleted, Username, Email, Created)
) ENGINE=InnoDB;

-- =========================
-- AccountIdentity
-- =========================
CREATE TABLE AccountIdentity (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    AccountId CHAR(36) NOT NULL,
    Issuer VARCHAR(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
    Subject VARCHAR(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    ActiveRecord TINYINT GENERATED ALWAYS AS (CASE WHEN Deleted IS NULL THEN 1 ELSE NULL END) STORED,
    PRIMARY KEY (Id),
    CONSTRAINT FK_AccountIdentity_Account
        FOREIGN KEY (AccountId) REFERENCES Account(Id),
    INDEX IX_AccountIdentity_AccountId (Deleted, AccountId),
    UNIQUE KEY UX_AccountIdentity_Issuer_Subject (Issuer, Subject, ActiveRecord)
) ENGINE=InnoDB;

-- =========================
-- AccountActivity
-- =========================
CREATE TABLE AccountActivity (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    AccountId CHAR(36) NOT NULL,
    `Module` VARCHAR(128) NOT NULL,
    `Action` VARCHAR(128) NOT NULL,
    EntityType VARCHAR(128) NULL,
    EntityId VARCHAR(128) NULL,
    Message VARCHAR(1024) NOT NULL,
    MetadataJson TEXT NULL,
    IpAddress VARCHAR(64) NULL,
    UserAgent VARCHAR(512) NULL,
    CreatedUtc DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    INDEX IX_AccountActivity_AccountId_CreatedUtc (Deleted, AccountId, CreatedUtc),
    INDEX IX_AccountActivity_Module_Action (Deleted, `Module`, `Action`, CreatedUtc),
    INDEX IX_AccountActivity_Entity (Deleted, EntityType, EntityId)
) ENGINE=InnoDB;

-- =========================
-- AccountPreference
-- =========================
CREATE TABLE AccountPreference (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    AccountId CHAR(36) NOT NULL,
    Culture VARCHAR(16) NOT NULL DEFAULT 'en-US',
    Theme SMALLINT NOT NULL DEFAULT 0,
    CompactNavigation BOOLEAN NOT NULL DEFAULT FALSE,
    EnhancedFocus BOOLEAN NOT NULL DEFAULT FALSE,
    ReduceMotion BOOLEAN NOT NULL DEFAULT FALSE,
    ShowDenseTables BOOLEAN NOT NULL DEFAULT FALSE,
    TimeZone VARCHAR(64) NOT NULL DEFAULT 'America/Hermosillo',
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    ActiveRecord TINYINT GENERATED ALWAYS AS (CASE WHEN Deleted IS NULL THEN 1 ELSE NULL END) STORED,
    PRIMARY KEY (Id),
    CONSTRAINT FK_AccountPreference_Account FOREIGN KEY (AccountId) REFERENCES Account(Id),
    CONSTRAINT CK_AccountPreference_Theme CHECK (Theme IN (0, 1)),
    UNIQUE KEY UX_AccountPreference_AccountId (AccountId, ActiveRecord),
    INDEX IX_AccountPreference_TimeZone (Deleted, TimeZone)
) ENGINE=InnoDB;

-- =========================
-- Address
-- =========================
CREATE TABLE Address (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    AccountId CHAR(36) NOT NULL,
    Name VARCHAR(256) NOT NULL,
    Description TEXT NULL,
    Line1 VARCHAR(256) NOT NULL,
    Line2 VARCHAR(256) NULL,
    ExternalNumber VARCHAR(16) NULL,
    InternalNumber VARCHAR(16) NULL,
    `References` VARCHAR(512) NULL,
    City VARCHAR(64) NOT NULL,
    PostalCode VARCHAR(16) NOT NULL,
    Country VARCHAR(128) NOT NULL,
    Phone VARCHAR(24) NULL,
    EmailAddress VARCHAR(256) NULL,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    INDEX IX_Address_AccountId (Deleted, AccountId),
    INDEX IX_Address_Location (Deleted, Country, City, PostalCode),
    INDEX IX_Address_Created (Deleted, Created)
) ENGINE=InnoDB;

-- =========================
-- Contact
-- =========================
CREATE TABLE Contact (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    AccountId CHAR(36) NOT NULL,
    Name VARCHAR(256) NOT NULL,
    Description TEXT NULL,
    EmailAddress VARCHAR(256) NULL,
    PhoneNumber VARCHAR(256) NULL,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    INDEX IX_Contact_AccountId (Deleted, AccountId),
    INDEX IX_Contact_Email (Deleted, EmailAddress)
) ENGINE=InnoDB;

-- =========================
-- AccountFile
-- =========================
CREATE TABLE AccountFile (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    AccountId CHAR(36) NOT NULL,
    FileName VARCHAR(256) NOT NULL,
    Path VARCHAR(1024) NOT NULL,
    ContentType VARCHAR(256) NULL,
    Size BIGINT NOT NULL DEFAULT 0,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    INDEX IX_AccountFile_AccountId (Deleted, AccountId),
    INDEX IX_AccountFile_Created (Deleted, Created),
    INDEX IX_AccountFile_FileName (Deleted, FileName)
) ENGINE=InnoDB;

-- =========================
-- Event
-- =========================
CREATE TABLE Event (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(256) NOT NULL,
    Description TEXT NULL,
    `Start` DATETIME(6) NOT NULL,
    `End` DATETIME(6) NOT NULL,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    INDEX IX_Event_Start_End (Deleted, `Start`, `End`),
    INDEX IX_Event_Name (Deleted, Name)
) ENGINE=InnoDB;

-- =========================
-- Place
-- =========================
CREATE TABLE Place (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(256) NOT NULL,
    Description TEXT NULL,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    INDEX IX_Place_Name (Deleted, Name)
) ENGINE=InnoDB;

-- =========================
-- Register
-- =========================
CREATE TABLE `Register` (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    AccountId CHAR(36) NOT NULL,
    Action SMALLINT NOT NULL,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    FOREIGN KEY (AccountId) REFERENCES Account(Id),
    INDEX IX_Register_AccountId (Deleted, AccountId),
    INDEX IX_Register_Created (Deleted, Created),
    INDEX IX_Register_Action (Deleted, Action, Created)
) ENGINE=InnoDB;

-- =========================
-- RegisterEvent
-- =========================
CREATE TABLE RegisterEvent (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    RegisterId BIGINT NOT NULL,
    EventId BIGINT NOT NULL,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    ActiveRecord TINYINT GENERATED ALWAYS AS (CASE WHEN Deleted IS NULL THEN 1 ELSE NULL END) STORED,
    FOREIGN KEY (RegisterId) REFERENCES `Register`(Id),
    FOREIGN KEY (EventId) REFERENCES Event(Id),
    UNIQUE KEY UX_RegisterEvent_Unique (RegisterId, EventId, ActiveRecord),
    INDEX IX_RegisterEvent_RegisterId (Deleted, RegisterId),
    INDEX IX_RegisterEvent_EventId (Deleted, EventId)
) ENGINE=InnoDB;

-- =========================
-- RegisterPlace
-- =========================
CREATE TABLE RegisterPlace (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    RegisterId BIGINT NOT NULL,
    PlaceId BIGINT NOT NULL,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    ActiveRecord TINYINT GENERATED ALWAYS AS (CASE WHEN Deleted IS NULL THEN 1 ELSE NULL END) STORED,
    FOREIGN KEY (RegisterId) REFERENCES `Register`(Id),
    FOREIGN KEY (PlaceId) REFERENCES Place(Id),
    UNIQUE KEY UX_RegisterPlace_Unique (RegisterId, PlaceId, ActiveRecord),
    INDEX IX_RegisterPlace_RegisterId (Deleted, RegisterId),
    INDEX IX_RegisterPlace_PlaceId (Deleted, PlaceId)
) ENGINE=InnoDB;

-- =========================
-- ApplicationConfiguration
-- =========================
CREATE TABLE ApplicationConfiguration (
    Id SMALLINT NOT NULL AUTO_INCREMENT,
    ServerTimeZone VARCHAR(64) NOT NULL DEFAULT 'America/Hermosillo',
    ApplicationVersion VARCHAR(64) NOT NULL,
    Language VARCHAR(16) NOT NULL DEFAULT 'en-US',
    Environment VARCHAR(64) NOT NULL,
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    PRIMARY KEY (Id),
    INDEX IX_ApplicationConfiguration_Created (Deleted, Created),
    INDEX IX_ApplicationConfiguration_Version (Deleted, ApplicationVersion, Environment)
) ENGINE=InnoDB;

-- =========================
-- MigrationRecord
-- =========================
CREATE TABLE MigrationRecord (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    MigrationId BIGINT NOT NULL,
    `Module` VARCHAR(128) NOT NULL,
    `Name` VARCHAR(256) NOT NULL,
    Checksum VARCHAR(128) NOT NULL,
    AppliedUtc DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Created DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Modified DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    Deleted DATETIME(6) NULL,
    ActiveRecord TINYINT GENERATED ALWAYS AS (CASE WHEN Deleted IS NULL THEN 1 ELSE NULL END) STORED,
    PRIMARY KEY (Id),
    UNIQUE KEY UX_MigrationRecord_MigrationId (MigrationId, ActiveRecord)
) ENGINE=InnoDB;

INSERT INTO MigrationRecord (MigrationId, `Module`, `Name`, Checksum, AppliedUtc, Deleted)
VALUES (1, 'Core', 'migration-1', 'seeded-by-migration-1', UTC_TIMESTAMP(6), NULL);

