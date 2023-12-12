using PwSafeClient.CLI.Contracts.Services;
using System;
using System.Collections.Generic;

namespace PwSafeClient.CLI.Services;

/// <summary>
/// Implement <see cref="IConsoleService"/>.
/// </summary>
public class ConsoleService : IConsoleService
{
    /// <inheritdoc/>
    public string ReadPassword()
    {
        List<char> password = [];

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

    public string ReadLine() => ReadLine(">");

    public string ReadLine(string symbol = ">")
    {
        Console.Write($"{symbol} ");
        string? input = Console.ReadLine();
        return input ?? string.Empty;
    }

    /// <inheritdoc/>
    public void LogError(string errorMessage)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errorMessage);
        Console.ResetColor();
    }

    /// <inheritdoc/>
    public void LogSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
