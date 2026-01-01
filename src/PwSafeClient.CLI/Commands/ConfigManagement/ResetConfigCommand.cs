using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class ResetConfigCommand : AsyncCommand
{
    private readonly IConfigManager _configManager;

    public ResetConfigCommand(IConfigManager configManager)
    {
        _configManager = configManager;
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context)
    {
        try
        {
            var configPath = _configManager.GetConfigFilePath();

            var overwrite = AnsiConsole.Confirm(
                $"This will reset your configuration at '{configPath}'. Continue?",
                defaultValue: false);

            if (!overwrite)
            {
                AnsiConsole.MarkupLine("[yellow]Canceled.[/]");
                return 0;
            }

            await _configManager.ResetConfigurationAsync();
            AnsiConsole.MarkupLine("[green]Configuration reset.[/]");
            return 0;
        }
        catch (Exception ex)
        {
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }
}
