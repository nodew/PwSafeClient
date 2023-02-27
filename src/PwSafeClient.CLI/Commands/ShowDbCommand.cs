using Medo.Security.Cryptography.PasswordSafe;
using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public class ShowDbCommand : Command
{
    public ShowDbCommand() : base("showdb", "Show the detail of PasswordSafe database")
    {
        AddOption(new Option<string>(
            aliases: new string[] { "--alias", "-a" },
            description: "The alias of the database"
        ));

        AddOption(new Option<FileInfo>(
            aliases: new string[] { "--file", "-f" },
            description: "The file path of your database file"
        ));

        Handler = CommandHandler.Create(Run);
    }

    private static async Task Run(string? alias, FileInfo? file, IHost host)
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
