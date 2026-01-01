using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Models;

using Spectre.Console;

namespace PwSafeClient.Cli.Services;

internal class DocumentService : IDocumentService
{
    private readonly IConfigManager configManager;
    private readonly IDatabaseManager databaseManager;
    private readonly ICliSession session;
    private readonly Dictionary<string, Document> documentCache = new();

    public DocumentService(IConfigManager configManager, IDatabaseManager databaseManager, ICliSession session)
    {
        this.configManager = configManager;
        this.databaseManager = databaseManager;
        this.session = session;
    }

    public async Task<Document?> TryLoadDocumentAsync(string? alias, string? filepath, bool readOnly, PasswordOptions? passwordOptions = null)
    {
        try
        {
            filepath = await GetDocumentFilePathAsync(alias, filepath);
            if (filepath == null)
                return null;

            if (documentCache.TryGetValue(filepath, out var cachedDoc))
            {
                cachedDoc.IsReadOnly = readOnly;
                return cachedDoc;
            }

            if (!File.Exists(filepath))
            {
                AnsiConsole.MarkupLine($"[yellow]File not found: {filepath}[/]");
                return null;
            }

            var password = await GetPasswordAsync(passwordOptions);
            if (password == null)
            {
                return null;
            }

            var document = Document.Load(filepath, password);
            document.IsReadOnly = readOnly;
            documentCache[filepath] = document;
            return document;
        }
        catch (FormatException)
        {
            AnsiConsole.MarkupLine("Invalid PwSafe file or password mismatch.");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error loading file: {ex.Message}[/]");
        }
        return null;
    }

    private async Task<string?> GetPasswordAsync(PasswordOptions? passwordOptions)
    {
        if (!string.IsNullOrEmpty(passwordOptions?.Password))
        {
            return passwordOptions.Password;
        }

        if (passwordOptions?.PasswordStdin == true)
        {
            var input = await Console.In.ReadToEndAsync();
            var password = input?.TrimEnd('\r', '\n');

            if (string.IsNullOrEmpty(password))
            {
                AnsiConsole.MarkupLine("[red]No password received from stdin.[/]");
                return null;
            }

            return password;
        }

        if (!string.IsNullOrWhiteSpace(passwordOptions?.PasswordEnvVar))
        {
            var password = Environment.GetEnvironmentVariable(passwordOptions.PasswordEnvVar);
            if (string.IsNullOrEmpty(password))
            {
                AnsiConsole.MarkupLine($"[red]Environment variable '{passwordOptions.PasswordEnvVar}' is not set or empty.[/]");
                return null;
            }

            return password;
        }

        if (!string.IsNullOrEmpty(session.UnlockedPassword))
        {
            return session.UnlockedPassword;
        }

        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter password:")
                .PromptStyle("green")
                .Secret());
    }

    public async Task SaveDocumentAsync(string? alias, string? filepath)
    {
        filepath = await GetDocumentFilePathAsync(alias, filepath);
        if (filepath == null)
            return;
        if (!documentCache.TryGetValue(filepath, out var document))
            return;
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

    public async Task<string> GetDocumentFilePathAsync(string? alias, string? filepath)
    {
        if (filepath != null)
        {
            return filepath;
        }

        if (!string.IsNullOrWhiteSpace(session.DefaultFilePath))
        {
            return session.DefaultFilePath;
        }

        var config = await configManager.LoadConfigurationAsync();

        var aliasToUse = alias ?? session.DefaultAlias ?? config.DefaultDatabase;

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
