using System.CommandLine;
using System.Threading.Tasks;

namespace PwSafeClient.Console.Commands
{
    public static class ListCommand
    {
        public static RootCommand AddListEntriesCommand(this RootCommand rootCommand)
        {
            Command command = new Command("list", "List the items in database");
            command.SetHandler(HandleListEntriesAsync);
            rootCommand.AddCommand(command);
            return rootCommand;
        }

        private static async Task HandleListEntriesAsync()
        {
            await Task.CompletedTask;
        }
    }
}
