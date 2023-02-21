using Medo.Security.Cryptography.PasswordSafe;
using PwSafeClient.Core;
using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public static class ListCommand
{
    public static RootCommand AddListEntriesCommand(this RootCommand rootCommand)
    {
        Command command = new Command("list", "List the items in database");

        var aliasOption = new Option<string>("--alias", "The alias of the database");
        aliasOption.AddAlias("-a");

        var fileOption = new Option<FileInfo>("--file", "The file path of your psafe3 file");
        fileOption.AddAlias("-f");

        command.AddOption(aliasOption);
        command.AddOption(fileOption);
        command.SetHandler(HandleListEntriesAsync, aliasOption, fileOption);

        rootCommand.AddCommand(command);
        return rootCommand;
    }

    private static async Task HandleListEntriesAsync(string alias, FileInfo? file)
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

            var groups = PwSafeClientHelper.ListGroupInfo(doc);

            foreach (var group in groups)
            {
                Console.WriteLine(group);
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.LogError(ex.Message);
        }
    }
}
