using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Json;
using PwSafeClient.Cli.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class GetPasswordCommand : AsyncCommand<GetPasswordCommand.Settings>
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
        [CommandOption("-f|--file|-p|--path <PATH>")]
        public string? FilePath { get; init; }

        [Description("Read database password from stdin")]
        [CommandOption("--password-stdin")]
        public bool PasswordStdin { get; init; }

        [Description("Read database password from environment variable")]
        [CommandOption("--password-env <VAR>")]
        public string? PasswordEnv { get; init; }

        [Description("Output results as JSON")]
        [CommandOption("--json")]
        public bool Json { get; init; }

        [Description("Suppress non-essential output")]
        [CommandOption("--quiet")]
        public bool Quiet { get; init; }

        [Description("Print the password to stdout")]
        [CommandOption("--print")]
        public bool Print { get; init; }

        [Description("Do not copy the password to the clipboard")]
        [CommandOption("--no-clipboard")]
        public bool NoClipboard { get; init; }

        [Description("Clear the clipboard after N seconds (requires clipboard copy)")]
        [CommandOption("--clear-after <SECONDS>")]
        public int? ClearAfterSeconds { get; init; }

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

            if (Json && Print)
            {
                return ValidationResult.Error("Use only one of --json or --print");
            }

            if (NoClipboard && ClearAfterSeconds is not null)
            {
                return ValidationResult.Error("--clear-after requires clipboard copy (do not use with --no-clipboard)");
            }

            if (ClearAfterSeconds is not null && ClearAfterSeconds <= 0)
            {
                return ValidationResult.Error("--clear-after must be a positive number of seconds");
            }

            return ValidationResult.Success();
        }
    }

    private readonly IDocumentService _documentService;

    public GetPasswordCommand(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, System.Threading.CancellationToken cancellationToken)
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
                if (settings.Json)
                {
                    Console.WriteLine(JsonSerializer.Serialize(
                        new ErrorResponse("Failed to load document"),
                        CliJsonContext.Default.ErrorResponse));
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Failed to load document.[/]");
                }
                return 1;
            }

            var entry = document.Entries.FirstOrDefault(e => e.Uuid == settings.Id);

            if (entry == null)
            {
                if (settings.Json)
                {
                    Console.WriteLine(JsonSerializer.Serialize(
                        new ErrorResponse($"Entry with ID '{settings.Id}' not found"),
                        CliJsonContext.Default.ErrorResponse));
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Entry with ID '{settings.Id}' not found.[/]");
                }
                return 1;
            }

            var copiedToClipboard = false;

            if (!settings.NoClipboard)
            {
                await TextCopy.ClipboardService.SetTextAsync(entry.Password);
                copiedToClipboard = true;

                if (settings.ClearAfterSeconds is not null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(settings.ClearAfterSeconds.Value));
                    await TextCopy.ClipboardService.SetTextAsync(string.Empty);
                }
            }

            if (settings.Json)
            {
                Console.WriteLine(JsonSerializer.Serialize(
                    new GetPasswordResponse(entry.Uuid, copiedToClipboard),
                    CliJsonContext.Default.GetPasswordResponse));
                return 0;
            }

            if (settings.Print)
            {
                Console.WriteLine(entry.Password);
            }

            if (!settings.Quiet)
            {
                if (copiedToClipboard)
                {
                    AnsiConsole.MarkupLine("[green]Password copied to clipboard.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[grey]Clipboard copy disabled.[/]");
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            if (settings.Json)
            {
                Console.WriteLine(JsonSerializer.Serialize(
                    new ErrorResponse(ex.Message),
                    CliJsonContext.Default.ErrorResponse));
            }
            else
            {
                CliError.WriteException(ex);
            }
            return ExitCodes.Error;
        }
    }
}
