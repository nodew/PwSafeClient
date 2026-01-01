using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class SetConfigCommand : AsyncCommand<SetConfigCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [Description("Set the default database alias")]
        [CommandOption("--default-db <ALIAS>")]
        public string? DefaultDatabase { get; init; }

        [Description("Set idle time in minutes")]
        [CommandOption("--idle-time <MINUTES>")]
        public int? IdleTime { get; init; }

        [Description("Set maximum number of backup files to keep")]
        [CommandOption("--max-backup-count <COUNT>")]
        public int? MaxBackupCount { get; init; }

        public override ValidationResult Validate()
        {
            if (DefaultDatabase == null && IdleTime == null && MaxBackupCount == null)
            {
                return ValidationResult.Error("At least one option must be provided: --default-db, --idle-time, --max-backup-count");
            }

            if (IdleTime is < 0)
            {
                return ValidationResult.Error("--idle-time must be >= 0");
            }

            if (MaxBackupCount is < 0)
            {
                return ValidationResult.Error("--max-backup-count must be >= 0");
            }

            return ValidationResult.Success();
        }
    }

    private readonly IConfigManager _configManager;

    public SetConfigCommand(IConfigManager configManager)
    {
        _configManager = configManager;
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, Settings settings)
    {
        try
        {
            var config = await _configManager.LoadConfigurationAsync();

            if (settings.DefaultDatabase != null)
            {
                if (!string.IsNullOrWhiteSpace(settings.DefaultDatabase) && !config.Databases.ContainsKey(settings.DefaultDatabase))
                {
                    AnsiConsole.MarkupLine($"[red]Database alias '{settings.DefaultDatabase}' not found in config. Add it first via 'pwsafe db add'.[/]");
                    return 1;
                }

                config.DefaultDatabase = string.IsNullOrWhiteSpace(settings.DefaultDatabase) ? null : settings.DefaultDatabase;
            }

            if (settings.IdleTime != null)
            {
                config.IdleTime = settings.IdleTime.Value;
            }

            if (settings.MaxBackupCount != null)
            {
                config.MaxBackupCount = settings.MaxBackupCount.Value;
            }

            await _configManager.SaveConfigurationAsync(config);
            AnsiConsole.MarkupLine("[green]Configuration updated.[/]");
            return 0;
        }
        catch (Exception ex)
        {
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }
}
