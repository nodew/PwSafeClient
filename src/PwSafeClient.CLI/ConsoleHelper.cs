using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PwSafeClient.CLI;

public static class ConsoleHelper
{
    private const string _configFileName = "pwsafe.json";

    public const string ApplicationName = "PasswordSafe CLI V1.0";

    public static string ReadPassword()
    {
        List<char> password = new List<char> { };
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

    public static string ReadString(string question)
    {
        System.Console.Write($"{question} ");
        string? answer = System.Console.ReadLine();
        return answer ?? string.Empty;
    }

    public static string? GetHomePath()
    {
        return (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            ? Environment.GetEnvironmentVariable("HOME")
            : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
    }

    public static async Task<string> GetPWSFilePathAsync(string? alias)
    {
        var config = await LoadConfigAsync();
        return ConfigManager.GetDbPath(config, alias ?? ConfigManager.DefaultAlias) ?? string.Empty;
    }

    public static string GetConfigPath()
    {
        string homeDir = GetHomePath() ?? string.Empty;
        return Path.Combine(homeDir, _configFileName);
    }

    public static async Task<Config> LoadConfigAsync()
    {
        string configPath = GetConfigPath();
        if (!File.Exists(configPath))
        {
            Config config = new Config();
            await ConfigManager.ToFileAsync(config, configPath);
            return config;
        }

        return await ConfigManager.FromFileAsync(configPath);
    }

    public static async Task UpdateConfigAsync(Config config)
    {
        string configPath = GetConfigPath();
        await ConfigManager.ToFileAsync(config, configPath);
    }

    public static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
