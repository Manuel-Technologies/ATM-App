using ATMApp.Models;
using ATMApp.Services;

namespace ATMApp.UI;

public static class AppScreen
{
    private static readonly IAtmService AtmService = AtmServiceFactory.Create();

    public static async Task RunAsync()
    {
        ShowWelcome();
        Utility.WriteInfo(AtmServiceFactory.GetModeDescription());
        Console.WriteLine();

        Account? account = await AuthenticateCustomerAsync();
        if (account is null)
        {
            Utility.WriteError("Authentication failed. Session ended.");
            return;
        }

        await RunMainMenuAsync(account);
    }

    private static void ShowWelcome()
    {
        Utility.TryPrepareConsole("TriemBank ATM");
        Utility.WriteHeader("Welcome to TriemBank ATM");
        Console.WriteLine("Secure banking for quick everyday transactions.");
        Console.WriteLine();
    }

    private static async Task<Account?> AuthenticateCustomerAsync()
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            string cardNumber = Utility.ReadRequiredString("Enter your ATM card number: ");
            string pin = Utility.ReadRequiredString("Enter your 4-digit PIN: ", isSecret: true);

            Account? account = await TryAuthenticateAsync(cardNumber, pin);
            if (account is not null)
            {
                Utility.WriteSuccess($"Welcome, {account.AccountName}. Card: {Utility.MaskCardNumber(account.CardNumber)}");
                return account;
            }

            Utility.WriteError($"Invalid card number or PIN. Attempts remaining: {3 - attempt}");
            Console.WriteLine();
        }

        return null;
    }

    private static async Task<Account?> TryAuthenticateAsync(string cardNumber, string pin)
    {
        try
        {
            return await AtmService.AuthenticateAsync(cardNumber, pin);
        }
        catch (Exception ex)
        {
            Utility.WriteError($"Unable to reach the backend: {ex.Message}");
            return null;
        }
    }

    private static async Task RunMainMenuAsync(Account account)
    {
        bool exitRequested = false;

        while (!exitRequested)
        {
            Utility.WriteHeader("Main Menu");
            Console.WriteLine($"Account: {account.AccountName}");
            Console.WriteLine($"Available balance: {account.Balance:C}");
            Console.WriteLine();
            Console.WriteLine("1. Check balance");
            Console.WriteLine("2. Deposit funds");
            Console.WriteLine("3. Withdraw cash");
            Console.WriteLine("4. Transfer funds");
            Console.WriteLine("5. View mini statement");
            Console.WriteLine("6. Exit");
            Console.WriteLine();

            int choice = Utility.ReadMenuChoice("Choose an option: ", 1, 6);
            Console.WriteLine();

            switch (choice)
            {
                case 1:
                    await ShowBalanceAsync(account);
                    break;
                case 2:
                    await DepositFundsAsync(account);
                    break;
                case 3:
                    await WithdrawFundsAsync(account);
                    break;
                case 4:
                    await TransferFundsAsync(account);
                    break;
                case 5:
                    await ShowMiniStatementAsync(account);
                    break;
                case 6:
                    exitRequested = true;
                    Utility.WriteInfo("Thank you for using TriemBank ATM.");
                    break;
            }

            if (!exitRequested)
            {
                Utility.PressEnter();
                Utility.TryClear();
            }
        }
    }

    private static async Task ShowBalanceAsync(Account account)
    {
        Utility.WriteHeader("Balance");
        Account? refreshedAccount = await TryExecuteServiceCallAsync(() => AtmService.GetAccountAsync(account.AccountNumber));
        if (refreshedAccount is null)
        {
            return;
        }

        account.Balance = refreshedAccount.Balance;
        Console.WriteLine($"Current balance: {account.Balance:C}");
    }

    private static async Task DepositFundsAsync(Account account)
    {
        Utility.WriteHeader("Deposit");
        decimal amount = Utility.ReadAmount("Enter amount to deposit: ");
        bool succeeded = await TryExecuteServiceCallAsync(() => AtmService.DepositAsync(account, amount));
        if (!succeeded)
        {
            return;
        }

        Utility.WriteSuccess($"Deposit successful. New balance: {account.Balance:C}");
    }

    private static async Task WithdrawFundsAsync(Account account)
    {
        Utility.WriteHeader("Withdraw");
        decimal amount = Utility.ReadAmount("Enter amount to withdraw: ");

        string? message = await TryExecuteServiceCallAsync(() => AtmService.WithdrawAsync(account, amount));
        if (message is null)
        {
            return;
        }

        Utility.WriteSuccess($"Withdrawal successful. New balance: {account.Balance:C}");
        Utility.WriteInfo(message);
    }

    private static async Task TransferFundsAsync(Account account)
    {
        Utility.WriteHeader("Transfer");
        string recipientAccountNumber = Utility.ReadRequiredString("Enter destination account number: ");
        decimal amount = Utility.ReadAmount("Enter amount to transfer: ");

        string? message = await TryExecuteServiceCallAsync(
            () => AtmService.TransferAsync(account, recipientAccountNumber, amount));
        if (message is null)
        {
            return;
        }

        Utility.WriteSuccess(message);
        Console.WriteLine($"New balance: {account.Balance:C}");
    }

    private static async Task ShowMiniStatementAsync(Account account)
    {
        Utility.WriteHeader("Mini Statement");
        IReadOnlyList<TransactionRecord>? transactions =
            await TryExecuteServiceCallAsync(() => AtmService.GetRecentTransactionsAsync(account));
        if (transactions is null)
        {
            return;
        }

        if (transactions.Count == 0)
        {
            Utility.WriteInfo("No transactions recorded yet.");
            return;
        }

        foreach (TransactionRecord transaction in transactions)
        {
            Console.WriteLine(
                $"{transaction.Timestamp:g} | {transaction.Type,-10} | {transaction.Amount,10:C} | {transaction.Description}");
        }
    }

    private static async Task<bool> TryExecuteServiceCallAsync(Func<Task> operation)
    {
        try
        {
            await operation();
            return true;
        }
        catch (Exception ex)
        {
            Utility.WriteError(ex.Message);
            return false;
        }
    }

    private static async Task<T?> TryExecuteServiceCallAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            Utility.WriteError(ex.Message);
            return default;
        }
    }
}
