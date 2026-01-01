using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal class AddDatabaseCommand : AsyncCommand<AddDatabaseCommand.Settings>
{
    internal class Settings : CommandSettings
    {
        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public required string Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-f|--file <PATH>")]
        public required string DatabasePath { get; init; }

        [Description("Set the database as default")]
        [CommandOption("-d|--default")]
        public bool IsDefault { get; init; }

        [Description("Force to update the alias if it already exists")]
        [CommandOption("--force")]
        public bool ForceUpdate { get; init; }

        [Description("Allow adding a database path that does not exist yet")]
        [CommandOption("--allow-missing")]
        public bool AllowMissing { get; init; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Alias))
            {
                return ValidationResult.Error("Alias is required");
            }

            if (string.IsNullOrWhiteSpace(DatabasePath))
            {
                return ValidationResult.Error("Database path is required");
            }

            if (!AllowMissing && !File.Exists(DatabasePath))
            {
                return ValidationResult.Error("Database file does not exist");
            }

            return base.Validate();
        }
    }

    private readonly IDatabaseManager _dbManager;

    public AddDatabaseCommand(IDatabaseManager databaseManager)
    {
        _dbManager = databaseManager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var dbs = await _dbManager.ListDatabasesAsync();

            if (dbs.Exists(x => x.Alias == settings.Alias))
            {
                if (!settings.ForceUpdate)
                {
                    AnsiConsole.MarkupLine($"[red]Database with alias '{settings.Alias}' already exists.[/]");
                    return 1;
                }
            }

            var fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(settings.DatabasePath));
            await _dbManager.AddDatabaseAsync(settings.Alias, fullPath, settings.IsDefault, forceUpdate: settings.ForceUpdate);

            return 0;
        }
        catch (Exception e)
        {
            CliError.WriteException(e);
            return ExitCodes.Error;
        }
    }
}
