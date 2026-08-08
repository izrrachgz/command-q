CREATE TABLE "EmailOutboxMessage"
(
    "Id" CHAR(36) NOT NULL PRIMARY KEY,
    "Payload" TEXT NOT NULL,
    "IdempotencyKey" VARCHAR(256) NULL,
    "AccountReference" VARCHAR(128) NULL,
    "CorrelationId" VARCHAR(128) NULL,
    "Status" INTEGER NOT NULL,
    "AttemptCount" INTEGER NOT NULL DEFAULT 0,
    "CreatedUtc" TIMESTAMPTZ NOT NULL,
    "ModifiedUtc" TIMESTAMPTZ NOT NULL,
    "NextAttemptUtc" TIMESTAMPTZ NOT NULL,
    "LockedUntilUtc" TIMESTAMPTZ NULL,
    "LockId" VARCHAR(64) NULL,
    "SentUtc" TIMESTAMPTZ NULL,
    "DeadLetterUtc" TIMESTAMPTZ NULL,
    "LastFailureKind" INTEGER NULL,
    "LastError" VARCHAR(2000) NULL,
    "MessageId" VARCHAR(512) NULL,
    "SmtpResponse" VARCHAR(2000) NULL
);

CREATE UNIQUE INDEX "UX_EmailOutboxMessage_IdempotencyKey"
ON "EmailOutboxMessage" ("IdempotencyKey")
WHERE "IdempotencyKey" IS NOT NULL;

CREATE INDEX "IX_EmailOutboxMessage_Claim"
ON "EmailOutboxMessage" ("Status", "NextAttemptUtc", "CreatedUtc");

CREATE INDEX "IX_EmailOutboxMessage_Lease"
ON "EmailOutboxMessage" ("Status", "LockedUntilUtc");

CREATE TABLE "EmailOutboxMigrationRecord"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "MigrationId" BIGINT NOT NULL,
    "Name" VARCHAR(256) NOT NULL,
    "Checksum" VARCHAR(128) NOT NULL,
    "AppliedUtc" TIMESTAMPTZ NOT NULL,
    "DeletedUtc" TIMESTAMPTZ NULL
);

CREATE UNIQUE INDEX "UX_EmailOutboxMigrationRecord_MigrationId"
ON "EmailOutboxMigrationRecord" ("MigrationId")
WHERE "DeletedUtc" IS NULL;

INSERT INTO "EmailOutboxMigrationRecord"
    ("MigrationId", "Name", "Checksum", "AppliedUtc", "DeletedUtc")
VALUES
    (1, 'migration-1.sql', 'seeded-by-migration-1', CURRENT_TIMESTAMP, NULL);
