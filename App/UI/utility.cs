namespace ATMApp.UI;

public static class Utility
{
    public static void TryPrepareConsole(string title)
    {
        try
        {
            Console.Title = title;
        }
        catch (IOException)
        {
        }

        TryClear();
    }

    public static void TryClear()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
        }
    }

    public static void PressEnter()
    {
        Console.WriteLine();
        Console.Write("Press Enter to continue...");
        Console.ReadLine();
    }

    public static void WriteHeader(string title)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('=', 48));
        Console.WriteLine(title);
        Console.WriteLine(new string('=', 48));
        Console.ResetColor();
    }

    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static string ReadRequiredString(string prompt, bool isSecret = false)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = isSecret ? ReadSecretInput() : Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            WriteError("A value is required.");
        }
    }

    public static decimal ReadAmount(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (decimal.TryParse(input, out decimal amount) && amount > 0)
            {
                return decimal.Round(amount, 2);
            }

            WriteError("Enter a valid amount greater than zero.");
        }
    }

    public static int ReadMenuChoice(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int choice) && choice >= min && choice <= max)
            {
                return choice;
            }

            WriteError($"Choose a number between {min} and {max}.");
        }
    }

    public static string MaskCardNumber(string cardNumber)
    {
        if (cardNumber.Length < 4)
        {
            return cardNumber;
        }

        string visibleDigits = cardNumber[^4..];
        return $"**** **** **** {visibleDigits}";
    }

    private static string ReadSecretInput()
    {
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine() ?? string.Empty;
        }

        var characters = new List<char>();

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return new string(characters.ToArray());
            }

            if (key.Key == ConsoleKey.Backspace && characters.Count > 0)
            {
                characters.RemoveAt(characters.Count - 1);
                Console.Write("\b \b");
                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                characters.Add(key.KeyChar);
                Console.Write('*');
            }
        }
    }
}
