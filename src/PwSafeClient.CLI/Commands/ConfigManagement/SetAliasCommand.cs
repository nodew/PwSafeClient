using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Options;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public class SetAliasCommand : Command
{
    public SetAliasCommand() : base("set", "Set alias for your psafe3 files")
    {
        AddOption(CommonOptions.AliasOption());
        AddOption(CommonOptions.FileOption());
        AddOption(new Option<bool>(new[] { "--default" }, "Whether to set the database as default"));
    }

    public class SetAliasCommandHandler : CommandHandler
    {
        private readonly IConfigManager configManager;
        private readonly IConsoleService consoleService;
        public SetAliasCommandHandler(IConfigManager configManager, IConsoleService consoleService)
        {
            this.configManager = configManager;
            this.consoleService = consoleService;
        }

        public string Alias { get; set; } = null!;

        public FileInfo File { get; set; } = null!;

        public bool Default { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            if (string.IsNullOrEmpty(Alias))
            {
                consoleService.LogError("The alias is required.");
                return 1;
            }

            if (!File.Exists)
            {

                consoleService.LogError($"The file '{File.FullName}' does not exist.");
                return 1;
            }

            try
            {
                await configManager.AddDatabaseAsync(Alias, File.FullName, Default);
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
