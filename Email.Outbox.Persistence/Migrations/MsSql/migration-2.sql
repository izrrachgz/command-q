ALTER TABLE EmailOutboxMessage
    ADD ProfileName NVARCHAR(64) NOT NULL
        CONSTRAINT DF_EmailOutboxMessage_ProfileName DEFAULT 'default';
