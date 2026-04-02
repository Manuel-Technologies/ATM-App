namespace ATM.Backend.Models;

public sealed class TransactionDto
{
    public required string Type { get; init; }

    public required decimal Amount { get; init; }

    public required string Description { get; init; }

    public DateTime Timestamp { get; init; }
}
