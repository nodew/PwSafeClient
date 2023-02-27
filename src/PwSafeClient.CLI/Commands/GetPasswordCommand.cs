using Medo.Security.Cryptography.PasswordSafe;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public class GetPasswordCommand : Command
{
    public GetPasswordCommand() : base("get", "Get the password")
    {
        AddArgument(new Argument<Guid>("GUID", "The ID of an entry"));

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

    public static async Task Run(Guid id, string alias, FileInfo? file)
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
