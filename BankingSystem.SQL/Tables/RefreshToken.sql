CREATE TABLE RefreshTokens
(
    TokenId      UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Token        NVARCHAR(500) NOT NULL,
    PersonId     NVARCHAR(450) NOT NULl,
    ExpiresOnUtc DATETIME2 NOT NULL,
    CONSTRAINT FK_RefreshTokens_Person FOREIGN KEY (PersonId) REFERENCES AspNetUsers (Id)
);
