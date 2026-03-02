-- Enable UUID generation (if not already enabled)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =========================
-- Table: HealthCheckLog
-- =========================
DROP TABLE IF EXISTS "HealthCheckLog" CASCADE;

CREATE TABLE "HealthCheckLog"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Modified" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL
);

CREATE INDEX "IX_HealthCheckLog_Created"
ON "HealthCheckLog" ("Created")
WHERE "Deleted" IS NULL;

-- =========================
-- Table: Account
-- =========================
DROP TABLE IF EXISTS "Account" CASCADE;

CREATE TABLE "Account"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "Username" VARCHAR(256) NOT NULL,
    "Email" VARCHAR(256) NOT NULL,
    "Password" VARCHAR(256) NOT NULL,
    "Roles" VARCHAR(512) NOT NULL,
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Modified" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL,
    CONSTRAINT "PK_Account_Id" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "UX_Account_Username"
ON "Account" ("Username")
WHERE "Deleted" IS NULL;

CREATE UNIQUE INDEX "UX_Account_Email"
ON "Account" ("Email")
WHERE "Deleted" IS NULL;

CREATE INDEX "IX_Account_Active"
ON "Account" ("Deleted", "Username", "Email", "Created");

INSERT INTO "Account"
("Id","Username","Email","Password","Roles")
VALUES
('3d6a4e33-5b5f-40ba-9e31-f21815b9d09f',
 'TurboDecoder',
 'admin@local.com',
 'e8424ea5339b85a750ab5f4c63bde5d5c62975792a3bed6ccb37c045207c6771',
 'User');

-- =========================
-- Table: AccountSession
-- =========================
DROP TABLE IF EXISTS "AccountSession" CASCADE;

CREATE TABLE "AccountSession"
(
    "AccountId" UUID NOT NULL,
    "TokenVersion" INT NOT NULL DEFAULT 0,
    "FailedLoginAttempts" SMALLINT NOT NULL DEFAULT 0,
    "LockedUntilUtc" TIMESTAMP NULL,
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Modified" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL,
    CONSTRAINT "FK_AccountSession_Account"
        FOREIGN KEY ("AccountId") REFERENCES "Account"("Id")
);

CREATE INDEX "IX_AccountSession_Active"
ON "AccountSession" ("AccountId")
WHERE "Deleted" IS NULL;

CREATE INDEX "IX_AccountSession_LockedUntilUtc"
ON "AccountSession" ("LockedUntilUtc")
WHERE "LockedUntilUtc" IS NOT NULL AND "Deleted" IS NULL;

CREATE INDEX "IX_AccountSession_FailedLoginAttempts"
ON "AccountSession" ("FailedLoginAttempts")
WHERE "FailedLoginAttempts" > 0 AND "Deleted" IS NULL;

INSERT INTO "AccountSession"
("AccountId","TokenVersion","FailedLoginAttempts","LockedUntilUtc")
VALUES
('3d6a4e33-5b5f-40ba-9e31-f21815b9d09f',0,0,NULL);

-- =========================
-- Table: Address
-- =========================
DROP TABLE IF EXISTS "Address" CASCADE;

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
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Modified" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL,
    CONSTRAINT "FK_Address_Account"
        FOREIGN KEY ("AccountId") REFERENCES "Account"("Id")
);

CREATE INDEX "IX_Address_AccountId"
ON "Address" ("AccountId")
WHERE "Deleted" IS NULL;

CREATE INDEX "IX_Address_Location"
ON "Address" ("Country","City","PostalCode")
WHERE "Deleted" IS NULL;

CREATE INDEX "IX_Address_Created"
ON "Address" ("Created")
WHERE "Deleted" IS NULL;

-- =========================
-- Table: Contact
-- =========================
DROP TABLE IF EXISTS "Contact" CASCADE;

CREATE TABLE "Contact"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AccountId" UUID NOT NULL,
    "Name" VARCHAR(256) NOT NULL,
    "Description" TEXT NULL,
    "EmailAddress" VARCHAR(256) NULL,
    "PhoneNumber" VARCHAR(256) NULL,
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Modified" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL,
    CONSTRAINT "FK_Contact_Account"
        FOREIGN KEY ("AccountId") REFERENCES "Account"("Id")
);

CREATE INDEX "IX_Contact_AccountId"
ON "Contact" ("AccountId")
WHERE "Deleted" IS NULL;

CREATE INDEX "IX_Contact_Email"
ON "Contact" ("EmailAddress")
WHERE "Deleted" IS NULL;

-- =========================
-- Table: Event
-- =========================
DROP TABLE IF EXISTS "Event" CASCADE;

CREATE TABLE "Event"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(256) NOT NULL,
    "Description" TEXT NULL,
    "Start" TIMESTAMP NOT NULL,
    "End" TIMESTAMP NOT NULL,
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Modified" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL
);

CREATE INDEX "IX_Event_Start_End"
ON "Event" ("Start","End")
WHERE "Deleted" IS NULL;

CREATE INDEX "IX_Event_Name"
ON "Event" ("Name")
WHERE "Deleted" IS NULL;

-- =========================
-- Table: Place
-- =========================
DROP TABLE IF EXISTS "Place" CASCADE;

CREATE TABLE "Place"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(256) NOT NULL,
    "Description" TEXT NULL,
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Modified" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL
);

CREATE INDEX "IX_Place_Name"
ON "Place" ("Name")
WHERE "Deleted" IS NULL;

-- =========================
-- Table: Register
-- =========================
DROP TABLE IF EXISTS "Register" CASCADE;

CREATE TABLE "Register"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AccountId" UUID NOT NULL,
    "Action" SMALLINT NOT NULL,
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Modified" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL,
    CONSTRAINT "FK_Register_Account"
        FOREIGN KEY ("AccountId") REFERENCES "Account"("Id")
);

CREATE INDEX "IX_Register_AccountId"
ON "Register" ("AccountId")
WHERE "Deleted" IS NULL;

CREATE INDEX "IX_Register_Created"
ON "Register" ("Created")
WHERE "Deleted" IS NULL;

CREATE INDEX "IX_Register_Action"
ON "Register" ("Action","Created")
WHERE "Deleted" IS NULL;

-- =========================
-- Table: RegisterEvent
-- =========================
DROP TABLE IF EXISTS "RegisterEvent" CASCADE;

CREATE TABLE "RegisterEvent"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "RegisterId" BIGINT NOT NULL,
    "EventId" BIGINT NOT NULL,
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Modified" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL,
    FOREIGN KEY ("EventId") REFERENCES "Event"("Id"),
    FOREIGN KEY ("RegisterId") REFERENCES "Register"("Id")
);

CREATE INDEX "IX_RegisterEvent_RegisterId"
ON "RegisterEvent" ("RegisterId")
WHERE "Deleted" IS NULL;

CREATE INDEX "IX_RegisterEvent_EventId"
ON "RegisterEvent" ("EventId")
WHERE "Deleted" IS NULL;

CREATE UNIQUE INDEX "UX_RegisterEvent_Unique"
ON "RegisterEvent" ("RegisterId","EventId")
WHERE "Deleted" IS NULL;

-- =========================
-- Table: RegisterPlace
-- =========================
DROP TABLE IF EXISTS "RegisterPlace" CASCADE;

CREATE TABLE "RegisterPlace"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "RegisterId" BIGINT NOT NULL,
    "PlaceId" BIGINT NOT NULL,
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Modified" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL,
    FOREIGN KEY ("PlaceId") REFERENCES "Place"("Id"),
    FOREIGN KEY ("RegisterId") REFERENCES "Register"("Id")
);

CREATE INDEX "IX_RegisterPlace_RegisterId"
ON "RegisterPlace" ("RegisterId")
WHERE "Deleted" IS NULL;

CREATE INDEX "IX_RegisterPlace_PlaceId"
ON "RegisterPlace" ("PlaceId")
WHERE "Deleted" IS NULL;

CREATE UNIQUE INDEX "UX_RegisterPlace_Unique"
ON "RegisterPlace" ("RegisterId","PlaceId")
WHERE "Deleted" IS NULL;

-- =========================
-- Table: Migrations
-- =========================
DROP TABLE IF EXISTS "Migrations" CASCADE;

CREATE TABLE "Migrations"
(
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Created" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Deleted" TIMESTAMP NULL
);

INSERT INTO "Migrations" ("Deleted") VALUES (NULL);