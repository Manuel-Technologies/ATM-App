namespace ATMApp.Services;

public static class AtmServiceFactory
{
    private const string BaseUrlVariableName = "ATM_API_BASE_URL";
    private const string TokenVariableName = "ATM_API_TOKEN";

    public static IAtmService Create()
    {
        string? baseUrl = Environment.GetEnvironmentVariable(BaseUrlVariableName);
        string? token = Environment.GetEnvironmentVariable(TokenVariableName);

        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            return new ApiAtmService(baseUrl, token);
        }

        return new BankingService();
    }

    public static bool IsUsingApiBackend()
    {
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(BaseUrlVariableName));
    }

    public static string GetModeDescription()
    {
        return IsUsingApiBackend()
            ? $"API backend mode ({BaseUrlVariableName})"
            : "Demo mode (set ATM_API_BASE_URL to use a real backend)";
    }
}
