using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class ListEntriesCommand : AsyncCommand<ListEntriesCommand.Settings>
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
        [CommandOption("-f|--file|-p|--path <PATH>")]
        public string? FilePath { get; init; }

        [Description("Filter by group path")]
        [CommandOption("-g|--group <GROUP>")]
        public string? Group { get; init; }

        [Description("Filter by title containing the given text")]
        [CommandOption("--title <TITLE>")]
        public string? Title { get; init; }

        [Description("Filter by username containing the given text")]
        [CommandOption("--username <USERNAME>")]
        public string? Username { get; init; }

        [Description("Filter by URL containing the given text")]
        [CommandOption("--url <URL>")]
        public string? Url { get; init; }

        [Description("Filter entries created on/after this date/time (parseable by .NET)")]
        [CommandOption("--created-since <DATE>")]
        public string? CreatedSince { get; init; }

        [Description("Filter entries modified on/after this date/time (parseable by .NET)")]
        [CommandOption("--modified-since <DATE>")]
        public string? ModifiedSince { get; init; }

        [Description("Output results as JSON")]
        [CommandOption("--json")]
        public bool Json { get; init; }

        [Description("Display results in a tree view grouped by group path")]
        [CommandOption("--tree-view")]
        public bool TreeView { get; init; }

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

            if (CreatedSince != null && !DateTime.TryParse(CreatedSince, out _))
            {
                return ValidationResult.Error("Invalid --created-since date/time");
            }

            if (ModifiedSince != null && !DateTime.TryParse(ModifiedSince, out _))
            {
                return ValidationResult.Error("Invalid --modified-since date/time");
            }

            return ValidationResult.Success();
        }
    }

    private readonly IDocumentService _documentService;

    public ListEntriesCommand(IDocumentService documentService)
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

            var entries = document.Entries.AsEnumerable();

            if (settings.Group != null)
            {
                entries = entries.Where(e => string.Equals(e.Group?.ToString() ?? string.Empty, settings.Group, StringComparison.OrdinalIgnoreCase));
            }

            if (settings.Title != null)
            {
                entries = entries.Where(e => (e.Title ?? string.Empty).Contains(settings.Title, StringComparison.OrdinalIgnoreCase));
            }

            if (settings.Username != null)
            {
                entries = entries.Where(e => (e.UserName ?? string.Empty).Contains(settings.Username, StringComparison.OrdinalIgnoreCase));
            }

            if (settings.Url != null)
            {
                entries = entries.Where(e => (e.Url ?? string.Empty).Contains(settings.Url, StringComparison.OrdinalIgnoreCase));
            }

            if (settings.CreatedSince != null)
            {
                var createdSince = DateTime.Parse(settings.CreatedSince);
                entries = entries.Where(e => e.CreationTime >= createdSince);
            }

            if (settings.ModifiedSince != null)
            {
                var modifiedSince = DateTime.Parse(settings.ModifiedSince);
                entries = entries.Where(e => e.LastModificationTime >= modifiedSince);
            }

            var entryList = entries.ToList();

            if (settings.Json)
            {
                var results = entryList.Select(entry => new
                {
                    id = entry.Uuid,
                    group = entry.Group?.ToString() ?? string.Empty,
                    title = entry.Title ?? string.Empty,
                    username = entry.UserName ?? string.Empty,
                    url = entry.Url ?? string.Empty,
                    created = entry.CreationTime,
                    modified = entry.LastModificationTime,
                });

                Console.WriteLine(JsonSerializer.Serialize(results, JsonOptions));
                return 0;
            }

            if (settings.TreeView)
            {
                RenderTreeView(entryList);
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

            foreach (var entry in entryList)
            {
                table.AddRow(
                    entry.Uuid.ToString(),
                    entry.Group?.ToString() ?? string.Empty,
                    entry.Title ?? string.Empty,
                    entry.UserName ?? string.Empty,
                    entry.Url ?? string.Empty,
                    entry.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    entry.LastModificationTime.ToString("yyyy-MM-dd HH:mm:ss")
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

    private static void RenderTreeView(IReadOnlyCollection<Entry> entries)
    {
        var orderedEntries = entries
            .OrderBy(e => e.Group?.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var tree = new Tree("Entries");
        var groupNodesByPath = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in orderedEntries)
        {
            var groupPath = entry.Group?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(groupPath))
            {
                tree.AddNode(GetEntryDisplayText(entry));
                continue;
            }

            var segments = groupPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length == 0)
            {
                tree.AddNode(GetEntryDisplayText(entry));
                continue;
            }

            TreeNode? currentGroupNode = null;

            for (var i = 0; i < segments.Length; i++)
            {
                var currentPath = string.Join('.', segments.Take(i + 1));
                if (groupNodesByPath.TryGetValue(currentPath, out var existing))
                {
                    currentGroupNode = existing;
                    continue;
                }

                var segmentText = Markup.Escape(segments[i]);

                TreeNode newNode;
                if (i == 0)
                {
                    newNode = tree.AddNode(segmentText);
                }
                else
                {
                    var parentPath = string.Join('.', segments.Take(i));
                    var parentNode = groupNodesByPath[parentPath];
                    newNode = parentNode.AddNode(segmentText);
                }

                groupNodesByPath[currentPath] = newNode;
                currentGroupNode = newNode;
            }

            currentGroupNode?.AddNode(GetEntryDisplayText(entry));
        }

        AnsiConsole.Write(tree);
    }

    private static string GetEntryDisplayText(Entry entry)
    {
        var title = string.IsNullOrWhiteSpace(entry.Title) ? "(no title)" : entry.Title;
        var username = entry.UserName ?? string.Empty;
        var id = entry.Uuid.ToString();

        var display = string.IsNullOrWhiteSpace(username)
            ? $"{title} ({id})"
            : $"{title} ({username}) ({id})";

        return Markup.Escape(display);
    }
}
