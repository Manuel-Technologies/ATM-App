using ATM.Backend.Data;
using ATM.Backend.Models;
using ATM.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AtmRepository>();
builder.Services.AddSingleton<AuthTokenStore>();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    name = "TriemBank ATM Backend",
    status = "ok",
    endpoints = new[]
    {
        "/api/auth/login",
        "/api/accounts/{accountNumber}",
        "/api/accounts/{accountNumber}/deposit",
        "/api/accounts/{accountNumber}/withdraw",
        "/api/accounts/{accountNumber}/transactions?take=5",
        "/api/transfers"
    }
}));

app.MapPost("/api/auth/login", (
    LoginRequest request,
    AtmRepository repository,
    AuthTokenStore tokenStore) =>
{
    AccountRecord? account = repository.Authenticate(request.CardNumber, request.Pin);
    if (account is null)
    {
        return Results.Unauthorized();
    }

    string token = tokenStore.IssueToken(account.AccountNumber);
    return Results.Ok(new LoginResponse(token, account.ToDto(includeSensitiveFields: true)));
});

app.MapGet("/api/accounts/{accountNumber}", (
    string accountNumber,
    HttpRequest request,
    AtmRepository repository,
    AuthTokenStore tokenStore) =>
{
    if (!TryAuthorize(request, tokenStore, accountNumber))
    {
        return Results.Unauthorized();
    }

    AccountRecord? account = repository.GetAccount(accountNumber);
    return account is null
        ? Results.NotFound(new ErrorResponse("Account not found."))
        : Results.Ok(account.ToDto());
});

app.MapPost("/api/accounts/{accountNumber}/deposit", (
    string accountNumber,
    AmountRequest requestBody,
    HttpRequest request,
    AtmRepository repository,
    AuthTokenStore tokenStore) =>
{
    if (!TryAuthorize(request, tokenStore, accountNumber))
    {
        return Results.Unauthorized();
    }

    if (requestBody.Amount <= 0)
    {
        return Results.BadRequest(new ErrorResponse("Amount must be greater than zero."));
    }

    AccountRecord? account = repository.Deposit(accountNumber, requestBody.Amount);
    return account is null
        ? Results.NotFound(new ErrorResponse("Account not found."))
        : Results.Ok(new AccountMutationResponse("Deposit successful.", account.Balance, account.ToDto()));
});

app.MapPost("/api/accounts/{accountNumber}/withdraw", (
    string accountNumber,
    AmountRequest requestBody,
    HttpRequest request,
    AtmRepository repository,
    AuthTokenStore tokenStore) =>
{
    if (!TryAuthorize(request, tokenStore, accountNumber))
    {
        return Results.Unauthorized();
    }

    if (requestBody.Amount <= 0)
    {
        return Results.BadRequest(new ErrorResponse("Amount must be greater than zero."));
    }

    AccountMutationResult result = repository.Withdraw(accountNumber, requestBody.Amount);
    return result.Status switch
    {
        MutationStatus.NotFound => Results.NotFound(new ErrorResponse(result.Message)),
        MutationStatus.Rejected => Results.BadRequest(new ErrorResponse(result.Message)),
        _ => Results.Ok(new AccountMutationResponse(result.Message, result.Account!.Balance, result.Account.ToDto()))
    };
});

app.MapGet("/api/accounts/{accountNumber}/transactions", (
    string accountNumber,
    int? take,
    HttpRequest request,
    AtmRepository repository,
    AuthTokenStore tokenStore) =>
{
    if (!TryAuthorize(request, tokenStore, accountNumber))
    {
        return Results.Unauthorized();
    }

    AccountRecord? account = repository.GetAccount(accountNumber);
    if (account is null)
    {
        return Results.NotFound(new ErrorResponse("Account not found."));
    }

    int itemCount = take.GetValueOrDefault(5);
    List<TransactionDto> transactions = account.Transactions
        .OrderByDescending(transaction => transaction.Timestamp)
        .Take(Math.Clamp(itemCount, 1, 20))
        .Select(transaction => transaction.ToDto())
        .ToList();

    return Results.Ok(transactions);
});

app.MapPost("/api/transfers", (
    TransferRequest requestBody,
    HttpRequest request,
    AtmRepository repository,
    AuthTokenStore tokenStore) =>
{
    if (!TryAuthorize(request, tokenStore, requestBody.FromAccountNumber))
    {
        return Results.Unauthorized();
    }

    if (requestBody.Amount <= 0)
    {
        return Results.BadRequest(new ErrorResponse("Amount must be greater than zero."));
    }

    TransferResult result = repository.Transfer(
        requestBody.FromAccountNumber,
        requestBody.ToAccountNumber,
        requestBody.Amount);

    return result.Status switch
    {
        MutationStatus.NotFound => Results.NotFound(new ErrorResponse(result.Message)),
        MutationStatus.Rejected => Results.BadRequest(new ErrorResponse(result.Message)),
        _ => Results.Ok(new TransferResponse(result.Message, result.SourceAccount!.ToDto()))
    };
});

app.Run();

static bool TryAuthorize(HttpRequest request, AuthTokenStore tokenStore, string accountNumber)
{
    if (!request.Headers.TryGetValue("Authorization", out var headerValue))
    {
        return false;
    }

    string header = headerValue.ToString();
    const string bearerPrefix = "Bearer ";
    if (!header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    string token = header[bearerPrefix.Length..].Trim();
    return tokenStore.IsValid(token, accountNumber);
}
