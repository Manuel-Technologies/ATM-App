namespace ATM.Backend.Data;

public sealed class AtmRepository
{
    private readonly object _syncRoot = new();
    private readonly List<AccountRecord> _accounts;

    public AtmRepository()
    {
        _accounts =
        [
            new AccountRecord
            {
                CardNumber = "1234567890123456",
                Pin = "1234",
                AccountNumber = "1002003001",
                AccountName = "Ada Obi",
                Balance = 2500.00m
            },
            new AccountRecord
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

    public AccountRecord? Authenticate(string cardNumber, string pin)
    {
        lock (_syncRoot)
        {
            return _accounts.FirstOrDefault(account =>
                account.CardNumber == cardNumber &&
                account.Pin == pin);
        }
    }

    public AccountRecord? GetAccount(string accountNumber)
    {
        lock (_syncRoot)
        {
            return _accounts.FirstOrDefault(account => account.AccountNumber == accountNumber);
        }
    }

    public AccountRecord? Deposit(string accountNumber, decimal amount)
    {
        lock (_syncRoot)
        {
            AccountRecord? account = GetAccountInternal(accountNumber);
            if (account is null)
            {
                return null;
            }

            account.Balance += amount;
            AddTransaction(account, "Deposit", amount, "Cash deposit");
            return account;
        }
    }

    public AccountMutationResult Withdraw(string accountNumber, decimal amount)
    {
        lock (_syncRoot)
        {
            AccountRecord? account = GetAccountInternal(accountNumber);
            if (account is null)
            {
                return new AccountMutationResult(MutationStatus.NotFound, "Account not found.", null);
            }

            if (amount > account.Balance)
            {
                return new AccountMutationResult(MutationStatus.Rejected, "Insufficient funds for this withdrawal.", null);
            }

            account.Balance -= amount;
            AddTransaction(account, "Withdrawal", amount, "ATM cash withdrawal");
            return new AccountMutationResult(MutationStatus.Success, "Withdrawal completed.", account);
        }
    }

    public TransferResult Transfer(string fromAccountNumber, string toAccountNumber, decimal amount)
    {
        lock (_syncRoot)
        {
            AccountRecord? sender = GetAccountInternal(fromAccountNumber);
            if (sender is null)
            {
                return new TransferResult(MutationStatus.NotFound, "Source account not found.", null);
            }

            AccountRecord? recipient = GetAccountInternal(toAccountNumber);
            if (recipient is null)
            {
                return new TransferResult(MutationStatus.NotFound, "Destination account was not found.", null);
            }

            if (sender.AccountNumber == recipient.AccountNumber)
            {
                return new TransferResult(MutationStatus.Rejected, "You cannot transfer to the same account.", null);
            }

            if (amount > sender.Balance)
            {
                return new TransferResult(MutationStatus.Rejected, "Insufficient funds for this transfer.", null);
            }

            sender.Balance -= amount;
            recipient.Balance += amount;
            AddTransaction(sender, "Transfer", amount, $"Transfer to {recipient.AccountName} ({recipient.AccountNumber})");
            AddTransaction(recipient, "Credit", amount, $"Transfer from {sender.AccountName} ({sender.AccountNumber})");

            return new TransferResult(MutationStatus.Success, $"Transfer successful to {recipient.AccountName}.", sender);
        }
    }

    private AccountRecord? GetAccountInternal(string accountNumber)
    {
        return _accounts.FirstOrDefault(account => account.AccountNumber == accountNumber);
    }

    private static void AddTransaction(AccountRecord account, string type, decimal amount, string description)
    {
        account.Transactions.Add(new TransactionRecord
        {
            Type = type,
            Amount = amount,
            Description = description,
            Timestamp = DateTime.UtcNow
        });
    }

    private void SeedTransactions()
    {
        AddTransaction(_accounts[0], "Deposit", 1500.00m, "Opening balance funding");
        AddTransaction(_accounts[0], "Withdrawal", 250.00m, "Initial cash withdrawal");
        AddTransaction(_accounts[1], "Deposit", 4200.00m, "Opening balance funding");
    }
}
