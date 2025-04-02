using System;
using System.ComponentModel;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal class SetDatabaseCommand : AsyncCommand<SetDatabaseCommand.Settings>
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

    public SetDatabaseCommand(IDatabaseManager databaseManager)
    {
        _dbManager = databaseManager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            await _dbManager.SetDefaultDatabaseAsync(settings.Alias);
            AnsiConsole.MarkupLine($"[green]Database '{settings.Alias}' set as default successfully[/]");
            return 0;
        }
        catch (InvalidOperationException e)
        {
            AnsiConsole.MarkupLine($"[yellow]{e.Message}[/]");
            return 1;
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e, ExceptionFormats.NoStackTrace);
            return 1;
        }
    }
}
