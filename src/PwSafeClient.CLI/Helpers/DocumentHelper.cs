using System;
using System.IO;
using System.Threading.Tasks;
using Medo.Security.Cryptography.PasswordSafe;
using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Exceptions;

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

        try {
            string filepath;
            if (fileInfo != null)
            {
                filepath = fileInfo.FullName;
            }
            else
            {
                filepath = await configManager.GetDbPath(alias);
            }

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
}