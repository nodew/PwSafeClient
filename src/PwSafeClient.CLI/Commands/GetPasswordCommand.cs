namespace PwSafeClient.CLI.Commands;

//public class GetPasswordCommand : Command
//{
//    public GetPasswordCommand() : base("get", "Get the password")
//    {
//        AddArgument(new Argument<Guid>("GUID", "The ID of an entry"));

//        AddOption(new Option<string>(
//            aliases: new string[] { "--alias", "-a" },
//            description: "The alias of the database"
//        ));

//        AddOption(new Option<FileInfo>(
//            aliases: new string[] { "--file", "-f" },
//            description: "The file path of your database file"
//        ));

//        Handler = CommandHandler.Create(Run);
//    }

//    public static async Task Run(Guid id, string alias, FileInfo? file)
//    {
//        string filepath;
//        if (file != null)
//        {
//            filepath = file.FullName;
//        }
//        else
//        {
//            filepath = await ConsoleService.GetPWSFilePathAsync(alias);
//        }

//        if (!File.Exists(filepath))
//        {
//            ConsoleService.LogError($"Can't locate a valid file, please check your command parameters or configuration in <HOMEDIR>/pwsafe.json");
//            return;
//        }

//        string password = ConsoleService.ReadPassword();

//        try
//        {
//            var doc = Document.Load(filepath, password);
//            doc.IsReadOnly = true;

//            var entry = doc.Entries.Where(entry => entry.Uuid == id).FirstOrDefault();

//            if (entry != null)
//            {
//                await TextCopy.ClipboardService.SetTextAsync(entry.Password);
//                Console.WriteLine("Copied password to your clipboard");
//            }
//            else
//            {
//                ConsoleService.LogError("Entry is not found");
//            }
//        }
//        catch (Exception ex)
//        {
//            ConsoleService.LogError(ex.Message);
//        }
//    }
//}
