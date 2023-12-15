using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using PwSafeClient.CLI.Contracts.Services;

namespace PwSafeClient.CLI.Commands;

public class ChooseDbCommand : Command
{
    public ChooseDbCommand() : base("choose", "Choose a database to operate")
    {
        AddArgument(new Argument<string>("ALIAS", "The alias of the database"));
    }

    public class ChooseDbCommandHandler : CommandHandler
    {
        private readonly IConfigManager configManager;
        private readonly IConsoleService consoleService;

        public ChooseDbCommandHandler(IConfigManager configManager, IConsoleService consoleService)
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
                await configManager.SetDefaultDatabaseAsync(Alias);
                return 0;
            }
            catch (Exception)
            {
                consoleService.LogError($"The alias '{Alias}' does not exist.");
                return 1;
            }
        }
    }
}
