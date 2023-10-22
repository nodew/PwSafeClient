using PwSafeClient.CLI.Contracts.Helpers;
using System;
using System.Collections.Generic;

namespace PwSafeClient.CLI.Helpers;

/// <summary>
/// Implement <see cref="IConsoleHelper"/>.
/// </summary>
public class ConsoleHelper : IConsoleHelper
{
    /// <inheritdoc/>
    public string ReadPassword()
    {
        List<char> password = new() { };

        Console.Write("Enter your password: ");
        while (true)
        {
            ConsoleKeyInfo i = Console.ReadKey(true);
            if (i.Key == ConsoleKey.Enter)
            {
                break;
            }
            else if (i.Key == ConsoleKey.Backspace)
            {
                if (password.Count > 0)
                {
                    password.RemoveAt(password.Count - 1);
                    Console.Write("\b \b");
                }
            }
            else if (i.KeyChar != '\u0000')
            {
                password.Add(i.KeyChar);
                Console.Write("*");
            }
        }

        Console.WriteLine("");

        return string.Join("", password);
    }

    /// <inheritdoc/>
    public string ReadQA(string question)
    {
        Console.Write($"{question} ");
        string? answer = Console.ReadLine();
        return answer ?? string.Empty;
    }

    /// <inheritdoc/>
    public void LogError(string errorMessage)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errorMessage);
        Console.ResetColor();
    }
}
