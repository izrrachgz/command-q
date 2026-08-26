-- Enable UUID generation (if not already enabled)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =========================
-- Table: HealthCheckLog
-- =========================

CREATE TABLE "HealthCheckLog"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL
);

CREATE INDEX "IX_HealthCheckLog_CreatedUtc"
ON "HealthCheckLog" ("CreatedUtc")
WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: Account
-- =========================

CREATE TABLE "Account"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "Username" VARCHAR(256) NOT NULL,
    "Email" VARCHAR(256) NOT NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL,
    CONSTRAINT "PK_Account_Id" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "UX_Account_Username"
ON "Account" ("Username")
WHERE "DeletedUtc" IS NULL;

CREATE UNIQUE INDEX "UX_Account_Email"
ON "Account" ("Email")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_Account_Active"
ON "Account" ("DeletedUtc", "Username", "Email", "CreatedUtc");

-- =========================
-- Table: AccountIdentity
-- =========================

CREATE TABLE "AccountIdentity"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AccountId" UUID NOT NULL,
    "Issuer" VARCHAR(500) NOT NULL,
    "Subject" VARCHAR(256) NOT NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL,
    CONSTRAINT "FK_AccountIdentity_Account"
        FOREIGN KEY ("AccountId") REFERENCES "Account"("Id")
);

CREATE INDEX "IX_AccountIdentity_AccountId"
ON "AccountIdentity" ("AccountId")
WHERE "DeletedUtc" IS NULL;

CREATE UNIQUE INDEX "UX_AccountIdentity_Issuer_Subject"
ON "AccountIdentity" ("Issuer", "Subject")
WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: AccountActivity
-- =========================

CREATE TABLE "AccountActivity"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AccountId" UUID NOT NULL,
    "Module" VARCHAR(128) NOT NULL,
    "Action" VARCHAR(128) NOT NULL,
    "EntityType" VARCHAR(128) NULL,
    "EntityId" VARCHAR(128) NULL,
    "Message" VARCHAR(1024) NOT NULL,
    "MetadataJson" TEXT NULL,
    "IpAddress" VARCHAR(64) NULL,
    "UserAgent" VARCHAR(512) NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL,
    CONSTRAINT "FK_AccountActivity_Account"
        FOREIGN KEY ("AccountId") REFERENCES "Account"("Id")
);

CREATE INDEX "IX_AccountActivity_AccountId_CreatedUtc"
ON "AccountActivity" ("AccountId","CreatedUtc" DESC)
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_AccountActivity_Module_Action"
ON "AccountActivity" ("Module","Action","CreatedUtc" DESC)
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_AccountActivity_Entity"
ON "AccountActivity" ("EntityType","EntityId")
WHERE "DeletedUtc" IS NULL AND "EntityType" IS NOT NULL AND "EntityId" IS NOT NULL;

-- =========================
-- Table: AccountPreference
-- =========================

CREATE TABLE "AccountPreference"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AccountId" UUID NOT NULL,
    "Culture" VARCHAR(16) NOT NULL DEFAULT 'en-US',
    "Theme" SMALLINT NOT NULL DEFAULT 0,
    "CompactNavigation" BOOLEAN NOT NULL DEFAULT FALSE,
    "EnhancedFocus" BOOLEAN NOT NULL DEFAULT FALSE,
    "ReduceMotion" BOOLEAN NOT NULL DEFAULT FALSE,
    "ShowDenseTables" BOOLEAN NOT NULL DEFAULT FALSE,
    "TimeZone" VARCHAR(64) NOT NULL DEFAULT 'America/Hermosillo',
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL,
    CONSTRAINT "FK_AccountPreference_Account" FOREIGN KEY ("AccountId") REFERENCES "Account"("Id"),
    CONSTRAINT "CK_AccountPreference_Theme" CHECK ("Theme" IN (0, 1))
);
CREATE UNIQUE INDEX "UX_AccountPreference_AccountId" ON "AccountPreference" ("AccountId") WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: Address
-- =========================

CREATE TABLE "Address"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AccountId" UUID NOT NULL,
    "Name" VARCHAR(256) NOT NULL,
    "Description" TEXT NULL,
    "Line1" VARCHAR(256) NOT NULL,
    "Line2" VARCHAR(256) NULL,
    "ExternalNumber" VARCHAR(16) NULL,
    "InternalNumber" VARCHAR(16) NULL,
    "References" VARCHAR(512) NULL,
    "City" VARCHAR(64) NOT NULL,
    "PostalCode" VARCHAR(16) NOT NULL,
    "Country" VARCHAR(128) NOT NULL,
    "Phone" VARCHAR(24) NULL,
    "EmailAddress" VARCHAR(256) NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL,
    CONSTRAINT "FK_Address_Account"
        FOREIGN KEY ("AccountId") REFERENCES "Account"("Id")
);

CREATE INDEX "IX_Address_AccountId"
ON "Address" ("AccountId")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_Address_Location"
ON "Address" ("Country","City","PostalCode")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_Address_CreatedUtc"
ON "Address" ("CreatedUtc")
WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: Contact
-- =========================

CREATE TABLE "Contact"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AccountId" UUID NOT NULL,
    "Name" VARCHAR(256) NOT NULL,
    "Description" TEXT NULL,
    "EmailAddress" VARCHAR(256) NULL,
    "PhoneNumber" VARCHAR(256) NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL,
    CONSTRAINT "FK_Contact_Account"
        FOREIGN KEY ("AccountId") REFERENCES "Account"("Id")
);

CREATE INDEX "IX_Contact_AccountId"
ON "Contact" ("AccountId")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_Contact_Email"
ON "Contact" ("EmailAddress")
WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: AccountFile
-- =========================

CREATE TABLE "AccountFile"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AccountId" UUID NOT NULL,
    "FileName" VARCHAR(256) NOT NULL,
    "Path" VARCHAR(1024) NOT NULL,
    "ContentType" VARCHAR(256) NULL,
    "Size" BIGINT NOT NULL DEFAULT 0,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL,
    CONSTRAINT "FK_AccountFile_Account"
        FOREIGN KEY ("AccountId") REFERENCES "Account"("Id")
);

CREATE INDEX "IX_AccountFile_AccountId"
ON "AccountFile" ("AccountId")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_AccountFile_CreatedUtc"
ON "AccountFile" ("CreatedUtc")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_AccountFile_FileName"
ON "AccountFile" ("FileName")
WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: Event
-- =========================

CREATE TABLE "Event"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(256) NOT NULL,
    "Description" TEXT NULL,
    "StartUtc" TIMESTAMPTZ(6) NOT NULL,
    "EndUtc" TIMESTAMPTZ(6) NOT NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL
);

CREATE INDEX "IX_Event_StartUtc_EndUtc"
ON "Event" ("StartUtc","EndUtc")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_Event_Name"
ON "Event" ("Name")
WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: Place
-- =========================

CREATE TABLE "Place"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(256) NOT NULL,
    "Description" TEXT NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL
);

CREATE INDEX "IX_Place_Name"
ON "Place" ("Name")
WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: Register
-- =========================

CREATE TABLE "Register"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AccountId" UUID NOT NULL,
    "Action" SMALLINT NOT NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL,
    CONSTRAINT "FK_Register_Account"
        FOREIGN KEY ("AccountId") REFERENCES "Account"("Id")
);

CREATE INDEX "IX_Register_AccountId"
ON "Register" ("AccountId")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_Register_CreatedUtc"
ON "Register" ("CreatedUtc")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_Register_Action"
ON "Register" ("Action","CreatedUtc")
WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: RegisterEvent
-- =========================

CREATE TABLE "RegisterEvent"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "RegisterId" BIGINT NOT NULL,
    "EventId" BIGINT NOT NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL,
    FOREIGN KEY ("EventId") REFERENCES "Event"("Id"),
    FOREIGN KEY ("RegisterId") REFERENCES "Register"("Id")
);

CREATE INDEX "IX_RegisterEvent_RegisterId"
ON "RegisterEvent" ("RegisterId")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_RegisterEvent_EventId"
ON "RegisterEvent" ("EventId")
WHERE "DeletedUtc" IS NULL;

CREATE UNIQUE INDEX "UX_RegisterEvent_Unique"
ON "RegisterEvent" ("RegisterId","EventId")
WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: RegisterPlace
-- =========================

CREATE TABLE "RegisterPlace"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "RegisterId" BIGINT NOT NULL,
    "PlaceId" BIGINT NOT NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL,
    FOREIGN KEY ("PlaceId") REFERENCES "Place"("Id"),
    FOREIGN KEY ("RegisterId") REFERENCES "Register"("Id")
);

CREATE INDEX "IX_RegisterPlace_RegisterId"
ON "RegisterPlace" ("RegisterId")
WHERE "DeletedUtc" IS NULL;

CREATE INDEX "IX_RegisterPlace_PlaceId"
ON "RegisterPlace" ("PlaceId")
WHERE "DeletedUtc" IS NULL;

CREATE UNIQUE INDEX "UX_RegisterPlace_Unique"
ON "RegisterPlace" ("RegisterId","PlaceId")
WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: ApplicationConfiguration
-- =========================

CREATE TABLE "ApplicationConfiguration"
(
    "Id" INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ServerTimeZone" VARCHAR(64) NOT NULL DEFAULT 'America/Hermosillo',
    "ApplicationVersion" VARCHAR(64) NOT NULL,
    "Language" VARCHAR(16) NOT NULL DEFAULT 'en-US',
    "Environment" VARCHAR(64) NOT NULL,
    "InstanceName" VARCHAR(128) NOT NULL,
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL
);
CREATE INDEX "IX_ApplicationConfiguration_CreatedUtc" ON "ApplicationConfiguration" ("CreatedUtc" DESC) WHERE "DeletedUtc" IS NULL;

-- =========================
-- Table: MigrationRecord
-- =========================

CREATE TABLE "MigrationRecord"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "MigrationId" BIGINT NOT NULL,
    "Module" VARCHAR(128) NOT NULL,
    "Name" VARCHAR(256) NOT NULL,
    "Checksum" VARCHAR(128) NOT NULL,
    "AppliedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "CreatedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "ModifiedUtc" TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
    "DeletedUtc" TIMESTAMPTZ(6) NULL
);

CREATE UNIQUE INDEX "UX_MigrationRecord_MigrationId"
ON "MigrationRecord" ("MigrationId")
WHERE "DeletedUtc" IS NULL;

INSERT INTO "MigrationRecord" ("MigrationId","Module","Name","Checksum","AppliedUtc","DeletedUtc")
VALUES (1,'Core','migration-1','seeded-by-migration-1',NOW(),NULL);

