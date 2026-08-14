ALTER TABLE EmailOutboxMessage
    ADD COLUMN ProfileName VARCHAR(64) NOT NULL DEFAULT 'default';
