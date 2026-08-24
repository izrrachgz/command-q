CREATE TABLE dbo.EmailOutboxMessage
(
    Id CHAR(36) NOT NULL,
    Payload NVARCHAR(MAX) NOT NULL,
    ProfileName NVARCHAR(64) NOT NULL CONSTRAINT DF_EmailOutboxMessage_ProfileName DEFAULT 'default',
    IdempotencyKey NVARCHAR(256) NULL,
    AccountReference NVARCHAR(128) NULL,
    CorrelationId NVARCHAR(128) NULL,
    TraceParent VARCHAR(55) NULL,
    TraceState NVARCHAR(512) NULL,
    Status INT NOT NULL,
    AttemptCount INT NOT NULL CONSTRAINT DF_EmailOutboxMessage_AttemptCount DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_EmailOutboxMessage_CreatedUtc DEFAULT SYSUTCDATETIME(),
    ModifiedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_EmailOutboxMessage_ModifiedUtc DEFAULT SYSUTCDATETIME(),
    NextAttemptUtc DATETIME2(7) NOT NULL CONSTRAINT DF_EmailOutboxMessage_NextAttemptUtc DEFAULT SYSUTCDATETIME(),
    LockedUntilUtc DATETIME2(7) NULL,
    LockId VARCHAR(64) NULL,
    SentUtc DATETIME2(7) NULL,
    DeadLetterUtc DATETIME2(7) NULL,
    LastFailureKind INT NULL,
    LastError NVARCHAR(2000) NULL,
    MessageId NVARCHAR(512) NULL,
    SmtpResponse NVARCHAR(2000) NULL,
    CONSTRAINT PK_EmailOutboxMessage PRIMARY KEY (Id)
);

CREATE UNIQUE INDEX UX_EmailOutboxMessage_IdempotencyKey
ON dbo.EmailOutboxMessage (IdempotencyKey)
WHERE IdempotencyKey IS NOT NULL;

CREATE INDEX IX_EmailOutboxMessage_Claim
ON dbo.EmailOutboxMessage (Status, NextAttemptUtc, CreatedUtc);

CREATE INDEX IX_EmailOutboxMessage_Lease
ON dbo.EmailOutboxMessage (Status, LockedUntilUtc);

CREATE TABLE dbo.EmailOutboxMigrationRecord
(
    Id BIGINT IDENTITY(1,1) NOT NULL,
    MigrationId BIGINT NOT NULL,
    Name NVARCHAR(256) NOT NULL,
    Checksum VARCHAR(128) NOT NULL,
    AppliedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_EmailOutboxMigrationRecord_AppliedUtc DEFAULT SYSUTCDATETIME(),
    DeletedUtc DATETIME2(7) NULL,
    CONSTRAINT PK_EmailOutboxMigrationRecord PRIMARY KEY (Id)
);

CREATE UNIQUE INDEX UX_EmailOutboxMigrationRecord_MigrationId
ON dbo.EmailOutboxMigrationRecord (MigrationId)
WHERE DeletedUtc IS NULL;

INSERT INTO dbo.EmailOutboxMigrationRecord
    (MigrationId, Name, Checksum, AppliedUtc, DeletedUtc)
VALUES
    (1, 'migration-1.sql', 'seeded-by-migration-1', SYSUTCDATETIME(), NULL);
