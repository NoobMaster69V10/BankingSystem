CREATE TABLE AccountTransactions
(
    Id              INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    Amount          decimal(18, 2) NOT NULL CHECK (Amount >= 0),
    TransactionDate DATETIME2      NOT NULL DEFAULT GETDATE(),
    FromAccountId   INT            NOT NULL,
    ToAccountId     INT NULL,
    TransactionFee  decimal(18, 2)          DEFAULT 0,
    TransactionType INT            NOT NULL DEFAULT 1,
    FOREIGN KEY (FromAccountId) REFERENCES BankAccounts (BankAccountId),
    FOREIGN KEY (ToAccountId) REFERENCES BankAccounts (BankAccountId),
    CHECK (
        (TransactionType = 0 AND ToAccountId IS NULL) OR
        (TransactionType = 1 AND ToAccountId IS NOT NULL)
        )
)
