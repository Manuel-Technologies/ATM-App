using ATMApp.Models;

namespace ATMApp.Services;

public sealed class ApiAtmService : IAtmService, IDisposable
{
    private readonly APIService _apiService;

    public ApiAtmService(string baseUrl, string? bearerToken = null)
    {
        _apiService = new APIService(baseUrl);

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            _apiService.AddBearerToken(bearerToken);
        }
    }

    public async Task<Account?> AuthenticateAsync(
        string cardNumber,
        string pin,
        CancellationToken cancellationToken = default)
    {
        LoginResponse? response = await _apiService.PostAsync<LoginRequest, LoginResponse>(
            "api/auth/login",
            new LoginRequest(cardNumber, pin),
            cancellationToken);

        if (response is null || response.Account is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(response.Token))
        {
            _apiService.AddBearerToken(response.Token);
        }

        return response.Account;
    }

    public async Task<Account> GetAccountAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        Account? account = await _apiService.GetAsync<Account>(
            $"api/accounts/{accountNumber}",
            cancellationToken);

        return account ?? throw new InvalidOperationException("The backend returned an empty account response.");
    }

    public async Task DepositAsync(Account account, decimal amount, CancellationToken cancellationToken = default)
    {
        AccountMutationResponse? response =
            await _apiService.PostAsync<AccountAmountRequest, AccountMutationResponse>(
                $"api/accounts/{account.AccountNumber}/deposit",
                new AccountAmountRequest(amount),
                cancellationToken);

        ApplyMutationResult(account, response, "Deposit completed.");
    }

    public async Task<string> WithdrawAsync(
        Account account,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        AccountMutationResponse? response =
            await _apiService.PostAsync<AccountAmountRequest, AccountMutationResponse>(
                $"api/accounts/{account.AccountNumber}/withdraw",
                new AccountAmountRequest(amount),
                cancellationToken);

        ApplyMutationResult(account, response, "Withdrawal completed.");
        return response?.Message ?? "Withdrawal completed.";
    }

    public async Task<string> TransferAsync(
        Account account,
        string recipientAccountNumber,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        TransferResponse? response =
            await _apiService.PostAsync<TransferRequest, TransferResponse>(
                "api/transfers",
                new TransferRequest(account.AccountNumber, recipientAccountNumber, amount),
                cancellationToken);

        if (response?.UpdatedAccount is not null)
        {
            CopyAccountState(account, response.UpdatedAccount);
        }
        else
        {
            await RefreshAccountAsync(account, cancellationToken);
        }

        return response?.Message ?? "Transfer completed.";
    }

    public async Task<IReadOnlyList<TransactionRecord>> GetRecentTransactionsAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TransactionRecord>? transactions = await _apiService.GetAsync<List<TransactionRecord>>(
            $"api/accounts/{account.AccountNumber}/transactions?take=5",
            cancellationToken);

        return transactions ?? [];
    }

    public void Dispose()
    {
        _apiService.Dispose();
    }

    private async Task RefreshAccountAsync(Account account, CancellationToken cancellationToken)
    {
        Account updatedAccount = await GetAccountAsync(account.AccountNumber, cancellationToken);
        CopyAccountState(account, updatedAccount);
    }

    private static void ApplyMutationResult(Account account, AccountMutationResponse? response, string defaultMessage)
    {
        if (response?.UpdatedAccount is not null)
        {
            CopyAccountState(account, response.UpdatedAccount);
            return;
        }

        if (response?.Balance is not null)
        {
            account.Balance = response.Balance.Value;
            return;
        }

        throw new InvalidOperationException(response?.Message ?? defaultMessage);
    }

    private static void CopyAccountState(Account target, Account source)
    {
        target.Balance = source.Balance;
        target.Transactions.Clear();
        target.Transactions.AddRange(source.Transactions);
    }

    private sealed record LoginRequest(string CardNumber, string Pin);

    private sealed record LoginResponse(string? Token, Account? Account);

    private sealed record AccountAmountRequest(decimal Amount);

    private sealed record TransferRequest(string FromAccountNumber, string ToAccountNumber, decimal Amount);

    private sealed record AccountMutationResponse(string? Message, decimal? Balance, Account? UpdatedAccount);

    private sealed record TransferResponse(string? Message, Account? UpdatedAccount);
}
