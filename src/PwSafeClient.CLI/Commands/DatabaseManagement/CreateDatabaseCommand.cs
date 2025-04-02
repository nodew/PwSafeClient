using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal class CreateDatabaseCommand : AsyncCommand<CreateDatabaseCommand.Settings>
{
    internal class Settings : CommandSettings
    {
        [Description("Path to the database file")]
        [CommandArgument(0, "<FILE_PATH>")]
        public required string FilePath { get; init; }

        [Description("Alias for the database")]
        [CommandOption("-a|--alias <ALIAS>")]
        public string? Alias { get; init; }

        [Description("Set the database as default")]
        [CommandOption("-d|--default")]
        public bool IsDefault { get; init; }

        [Description("Force to create the database file if it already exists")]
        [CommandOption("-f|--force")]
        public bool ForceCreate { get; init; }
    }

    private readonly IDatabaseManager _dbManager;

    public CreateDatabaseCommand(IDatabaseManager databaseManager)
    {
        _dbManager = databaseManager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var alias = settings.Alias ?? Path.GetFileNameWithoutExtension(settings.FilePath);

            var dbs = await _dbManager.ListDatabasesAsync();

            if (dbs.Any(db => db.Alias == alias))
            {
                AnsiConsole.MarkupLine($"[yellow]Database with alias '{alias}' already exists[/]");
                return 1;
            }

            if (File.Exists(settings.FilePath))
            {
                if (!settings.ForceCreate)
                {
                    AnsiConsole.MarkupLine("[yellow]Database file already exists[/]");
                    return 1;
                }
            }

            var password = AnsiConsole.Prompt(new TextPrompt<string>("Enter the password for the database:").Secret());

            if (password.Length < 8)
            {
                AnsiConsole.MarkupLine("[red]Password must be at least 8 characters long[/]");
                return 1;
            }

            var document = new Document(password);

            document.Save(settings.FilePath);

            await _dbManager.AddDatabaseAsync(alias, settings.FilePath, settings.IsDefault);

            return 0;
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 1;
        }
    }
}
