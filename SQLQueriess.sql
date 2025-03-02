CREATE TABLE BankAccounts(
	BankAccountId INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	IBAN VARCHAR(34) unique NOT NULL CHECK (LEN(IBAN) BETWEEN 15 AND 34),
	Balance decimal(18,2) NOT NULL CHECK (Balance >= 0),
	Currency NVARCHAR(3) NOT NULL CHECK (Currency IN ('USD', 'EUR', 'GEL')),
	PersonId NVARCHAR(450) NOT NULL,

	FOREIGN KEY (PersonId)
	REFERENCES AspNetUsers(Id) ON DELETE CASCADE
)


CREATE TABLE BankCards(
	BankCardId INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    Firstname NVARCHAR(50) NOT NULL,
    Lastname NVARCHAR(50) NOT NULL,
    CardNumber NVARCHAR(19) UNIQUE NOT NULL CHECK (
        LEN(CardNumber) BETWEEN 13 AND 19
        AND CardNumber NOT LIKE '%[^0-9]%'
    ),
    ExpirationDate DATE NOT NULL CHECK (ExpirationDate > GETDATE()),
    CVV NVARCHAR(128) NOT NULL,
    PinCode NVARCHAR(128) NOT NULL,
    AccountId INT NOT NULL,
    FOREIGN KEY (AccountId) REFERENCES BankAccounts(BankAccountId) ON DELETE CASCADE
)


CREATE TABLE AccountTransactions(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Amount decimal(18,2) NOT NULL CHECK (Amount >= 0),
	Currency NVARCHAR(3) NOT NULL CHECK (Currency IN ('USD', 'EUR', 'GEL')),
	TransactionDate DATETIME2 NOT NULL DEFAULT GETDATE(),
	FromAccountId INT NOT NULL,
	ToAccountId INT NOT NULL,
	TransactionFee decimal DEFAULT 0,

	FOREIGN KEY (FromAccountId)
	REFERENCES BankAccounts(BankAccountId),

	FOREIGN KEY (ToAccountId)
	REFERENCES BankAccounts(BankAccountId)
)

Create Table AtmTransactions(
	AtmTransactionId INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Amount decimal(18,2) NOT NULL CHECK (Amount >= 0),
	TransactionDate DATETIME2 NOT NULL DEFAULT GETDATE(),
	AccountId INT NOT NULL,
	TransactionFee decimal NOT Null
	FOREIGN KEY (AccountId)
	REFERENCES BankAccounts(BankAccountId),
)

