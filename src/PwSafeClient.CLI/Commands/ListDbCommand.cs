using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Threading.Tasks;
using PwSafeClient.CLI.Helpers;

namespace PwSafeClient.CLI.Commands;

public class ListDbCommand : Command
{
    public ListDbCommand() : base("listdb", "List all databases")
    {
        Handler = CommandHandler.Create(Run);
    }

    private static async Task Run()
    {
        var config = await ConsoleHelper.LoadConfigAsync();
        if (config.Databases.Count == 0)
        {
            Console.WriteLine("No database configured, you can use 'createdb' command to create a new one.");
            return;
        }

        foreach (var db in config.Databases)
        {
            Console.WriteLine($"{db.Key}: {db.Value}");
        }

        Console.WriteLine(string.Format("Default database: {0}", config.DefaultDatabase ?? "Unknown"));
    }
}
