CREATE TABLE BankAccounts(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	IBAN VARCHAR(34) unique NOT NULL CHECK (LEN(IBAN) BETWEEN 15 AND 34),
	Balance decimal(18,2) NOT NULL CHECK (Balance >= 0),
	Currency NVARCHAR(3) NOT NULL CHECK (Currency IN ('USD', 'EUR', 'GEL')),
	PersonId NVARCHAR(450) NOT NULL,

	FOREIGN KEY (PersonId)
	REFERENCES AspNetUsers(Id)
)


CREATE TABLE BankCards(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    Firstname NVARCHAR(50) NOT NULL,
    Lastname NVARCHAR(50) NOT NULL,
    CardNumber NVARCHAR(19) unique NOT NULL CHECK (
        LEN(CardNumber) BETWEEN 13 AND 19
        AND CardNumber NOT LIKE '%[^0-9]%'
    ),
    ExpirationDate DATE NOT NULL CHECK (ExpirationDate > GETDATE()),
    CVV NVARCHAR(128) NOT NULL,
    PinCode NVARCHAR(128) NOT NULL,
	Salt NVARCHAR(64) NOT NULL,

    AccountId INT NOT NULL,
    FOREIGN KEY (AccountId) REFERENCES BankAccounts(Id)
)


CREATE TABLE AccountTransactions(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Amount decimal(18,2) NOT NULL CHECK (Amount >= 0),
	Currency NVARCHAR(3) NOT NULL CHECK (Currency IN ('USD', 'EUR', 'GEL')),
	TransactionDate DATE NOT NULL DEFAULT GETDATE(),
	FromAccountId INT NOT NULL,
	ToAccountId INT NOT NULL,
	FromATM BIT DEFAULT 0,
	TransactionFee decimal(18,2) DEFAULT 0,

	FOREIGN KEY (FromAccountId)
	REFERENCES BankAccounts(Id),

	FOREIGN KEY (ToAccountId)
	REFERENCES BankAccounts(Id)
)

Create Table AtmWithdrawals(
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Amount decimal(18,2) NOT NULL CHECK (Amount >= 0),
	Currency NVARCHAR(3) NOT NULL CHECK (Currency IN ('USD', 'EUR', 'GEL')),
	TransactionDate DATE NOT NULL DEFAULT GETDATE(),
	AccountId INT NOT NULL,
	TransactionFee decimal NOT Null
	FOREIGN KEY (AccountId)
	REFERENCES BankAccounts(Id),
)

