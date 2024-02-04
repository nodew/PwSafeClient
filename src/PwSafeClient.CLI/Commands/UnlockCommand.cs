using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Options;

namespace PwSafeClient.CLI.Commands;

public class UnlockCommand : Command
{
    public UnlockCommand()
        : base("unlock", "Unlock a database")
    {
        AddOption(CommonOptions.AliasOption());

        AddOption(CommonOptions.FileOption());

        AddOption(CommonOptions.ReadOnlyOption());
    }

    public class UnlockCommandHandler : CommandHandler
    {
        private readonly IDocumentHelper documentHelper;
        private readonly IConsoleService consoleService;
        private readonly IConfigManager configManager;

        private static readonly string[] allowedCommands =
        [
            "get",
            "add",
            "list",
            "rm",
            "update",
            "renew",
            "showdb",
            "policy",
        ];

        private const string exitCommand = ":exit";

        public UnlockCommandHandler(IDocumentHelper documentHelper, IConsoleService consoleService, IConfigManager configManager)
        {
            this.documentHelper = documentHelper;
            this.consoleService = consoleService;
            this.configManager = configManager;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public bool ReadOnly { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            var doc = await documentHelper.TryLoadDocumentAsync(Alias, File, ReadOnly);

            if (doc == null)
            {
                return 1;
            }

            var idleTime = await configManager.GetIdleTimeAsync();
            var displayName = await documentHelper.GetDocumentDisplayNameAsync(Alias, File);

            Timer timer = new(_ => HandleExit(), null, (int)TimeSpan.FromMinutes(idleTime).TotalMilliseconds, Timeout.Infinite);
            Console.CancelKeyPress += (_, _) => HandleExit();

            string input;

            do
            {
                input = consoleService.ReadLine($"{displayName}>").Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (input == exitCommand)
                {
                    break;
                }

                var result = Program.Parser!.Parse(input);

                if (!allowedCommands.Contains(result.CommandResult.Command.Name))
                {
                    Console.WriteLine($"Unsupported operation: {input}");
                    continue;
                }

                await Program.Parser!.InvokeAsync(input);

                timer.Change((int)TimeSpan.FromMinutes(idleTime).TotalMilliseconds, Timeout.Infinite);
            } while (input != exitCommand);

            HandleExit();

            return 0;
        }

        private void HandleExit()
        {
            Console.WriteLine("Locking database and exiting...");
            documentHelper.SaveDocumentAsync(Alias, File);
            Process.GetCurrentProcess().Kill();
        }
    }
}
