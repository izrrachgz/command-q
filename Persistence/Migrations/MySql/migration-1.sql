SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS Migrations;
DROP TABLE IF EXISTS RegisterPlace;
DROP TABLE IF EXISTS RegisterEvent;
DROP TABLE IF EXISTS `Register`;
DROP TABLE IF EXISTS Place;
DROP TABLE IF EXISTS Event;
DROP TABLE IF EXISTS AccountFile;
DROP TABLE IF EXISTS Contact;
DROP TABLE IF EXISTS Address;
DROP TABLE IF EXISTS AccountSession;
DROP TABLE IF EXISTS Account;
DROP TABLE IF EXISTS HealthCheckLog;

SET FOREIGN_KEY_CHECKS = 1;

-- =========================
-- HealthCheckLog
-- =========================
CREATE TABLE HealthCheckLog (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
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
    Password VARCHAR(256) NOT NULL,
    Roles VARCHAR(512) NOT NULL,
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
    ActiveRecord TINYINT GENERATED ALWAYS AS (CASE WHEN Deleted IS NULL THEN 1 ELSE NULL END) STORED,
    PRIMARY KEY (Id),
    UNIQUE KEY UX_Account_Username (Username, ActiveRecord),
    UNIQUE KEY UX_Account_Email (Email, ActiveRecord),
    INDEX IX_Account_Active (Deleted, Username, Email, Created)
) ENGINE=InnoDB;

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
    AccountId CHAR(36) NOT NULL,
    TokenVersion INT NOT NULL DEFAULT 0,
    FailedLoginAttempts SMALLINT NOT NULL DEFAULT 0,
    LockedUntilUtc DATETIME NULL,
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
    CONSTRAINT FK_AccountSession_Account
        FOREIGN KEY (AccountId) REFERENCES Account(Id),
    INDEX IX_AccountSession_Active (Deleted, AccountId),
    INDEX IX_AccountSession_LockedUntilUtc (Deleted, LockedUntilUtc),
    INDEX IX_AccountSession_FailedLoginAttempts (Deleted, FailedLoginAttempts)
) ENGINE=InnoDB;

INSERT INTO AccountSession
(AccountId, TokenVersion, FailedLoginAttempts, LockedUntilUtc)
VALUES
('3D6A4E33-5B5F-40BA-9E31-F21815B9D09F', 0, 0, NULL);

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
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
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
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
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
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
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
    `Start` DATETIME NOT NULL,
    `End` DATETIME NOT NULL,
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
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
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
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
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
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
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
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
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
    PRIMARY KEY (Id),
    ActiveRecord TINYINT GENERATED ALWAYS AS (CASE WHEN Deleted IS NULL THEN 1 ELSE NULL END) STORED,
    FOREIGN KEY (RegisterId) REFERENCES `Register`(Id),
    FOREIGN KEY (PlaceId) REFERENCES Place(Id),
    UNIQUE KEY UX_RegisterPlace_Unique (RegisterId, PlaceId, ActiveRecord),
    INDEX IX_RegisterPlace_RegisterId (Deleted, RegisterId),
    INDEX IX_RegisterPlace_PlaceId (Deleted, PlaceId)
) ENGINE=InnoDB;

-- =========================
-- Migrations
-- =========================
CREATE TABLE Migrations (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Deleted DATETIME NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB;

INSERT INTO Migrations (Deleted) VALUES (NULL);
