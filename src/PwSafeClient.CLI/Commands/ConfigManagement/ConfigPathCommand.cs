using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class ConfigPathCommand : AsyncCommand
{
    private readonly IConfigManager _configManager;

    public ConfigPathCommand(IConfigManager configManager)
    {
        _configManager = configManager;
    }

    public override Task<int> ExecuteAsync([NotNull] CommandContext context, CancellationToken cancellationToken)
    {
        try
        {
            AnsiConsole.WriteLine(_configManager.GetConfigFilePath());
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            CliError.WriteException(ex);
            return Task.FromResult(ExitCodes.Error);
        }
    }
}
