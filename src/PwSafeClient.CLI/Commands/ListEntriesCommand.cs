using Medo.Security.Cryptography.PasswordSafe;
using PwSafeClient.Core;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public enum EntriesViewMode
{
    List = 0,
    Tree
}

public static class ListCommand
{
    public static RootCommand AddListEntriesCommand(this RootCommand rootCommand)
    {
        Command command = new Command("list", "List the items in database");

        var aliasOption = new Option<string>("--alias", "The alias of the database");
        aliasOption.AddAlias("-a");

        var fileOption = new Option<FileInfo>("--file", "The file path of your psafe3 file");
        fileOption.AddAlias("-f");

        var viewModeOption = new Option<EntriesViewMode>("--mode", "The view mode, list view or tree view");
        viewModeOption.AddAlias("-m");
        viewModeOption.SetDefaultValue(EntriesViewMode.List);

        var filterOption = new Option<string>("--filter", "Filter items by title");

        command.AddOption(aliasOption);
        command.AddOption(fileOption);
        command.AddOption(viewModeOption);
        command.AddOption(filterOption);
        command.SetHandler(HandleListEntriesAsync, aliasOption, fileOption, viewModeOption, filterOption);

        rootCommand.AddCommand(command);
        return rootCommand;
    }

    private static async Task HandleListEntriesAsync(string alias, FileInfo? file, EntriesViewMode viewMode, string? filter)
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

            List<Entry> entries = doc.Entries.ToList();

            if (!string.IsNullOrEmpty(filter))
            {
                entries = entries.Where(entry => entry.Title.ToLower().Contains(filter.ToLower())).ToList();
            }

            if (entries.Count == 0)
            {
                Console.WriteLine("No available item.");
                return;
            }

            if (viewMode.Equals(EntriesViewMode.List))
            {
                PrintListView(entries);
            }
            else if (viewMode.Equals(EntriesViewMode.Tree))
            {
                PrintTreeView(entries);
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.LogError(ex.ToString());
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
