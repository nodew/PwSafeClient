using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Models;
using PwSafeClient.Shared;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class ListPoliciesCommand : AsyncCommand<ListPoliciesCommand.Settings>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    internal sealed class Settings : CommandSettings
    {
        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-f|--file <PATH>")]
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

    public ListPoliciesCommand(IDocumentService documentService)
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

            var document = await _documentService.TryLoadDocumentAsync(settings.Alias, settings.FilePath, true, passwordOptions);

            if (document == null)
            {
                AnsiConsole.MarkupLine("[red]Failed to load document.[/]");
                return 1;
            }

            if (document.NamedPasswordPolicies.Count == 0)
            {
                if (settings.Json)
                {
                    Console.WriteLine("[]");
                }
                else if (!settings.Quiet)
                {
                    AnsiConsole.MarkupLine("[yellow]No password policies found.[/]");
                }

                return 0;
            }

            if (settings.Json)
            {
                var results = document.NamedPasswordPolicies.Select(policy =>
                {
                    var isPronounceable = policy.Style.HasFlag(PasswordPolicyStyle.MakePronounceable);
                    var useLowercase = policy.Style.HasFlag(PasswordPolicyStyle.UseLowercase);
                    var useUppercase = policy.Style.HasFlag(PasswordPolicyStyle.UseUppercase);
                    var useDigits = policy.Style.HasFlag(PasswordPolicyStyle.UseDigits);
                    var useSymbols = policy.Style.HasFlag(PasswordPolicyStyle.UseSymbols);
                    var useHexDigits = policy.Style.HasFlag(PasswordPolicyStyle.UseHexDigits);
                    var useEasyVision = policy.Style.HasFlag(PasswordPolicyStyle.UseEasyVision);

                    var symbols = policy.GetSpecialSymbolSet();
                    var symbolSet = symbols.Length == 0 ? string.Empty : string.Join("", symbols);

                    return new
                    {
                        name = policy.Name,
                        length = policy.TotalPasswordLength,
                        pronounceable = isPronounceable,
                        useLowercase,
                        minimumLowercaseCount = isPronounceable ? 0 : policy.MinimumLowercaseCount,
                        useUppercase,
                        minimumUppercaseCount = isPronounceable ? 0 : policy.MinimumUppercaseCount,
                        useDigits,
                        minimumDigitCount = isPronounceable ? 0 : policy.MinimumDigitCount,
                        useSymbols,
                        minimumSymbolCount = isPronounceable ? 0 : policy.MinimumSymbolCount,
                        symbolSet,
                        useEasyVision,
                        useHexDigits,
                    };
                });

                Console.WriteLine(JsonSerializer.Serialize(results, JsonOptions));
                return 0;
            }

            foreach (var policy in document.NamedPasswordPolicies)
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .Title($"[blue]{policy.Name}[/]")
                    .AddColumn(new TableColumn("Setting").LeftAligned())
                    .AddColumn(new TableColumn("Value").LeftAligned());

                var isPronounceable = policy.Style.HasFlag(PasswordPolicyStyle.MakePronounceable);
                var useLowercase = policy.Style.HasFlag(PasswordPolicyStyle.UseLowercase);
                var useUppercase = policy.Style.HasFlag(PasswordPolicyStyle.UseUppercase);
                var useDigits = policy.Style.HasFlag(PasswordPolicyStyle.UseDigits);
                var useSymbols = policy.Style.HasFlag(PasswordPolicyStyle.UseSymbols);
                var useHexDigits = policy.Style.HasFlag(PasswordPolicyStyle.UseHexDigits);
                var useEasyVision = policy.Style.HasFlag(PasswordPolicyStyle.UseEasyVision);

                var symbols = policy.GetSpecialSymbolSet();
                if (symbols.Length == 0 && !isPronounceable)
                {
                    symbols = useEasyVision ? PwCharPool.EasyVisionDigitChars : PwCharPool.StdDigitChars;
                }

                table.AddRow("Password length", policy.TotalPasswordLength.ToString());
                table.AddRow("Use lowercase characters", RenderPolicyValue(useLowercase, isPronounceable ? 0 : policy.MinimumLowercaseCount));
                table.AddRow("Use uppercase characters", RenderPolicyValue(useUppercase, isPronounceable ? 0 : policy.MinimumUppercaseCount));
                table.AddRow("Use digits", RenderPolicyValue(useDigits, isPronounceable ? 0 : policy.MinimumDigitCount));
                table.AddRow("Use symbols", RenderPolicyValue(useSymbols, isPronounceable ? 0 : policy.MinimumSymbolCount, string.Join("", symbols)));
                table.AddRow("Use easy vision characters", RenderPolicyValue(useEasyVision, 0));
                table.AddRow("Pronounceable passwords", RenderPolicyValue(isPronounceable, 0));
                table.AddRow("Hexadecimal characters", RenderPolicyValue(useHexDigits, 0));

                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
            }

            return 0;
        }
        catch (Exception ex)
        {
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }

    private static string RenderPolicyValue(bool enabled, int minimumCount, string? symbols = null)
    {
        if (!enabled)
        {
            return "[red]No[/]";
        }

        if (minimumCount == 0)
        {
            return "[green]Yes[/]";
        }

        var result = $"[green]Yes[/] - (At least {minimumCount})";
        if (!string.IsNullOrWhiteSpace(symbols))
        {
            result += $" from set '{symbols}'";
        }

        return result;
    }
}
