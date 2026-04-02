namespace ATM.Backend.Models;

public sealed record LoginResponse(string Token, AccountDto Account);

public sealed record AccountMutationResponse(string Message, decimal Balance, AccountDto UpdatedAccount);

public sealed record TransferResponse(string Message, AccountDto UpdatedAccount);

public sealed record ErrorResponse(string Message);
