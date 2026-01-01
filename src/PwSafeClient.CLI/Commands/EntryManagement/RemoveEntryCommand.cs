using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class RemoveEntryCommand : AsyncCommand<RemoveEntryCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [Description("The ID of the entry to remove")]
        [CommandArgument(0, "<ID>")]
        public required Guid Id { get; init; }

        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-f|--file|-p|--path <PATH>")]
        public string? FilePath { get; init; }

        [Description("Skip confirmation prompt")]
        [CommandOption("-y|--yes")]
        public bool Force { get; init; }

        [Description("Read database password from stdin")]
        [CommandOption("--password-stdin")]
        public bool PasswordStdin { get; init; }

        [Description("Read database password from environment variable")]
        [CommandOption("--password-env <VAR>")]
        public string? PasswordEnv { get; init; }

        public override ValidationResult Validate()
        {
            if (FilePath != null && !File.Exists(FilePath))
            {
                return ValidationResult.Error("Database file does not exist");
            }

            if (PasswordStdin && !string.IsNullOrWhiteSpace(PasswordEnv))
            {
                return ValidationResult.Error("Use only one of --password-stdin or --password-env");
            }

            return ValidationResult.Success();
        }
    }

    private readonly IDocumentService _documentService;

    public RemoveEntryCommand(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var passwordOptions = new PasswordOptions
            {
                PasswordStdin = settings.PasswordStdin,
                PasswordEnvVar = settings.PasswordEnv,
            };

            var document = await _documentService.TryLoadDocumentAsync(settings.Alias, settings.FilePath, false, passwordOptions);

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

            if (!settings.Force)
            {
                var table = new Table();
                table.AddColumn("Field");
                table.AddColumn("Value");

                table.AddRow("ID", entry.Uuid.ToString());
                table.AddRow("Title", entry.Title ?? string.Empty);
                table.AddRow("Username", entry.UserName ?? string.Empty);
                table.AddRow("URL", entry.Url ?? string.Empty);

                AnsiConsole.MarkupLine("[yellow]The following entry will be removed:[/]");
                AnsiConsole.Write(table);

                if (!AnsiConsole.Confirm("Do you want to continue?"))
                {
                    AnsiConsole.MarkupLine("[yellow]Operation cancelled.[/]");
                    return 0;
                }
            }

            document.Entries.Remove(entry);
            await _documentService.SaveDocumentAsync(document, settings.Alias, settings.FilePath);

            AnsiConsole.MarkupLine("[green]Entry removed successfully.[/]");
            return 0;
        }
        catch (Exception ex)
        {
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }
}
