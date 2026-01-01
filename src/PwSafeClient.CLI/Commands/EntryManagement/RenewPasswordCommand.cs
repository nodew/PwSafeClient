using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Models;
using PwSafeClient.Shared;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class RenewPasswordCommand : AsyncCommand<RenewPasswordCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [Description("The ID of the entry to update")]
        [CommandArgument(0, "<ID>")]
        public required Guid Id { get; init; }

        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-f|--file <PATH>")]
        public string? FilePath { get; init; }

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

    public RenewPasswordCommand(IDocumentService documentService)
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

            var generator = new PasswordGenerator(entry.PasswordPolicy);

            entry.Password = generator.GeneratePassword();

            entry.LastModificationTime = DateTime.Now;

            await _documentService.SaveDocumentAsync(document, settings.Alias, settings.FilePath);

            var table = new Table();
            table.AddColumn("Field");
            table.AddColumn("Value");

            table.AddRow("ID", entry.Uuid.ToString());
            table.AddRow("Title", entry.Title ?? string.Empty);
            table.AddRow("Password", "********");
            table.AddRow("Modified", entry.LastModificationTime.ToString("yyyy-MM-dd HH:mm:ss"));

            AnsiConsole.MarkupLine("[green]Password renewed successfully:[/]");
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
