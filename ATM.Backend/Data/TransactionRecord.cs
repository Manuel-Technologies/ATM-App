using ATM.Backend.Models;

namespace ATM.Backend.Data;

public sealed class TransactionRecord
{
    public required string Type { get; init; }

    public required decimal Amount { get; init; }

    public required string Description { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public TransactionDto ToDto()
    {
        return new TransactionDto
        {
            Type = Type,
            Amount = Amount,
            Description = Description,
            Timestamp = Timestamp
        };
    }
}
