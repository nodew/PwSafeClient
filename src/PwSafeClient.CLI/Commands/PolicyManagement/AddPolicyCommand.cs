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

internal sealed class AddPolicyCommand : AsyncCommand<AddPolicyCommand.Settings>
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

        [Description("Name of the password policy")]
        [CommandOption("-n|--name <NAME>")]
        [DefaultValue("")]
        public required string Name { get; init; }

        [Description("Minimum number of digits")]
        [CommandOption("-d|--digits <COUNT>")]
        [DefaultValue(0)]
        public int Digits { get; init; }

        [Description("Minimum number of uppercase letters")]
        [CommandOption("-u|--uppercase <COUNT>")]
        [DefaultValue(0)]
        public int Uppercase { get; init; }

        [Description("Minimum number of lowercase letters")]
        [CommandOption("-l|--lowercase <COUNT>")]
        [DefaultValue(0)]
        public int Lowercase { get; init; }

        [Description("Minimum number of symbols")]
        [CommandOption("-s|--symbols <COUNT>")]
        [DefaultValue(0)]
        public int Symbols { get; init; }

        [Description("Custom symbol characters")]
        [CommandOption("--symbol-chars <CHARS>")]
        public string? SymbolChars { get; init; }

        [Description("Use hexadecimal characters only")]
        [CommandOption("--hex-only")]
        [DefaultValue(false)]
        public bool HexOnly { get; init; }

        [Description("Use easy vision characters")]
        [CommandOption("--easy-vision")]
        [DefaultValue(false)]
        public bool EasyVision { get; init; }

        [Description("Generate pronounceable passwords")]
        [CommandOption("--pronounceable")]
        [DefaultValue(false)]
        public bool Pronounceable { get; init; }

        [Description("Password length")]
        [CommandOption("--length <LENGTH>")]
        [DefaultValue(12)]
        public int Length { get; init; }

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

            if (Length < 6)
            {
                return ValidationResult.Error("The password must contain no less than 6 characters");
            }

            var constraintsLength = FilterNegativeValue(Digits) + FilterNegativeValue(Uppercase)
                + FilterNegativeValue(Lowercase) + FilterNegativeValue(Symbols);

            if (constraintsLength > Length)
            {
                return ValidationResult.Error("The password length is less than the sum of 'at least' constraints");
            }

            if (EasyVision && Pronounceable)
            {
                return ValidationResult.Error("The options '--easy-vision' and '--pronounceable' cannot be used together");
            }

            if (!string.IsNullOrWhiteSpace(SymbolChars))
            {
                if (PwCharPool.HasDuplicatedCharacters(SymbolChars))
                {
                    return ValidationResult.Error("The symbol characters contain duplicated characters");
                }

                if (!PwCharPool.IsValidSymbols(SymbolChars))
                {
                    return ValidationResult.Error("The symbol characters contain invalid characters");
                }
            }

            return ValidationResult.Success();
        }

        private static int FilterNegativeValue(int value) => value < 0 ? 0 : value;
    }

    private readonly IDocumentService _documentService;

    public AddPolicyCommand(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            if (settings.HexOnly && (settings.Digits > 0 || settings.Uppercase > 0 || settings.Lowercase > 0 || settings.Symbols > 0))
            {
                var confirm = AnsiConsole.Confirm("When --hex-only is set, all other options will be ignored. Continue?");
                if (!confirm)
                {
                    return 0;
                }
            }

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

            if (document.NamedPasswordPolicies.Any(p => p.Name == settings.Name))
            {
                AnsiConsole.MarkupLine($"[red]The password policy with name '{settings.Name}' already exists.[/]");
                return 1;
            }

            var policy = new NamedPasswordPolicy(settings.Name, settings.Length);

            PasswordPolicyStyle style = 0;
            var minimumDigitCount = 0;
            var minimumUppercaseCount = 0;
            var minimumLowercaseCount = 0;
            var minimumSymbolCount = 0;

            if (settings.HexOnly)
            {
                style |= PasswordPolicyStyle.UseHexDigits;
            }
            else
            {
                if (settings.Pronounceable)
                {
                    style |= PasswordPolicyStyle.MakePronounceable;
                }

                if (settings.EasyVision)
                {
                    style |= PasswordPolicyStyle.UseEasyVision;
                }

                if (settings.Digits >= 0)
                {
                    style |= PasswordPolicyStyle.UseDigits;
                    minimumDigitCount = settings.Pronounceable ? 0 : settings.Digits;
                }

                if (settings.Uppercase >= 0)
                {
                    style |= PasswordPolicyStyle.UseUppercase;
                    minimumUppercaseCount = settings.Pronounceable ? 0 : settings.Uppercase;
                }

                if (settings.Lowercase >= 0)
                {
                    style |= PasswordPolicyStyle.UseLowercase;
                    minimumLowercaseCount = settings.Pronounceable ? 0 : settings.Lowercase;
                }

                if (settings.Symbols >= 0)
                {
                    style |= PasswordPolicyStyle.UseSymbols;
                    minimumSymbolCount = settings.Pronounceable ? 0 : settings.Symbols;
                }
            }

            policy.Style = style;
            policy.MinimumDigitCount = minimumDigitCount;
            policy.MinimumUppercaseCount = minimumUppercaseCount;
            policy.MinimumLowercaseCount = minimumLowercaseCount;
            policy.MinimumSymbolCount = minimumSymbolCount;

            if (settings.Pronounceable)
            {
                policy.SetSpecialSymbolSet(PwCharPool.PronounceableSymbolChars);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(settings.SymbolChars))
                {
                    policy.SetSpecialSymbolSet(settings.SymbolChars.ToArray());
                }
                else
                {
                    if (settings.EasyVision)
                    {
                        policy.SetSpecialSymbolSet(PwCharPool.EasyVisionSymbolChars);
                    }
                    else
                    {
                        policy.SetSpecialSymbolSet(PwCharPool.StdSymbolChars);
                    }
                }
            }

            document.NamedPasswordPolicies.Add(policy);
            await _documentService.SaveDocumentAsync(document, settings.Alias, settings.FilePath);

            AnsiConsole.MarkupLine($"[green]Password policy '{policy.Name}' was added successfully[/]");
            return 0;
        }
        catch (Exception ex)
        {
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }
}
