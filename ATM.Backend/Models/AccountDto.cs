namespace ATM.Backend.Models;

public sealed class AccountDto
{
    public required string CardNumber { get; init; }

    public required string Pin { get; init; }

    public required string AccountNumber { get; init; }

    public required string AccountName { get; init; }

    public decimal Balance { get; init; }

    public List<TransactionDto> Transactions { get; init; } = [];
}
