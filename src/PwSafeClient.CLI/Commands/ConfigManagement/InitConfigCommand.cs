using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Models;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public class InitConfigCommand : Command
{
    public InitConfigCommand() : base("init", "Init your pwsafe config if it doesn't exist")
    {
    }

    public class InitConfigCommandHandler : CommandHandler
    {
        private readonly IConfigManager configManager;
        private readonly IConsoleService consoleService;

        public InitConfigCommandHandler(IConfigManager configManager, IConsoleService consoleService)
        {
            this.configManager = configManager;
            this.consoleService = consoleService;
        }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            if (configManager.ConfigExists())
            {
                consoleService.LogError("The pwsafe.json has already existed");
                return 1;
            }

            try {
                await configManager.SaveAsync(new Config());
                return 0;
            } catch (Exception e) {
                consoleService.LogError(e.Message);
                return 1;
            }
        }
    }
}
