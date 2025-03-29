-- SEQUENCE: public.healthchecklog_seq_id

-- DROP SEQUENCE IF EXISTS public.healthchecklog_seq_id;

CREATE SEQUENCE IF NOT EXISTS public.healthchecklog_seq_id
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.healthchecklog_seq_id
    OWNER TO postgres;

-- Table: public.HealthCheckLog

-- DROP TABLE IF EXISTS public."HealthCheckLog";

CREATE TABLE IF NOT EXISTS public."HealthCheckLog"
(
    "Id" bigint NOT NULL DEFAULT nextval('healthchecklog_seq_id'::regclass),
    "Created" timestamp without time zone NOT NULL DEFAULT now(),
    "Modified" timestamp without time zone NOT NULL DEFAULT now(),
    "Deleted" timestamp without time zone,
    CONSTRAINT "HealthCheckLog_Pk_Id" PRIMARY KEY ("Id")
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."HealthCheckLog"
    OWNER to postgres;

ALTER SEQUENCE public.healthchecklog_seq_id
    OWNED BY public."HealthCheckLog"."Id";

-- SEQUENCE: public.migrations_seq_id

-- DROP SEQUENCE IF EXISTS public.migrations_seq_id;

CREATE SEQUENCE IF NOT EXISTS public.migrations_seq_id
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.migrations_seq_id
    OWNER TO postgres;

-- Table: public.Migrations

-- DROP TABLE IF EXISTS public."Migrations";

CREATE TABLE IF NOT EXISTS public."Migrations"
(
    "Id" bigint NOT NULL DEFAULT nextval('migrations_seq_id'::regclass),
    "Created" timestamp without time zone NOT NULL DEFAULT now(),
    "Deleted" timestamp without time zone,
    CONSTRAINT "Migrations_Pk_Id" PRIMARY KEY ("Id")
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Migrations"
    OWNER to postgres;

ALTER SEQUENCE public.migrations_seq_id
    OWNED BY public."Migrations"."Id";

INSERT INTO public."Migrations" ("Deleted") VALUES (NULL);
