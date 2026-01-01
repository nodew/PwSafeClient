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

internal sealed class RemovePolicyCommand : AsyncCommand<RemovePolicyCommand.Settings>
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

        [Description("Name of the password policy to remove")]
        [CommandArgument(0, "<NAME>")]
        public string Name { get; init; } = string.Empty;

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
                return ValidationResult.Error("Password policy name is required");
            }

            return ValidationResult.Success();
        }
    }

    private readonly IDocumentService _documentService;

    public RemovePolicyCommand(IDocumentService documentService)
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

            var document = await _documentService.TryLoadDocumentAsync(settings.Alias, settings.FilePath, false, passwordOptions);

            if (document == null)
            {
                AnsiConsole.MarkupLine("[red]Failed to load document.[/]");
                return 1;
            }

            var policy = document.NamedPasswordPolicies.FirstOrDefault(p => p.Name == settings.Name);
            if (policy == null)
            {
                AnsiConsole.MarkupLine($"[red]Password policy '{settings.Name}' not found[/]");
                return 1;
            }

            document.NamedPasswordPolicies.Remove(policy);
            await _documentService.SaveDocumentAsync(document, settings.Alias, settings.FilePath);

            AnsiConsole.MarkupLine($"[green]Password policy '{settings.Name}' was removed successfully[/]");
            return 0;
        }
        catch (Exception ex)
        {
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }
}
