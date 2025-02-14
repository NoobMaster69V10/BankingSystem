CREATE TABLE BankAccounts(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	IBAN VARCHAR(34) NOT NULL CHECK (LEN(IBAN) BETWEEN 15 AND 34),
	Balance decimal(18,2) NOT NULL CHECK (Balance >= 0),
	Currency NVARCHAR(3) NOT NULL CHECK (Currency IN ('USD', 'EUR', 'GEL')),
	PersonId NVARCHAR(450) NOT NULL,

	FOREIGN KEY (PersonId)
	REFERENCES AspNetUsers(Id)
)



CREATE TABLE AccountTransactions(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Amount decimal(18,2) NOT NULL CHECK (Amount >= 0),
	Currency NVARCHAR(3) NOT NULL CHECK (Currency IN ('USD', 'EUR', 'GEL')),
	TransactionDate DATE NOT NULL DEFAULT GETDATE(),
	FromAccountId INT NOT NULL,
	ToAccountId INT NOT NULL,
	FOREIGN KEY (FromAccountId)
	REFERENCES BankAccounts(Id),

	FOREIGN KEY (ToAccountId)
	REFERENCES BankAccounts(Id)
)

CREATE TABLE BankCards(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    Firstname NVARCHAR(50) NOT NULL,
    Lastname NVARCHAR(50) NOT NULL,
    CardNumber NVARCHAR(19) NOT NULL CHECK (
        LEN(CardNumber) BETWEEN 13 AND 19
        AND CardNumber NOT LIKE '%[^0-9]%'
    ),
    ExpirationDate DATE NOT NULL CHECK (ExpirationDate > GETDATE()),
    CVV NVARCHAR(3) NOT NULL CHECK (LEN(CVV) = 3 AND CVV NOT LIKE '%[^0-9]%'),
    PinCode NVARCHAR(4) NOT NULL CHECK (LEN(PinCode) = 4 AND PinCode NOT LIKE '%[^0-9]%'),

    AccountId INT NOT NULL,
    FOREIGN KEY (AccountId) REFERENCES AccountTransactions(Id)
)

