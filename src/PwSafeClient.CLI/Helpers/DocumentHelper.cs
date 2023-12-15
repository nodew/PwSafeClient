using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Exceptions;
using PwSafeClient.CLI.Models;

public class DocumentHelper : IDocumentHelper
{
    private readonly IConfigManager configManager;
    private readonly IConsoleService consoleService;
    private static Document? document;

    public DocumentHelper(IConfigManager configManager, IConsoleService consoleService)
    {
        this.configManager = configManager;
        this.consoleService = consoleService;
    }

    public async Task<Document?> TryLoadDocumentAsync(string? alias, FileInfo? fileInfo, bool readOnly)
    {
        if (document != null)
        {
            return document;
        }

        try
        {
            string filepath = await GetDocumentFilePathAsync(alias, fileInfo);

            if (!File.Exists(filepath))
            {
                consoleService.LogError($"Can't locate a valid file, please check your command parameters or configuration in <HOMEDIR>/pwsafe.json");
                return null;
            }

            string password = consoleService.ReadPassword();

            document = Document.Load(filepath, password);
            document.IsReadOnly = readOnly;
        }
        catch (FormatException)
        {
            consoleService.LogError("Invalid PwSafe file or password mismatch.");
        }
        catch (DatabaseNotFoundException ex)
        {
            consoleService.LogError(ex.Message);
        }
        catch (Exception ex)
        {
            consoleService.LogError($"Can't load the file, error: {ex.Message}");
        }

        return document;
    }

    public async Task SaveDocumentAsync(string? alias, FileInfo? fileInfo)
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
            string filepath = await GetDocumentFilePathAsync(alias, fileInfo);
            await BackupDocumentAsync(filepath);
            document.Save(filepath);
        }
        catch (Exception ex)
        {
            consoleService.LogError($"Can't save the file, error: {ex.Message}");
        }
    }

    public async Task<string> GetDocumentDisplayNameAsync(string? alias, FileInfo? fileInfo)
    {
        if (fileInfo != null)
        {
            return fileInfo.Name;
        }

        Config config = await configManager.LoadConfigAsync();

        return alias ?? config.DefaultDatabase ?? string.Empty;
    }

    private async Task<string> GetDocumentFilePathAsync(string? alias, FileInfo? fileInfo)
    {
        if (fileInfo != null)
        {
            return fileInfo.FullName;
        }

        return await configManager.GetDbPathAsync(alias);
    }

    private async Task BackupDocumentAsync(string filepath)
    {
        if (!File.Exists(filepath))
        {
            return;
        }

        string? targetFolder = Path.GetDirectoryName(filepath);
        string? filename = Path.GetFileNameWithoutExtension(filepath);
        string? extension = Path.GetExtension(filepath);

        if (targetFolder == null || filename == null)
        {
            return;
        }

        string[] backupFiles = Directory.GetFiles(targetFolder, $"{filename}_*.ibak")
            .OrderByDescending(GetBackupVersion)
            .ToArray();

        int maxBackupCount = await configManager.GetMaxBackupCountAsync();

        int backupVersion = 1;

        if (backupFiles.Length > 0)
        {
            backupVersion = GetBackupVersion(backupFiles[0]) + 1;
        }

        Console.WriteLine($"Backup file: {filepath} to {Path.Combine(targetFolder, $"{filename}_{backupVersion}.ibak")}");

        File.Copy(filepath, Path.Combine(targetFolder, $"{filename}_{backupVersion}.ibak"), true);

        if (backupFiles.Length >= maxBackupCount)
        {
            for (int i = maxBackupCount - 1; i < backupFiles.Length; i++)
            {
                File.Delete(backupFiles[i]);
            }
        }
    }

    private int GetBackupVersion(string filepath)
    {
        string? filename = Path.GetFileNameWithoutExtension(filepath);
        string? extension = Path.GetExtension(filepath);

        if (filename == null || extension == null)
        {
            return 0;
        }

        string[]? nameAndVersionPair = filename.Split('_');
        if (nameAndVersionPair != null && nameAndVersionPair.Length == 2)
        {
            if (int.TryParse(nameAndVersionPair[1], out int version))
            {
                return version;
            }
        }

        return 0;
    }
}
