using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Models;
using PwSafeClient.Shared;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class GeneratePasswordCommand : AsyncCommand<GeneratePasswordCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
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

        [Description("Name of the policy to use")]
        [CommandOption("-n|--name <NAME>")]
        [DefaultValue("")]
        public string Name { get; init; } = string.Empty;

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

            if (string.IsNullOrWhiteSpace(Name))
            {
                return ValidationResult.Error("Policy name is required");
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

    public GeneratePasswordCommand(IDocumentService documentService)
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

            var namedPolicy = document.NamedPasswordPolicies.FirstOrDefault(p => p.Name == settings.Name);
            if (namedPolicy == null)
            {
                AnsiConsole.MarkupLine($"[red]Policy '{settings.Name}' not found[/]");
                return 1;
            }

            var policy = new PasswordPolicy(namedPolicy.TotalPasswordLength)
            {
                Style = namedPolicy.Style,
                MinimumLowercaseCount = namedPolicy.MinimumLowercaseCount,
                MinimumUppercaseCount = namedPolicy.MinimumUppercaseCount,
                MinimumDigitCount = namedPolicy.MinimumDigitCount,
                MinimumSymbolCount = namedPolicy.MinimumSymbolCount
            };

            policy.SetSpecialSymbolSet(namedPolicy.GetSpecialSymbolSet());

            var generator = new PasswordGenerator(policy);
            var password = generator.GeneratePassword();

            if (string.IsNullOrEmpty(password))
            {
                AnsiConsole.MarkupLine("[red]Failed to generate a password[/]");
                return 1;
            }

            if (settings.Print)
            {
                Console.WriteLine(password);
            }
            else
            {
                AnsiConsole.MarkupLine($"[green]{password}[/]");
            }

            if (!settings.NoClipboard)
            {
                await TextCopy.ClipboardService.SetTextAsync(password);
                AnsiConsole.MarkupLine("[grey]Password copied to clipboard[/]");

                if (settings.ClearAfterSeconds is not null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(settings.ClearAfterSeconds.Value));
                    await TextCopy.ClipboardService.SetTextAsync(string.Empty);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }
}
