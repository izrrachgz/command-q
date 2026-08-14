ALTER TABLE EmailOutboxMessage
    ADD COLUMN ProfileName TEXT NOT NULL DEFAULT 'default';
