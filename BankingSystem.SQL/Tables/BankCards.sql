CREATE TABLE BankCards
(
    BankCardId     INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    CardNumber     NVARCHAR(19) UNIQUE NOT NULL CHECK (
        LEN(CardNumber) BETWEEN 13 AND 19
        AND CardNumber NOT LIKE '%[^0-9]%'
    ),
    ExpirationDate DATE NOT NULL CHECK (ExpirationDate > GETDATE()),
    CVV            NVARCHAR(128) NOT NULL,
    PinCode        NVARCHAR(128) NOT NULL,
    IsActive       BIT NOT NULL DEFAULT 1,
    AccountId      INT NOT NULL,
    FOREIGN KEY (AccountId) REFERENCES BankAccounts (BankAccountId) ON DELETE CASCADE,

    CONSTRAINT CK_BankCards_IsActive_ExpirationDate CHECK (IsActive = 0 OR ExpirationDate > GETDATE()),

    INDEX IX_BankCards_CardNumber NONCLUSTERED (CardNumber)
)