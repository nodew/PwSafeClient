using System.CommandLine;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public static class ChooseDbCommand
{
    public static RootCommand AddChooseDbCommand(this RootCommand rootCommand)
    {
        Command chooseDbCommand = new Command("choose", "Choose a database to operate");
        var aliasArgument = new Argument<string>("ALIAS", "The alias of database");
        chooseDbCommand.AddArgument(aliasArgument);
        chooseDbCommand.SetHandler(ChooseDbAsync, aliasArgument);

        rootCommand.AddCommand(chooseDbCommand);
        return rootCommand;
    }

    public static async Task ChooseDbAsync(string alias)
    {
        if (string.IsNullOrEmpty(alias))
        {
            ConsoleHelper.LogError("Invalid alias");
            return;
        }

        var config = await ConsoleHelper.LoadConfigAsync();

        if (config.Databases.ContainsKey(alias))
        {
            config.DefaultDatabase = alias;

            await ConsoleHelper.UpdateConfigAsync(config);
        }
        else
        {
            ConsoleHelper.LogError("Invalid alias");
        }
    }
}
