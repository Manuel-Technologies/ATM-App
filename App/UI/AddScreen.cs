using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.CSharp;
using System.Text;

public static class AppScreen
{
    internal static async Task WelcomeSreen()
    {
         Console.Clear();
            string? title = "triemBank";
            Console.Title=title;
            Console.ForegroundColor=ConsoleColor.DarkYellow;
            Console.WriteLine(" ----------welcome to our ATM system ----------------");
            Console.WriteLine("\ninsert your ATM card number");
            // HttpClientHandler httpClientHandler = new HttpClientHandler();
            // httpClientHandler.Credentials = nrwj;
            
        
            
    }
}