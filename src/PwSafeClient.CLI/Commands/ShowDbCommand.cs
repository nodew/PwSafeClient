using Medo.Security.Cryptography.PasswordSafe;
using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public static class ShowDbCommand
{
    public static RootCommand AddShowDbCommand(this RootCommand rootCommand)
    {
        Command command = new Command("showdb", "Show the detail of PasswordSafe database");

        var aliasOption = new Option<string>("--alias", "The alias of the database");
        aliasOption.AddAlias("-a");

        var fileOption = new Option<FileInfo>("--file", "The file path of your psafe3 file");
        fileOption.AddAlias("-f");

        command.AddOption(aliasOption);
        command.AddOption(fileOption);
        command.SetHandler(HandleShowDbAsync, aliasOption, fileOption);

        rootCommand.AddCommand(command);
        return rootCommand;
    }

    private static async Task HandleShowDbAsync(string alias, FileInfo? file)
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

            string format = "{0, -30}{1}";
            Console.WriteLine(format, "Database UUID:", doc.Uuid);
            Console.WriteLine(format, "Name:", doc.Name ?? "-");
            Console.WriteLine(format, "Description:", doc.Description ?? "-");
            Console.WriteLine(format, "Version", doc.Version);
            Console.WriteLine(format, "Last saved by:", doc.LastSaveUser);
            Console.WriteLine(format, "Last saved on:", doc.LastSaveTime);
            Console.WriteLine(format, "Last saved application: ", doc.LastSaveApplication);
            Console.WriteLine(format, "Last saved machine: ", doc.LastSaveHost);
            Console.WriteLine(format, "Last saved user: ", doc.LastSaveUser);
            Console.WriteLine(format, "Item count: ", doc.Entries.Count);
        }
        catch (Exception ex)
        {
            ConsoleHelper.LogError(ex.Message);
        }
    }
}
