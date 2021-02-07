using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PwSafeClient.Console
{
    public static class ConsoleHelper
    {
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

        public static string GetPWSFilePath(string alias = "Default")
        {
            string homeDir = GetHomePath() ?? string.Empty;
            string configPath = Path.Combine(homeDir, ".pwsafe");
            if (!File.Exists(configPath))
            {
                return string.Empty;
            }
            string[] rows = File.ReadAllLines(configPath);
            foreach (string row in rows)
            {
                string _row = row.Trim();
                string[] config = _row.Split(' ');
                if (config.Length == 2)
                {
                    string _alias = config[0].Trim();
                    string filepath = _row[(_alias.Length)..].Trim();

                    if (alias == _alias)
                    {
                        return filepath;
                    }
                }
            }
            return string.Empty;
        }
    }
}
