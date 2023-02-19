using System.CommandLine;
using System.Threading.Tasks;

namespace PwSafeClient.Console.Commands
{
    public static class ListCommand
    {
        public static RootCommand AddListCommand(this RootCommand rootCommand)
        {
            Command command = new Command("list", "List the items in database");

            rootCommand.AddCommand(command);

            return rootCommand;
        }

        private static async Task HandleListCommand(string alias, string file, string password, string group, IConsole console)
        {
        }
    }
}
