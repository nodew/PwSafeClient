using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Shared;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class NewEntryCommand : AsyncCommand<NewEntryCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [Description("Title of the entry")]
        [CommandOption("-t|--title <TITLE>")]
        public required string Title { get; init; }

        [Description("Username for the entry")]
        [CommandOption("-u|--username <USERNAME>")]
        public string? Username { get; init; }

        [Description("Password for the entry (will be generated if not provided)")]
        [CommandOption("-p|--password <PASSWORD>")]
        public string? Password { get; init; }

        [Description("URL associated with the entry")]
        [CommandOption("--url <URL>")]
        public string? Url { get; init; }

        [Description("Password policy for the entry")]
        [CommandOption("--policy <POLICY>")]
        public string? Policy { get; init; }

        [Description("Notes for the entry")]
        [CommandOption("-n|--notes <NOTES>")]
        public string? Notes { get; init; }

        [Description("Minimum password length")]
        [CommandOption("--min-length <LENGTH>")]
        [DefaultValue(8)]
        public int MinLength { get; init; }

        [Description("Maximum password length")]
        [CommandOption("--max-length <LENGTH>")]
        [DefaultValue(16)]
        public int MaxLength { get; init; }

        [Description("Include lowercase letters")]
        [CommandOption("--lowercase")]
        [DefaultValue(true)]
        public bool IncludeLowercase { get; init; }

        [Description("Include uppercase letters")]
        [CommandOption("--uppercase")]
        [DefaultValue(true)]
        public bool IncludeUppercase { get; init; }

        [Description("Include digits")]
        [CommandOption("--digits")]
        [DefaultValue(true)]
        public bool IncludeDigits { get; init; }

        [Description("Include special characters")]
        [CommandOption("--special")]
        [DefaultValue(true)]
        public bool IncludeSpecial { get; init; }

        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-f|--file <PATH>")]
        public string? FilePath { get; init; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                return ValidationResult.Error("Title is required");
            }

            if (FilePath != null && !File.Exists(FilePath))
            {
                return ValidationResult.Error("Database file does not exist");
            }

            return ValidationResult.Success();
        }
    }

    private readonly IDocumentService _documentService;

    public NewEntryCommand(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var document = await _documentService.TryLoadDocumentAsync(settings.Alias, settings.FilePath, false);

            if (document == null)
            {
                AnsiConsole.MarkupLine("[red]Failed to load document.[/]");
                return 1;
            }

            var entry = new Entry
            {
                Title = settings.Title,
                UserName = settings.Username ?? string.Empty,
                Url = settings.Url ?? string.Empty,
                Notes = settings.Notes ?? string.Empty,
                CreationTime = DateTime.Now,
                LastModificationTime = DateTime.Now,
            };

            if (string.IsNullOrWhiteSpace(settings.Password))
            {
                NamedPasswordPolicy? namedPasswordPolicy = null;

                if (!string.IsNullOrEmpty(settings.Policy))
                {
                    namedPasswordPolicy = document.NamedPasswordPolicies.FirstOrDefault(p => p.Name == settings.Policy);
                    if (namedPasswordPolicy == null)
                    {
                        AnsiConsole.MarkupLine($"[red]Password policy '{settings.Policy}' not found.[/]");
                        return 1;
                    }
                }

                if (namedPasswordPolicy != null)
                {
                    entry.PasswordPolicy.TotalPasswordLength = namedPasswordPolicy.TotalPasswordLength;
                    entry.PasswordPolicy.MinimumLowercaseCount = namedPasswordPolicy.MinimumLowercaseCount;
                    entry.PasswordPolicy.MinimumUppercaseCount = namedPasswordPolicy.MinimumUppercaseCount;
                    entry.PasswordPolicy.MinimumDigitCount = namedPasswordPolicy.MinimumDigitCount;
                    entry.PasswordPolicy.MinimumSymbolCount = namedPasswordPolicy.MinimumSymbolCount;
                    entry.PasswordPolicy.Style = namedPasswordPolicy.Style;
                    entry.PasswordPolicy.SetSpecialSymbolSet(namedPasswordPolicy.GetSpecialSymbolSet());

                    entry.PasswordPolicyName = namedPasswordPolicy.Name;
                    var newPassword = new PasswordGenerator(entry.PasswordPolicy).GeneratePassword();

                    entry.Password = newPassword;
                }
            }
            else
            {
                entry.Password = settings.Password;
            }

            document.Entries.Add(entry);

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
            table.AddRow("Created", entry.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));

            AnsiConsole.MarkupLine("[green]Entry created successfully:[/]");
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
