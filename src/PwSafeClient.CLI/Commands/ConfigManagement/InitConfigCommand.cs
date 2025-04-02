using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class InitConfigCommand : AsyncCommand
{

    private readonly IConfigManager _configManager;

    public InitConfigCommand(IConfigManager configManager)
    {
        _configManager = configManager;
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context)
    {
        try
        {
            await _configManager.InitConfigurationAsync();
            return 0;
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 1;
        }
    }
}
