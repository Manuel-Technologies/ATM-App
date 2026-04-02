using ATM.Backend.Models;

namespace ATM.Backend.Data;

public sealed class AccountRecord
{
    public required string CardNumber { get; init; }

    public required string Pin { get; init; }

    public required string AccountNumber { get; init; }

    public required string AccountName { get; init; }

    public decimal Balance { get; set; }

    public List<TransactionRecord> Transactions { get; } = [];

    public AccountDto ToDto(bool includeSensitiveFields = false)
    {
        return new AccountDto
        {
            CardNumber = includeSensitiveFields ? CardNumber : string.Empty,
            Pin = includeSensitiveFields ? Pin : string.Empty,
            AccountNumber = AccountNumber,
            AccountName = AccountName,
            Balance = Balance,
            Transactions = Transactions
                .OrderByDescending(transaction => transaction.Timestamp)
                .Take(10)
                .Select(transaction => transaction.ToDto())
                .ToList()
        };
    }
}
