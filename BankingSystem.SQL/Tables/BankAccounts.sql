CREATE TABLE BankAccounts
(
    BankAccountId INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    IBAN          VARCHAR(34) UNIQUE NOT NULL CHECK (LEN(IBAN) BETWEEN 15 AND 34),
    Balance       DECIMAL(18, 2)     NOT NULL CHECK (Balance >= 0),
    Currency      INT                NOT NULL,
    IsActive      BIT                NOT NULL DEFAULT 1,
    PersonId      NVARCHAR(450) NOT NULL,

    FOREIGN KEY (PersonId)
        REFERENCES AspNetUsers (Id) ON DELETE CASCADE,

    INDEX         IX_BankAccounts_IBAN NONCLUSTERED (IBAN)
)
