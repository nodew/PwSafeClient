using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal class ShowDatabaseCommand : AsyncCommand<ShowDatabaseCommand.Settings>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    internal class Settings : CommandSettings
    {
        [Description("Alias of the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Path to the database file")]
        [CommandOption("-f|--file <FILE_PATH>")]
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
            if (PasswordStdin && !string.IsNullOrWhiteSpace(PasswordEnv))
            {
                return ValidationResult.Error("Use only one of --password-stdin or --password-env");
            }

            return ValidationResult.Success();
        }
    }

    private readonly IConfigManager _configManager;
    private readonly IDatabaseManager _dbManager;

    public ShowDatabaseCommand(IConfigManager configManager, IDatabaseManager databaseManager)
    {
        _configManager = configManager;
        _dbManager = databaseManager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var filepath = settings.FilePath;

        if (string.IsNullOrWhiteSpace(settings.Alias) && string.IsNullOrWhiteSpace(settings.FilePath))
        {
            var config = await _configManager.LoadConfigurationAsync();
            if (string.IsNullOrWhiteSpace(config.DefaultDatabase))
            {
                if (settings.Json)
                {
                    Console.WriteLine(JsonSerializer.Serialize(new { error = "No database specified and no default database configured." }, JsonOptions));
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]No database specified and no default database configured. Use 'pwsafe db set -a <ALIAS>' or pass '--alias/--file'.[/]");
                }
                return 1;
            }

            settings = new Settings
            {
                Alias = config.DefaultDatabase,
                FilePath = null
            };
        }

        if (!string.IsNullOrWhiteSpace(settings.Alias))
        {
            filepath = await _dbManager.GetDbPathByAliasAsync(settings.Alias);
            if (filepath is null)
            {
                if (settings.Json)
                {
                    Console.WriteLine(JsonSerializer.Serialize(new { error = $"Database with alias '{settings.Alias}' not found" }, JsonOptions));
                }
                else
                {
                    AnsiConsole.MarkupLine($"[yellow]Database with alias '{settings.Alias}' not found[/]");
                }
                return 1;
            }
        }

        if (!File.Exists(filepath))
        {
            if (settings.Json)
            {
                Console.WriteLine(JsonSerializer.Serialize(new { error = $"Database file not found at '{filepath}'" }, JsonOptions));
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Database file not found at '{filepath}'[/]");
            }
            return 1;
        }

        var (password, passwordError) = await TryGetPasswordAsync(settings.PasswordStdin, settings.PasswordEnv);
        if (password == null)
        {
            if (settings.Json)
            {
                Console.WriteLine(JsonSerializer.Serialize(new { error = passwordError ?? "Failed to read password" }, JsonOptions));
            }
            else if (!settings.Quiet)
            {
                AnsiConsole.MarkupLine($"[red]{passwordError ?? "Failed to read password"}[/]");
            }
            return 1;
        }

        try
        {
            var document = Document.Load(filepath, password);

            if (settings.Json)
            {
                var result = new
                {
                    uuid = document.Uuid,
                    name = document.Name ?? string.Empty,
                    description = document.Description ?? string.Empty,
                    version = document.Version.ToString(),
                    lastSavedBy = document.LastSaveUser,
                    lastSavedOn = document.LastSaveTime,
                    lastSavedApplication = document.LastSaveApplication,
                    lastSavedMachine = document.LastSaveHost,
                    itemCount = document.Entries.Count,
                };

                Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
                return 0;
            }

            var grid = new Grid();

            grid.AddColumn();
            grid.AddColumn();

            grid.AddRow("Database UUID:", document.Uuid.ToString());
            grid.AddRow("Name:", document.Name ?? "-");
            grid.AddRow("Description:", document.Description ?? "-");
            grid.AddRow("Version:", document.Version.ToString());
            grid.AddRow("Last saved by:", document.LastSaveUser);
            grid.AddRow("Last saved on:", document.LastSaveTime.ToString());
            grid.AddRow("Last saved application:", document.LastSaveApplication);
            grid.AddRow("Last saved machine:", document.LastSaveHost);
            grid.AddRow("Item count:", document.Entries.Count.ToString());

            AnsiConsole.Write(grid);

            return 0;
        }
        catch (Exception)
        {
            if (settings.Json)
            {
                Console.WriteLine(JsonSerializer.Serialize(new { error = "Invalid password or database file" }, JsonOptions));
            }
            else if (!settings.Quiet)
            {
                AnsiConsole.MarkupLine("[red]Invalid password or database file[/]");
            }
            return 1;
        }

    }

    private static async Task<(string? Password, string? Error)> TryGetPasswordAsync(bool passwordStdin, string? passwordEnv)
    {
        if (passwordStdin)
        {
            var input = await Console.In.ReadToEndAsync();
            var password = input?.TrimEnd('\r', '\n');

            if (string.IsNullOrEmpty(password))
            {
                return (null, "No password received from stdin.");
            }

            return (password, null);
        }

        if (!string.IsNullOrWhiteSpace(passwordEnv))
        {
            var password = Environment.GetEnvironmentVariable(passwordEnv);
            if (string.IsNullOrEmpty(password))
            {
                return (null, $"Environment variable '{passwordEnv}' is not set or empty.");
            }

            return (password, null);
        }

        var prompted = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter password:")
                .PromptStyle("green")
                .Secret());

        return (prompted, null);
    }
}
