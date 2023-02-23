using Medo.Security.Cryptography.PasswordSafe;
using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public static class GetPasswordCommand
{
    public static RootCommand AddGetPasswordCommand(this RootCommand rootCommand)
    {
        var command = new Command("get", "Get the password");

        var entryIdArgument = new Argument<Guid>("GUID", "The ID of an entry");

        var aliasOption = new Option<string>("--alias", "The alias of the database");
        aliasOption.AddAlias("-a");

        var fileOption = new Option<FileInfo>("--file", "The file path of your psafe3 file");
        fileOption.AddAlias("-f");

        command.AddOption(aliasOption);
        command.AddOption(fileOption);

        command.AddArgument(entryIdArgument);
        command.SetHandler(HandleGetPassoword, entryIdArgument, aliasOption, fileOption);

        rootCommand.Add(command);
        return rootCommand;
    }

    public static async Task HandleGetPassoword(Guid id, string alias, FileInfo? file)
    {
        string filepath;
        if (file != null)
        {
            filepath = file.FullName;
        }
        else
        {
            filepath = await ConsoleHelper.GetPWSFilePathAsync(alias);
        }

        if (!File.Exists(filepath))
        {
            ConsoleHelper.LogError($"Can't locate a valid file, please check your command parameters or configuration in <HOMEDIR>/pwsafe.json");
            return;
        }

        string password = ConsoleHelper.ReadPassword();

        try
        {
            var doc = Document.Load(filepath, password);
            doc.IsReadOnly = true;

            var entry = doc.Entries.Where(entry => entry.Uuid == id).FirstOrDefault();

            if (entry != null)
            {
                await TextCopy.ClipboardService.SetTextAsync(entry.Password);
                Console.WriteLine("Copied password to your clipboard");
            }
            else
            {
                ConsoleHelper.LogError("Entry is not found");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.LogError(ex.Message);
        }
    }
}
