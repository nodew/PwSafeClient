using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using PwSafeClient.Console.Commands;
using PwSafeClient.Core;
using PwSafeLib.Filesystem;

namespace PwSafeClient.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            RootCommand rootCommand = new RootCommand()
            {
                Description = "The best PasswordSafe CLI"
            };

            rootCommand
                .AddCreateDbCommand()
                .AddShowDbCommand();

            rootCommand.AddOption(new Option(new string[] { "--alias", "-a" }, "Alias to the database, config at $HOME/.pwsafe")
            {
                Argument = new Argument("ALIAS")
            });

            rootCommand.AddOption(new Option(new string[] { "--file", "-f" }, "Path to your PasswordSafe file")
            {
                Argument = new Argument("FILE")
            });

            rootCommand.AddOption(new Option(new string[] { "--title", "-t" }, "Title of your password")
            {
                Argument = new Argument("Title")
            });

            rootCommand.AddOption(new Option(new string[] { "--password", "-p" }, "Password of your PasswordSafe file")
            {
                Argument = new Argument("PASSWORD")
            });

            rootCommand.Handler = CommandHandler.Create<string, string, string, string, IConsole>(HandleRootCommand);

            rootCommand.Invoke(args);
        }

        private static async Task HandleRootCommand(string ALIAS, string FILE, string TITLE, string PASSWORD, IConsole console)
        {
            string filepath;
            if (!string.IsNullOrEmpty(FILE))
            {
                filepath = FILE;
            }
            else
            {
                filepath = await ConsoleHelper.GetPWSFilePath(ALIAS ?? ConfigManager.DefaultAlias);
            }

            if (!File.Exists(filepath))
            {
                System.Console.Error.WriteLine($"Can't locate a valid file, please check your parameters or configuration");
                return;
            }

            if (string.IsNullOrWhiteSpace(TITLE))
            {
                TITLE = ConsoleHelper.ReadString("Title:");
                if (string.IsNullOrWhiteSpace(TITLE)) {
                    System.Console.Error.WriteLine($"Title is required");
                    return;
                }
            }

            SecureString secureString = ConsoleHelper.GetSecureString(PASSWORD);

            try
            {
                using FileStream stream = File.OpenRead(filepath);
                using MemoryStream ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                PwsFileV3 pwsFile = (PwsFileV3)await PwsFile.OpenAsync(ms, secureString);
                List<ItemData> items = await PwsFileHelper.ReadAllItemsFromPwsFileV3Async(pwsFile);

                ItemData? item = items.Where(item => item.Title == TITLE).FirstOrDefault();
                if (item is null)
                {
                    System.Console.Error.WriteLine($"Can't find the password of {TITLE}");
                    return;
                }

                TextCopy.ClipboardService.SetText(item.Password);
                System.Console.WriteLine("Successfuly copy password to clipboard");
                pwsFile.Dispose();
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine("Unexpected error: {0}", e.Message);
                throw;
            }
        }
    }
}
