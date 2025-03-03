Create Table AtmTransactions(
	AtmTransactionId INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Amount decimal(18,2) NOT NULL CHECK (Amount >= 0),
	TransactionDate DATETIME2 NOT NULL DEFAULT GETDATE(),
	AccountId INT NOT NULL,
	TransactionFee decimal NOT Null
	FOREIGN KEY (AccountId)
	REFERENCES BankAccounts(BankAccountId),
)
