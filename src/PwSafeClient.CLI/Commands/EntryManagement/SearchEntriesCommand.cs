using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Json;
using PwSafeClient.Cli.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class SearchEntriesCommand : AsyncCommand<SearchEntriesCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [Description("Search query (plain text or regex pattern)")]
        [CommandArgument(0, "<QUERY>")]
        public required string Query { get; init; }

        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-f|--file|-p|--path <PATH>")]
        public string? FilePath { get; init; }

        [Description("Filter results to the given group path")]
        [CommandOption("-g|--group <GROUP>")]
        public string? Group { get; init; }

        [Description("Use case-sensitive matching")]
        [CommandOption("--case-sensitive")]
        public bool CaseSensitive { get; init; }

        [Description("Treat QUERY as a regular expression")]
        [CommandOption("--regex")]
        public bool Regex { get; init; }

        [Description("Maximum number of results to show (0 = no limit)")]
        [CommandOption("--limit <LIMIT>")]
        public int Limit { get; init; }

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

            if (Limit < 0)
            {
                return ValidationResult.Error("Limit must be >= 0");
            }

            if (Regex)
            {
                try
                {
                    _ = new System.Text.RegularExpressions.Regex(Query);
                }
                catch (ArgumentException ex)
                {
                    return ValidationResult.Error($"Invalid regex: {ex.Message}");
                }
            }

            return ValidationResult.Success();
        }
    }

    private readonly IDocumentService _documentService;

    public SearchEntriesCommand(IDocumentService documentService)
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
                AnsiConsole.MarkupLine("[red]Failed to load document.[/]");
                return 1;
            }

            var stringComparison = settings.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            System.Text.RegularExpressions.Regex? regex = null;
            if (settings.Regex)
            {
                var options = RegexOptions.CultureInvariant;
                if (!settings.CaseSensitive)
                {
                    options |= RegexOptions.IgnoreCase;
                }

                regex = new System.Text.RegularExpressions.Regex(settings.Query, options);
            }

            var matchesList = new List<EntrySearchResult>();

            foreach (var entry in document.Entries)
            {
                if (settings.Group != null)
                {
                    var entryGroup = entry.Group?.ToString() ?? string.Empty;
                    if (!string.Equals(entryGroup, settings.Group, stringComparison))
                    {
                        continue;
                    }
                }

                var fields = new[]
                {
                    entry.Title ?? string.Empty,
                    entry.UserName ?? string.Empty,
                    entry.Url ?? string.Empty,
                    entry.Notes ?? string.Empty,
                };

                var matches = false;

                if (regex != null)
                {
                    foreach (var field in fields)
                    {
                        if (regex.IsMatch(field))
                        {
                            matches = true;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var field in fields)
                    {
                        if (field.Contains(settings.Query, stringComparison))
                        {
                            matches = true;
                            break;
                        }
                    }
                }

                if (!matches)
                {
                    continue;
                }

                matchesList.Add(new EntrySearchResult(
                    entry.Uuid,
                    entry.Group?.ToString() ?? string.Empty,
                    entry.Title ?? string.Empty,
                    entry.UserName ?? string.Empty,
                    entry.Url ?? string.Empty,
                    entry.CreationTime,
                    entry.LastModificationTime));

                if (settings.Limit > 0 && matchesList.Count >= settings.Limit)
                {
                    break;
                }
            }

            if (settings.Json)
            {
                Console.WriteLine(JsonSerializer.Serialize(matchesList, CliJsonContext.Default.EntrySearchResults));
                return 0;
            }

            if (matchesList.Count == 0)
            {
                if (!settings.Quiet)
                {
                    AnsiConsole.MarkupLine("[yellow]No matching entries found.[/]");
                }
                return 0;
            }

            var table = new Table();
            table.AddColumn("ID");
            table.AddColumn("Group");
            table.AddColumn("Title");
            table.AddColumn("Username");
            table.AddColumn("URL");
            table.AddColumn("Created");
            table.AddColumn("Modified");

            foreach (var item in matchesList)
            {
                table.AddRow(
                    item.Id.ToString(),
                    item.Group,
                    item.Title,
                    item.Username,
                    item.Url,
                    item.Created.ToString("yyyy-MM-dd HH:mm:ss"),
                    item.Modified.ToString("yyyy-MM-dd HH:mm:ss")
                );
            }

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
