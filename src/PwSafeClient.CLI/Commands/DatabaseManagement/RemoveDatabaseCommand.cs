using System;
using System.ComponentModel;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal class RemoveDatabaseCommand : AsyncCommand<RemoveDatabaseCommand.Settings>
{
    internal class Settings : CommandSettings
    {
        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public required string Alias { get; init; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Alias))
            {
                return ValidationResult.Error("Alias is required");
            }

            return base.Validate();
        }
    }

    private readonly IDatabaseManager _dbManager;

    public RemoveDatabaseCommand(IDatabaseManager databaseManager)
    {
        _dbManager = databaseManager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            await _dbManager.RemoveDatabaseAsync(settings.Alias);
            AnsiConsole.MarkupLine($"[green]Database '{settings.Alias}' removed successfully[/]");
            return 0;
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 1;
        }
    }
}
