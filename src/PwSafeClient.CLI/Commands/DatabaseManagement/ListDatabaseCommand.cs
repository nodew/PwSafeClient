using System;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal class ListDatabaseCommand : AsyncCommand
{
    private readonly IDatabaseManager _dbManager;

    public ListDatabaseCommand(IDatabaseManager databaseManager)
    {
        _dbManager = databaseManager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        try
        {
            var databases = await _dbManager.ListDatabasesAsync();

            if (databases.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No database found[/]");
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
            AnsiConsole.WriteException(e);
            return 1;
        }
    }
}
