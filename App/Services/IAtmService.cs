using ATMApp.Models;

namespace ATMApp.Services;

public interface IAtmService
{
    Task<Account?> AuthenticateAsync(string cardNumber, string pin, CancellationToken cancellationToken = default);

    Task<Account> GetAccountAsync(string accountNumber, CancellationToken cancellationToken = default);

    Task DepositAsync(Account account, decimal amount, CancellationToken cancellationToken = default);

    Task<string> WithdrawAsync(Account account, decimal amount, CancellationToken cancellationToken = default);

    Task<string> TransferAsync(
        Account account,
        string recipientAccountNumber,
        decimal amount,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TransactionRecord>> GetRecentTransactionsAsync(
        Account account,
        CancellationToken cancellationToken = default);
}
