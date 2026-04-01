using ATMApp.Models;
using ATMApp.Services;

namespace ATMApp.UI;

public static class AppScreen
{
    private static readonly BankingService BankingService = new();

    public static Task RunAsync()
    {
        Console.Title = "TriemBank ATM";
        ShowWelcome();

        Account? account = AuthenticateCustomer();
        if (account is null)
        {
            Utility.WriteError("Authentication failed. Session ended.");
            return Task.CompletedTask;
        }

        RunMainMenu(account);
        return Task.CompletedTask;
    }

    private static void ShowWelcome()
    {
        Utility.TryPrepareConsole("TriemBank ATM");
        Utility.WriteHeader("Welcome to TriemBank ATM");
        Console.WriteLine("Secure banking for quick everyday transactions.");
        Console.WriteLine();
    }

    private static Account? AuthenticateCustomer()
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            string cardNumber = Utility.ReadRequiredString("Enter your ATM card number: ");
            string pin = Utility.ReadRequiredString("Enter your 4-digit PIN: ", isSecret: true);

            if (BankingService.TryAuthenticate(cardNumber, pin, out Account? account))
            {
                Utility.WriteSuccess(
                    $"Welcome, {account!.AccountName}. Card: {Utility.MaskCardNumber(account.CardNumber)}");
                return account;
            }

            Utility.WriteError($"Invalid card number or PIN. Attempts remaining: {3 - attempt}");
            Console.WriteLine();
        }

        return null;
    }

    private static void RunMainMenu(Account account)
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
                    ShowBalance(account);
                    break;
                case 2:
                    DepositFunds(account);
                    break;
                case 3:
                    WithdrawFunds(account);
                    break;
                case 4:
                    TransferFunds(account);
                    break;
                case 5:
                    ShowMiniStatement(account);
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

    private static void ShowBalance(Account account)
    {
        Utility.WriteHeader("Balance");
        Console.WriteLine($"Current balance: {account.Balance:C}");
    }

    private static void DepositFunds(Account account)
    {
        Utility.WriteHeader("Deposit");
        decimal amount = Utility.ReadAmount("Enter amount to deposit: ");
        BankingService.Deposit(account, amount);
        Utility.WriteSuccess($"Deposit successful. New balance: {account.Balance:C}");
    }

    private static void WithdrawFunds(Account account)
    {
        Utility.WriteHeader("Withdraw");
        decimal amount = Utility.ReadAmount("Enter amount to withdraw: ");

        if (!BankingService.TryWithdraw(account, amount, out string message))
        {
            Utility.WriteError(message);
            return;
        }

        Utility.WriteSuccess($"Withdrawal successful. New balance: {account.Balance:C}");
    }

    private static void TransferFunds(Account account)
    {
        Utility.WriteHeader("Transfer");
        string recipientAccountNumber = Utility.ReadRequiredString("Enter destination account number: ");
        decimal amount = Utility.ReadAmount("Enter amount to transfer: ");

        if (!BankingService.TryTransfer(account, recipientAccountNumber, amount, out string message))
        {
            Utility.WriteError(message);
            return;
        }

        Utility.WriteSuccess(message);
        Console.WriteLine($"New balance: {account.Balance:C}");
    }

    private static void ShowMiniStatement(Account account)
    {
        Utility.WriteHeader("Mini Statement");
        IReadOnlyList<TransactionRecord> transactions = BankingService.GetRecentTransactions(account);

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
}
