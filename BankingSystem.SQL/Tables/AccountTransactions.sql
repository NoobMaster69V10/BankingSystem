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