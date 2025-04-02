using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Shared;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class UpdateEntryCommand : AsyncCommand<UpdateEntryCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [Description("The ID of the entry to update")]
        [CommandArgument(0, "<ID>")]
        public required Guid Id { get; init; }

        [Description("Title of the entry")]
        [CommandOption("-t|--title <TITLE>")]
        public string? Title { get; init; }

        [Description("Username for the entry")]
        [CommandOption("-u|--username <USERNAME>")]
        public string? Username { get; init; }

        [Description("Password for the entry")]
        [CommandOption("-p|--password <PASSWORD>")]
        public string? Password { get; init; }

        [Description("URL associated with the entry")]
        [CommandOption("--url <URL>")]
        public string? Url { get; init; }

        [Description("Notes for the entry")]
        [CommandOption("-n|--notes <NOTES>")]
        public string? Notes { get; init; }

        [Description("Password policy for the entry")]
        [CommandOption("--policy <POLICY>")]
        public string? Policy { get; init; }

        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-f|--file <PATH>")]
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

    public UpdateEntryCommand(IDocumentService documentService)
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

            if (settings.Password != null)
            {
                if (string.IsNullOrEmpty(settings.Password))
                {
                    var newPass = AnsiConsole.Prompt(
                        new TextPrompt<string>("Enter new password:")
                            .PromptStyle("green")
                            .Secret());

                    entry.Password = newPass;
                }
                else
                {
                    entry.Password = settings.Password;
                }
            }

            if (!string.IsNullOrEmpty(settings.Policy))
            {
                var namedPasswordPolicy = document.NamedPasswordPolicies.FirstOrDefault(p => p.Name == settings.Policy);
                if (namedPasswordPolicy == null)
                {
                    AnsiConsole.MarkupLine($"[red]Password policy '{settings.Policy}' not found.[/]");
                    return 1;
                }
                else
                {
                    entry.PasswordPolicy.TotalPasswordLength = namedPasswordPolicy.TotalPasswordLength;
                    entry.PasswordPolicy.MinimumLowercaseCount = namedPasswordPolicy.MinimumLowercaseCount;
                    entry.PasswordPolicy.MinimumUppercaseCount = namedPasswordPolicy.MinimumUppercaseCount;
                    entry.PasswordPolicy.MinimumDigitCount = namedPasswordPolicy.MinimumDigitCount;
                    entry.PasswordPolicy.MinimumSymbolCount = namedPasswordPolicy.MinimumSymbolCount;
                    entry.PasswordPolicy.Style = namedPasswordPolicy.Style;
                    entry.PasswordPolicy.SetSpecialSymbolSet(namedPasswordPolicy.GetSpecialSymbolSet());

                    entry.PasswordPolicyName = namedPasswordPolicy.Name;
                }
            }

            if (settings.Title != null)
            {
                entry.Title = settings.Title;
            }

            if (settings.Username != null)
            {
                entry.UserName = settings.Username;
            }

            if (settings.Url != null)
            {
                entry.Url = settings.Url;
            }

            if (settings.Notes != null)
            {
                entry.Notes = settings.Notes;
            }

            entry.LastModificationTime = DateTime.Now;

            await _documentService.SaveDocumentAsync(document, settings.Alias, settings.FilePath);

            var table = new Table();
            table.AddColumn("Field");
            table.AddColumn("Value");

            table.AddRow("ID", entry.Uuid.ToString());
            table.AddRow("Title", entry.Title ?? string.Empty);
            table.AddRow("Username", entry.UserName ?? string.Empty);
            table.AddRow("Password", "********");
            table.AddRow("URL", entry.Url ?? string.Empty);
            table.AddRow("Notes", entry.Notes ?? string.Empty);
            table.AddRow("Modified", entry.LastModificationTime.ToString("yyyy-MM-dd HH:mm:ss"));

            AnsiConsole.MarkupLine("[green]Entry updated successfully:[/]");
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
