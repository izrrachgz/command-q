ALTER TABLE EmailOutboxMessage
    ADD COLUMN TraceParent TEXT NULL;

ALTER TABLE EmailOutboxMessage
    ADD COLUMN TraceState TEXT NULL;
