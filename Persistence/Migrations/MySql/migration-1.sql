SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS Migrations;
DROP TABLE IF EXISTS RegisterPlace;
DROP TABLE IF EXISTS RegisterEvent;
DROP TABLE IF EXISTS `Register`;
DROP TABLE IF EXISTS Place;
DROP TABLE IF EXISTS Event;
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
    INDEX IX_HealthCheckLog_Created (Created)
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
    PRIMARY KEY (Id),
    UNIQUE KEY UX_Account_Username (Username),
    UNIQUE KEY UX_Account_Email (Email),
    INDEX IX_Account_Deleted (Deleted)
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
    PRIMARY KEY (AccountId),
    CONSTRAINT FK_AccountSession_Account
        FOREIGN KEY (AccountId) REFERENCES Account(Id)
        ON DELETE CASCADE,
    INDEX IX_AccountSession_LockedUntilUtc (LockedUntilUtc),
    INDEX IX_AccountSession_FailedLoginAttempts (FailedLoginAttempts)
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
    FOREIGN KEY (AccountId) REFERENCES Account(Id) ON DELETE CASCADE,
    INDEX IX_Address_AccountId (AccountId),
    INDEX IX_Address_Location (Country, City, PostalCode),
    INDEX IX_Address_Created (Created)
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
    FOREIGN KEY (AccountId) REFERENCES Account(Id) ON DELETE CASCADE,
    INDEX IX_Contact_AccountId (AccountId),
    INDEX IX_Contact_Email (EmailAddress)
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
    INDEX IX_Event_Start_End (`Start`, `End`),
    INDEX IX_Event_Name (Name)
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
    INDEX IX_Place_Name (Name)
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
    FOREIGN KEY (AccountId) REFERENCES Account(Id) ON DELETE CASCADE,
    INDEX IX_Register_AccountId (AccountId),
    INDEX IX_Register_Created (Created),
    INDEX IX_Register_Action (Action, Created)
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
    FOREIGN KEY (RegisterId) REFERENCES `Register`(Id) ON DELETE CASCADE,
    FOREIGN KEY (EventId) REFERENCES Event(Id) ON DELETE CASCADE,
    UNIQUE KEY UX_RegisterEvent_Unique (RegisterId, EventId),
    INDEX IX_RegisterEvent_RegisterId (RegisterId),
    INDEX IX_RegisterEvent_EventId (EventId)
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
    FOREIGN KEY (RegisterId) REFERENCES `Register`(Id) ON DELETE CASCADE,
    FOREIGN KEY (PlaceId) REFERENCES Place(Id) ON DELETE CASCADE,
    UNIQUE KEY UX_RegisterPlace_Unique (RegisterId, PlaceId),
    INDEX IX_RegisterPlace_RegisterId (RegisterId),
    INDEX IX_RegisterPlace_PlaceId (PlaceId)
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