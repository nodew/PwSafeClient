using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace PwSafeClient.Console
{
    public static class ConsoleHelper
    {
        private const string _configFileName = "pwsafe.json";

        public static SecureString ReadPassword()
        {
            var pwd = new SecureString();
            System.Console.Write("Enter your password: ");
            while (true)
            {
                ConsoleKeyInfo i = System.Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        System.Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000')
                {
                    pwd.AppendChar(i.KeyChar);
                    System.Console.Write("*");
                }
            }
            System.Console.WriteLine("");

            return pwd;
        }

        public static string ReadString(string question) {
            System.Console.Write($"{question} ");
            string? answer = System.Console.ReadLine();
            return answer ?? string.Empty;
        }

        public static SecureString GetSecureString(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return ReadPassword();
            }

            SecureString secureString = new SecureString();
            password.ToList().ForEach((char c) =>
            {
                secureString.AppendChar(c);
            });
            return secureString;
        }

        public static string? GetHomePath()
        {
            return (Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }

        public static async Task<string> GetPWSFilePath(string alias = ConfigManager.DefaultAlias)
        {
            var config = await LoadConfigAsync();
            return ConfigManager.GetDbPath(config, alias) ?? string.Empty;
        }

        public static async Task<Config> LoadConfigAsync()
        {
            string homeDir = GetHomePath() ?? string.Empty;
            string configPath = Path.Combine(homeDir, _configFileName);
            if (!File.Exists(configPath))
            {
                Config config = new Config();
                await ConfigManager.ToFile(config, configPath);
                return config;
            }

            return await ConfigManager.FromFile(configPath);
        }
    }
}
