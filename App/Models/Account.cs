namespace ATMApp.Models;

public sealed class Account
{
    public required string CardNumber { get; init; }

    public required string Pin { get; init; }

    public required string AccountNumber { get; init; }

    public required string AccountName { get; init; }

    public decimal Balance { get; set; }

    public List<TransactionRecord> Transactions { get; } = new();
}
