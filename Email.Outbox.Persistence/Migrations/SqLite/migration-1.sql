CREATE TABLE EmailOutboxMessage (
    Id TEXT NOT NULL PRIMARY KEY,
    Payload TEXT NOT NULL,
    IdempotencyKey TEXT NULL,
    AccountReference TEXT NULL,
    CorrelationId TEXT NULL,
    Status INTEGER NOT NULL,
    AttemptCount INTEGER NOT NULL DEFAULT 0,
    CreatedUtc DATETIME NOT NULL,
    ModifiedUtc DATETIME NOT NULL,
    NextAttemptUtc DATETIME NOT NULL,
    LockedUntilUtc DATETIME NULL,
    LockId TEXT NULL,
    SentUtc DATETIME NULL,
    DeadLetterUtc DATETIME NULL,
    LastFailureKind INTEGER NULL,
    LastError TEXT NULL,
    MessageId TEXT NULL,
    SmtpResponse TEXT NULL
);

CREATE UNIQUE INDEX UX_EmailOutboxMessage_IdempotencyKey
ON EmailOutboxMessage (IdempotencyKey)
WHERE IdempotencyKey IS NOT NULL;

CREATE INDEX IX_EmailOutboxMessage_Claim
ON EmailOutboxMessage (Status, NextAttemptUtc, CreatedUtc);

CREATE INDEX IX_EmailOutboxMessage_Lease
ON EmailOutboxMessage (Status, LockedUntilUtc);

CREATE TABLE EmailOutboxMigrationRecord (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    MigrationId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Checksum TEXT NOT NULL,
    AppliedUtc DATETIME NOT NULL,
    DeletedUtc DATETIME NULL
);

CREATE UNIQUE INDEX UX_EmailOutboxMigrationRecord_MigrationId
ON EmailOutboxMigrationRecord (MigrationId)
WHERE DeletedUtc IS NULL;

INSERT INTO EmailOutboxMigrationRecord
    (MigrationId, Name, Checksum, AppliedUtc, DeletedUtc)
VALUES
    (1, 'migration-1.sql', 'seeded-by-migration-1', CURRENT_TIMESTAMP, NULL);
