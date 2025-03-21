CREATE TRIGGER trg_InsertBankAccount
    ON BankAccounts AFTER INSERT
AS
BEGIN
    DECLARE @countOfManagersAndOperators INT;

    SELECT @countOfManagersAndOperators = COUNT(*) 
    FROM inserted ba
    LEFT JOIN AspNetUsers au ON ba.PersonId = au.Id
    LEFT JOIN AspNetUserRoles aur ON aur.UserId = ba.PersonId
    LEFT JOIN AspNetRoles ar ON ar.Id = aur.RoleId
    WHERE ar.[Name] = 'Manager' OR ar.Name = 'Operator';

    IF @countOfManagersAndOperators != 0
        THROW 51000, 'Manager or operator can not have bank account', 1;
END;