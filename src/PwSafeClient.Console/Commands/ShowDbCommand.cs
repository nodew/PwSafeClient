using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using PwSafeLib.Filesystem;
using PwSafeClient.Core;

namespace PwSafeClient.Console.Commands
{
    public static class ShowDbCommand
    {
        public static RootCommand AddShowDbCommand(this RootCommand rootCommand)
        {
            Command command = new Command("show", "show the detail of PasswordSafe database");
            command.AddArgument(new Argument("FILEPATH"));
            command.AddOption(new Option(new string[] { "--password", "-p" }, "The security password") { Argument = new Argument("PASSWORD") });

            command.Handler = CommandHandler.Create<string, string, IConsole>(HandleShowCommand);

            rootCommand.AddCommand(command);
            return rootCommand;
        }

        private static async void HandleShowCommand(string FILEPATH, string PASSWORD, IConsole console)
        {
            SecureString secureString;

            if (string.IsNullOrEmpty(FILEPATH) || !File.Exists(FILEPATH)) {
                System.Console.Error.WriteLine("File doesn't exist");
                return;
            }

            if (string.IsNullOrEmpty(PASSWORD))
            {
                secureString = ConsoleHelper.ReadPassword();
            }
            else
            {
                secureString = ConsoleHelper.GetSecureString(PASSWORD);
            }

            try
            {
                using FileStream stream = File.OpenRead(FILEPATH);
                using MemoryStream ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                PwsFileV3 pwsFile = new PwsFileV3(ms, secureString, FileMode.Open);
                await pwsFile.OpenAsync();
                string polices = "";
                pwsFile.PasswordPolicies.Keys.ToList().ForEach((string key) =>
                {
                    polices += $"{key}, ";
                });

                if (polices.Length >= 2) {
                    polices = polices.Substring(0, 2);
                }

                Group group = await Group.ReadFromPwsFileV3Async(pwsFile);

                string format = "{0, 20}{1}";
                System.Console.WriteLine(format, "Database format", pwsFile.Header.Version);
                System.Console.WriteLine(format, "Database UUID:", pwsFile.Header.Uuid);
                System.Console.WriteLine(format, "Name:", pwsFile.Header.DbName);
                System.Console.WriteLine(format, "Description:", pwsFile.Header.DbDescription);
                System.Console.WriteLine(format, "Version", pwsFile.Header.Version);
                System.Console.WriteLine(format, "Last saved by:", pwsFile.Header.LastSavedBy);
                System.Console.WriteLine(format, "Last saved on:", pwsFile.Header.LastSavedOn);
                System.Console.WriteLine(format, "Has policies: ", polices);
                System.Console.WriteLine(format, "Group count: ", group.GetGroupNames().Count - 2);
                System.Console.WriteLine(format, "Item count: ", group.ItemSize);

                pwsFile.Dispose();
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.Message);
            }

        }
    }
}
