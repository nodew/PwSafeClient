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
            Command command = new Command("showdb", "Show the detail of PasswordSafe database");
            command.AddOption(new Option(new string[] { "--alias", "-a" }, "Alias to the database, config at $HOME/.pwsafe")
            {
                Argument = new Argument("ALIAS")
            });
            command.AddOption(new Option(new string[] { "--file", "-f" }, "Path to your PasswordSafe file")
            {
                Argument = new Argument("FILE")
            });
            command.AddOption(new Option(new string[] { "--password", "-p" }, "Password for current database") {
                Argument = new Argument("PASSWORD")
            });

            command.Handler = CommandHandler.Create<string, string, string, IConsole>(HandleShowCommand);

            rootCommand.AddCommand(command);
            return rootCommand;
        }

        private static async Task HandleShowCommand(string ALIAS, string FILE, string PASSWORD, IConsole console)
        {
            string filepath = await ConsoleHelper.GetPwsFilePath(FILE, ALIAS);

            if (!File.Exists(filepath)) {
                System.Console.Error.WriteLine($"Can't locate a valid file, please check your command parameters or configuration in ~/pwsafe.json");
                return;
            }

            SecureString secureString = ConsoleHelper.GetSecureString(PASSWORD);

            try
            {
                using FileStream stream = File.OpenRead(filepath);
                using MemoryStream ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                PwsFileV3 pwsFile = (PwsFileV3)await PwsFile.OpenAsync(ms, secureString);

                string polices = "";
                pwsFile.PasswordPolicies.Keys.ToList().ForEach((string key) =>
                {
                    polices += $"{key}, ";
                });

                if (polices.Length >= 2) {
                    polices = polices[..(polices.Length - 2)];
                }

                List<ItemData> items = await PwsFileHelper.ReadAllItemsFromPwsFileV3Async(pwsFile);
                Group group = Group.GroupItems(items);

                string format = "{0, -20}{1}";
                System.Console.WriteLine(format, "Database format", pwsFile.Header.Version);
                System.Console.WriteLine(format, "Database UUID:", pwsFile.Header.Uuid);
                System.Console.WriteLine(format, "Name:", pwsFile.Header.DbName ?? "-");
                System.Console.WriteLine(format, "Description:", pwsFile.Header.DbDescription ?? "-");
                System.Console.WriteLine(format, "Version", pwsFile.Header.Version);
                System.Console.WriteLine(format, "Last saved by:", pwsFile.Header.LastSavedBy);
                System.Console.WriteLine(format, "Last saved on:", pwsFile.Header.LastSavedOn);
                System.Console.WriteLine(format, "Has policies: ", polices.Length > 0 ? polices : "None" );
                System.Console.WriteLine(format, "Group count: ", group.GetAllNestedGroups().ToList().Count);
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
