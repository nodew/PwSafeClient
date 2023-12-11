using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Medo.Security.Cryptography.PasswordSafe;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.Shared;

namespace PwSafeClient.CLI.Commands;

public enum EntriesViewMode
{
    List = 0,
    Tree
}

public class ListEntriesCommand : Command {
    public ListEntriesCommand() : base("list", "List the items in database")
    {
        AddOption(new Option<string>(
            aliases: ["--alias", "-a"],
            description: "The alias of the database"
        ));

        AddOption(new Option<FileInfo>(
            aliases: ["--file", "-f"],
            description: "The file path of your database file"
        ));

        AddOption(new Option<EntriesViewMode>(
            aliases: ["--mode", "-m"],
            description: "The view mode, list view or tree view",
            getDefaultValue: () => EntriesViewMode.List
        ));

        AddOption(new Option<string>(
            name: "--filter",
            description: "Filter items by title"
        ));
    }

    public class ListEntriesCommandHandler : CommandHandler
    {
        private readonly IConfigManager configManager;
        private readonly IConsoleService consoleService;

        public ListEntriesCommandHandler(IConfigManager configManager, IConsoleService consoleService)
        {
            this.configManager = configManager;
            this.consoleService = consoleService;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public EntriesViewMode Mode { get; set; }

        public string? Filter { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            string filepath;

            if (File != null)
            {
                filepath = File.FullName;
            }
            else
            {
                filepath = await configManager.GetDbPath(Alias);
            }

            if (!System.IO.File.Exists(filepath))
            {
                consoleService.LogError($"Can't locate a valid file, please check your command parameters or configuration in <HOMEDIR>/pwsafe.json");
                return 1;
            }

            string password = consoleService.ReadPassword();

            try
            {
                var doc = Document.Load(filepath, password);
                doc.IsReadOnly = true;

                List<Entry> entries = doc.Entries.ToList();

                if (!string.IsNullOrEmpty(Filter))
                {
                    entries = entries.Where(entry => entry.Title.ToLower().Contains(Filter.ToLower())).ToList();
                }

                if (entries.Count == 0)
                {
                    Console.WriteLine("No available item.");
                    return 0;
                }

                if (Mode == EntriesViewMode.List)
                {
                    PrintListView(entries);
                }
                else if (Mode == EntriesViewMode.Tree)
                {
                    PrintTreeView(entries);
                }

                return 0;
            }
            catch (Exception ex)
            {
                consoleService.LogError(ex.ToString());
                return 1;
            }
        }

        private static void PrintListView(List<Entry> entries)
        {
            var fmt = "{0,-36} | {1,-40} | {2,-32} | {3}";
            var oderedEntries = entries.OrderBy(entry => entry.Title);

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine(fmt, "Uuid", "Title", "Username", "Group");

            foreach (Entry entry in oderedEntries)
            {
                Console.WriteLine(fmt, entry.Uuid, entry.Title, entry.UserName, entry.Group);
            }
        }

        private static void PrintTreeView(List<Entry> entries)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var group = PwSafeClientHelper.GetGroupInfo(entries);
            PrintTreeView(entries, group, 0);
        }

        private static void PrintTreeView(List<Entry> entries, Group group, int depth)
        {
            var groupPath = group.GetGroupPath();

            var subEntries = entries
                .Where(entry => entry.Group == groupPath)
                .OrderBy(entry => entry.Title);

            if (depth >= 1)
            {
                Console.WriteLine("{0}|- {1}", new string(' ', (depth - 1) * 2), group.Name);
            }


            foreach (Entry entry in subEntries)
            {
                var fmt = "{0}|- {1}({2}) [{3}]";
                Console.WriteLine(fmt, new string(' ', depth * 2), entry.Title, entry.UserName, entry.Uuid);
            }

            foreach (Group child in group.Children)
            {
                PrintTreeView(entries, child, depth + 1);
            }
        }
    }
}
