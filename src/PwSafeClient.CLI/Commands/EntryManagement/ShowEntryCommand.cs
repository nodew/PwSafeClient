using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class ShowEntryCommand : AsyncCommand<ShowEntryCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [Description("The ID of the entry")]
        [CommandArgument(0, "<ID>")]
        public required Guid Id { get; init; }

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

    public ShowEntryCommand(IDocumentService documentService)
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

            var entry = document.Entries.FirstOrDefault(e => e.Uuid == settings.Id);

            if (entry == null)
            {
                AnsiConsole.MarkupLine($"[red]Entry with ID '{settings.Id}' not found.[/]");
                return 1;
            }

            var table = new Table();
            table.AddColumn("Field");
            table.AddColumn("Value");

            table.AddRow("ID", entry.Uuid.ToString());
            table.AddRow("Title", entry.Title ?? string.Empty);
            table.AddRow("Username", entry.UserName ?? string.Empty);
            table.AddRow("Password", "********");
            table.AddRow("URL", entry.Url ?? string.Empty);
            table.AddRow("Notes", entry.Notes ?? string.Empty);
            table.AddRow("Password Policy", entry.PasswordPolicyName ?? string.Empty);
            table.AddRow("Created", entry.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
            table.AddRow("Modified", entry.LastModificationTime.ToString("yyyy-MM-dd HH:mm:ss"));

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
