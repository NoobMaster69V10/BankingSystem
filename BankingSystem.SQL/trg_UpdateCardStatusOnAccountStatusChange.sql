CREATE TRIGGER trg_UpdateCardStatusOnAccountStatusChange
    ON BankAccounts
    AFTER UPDATE
              AS
BEGIN
    IF UPDATE(IsActive)
BEGIN
UPDATE bc
SET IsActive = 0
    FROM BankCards bc
        INNER JOIN inserted i ON bc.AccountId = i.BankAccountId
WHERE i.IsActive = 0;
END
END;
