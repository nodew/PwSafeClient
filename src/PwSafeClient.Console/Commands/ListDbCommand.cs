using System.CommandLine;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public static class ListDbCommand
{
    public static RootCommand AddListDbCommand(this RootCommand rootCommand)
    {
        Command command = new Command("listdb", "List all databases");
        command.SetHandler(HandleListDbAsync);
        rootCommand.AddCommand(command);
        return rootCommand;
    }

    private static async Task HandleListDbAsync()
    {
        var config = await ConsoleHelper.LoadConfigAsync();
        if (config.Databases.Count == 0)
        {
            System.Console.WriteLine("No database configured, you can use 'createdb' command to create a new one.");
            return;
        }

        foreach (var db in config.Databases)
        {
            System.Console.WriteLine($"{db.Key}: {db.Value}");
        }

        System.Console.WriteLine(string.Format("Default database: {0}", config.DefaultDatabase ?? "Unknown"));
    }
}
