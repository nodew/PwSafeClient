namespace PwSafeClient.CLI.Commands;

public enum EntriesViewMode
{
    List = 0,
    Tree
}

//public class ListEntriesCommand : Command
//{
//    public ListEntriesCommand() : base("list", "List the items in database")
//    {
//        var aliasOption = new Option<string>("--alias", "The alias of the database");
//        aliasOption.AddAlias("-a");

//        var fileOption = new Option<FileInfo>("--file", "The file path of your psafe3 file");
//        fileOption.AddAlias("-f");

//        var viewModeOption = new Option<EntriesViewMode>("--mode", "The view mode, list view or tree view");
//        viewModeOption.AddAlias("-m");
//        viewModeOption.SetDefaultValue(EntriesViewMode.List);

//        var filterOption = new Option<string>("--filter", "Filter items by title");

//        AddOption(new Option<string>(
//            aliases: new string[] { "--alias", "-a" },
//            description: "The alias of the database"
//        ));

//        AddOption(new Option<FileInfo>(
//            aliases: new string[] { "--file", "-f" },
//            description: "The file path of your database file"
//        ));

//        AddOption(new Option<EntriesViewMode>(
//            aliases: new string[] { "--mode", "-m" },
//            description: "The view mode, list view or tree view",
//            getDefaultValue: () => EntriesViewMode.List
//        ));

//        AddOption(new Option<string>(
//            name: "--filter",
//            description: "Filter items by title"
//        ));

//        Handler = CommandHandler.Create(Run);
//    }

//    private static async Task Run(string alias, FileInfo? file, EntriesViewMode mode, string? filter)
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

//            List<Entry> entries = doc.Entries.ToList();

//            if (!string.IsNullOrEmpty(filter))
//            {
//                entries = entries.Where(entry => entry.Title.ToLower().Contains(filter.ToLower())).ToList();
//            }

//            if (entries.Count == 0)
//            {
//                Console.WriteLine("No available item.");
//                return;
//            }

//            if (mode.Equals(EntriesViewMode.List))
//            {
//                PrintListView(entries);
//            }
//            else if (mode.Equals(EntriesViewMode.Tree))
//            {
//                PrintTreeView(entries);
//            }
//        }
//        catch (Exception ex)
//        {
//            ConsoleService.LogError(ex.ToString());
//        }
//    }

//    private static void PrintListView(List<Entry> entries)
//    {
//        var fmt = "{0,-36} | {1,-40} | {2,-32} | {3}";
//        var oderedEntries = entries.OrderBy(entry => entry.Title);

//        Console.OutputEncoding = System.Text.Encoding.UTF8;
//        Console.WriteLine(fmt, "Uuid", "Title", "Username", "Group");

//        foreach (Entry entry in oderedEntries)
//        {
//            Console.WriteLine(fmt, entry.Uuid, entry.Title, entry.UserName, entry.Group);
//        }
//    }

//    private static void PrintTreeView(List<Entry> entries)
//    {
//        Console.OutputEncoding = System.Text.Encoding.UTF8;

//        var group = PwSafeClientHelper.GetGroupInfo(entries);
//        PrintTreeView(entries, group, 0);
//    }

//    private static void PrintTreeView(List<Entry> entries, Group group, int depth)
//    {
//        var groupPath = group.GetGroupPath();

//        var subEntries = entries
//            .Where(entry => entry.Group == groupPath)
//            .OrderBy(entry => entry.Title);

//        if (depth >= 1)
//        {
//            Console.WriteLine("{0}|- {1}", new string(' ', (depth - 1) * 2), group.Name);
//        }


//        foreach (Entry entry in subEntries)
//        {
//            var fmt = "{0}|- {1}({2}) [{3}]";
//            Console.WriteLine(fmt, new string(' ', depth * 2), entry.Title, entry.UserName, entry.Uuid);
//        }

//        foreach (Group child in group.Children)
//        {
//            PrintTreeView(entries, child, depth + 1);
//        }
//    }
//}
