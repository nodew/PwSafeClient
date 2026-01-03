using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Json;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal class ListDatabaseCommand : AsyncCommand<ListDatabaseCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [Description("Output results as JSON")]
        [CommandOption("--json")]
        public bool Json { get; init; }

        [Description("Suppress non-essential output")]
        [CommandOption("--quiet")]
        public bool Quiet { get; init; }
    }

    private readonly IDatabaseManager _dbManager;

    public ListDatabaseCommand(IDatabaseManager databaseManager)
    {
        _dbManager = databaseManager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, System.Threading.CancellationToken cancellationToken)
    {
        try
        {
            var databases = await _dbManager.ListDatabasesAsync();

            if (databases.Count == 0)
            {
                if (settings.Json)
                {
                    Console.WriteLine("[]");
                    return 0;
                }

                if (!settings.Quiet)
                {
                    AnsiConsole.MarkupLine("[yellow]No database found[/]");
                }
                return 0;
            }

            if (settings.Json)
            {
                var results = databases
                    .Select(db => new DatabaseListItem(db.Alias, db.Path, db.IsDefault))
                    .ToList();

                Console.WriteLine(JsonSerializer.Serialize(results, CliJsonContext.Default.DatabaseList));
                return 0;
            }

            var table = new Table();
            table.AddColumn("Alias");
            table.AddColumn("Path");
            table.AddColumn("Default");

            foreach (var db in databases)
            {
                table.AddRow(db.Alias, db.Path, db.IsDefault ? ":check_mark:" : "");
            }

            AnsiConsole.Write(table);

            return 0;
        }
        catch (Exception e)
        {
            CliError.WriteException(e);
            return ExitCodes.Error;
        }
    }
}
