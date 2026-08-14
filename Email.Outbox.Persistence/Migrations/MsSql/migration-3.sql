ALTER TABLE dbo.EmailOutboxMessage
    ADD TraceParent VARCHAR(55) NULL,
        TraceState NVARCHAR(512) NULL;
