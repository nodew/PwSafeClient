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

        [Description("Name of the policy to use")]
        [CommandOption("-n|--name <NAME>")]
        [DefaultValue("")]
        public string Name { get; init; } = string.Empty;

        public override ValidationResult Validate()
        {
            if (FilePath != null && !File.Exists(FilePath))
            {
                return ValidationResult.Error("Database file does not exist");
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                return ValidationResult.Error("Policy name is required");
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
            var document = await _documentService.TryLoadDocumentAsync(settings.Alias, settings.FilePath, false);

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

            AnsiConsole.MarkupLine($"[green]{password}[/]");
            await TextCopy.ClipboardService.SetTextAsync(password);
            AnsiConsole.MarkupLine("[grey]Password copied to clipboard[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
}
