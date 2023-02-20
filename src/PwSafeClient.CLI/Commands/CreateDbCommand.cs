using System.CommandLine;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public static class CreateDbCommand
{
    private static readonly string ext = "psafe3";

    public static RootCommand AddCreateDbCommand(this RootCommand rootCommand)
    {
        Command command = new Command("createdb", "Create an empty new PasswordSafe v3 database file");
        command.SetHandler(HandleCreateDbAsync);
        rootCommand.AddCommand(command);
        return rootCommand;
    }

    private static async Task HandleCreateDbAsync()
    {
        await Task.CompletedTask;
    }
}
