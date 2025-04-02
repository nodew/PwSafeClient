using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class ListEntriesCommand : AsyncCommand<ListEntriesCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-p|--path <PATH>")]
        public string? FilePath { get; init; }

        public override ValidationResult Validate()
        {
            if (FilePath != null && !File.Exists(FilePath))
            {
                return ValidationResult.Error("Database file does not exist");
            }

            return ValidationResult.Success();
        }
    }

    private readonly IDocumentService _documentService;

    public ListEntriesCommand(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var document = await _documentService.TryLoadDocumentAsync(settings.Alias, settings.FilePath, true);

            if (document == null)
            {
                AnsiConsole.MarkupLine("[red]Failed to load document.[/]");
                return 1;
            }

            var table = new Table();
            table.AddColumn("ID");
            table.AddColumn("Title");
            table.AddColumn("Username");
            table.AddColumn("URL");
            table.AddColumn("Created");
            table.AddColumn("Modified");

            foreach (var entry in document.Entries)
            {
                table.AddRow(
                    entry.Uuid.ToString(),
                    entry.Title ?? string.Empty,
                    entry.UserName ?? string.Empty,
                    entry.Url ?? string.Empty,
                    entry.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    entry.LastModificationTime.ToString("yyyy-MM-dd HH:mm:ss")
                );
            }

            AnsiConsole.Write(table);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
}
