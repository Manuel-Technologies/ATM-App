namespace ATMApp.Models;

public sealed class TransactionRecord
{
    public required string Type { get; init; }

    public required decimal Amount { get; init; }

    public required string Description { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.Now;
}
