CREATE TRIGGER trg_UpdateIsActiveOnExpiration
    ON BankCards AFTER INSERT,
UPDATE
    AS
BEGIN
UPDATE BankCards
SET IsActive = 0
WHERE ExpirationDate <= GETDATE()
  AND IsActive = 1;
END
