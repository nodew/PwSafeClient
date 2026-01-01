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

        [Description("Group path for the entry")]
        [CommandOption("-g|--group <GROUP>")]
        public string? Group { get; init; }

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

        [Description("Read database password from stdin")]
        [CommandOption("--password-stdin")]
        public bool PasswordStdin { get; init; }

        [Description("Read database password from environment variable")]
        [CommandOption("--password-env <VAR>")]
        public string? PasswordEnv { get; init; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                return ValidationResult.Error("Title is required");
            }

            if (MinLength < 6)
            {
                return ValidationResult.Error("--min-length must be at least 6");
            }

            if (MaxLength < MinLength)
            {
                return ValidationResult.Error("--max-length must be greater than or equal to --min-length");
            }

            if (string.IsNullOrWhiteSpace(Password) && string.IsNullOrWhiteSpace(Policy))
            {
                var enabledCategories = 0;
                if (IncludeLowercase) enabledCategories++;
                if (IncludeUppercase) enabledCategories++;
                if (IncludeDigits) enabledCategories++;
                if (IncludeSpecial) enabledCategories++;

                if (enabledCategories == 0)
                {
                    return ValidationResult.Error("At least one of --lowercase/--uppercase/--digits/--special must be enabled when generating a password");
                }

                if (MinLength < enabledCategories)
                {
                    return ValidationResult.Error($"--min-length must be >= number of enabled character categories ({enabledCategories})");
                }
            }

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

    public NewEntryCommand(IDocumentService documentService)
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

            var entry = new Entry
            {
                Title = settings.Title,
                UserName = settings.Username ?? string.Empty,
                Group = settings.Group ?? string.Empty,
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
                else
                {
                    var passwordPolicy = BuildPasswordPolicy(settings);
                    var newPassword = new PasswordGenerator(passwordPolicy).GeneratePassword();

                    if (string.IsNullOrWhiteSpace(newPassword))
                    {
                        AnsiConsole.MarkupLine("[red]Failed to generate password with the provided options.[/]");
                        return 1;
                    }

                    entry.PasswordPolicy.TotalPasswordLength = passwordPolicy.TotalPasswordLength;
                    entry.PasswordPolicy.MinimumLowercaseCount = passwordPolicy.MinimumLowercaseCount;
                    entry.PasswordPolicy.MinimumUppercaseCount = passwordPolicy.MinimumUppercaseCount;
                    entry.PasswordPolicy.MinimumDigitCount = passwordPolicy.MinimumDigitCount;
                    entry.PasswordPolicy.MinimumSymbolCount = passwordPolicy.MinimumSymbolCount;
                    entry.PasswordPolicy.Style = passwordPolicy.Style;
                    entry.PasswordPolicy.SetSpecialSymbolSet(passwordPolicy.GetSpecialSymbolSet());

                    entry.PasswordPolicyName = string.Empty;
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
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }

    private static PasswordPolicy BuildPasswordPolicy(Settings settings)
    {
        var min = settings.MinLength;
        var max = settings.MaxLength;
        var length = min == max ? min : Random.Shared.Next(min, max + 1);

        PasswordPolicyStyle style = 0;
        if (settings.IncludeLowercase) style |= PasswordPolicyStyle.UseLowercase;
        if (settings.IncludeUppercase) style |= PasswordPolicyStyle.UseUppercase;
        if (settings.IncludeDigits) style |= PasswordPolicyStyle.UseDigits;
        if (settings.IncludeSpecial) style |= PasswordPolicyStyle.UseSymbols;

        var policy = new PasswordPolicy(length)
        {
            Style = style,
            MinimumLowercaseCount = settings.IncludeLowercase ? 1 : 0,
            MinimumUppercaseCount = settings.IncludeUppercase ? 1 : 0,
            MinimumDigitCount = settings.IncludeDigits ? 1 : 0,
            MinimumSymbolCount = settings.IncludeSpecial ? 1 : 0,
        };

        // Ensure the sum of minimums doesn't exceed total length.
        var totalMinimum = policy.MinimumLowercaseCount + policy.MinimumUppercaseCount + policy.MinimumDigitCount + policy.MinimumSymbolCount;
        while (totalMinimum > policy.TotalPasswordLength)
        {
            if (policy.MinimumSymbolCount > 0) policy.MinimumSymbolCount--;
            else if (policy.MinimumDigitCount > 0) policy.MinimumDigitCount--;
            else if (policy.MinimumUppercaseCount > 0) policy.MinimumUppercaseCount--;
            else if (policy.MinimumLowercaseCount > 0) policy.MinimumLowercaseCount--;

            totalMinimum = policy.MinimumLowercaseCount + policy.MinimumUppercaseCount + policy.MinimumDigitCount + policy.MinimumSymbolCount;
        }

        policy.SetSpecialSymbolSet(PwCharPool.StdSymbolChars);
        return policy;
    }
}
