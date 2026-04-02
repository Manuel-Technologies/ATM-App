namespace ATM.Backend.Services;

public sealed class AuthTokenStore
{
    private readonly Dictionary<string, string> _tokenToAccountNumber = new(StringComparer.Ordinal);
    private readonly object _syncRoot = new();

    public string IssueToken(string accountNumber)
    {
        string token = $"atm-{Guid.NewGuid():N}";

        lock (_syncRoot)
        {
            _tokenToAccountNumber[token] = accountNumber;
        }

        return token;
    }

    public bool IsValid(string token, string accountNumber)
    {
        lock (_syncRoot)
        {
            return _tokenToAccountNumber.TryGetValue(token, out string? storedAccountNumber)
                && storedAccountNumber == accountNumber;
        }
    }
}
