using System.CommandLine;
using System.Threading.Tasks;

namespace PwSafeClient.Console.Commands
{
    public static class ShowDbCommand
    {
        public static RootCommand AddShowDbCommand(this RootCommand rootCommand)
        {
            Command command = new Command("showdb", "Show the detail of PasswordSafe database");
            command.SetHandler(HandleShowDbAsync);
            rootCommand.AddCommand(command);
            return rootCommand;
        }

        private static async Task HandleShowDbAsync()
        {
            await Task.CompletedTask;
        }
    }
}
