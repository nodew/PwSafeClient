using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

using PwSafeClient.CLI.Contracts.Services;

namespace PwSafeClient.CLI.Commands;

public class ListDbCommand : Command
{
    public ListDbCommand() : base("listdb", "List all databases")
    {
    }

    public class ListDbCommandHandler : CommandHandler
    {
        private readonly IConfigManager configManager;
        private readonly IConsoleService consoleService;

        public ListDbCommandHandler(IConfigManager configManager, IConsoleService consoleService)
        {
            this.configManager = configManager;
            this.consoleService = consoleService;
        }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            try
            {
                var config = await configManager.LoadConfigAsync();

                if (config.Databases.Count == 0)
                {
                    Console.WriteLine("No database configured, you can use 'createdb' command to create a new one.");
                    return 0;
                }

                foreach (var db in config.Databases)
                {
                    Console.WriteLine($"{db.Key}: {db.Value}");
                }

                Console.WriteLine(string.Format("Default database: {0}", config.DefaultDatabase ?? "Unknown"));
                return 0;
            }
            catch (Exception e)
            {
                consoleService.LogError(e.Message);
                return 1;
            }
        }
    }
}
