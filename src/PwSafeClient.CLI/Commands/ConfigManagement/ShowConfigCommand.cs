using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class ShowConfigCommand : AsyncCommand
{
    private readonly IConfigManager _configManager;

    public ShowConfigCommand(IConfigManager configManager)
    {
        _configManager = configManager;
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, CancellationToken cancellationToken)
    {
        try
        {
            var config = await _configManager.LoadConfigurationAsync();

            var table = new Table();
            table.AddColumn("Key");
            table.AddColumn("Value");

            table.AddRow("Config file", _configManager.GetConfigFilePath());
            table.AddRow("Default database", config.DefaultDatabase ?? string.Empty);
            table.AddRow("Idle time (minutes)", config.IdleTime.ToString());
            table.AddRow("Max backup count", config.MaxBackupCount.ToString());
            table.AddRow("Database count", config.Databases.Count.ToString());

            AnsiConsole.Write(table);

            if (config.Databases.Count > 0)
            {
                AnsiConsole.WriteLine();

                var dbTable = new Table();
                dbTable.AddColumn("Alias");
                dbTable.AddColumn("Path");

                foreach (var kv in config.Databases)
                {
                    dbTable.AddRow(kv.Key, kv.Value);
                }

                AnsiConsole.Write(dbTable);
            }

            return 0;
        }
        catch (Exception ex)
        {
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }
}
