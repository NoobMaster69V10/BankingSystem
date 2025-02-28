using System.Data;

public interface ITransaction
{
    void SetTransaction(IDbTransaction transaction);
}
