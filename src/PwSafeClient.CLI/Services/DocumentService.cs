using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;

namespace PwSafeClient.Cli.Services;

internal class DocumentService : IDocumentService
{
    private readonly IConfigManager configManager;
    private readonly IDatabaseManager databaseManager;
    private static Document? document;

    public DocumentService(IConfigManager configManager, IDatabaseManager databaseManager)
    {
        this.configManager = configManager;
        this.databaseManager = databaseManager;
    }

    public async Task<Document?> TryLoadDocumentAsync(string? alias, string? filepath, bool readOnly)
    {
        if (document != null)
        {
            return document;
        }

        try
        {
            filepath = await GetDocumentFilePathAsync(alias, filepath);

            if (!File.Exists(filepath))
            {
                AnsiConsole.MarkupLine($"[yellow]File not found: {filepath}[/]");
                return null;
            }

            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter password:")
                    .PromptStyle("green")
                    .Secret());

            document = Document.Load(filepath, password);
            document.IsReadOnly = readOnly;
        }
        catch (FormatException)
        {
            AnsiConsole.MarkupLine("Invalid PwSafe file or password mismatch.");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error loading file: {ex.Message}[/]");
        }

        return document;
    }

    public async Task SaveDocumentAsync(string? alias, string? filepath)
    {
        if (document == null)
        {
            return;
        }

        await SaveDocumentAsync(document, alias, filepath);
    }

    public async Task SaveDocumentAsync(Document document, string? alias, string? filepath)
    {
        if (document == null)
        {
            return;
        }

        if (document.IsReadOnly)
        {
            return;
        }

        if (!document.HasChanged)
        {
            return;
        }

        try
        {
            filepath = await GetDocumentFilePathAsync(alias, filepath);
            await BackupDocumentAsync(filepath);
            document.Save(filepath);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error saving file: {ex.Message}[/]");
        }
    }

    public async Task<string> GetDocumentDisplayNameAsync(string? alias, string? filepath)
    {
        if (filepath != null)
        {
            return Path.GetFileName(filepath) ?? string.Empty;
        }

        var config = await configManager.LoadConfigurationAsync();

        return alias ?? config.DefaultDatabase ?? string.Empty;
    }

    private async Task<string> GetDocumentFilePathAsync(string? alias, string? filepath)
    {
        if (filepath != null)
        {
            return filepath;
        }

        var config = await configManager.LoadConfigurationAsync();

        var aliasToUse = alias ?? config.DefaultDatabase;

        var database = config.Databases.FirstOrDefault(db => db.Key == aliasToUse);

        if (database.Value == null)
        {
            throw new InvalidOperationException("Database not found");
        }

        return database.Value;
    }

    private async Task BackupDocumentAsync(string filepath)
    {
        if (!File.Exists(filepath))
        {
            return;
        }

        var targetFolder = Path.GetDirectoryName(filepath);
        var filename = Path.GetFileNameWithoutExtension(filepath);
        var extension = Path.GetExtension(filepath);

        if (targetFolder == null || filename == null)
        {
            return;
        }

        var backupFiles = Directory.GetFiles(targetFolder, $"{filename}_*.ibak")
            .OrderByDescending(GetBackupVersion)
            .ToArray();

        var config = await configManager.LoadConfigurationAsync();
        var maxBackupCount = config.MaxBackupCount;

        var backupVersion = 1;

        if (backupFiles.Length > 0)
        {
            backupVersion = GetBackupVersion(backupFiles[0]) + 1;
        }

        File.Copy(filepath, Path.Combine(targetFolder, $"{filename}_{backupVersion}.ibak"), true);

        AnsiConsole.MarkupLine($"[green]Backup file: {filepath} to {Path.Combine(targetFolder, $"{filename}_{backupVersion}.ibak")}[/]");

        if (backupFiles.Length >= maxBackupCount)
        {
            for (var i = maxBackupCount - 1; i < backupFiles.Length; i++)
            {
                File.Delete(backupFiles[i]);
            }
        }
    }

    private int GetBackupVersion(string filepath)
    {
        var filename = Path.GetFileNameWithoutExtension(filepath);
        var extension = Path.GetExtension(filepath);

        if (filename == null || extension == null)
        {
            return 0;
        }

        var nameAndVersionPair = filename.Split('_');
        if (nameAndVersionPair != null && nameAndVersionPair.Length == 2)
        {
            if (int.TryParse(nameAndVersionPair[1], out var version))
            {
                return version;
            }
        }

        return 0;
    }
}
