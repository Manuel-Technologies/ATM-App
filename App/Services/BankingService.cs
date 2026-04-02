using ATMApp.Models;

namespace ATMApp.Services;

public sealed class BankingService
    : IAtmService
{
    private readonly List<Account> _accounts;

    public BankingService()
    {
        _accounts =
        [
            new Account
            {
                CardNumber = "1234567890123456",
                Pin = "1234",
                AccountNumber = "1002003001",
                AccountName = "Ada Obi",
                Balance = 2500.00m
            },
            new Account
            {
                CardNumber = "1111222233334444",
                Pin = "4321",
                AccountNumber = "1002003002",
                AccountName = "Michael Stone",
                Balance = 4200.00m
            }
        ];

        SeedTransactions();
    }

    public bool TryAuthenticate(string cardNumber, string pin, out Account? account)
    {
        account = _accounts.FirstOrDefault(candidate =>
            candidate.CardNumber == cardNumber &&
            candidate.Pin == pin);

        return account is not null;
    }

    public Task<Account?> AuthenticateAsync(string cardNumber, string pin, CancellationToken cancellationToken = default)
    {
        TryAuthenticate(cardNumber, pin, out Account? account);
        return Task.FromResult(account);
    }

    public Task<Account> GetAccountAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        Account account = _accounts.FirstOrDefault(candidate => candidate.AccountNumber == accountNumber)
            ?? throw new InvalidOperationException("Account was not found.");

        return Task.FromResult(account);
    }

    public void Deposit(Account account, decimal amount)
    {
        account.Balance += amount;
        AddTransaction(account, "Deposit", amount, "Cash deposit");
    }

    public Task DepositAsync(Account account, decimal amount, CancellationToken cancellationToken = default)
    {
        Deposit(account, amount);
        return Task.CompletedTask;
    }

    public bool TryWithdraw(Account account, decimal amount, out string message)
    {
        if (amount > account.Balance)
        {
            message = "Insufficient funds for this withdrawal.";
            return false;
        }

        account.Balance -= amount;
        AddTransaction(account, "Withdrawal", amount, "ATM cash withdrawal");
        message = "Withdrawal completed.";
        return true;
    }

    public Task<string> WithdrawAsync(Account account, decimal amount, CancellationToken cancellationToken = default)
    {
        if (!TryWithdraw(account, amount, out string message))
        {
            throw new InvalidOperationException(message);
        }

        return Task.FromResult(message);
    }

    public bool TryTransfer(Account sender, string recipientAccountNumber, decimal amount, out string message)
    {
        Account? recipient = _accounts.FirstOrDefault(account =>
            account.AccountNumber == recipientAccountNumber);

        if (recipient is null)
        {
            message = "Destination account was not found.";
            return false;
        }

        if (recipient.AccountNumber == sender.AccountNumber)
        {
            message = "You cannot transfer to the same account.";
            return false;
        }

        if (amount > sender.Balance)
        {
            message = "Insufficient funds for this transfer.";
            return false;
        }

        sender.Balance -= amount;
        recipient.Balance += amount;

        AddTransaction(sender, "Transfer", amount, $"Transfer to {recipient.AccountName} ({recipient.AccountNumber})");
        AddTransaction(recipient, "Credit", amount, $"Transfer from {sender.AccountName} ({sender.AccountNumber})");

        message = $"Transfer successful to {recipient.AccountName}.";
        return true;
    }

    public Task<string> TransferAsync(
        Account account,
        string recipientAccountNumber,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (!TryTransfer(account, recipientAccountNumber, amount, out string message))
        {
            throw new InvalidOperationException(message);
        }

        return Task.FromResult(message);
    }

    public IReadOnlyList<TransactionRecord> GetRecentTransactions(Account account)
    {
        return account.Transactions
            .OrderByDescending(transaction => transaction.Timestamp)
            .Take(5)
            .ToList();
    }

    public Task<IReadOnlyList<TransactionRecord>> GetRecentTransactionsAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetRecentTransactions(account));
    }

    private static void AddTransaction(Account account, string type, decimal amount, string description)
    {
        account.Transactions.Add(new TransactionRecord
        {
            Type = type,
            Amount = amount,
            Description = description,
            Timestamp = DateTime.Now
        });
    }

    private void SeedTransactions()
    {
        AddTransaction(_accounts[0], "Deposit", 1500.00m, "Opening balance funding");
        AddTransaction(_accounts[0], "Withdrawal", 250.00m, "Initial cash withdrawal");
        AddTransaction(_accounts[1], "Deposit", 4200.00m, "Opening balance funding");
    }
}
