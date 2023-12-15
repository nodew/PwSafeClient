using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

using PwSafeClient.CLI.Contracts.Services;

namespace PwSafeClient.CLI.Commands;

public class RemoveAliasCommand : Command
{
    public RemoveAliasCommand() : base("rm", "Remove database from the config file")
    {
        AddArgument(new Argument<string>("ALIAS", "The alias of database"));
    }

    public class RemoveAliasCommandHandler : CommandHandler
    {
        private readonly IConfigManager configManager;
        private readonly IConsoleService consoleService;

        public RemoveAliasCommandHandler(IConfigManager configManager, IConsoleService consoleService)
        {
            this.configManager = configManager;
            this.consoleService = consoleService;
        }

        public string Alias { get; set; } = null!;

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            if (string.IsNullOrEmpty(Alias))
            {
                consoleService.LogError("The alias is required.");
                return 1;
            }

            try
            {
                await configManager.RemoveDatabaseAsync(Alias);
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
