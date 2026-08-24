SET time_zone = '+00:00';

CREATE TABLE EmailOutboxMessage
(
    Id CHAR(36) NOT NULL,
    Payload LONGTEXT NOT NULL,
    ProfileName VARCHAR(64) NOT NULL DEFAULT 'default',
    IdempotencyKey VARCHAR(256) NULL,
    AccountReference VARCHAR(128) NULL,
    CorrelationId VARCHAR(128) NULL,
    TraceParent VARCHAR(55) NULL,
    TraceState VARCHAR(512) NULL,
    Status INT NOT NULL,
    AttemptCount INT NOT NULL DEFAULT 0,
    CreatedUtc DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    ModifiedUtc DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    NextAttemptUtc DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    LockedUntilUtc DATETIME(6) NULL,
    LockId VARCHAR(64) NULL,
    SentUtc DATETIME(6) NULL,
    DeadLetterUtc DATETIME(6) NULL,
    LastFailureKind INT NULL,
    LastError VARCHAR(2000) NULL,
    MessageId VARCHAR(512) NULL,
    SmtpResponse VARCHAR(2000) NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY UX_EmailOutboxMessage_IdempotencyKey (IdempotencyKey),
    INDEX IX_EmailOutboxMessage_Claim (Status, NextAttemptUtc, CreatedUtc),
    INDEX IX_EmailOutboxMessage_Lease (Status, LockedUntilUtc)
);

CREATE TABLE EmailOutboxMigrationRecord
(
    Id BIGINT NOT NULL AUTO_INCREMENT,
    MigrationId BIGINT NOT NULL,
    Name VARCHAR(256) NOT NULL,
    Checksum VARCHAR(128) NOT NULL,
    AppliedUtc DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    DeletedUtc DATETIME(6) NULL,
    ActiveRecord TINYINT GENERATED ALWAYS AS (IF(DeletedUtc IS NULL, 1, NULL)) STORED,
    PRIMARY KEY (Id),
    UNIQUE KEY UX_EmailOutboxMigrationRecord_MigrationId (MigrationId, ActiveRecord)
);

INSERT INTO EmailOutboxMigrationRecord
    (MigrationId, Name, Checksum, AppliedUtc, DeletedUtc)
VALUES
    (1, 'migration-1.sql', 'seeded-by-migration-1', UTC_TIMESTAMP(6), NULL);
