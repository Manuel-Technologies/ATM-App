namespace ATM.Backend.Models;

public sealed record LoginRequest(string CardNumber, string Pin);

public sealed record AmountRequest(decimal Amount);

public sealed record TransferRequest(string FromAccountNumber, string ToAccountNumber, decimal Amount);
