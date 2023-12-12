using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Medo.Security.Cryptography.PasswordSafe;
using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;

namespace PwSafeClient.CLI.Commands;

public class UnlockCommand : Command
{
    public UnlockCommand()
        : base("unlock", "Unlock a database")
    {
        AddOption(new Option<string>(
            aliases: ["--alias", "-a"],
            description: "The alias of the database"
        ));

        AddOption(new Option<FileInfo>(
            aliases: ["--file", "-f"],
            description: "The file path of your database file"
        ));

        AddOption(new Option<bool>(
            name: "--readonly",
            description: "Open database in read-only mode",
            getDefaultValue: () => true
        ));
    }

    public class UnlockCommandHandler : CommandHandler {
        private IDocumentHelper documentHelper;
        private IConsoleService consoleService;

        public UnlockCommandHandler(IDocumentHelper documentHelper, IConsoleService consoleService)
        {
            this.documentHelper = documentHelper;
            this.consoleService = consoleService;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public bool ReadOnly { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            Document? doc = await documentHelper.TryLoadDocumentAsync(Alias, File, ReadOnly);

            if (doc == null)
            {
                return 1;
            }

            Timer timer = new(_ => LockDatabase(), null, (int)TimeSpan.FromMinutes(1).TotalMilliseconds, Timeout.Infinite);
            Console.CancelKeyPress += (_, _) => HandleCancelEvent();

            string input;

            do
            {
                input = consoleService.ReadLine();
                if (input == "exit")
                {
                    break;
                }

                string[] args = input.Split(" ");
                if (args.Length == 0)
                {
                    continue;
                }

                await Program.Parser!.InvokeAsync(args);

                timer.Change((int)TimeSpan.FromMinutes(1).TotalMilliseconds, Timeout.Infinite);
            } while (input != "exit");

            return 0;
        }

        private static void LockDatabase() {
            Console.WriteLine("Locking database...");
            Process.GetCurrentProcess().Kill();
        }

        private static void HandleCancelEvent() {
            Console.WriteLine("Exiting...");
            Process.GetCurrentProcess().Kill();
        }
    }
}
