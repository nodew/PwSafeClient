using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal class ShowDatabaseCommand : AsyncCommand<ShowDatabaseCommand.Settings>
{
    internal class Settings : CommandSettings
    {
        [Description("Alias of the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-f|--file <FILE_PATH>")]
        public string? FilePath { get; init; }
    }

    private readonly IDatabaseManager _dbManager;

    public ShowDatabaseCommand(IDatabaseManager databaseManager)
    {
        _dbManager = databaseManager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Alias) && string.IsNullOrWhiteSpace(settings.FilePath))
        {
            AnsiConsole.MarkupLine("[red]Either alias or file path is required[/]");
            return 1;
        }

        var filepath = settings.FilePath;

        if (!string.IsNullOrWhiteSpace(settings.Alias))
        {
            filepath = await _dbManager.GetDbPathByAliasAsync(settings.Alias);
            if (filepath is null)
            {
                AnsiConsole.MarkupLine($"[yellow]Database with alias '{settings.Alias}' not found[/]");
                return 1;
            }
        }

        if (!File.Exists(filepath))
        {
            AnsiConsole.MarkupLine($"[red]Database file not found at '{filepath}'[/]");
            return 1;
        }

        var password = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter password:")
                .PromptStyle("green")
                .Secret());

        try
        {
            var document = Document.Load(filepath, password);

            var grid = new Grid();

            grid.AddColumn();
            grid.AddColumn();

            grid.AddRow("Database UUID:", document.Uuid.ToString());
            grid.AddRow("Name:", document.Name ?? "-");
            grid.AddRow("Description:", document.Description ?? "-");
            grid.AddRow("Version:", document.Version.ToString());
            grid.AddRow("Last saved by:", document.LastSaveUser);
            grid.AddRow("Last saved on:", document.LastSaveTime.ToString());
            grid.AddRow("Last saved application:", document.LastSaveApplication);
            grid.AddRow("Last saved machine:", document.LastSaveHost);
            grid.AddRow("Item count:", document.Entries.Count.ToString());

            AnsiConsole.Write(grid);

            return 0;
        }
        catch (Exception)
        {
            AnsiConsole.MarkupLine("[red]Invalid password or database file[/]");
            return 1;
        }

    }
}
