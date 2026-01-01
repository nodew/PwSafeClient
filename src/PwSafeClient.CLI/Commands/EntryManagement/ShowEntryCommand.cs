using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class ShowEntryCommand : AsyncCommand<ShowEntryCommand.Settings>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    internal sealed class Settings : CommandSettings
    {
        [Description("The ID of the entry")]
        [CommandArgument(0, "<ID>")]
        public required Guid Id { get; init; }

        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-f|--file|-p|--path <PATH>")]
        public string? FilePath { get; init; }

        [Description("Output results as JSON")]
        [CommandOption("--json")]
        public bool Json { get; init; }

        [Description("Suppress non-essential output")]
        [CommandOption("--quiet")]
        public bool Quiet { get; init; }

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

    public ShowEntryCommand(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var passwordOptions = new PasswordOptions
            {
                PasswordStdin = settings.PasswordStdin,
                PasswordEnvVar = settings.PasswordEnv,
            };

            var document = await _documentService.TryLoadDocumentAsync(settings.Alias, settings.FilePath, true, passwordOptions);

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

            if (settings.Json)
            {
                var result = new
                {
                    id = entry.Uuid,
                    group = entry.Group?.ToString() ?? string.Empty,
                    title = entry.Title ?? string.Empty,
                    username = entry.UserName ?? string.Empty,
                    url = entry.Url ?? string.Empty,
                    notes = entry.Notes ?? string.Empty,
                    passwordPolicy = entry.PasswordPolicyName ?? string.Empty,
                    created = entry.CreationTime,
                    modified = entry.LastModificationTime,
                    hasPassword = !string.IsNullOrEmpty(entry.Password),
                };

                Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
                return 0;
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
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }
}
