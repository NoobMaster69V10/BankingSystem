CREATE PROCEDURE TransferBetweenAccounts
    @FromAccountId INT,
    @ToAccountId INT,
    @Amount DECIMAL(18, 2),
	@TransactionFee DECIMAL(18, 2),
	@ConvertedAmount DECIMAL(18, 2)
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE BankAccounts SET Balance = Balance - (@Amount + @TransactionFee) WHERE BankAccountId = @FromAccountId;
        UPDATE BankAccounts SET Balance = Balance + @ConvertedAmount WHERE BankAccountId = @ToAccountId;
        
        INSERT INTO AccountTransactions(FromAccountId, ToAccountId, Amount, TransactionDate, TransactionFee, TransactionType)
        VALUES (@FromAccountId, @ToAccountId, @Amount, GETUTCDATE(), @TransactionFee, 1);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
