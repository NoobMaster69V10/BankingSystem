CREATE VIEW vw_PersonInfo AS
SELECT 
    u.Id AS PersonID, 
    u.FirstName,
    u.LastName, 
    u.Email, 
    u.IdNumber,
	u.UserName AS Username,
    u.BirthDate,
    b.BankAccountId, 
    b.IBAN, 
    b.Balance, 
    b.Currency,
    b.IsActive AS BankAccountStatus,
    bc.BankCardId, 
    bc.CardNumber, 
    bc.ExpirationDate, 
    bc.IsActive AS BankCardStatus,
    bc.PinCode, 
    bc.CVV, 
    bc.AccountId
FROM AspNetUsers u
LEFT JOIN BankAccounts b ON u.Id = b.PersonId
LEFT JOIN BankCards bc ON b.BankAccountId = bc.AccountId;