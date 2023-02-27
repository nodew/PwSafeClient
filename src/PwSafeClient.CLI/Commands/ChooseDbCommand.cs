using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public class ChooseDbCommand : Command
{
    public ChooseDbCommand() : base("choose", "Choose a database to operate")
    {
        AddArgument(new Argument<string>("ALIAS", "The alias of the database"));

        Handler = CommandHandler.Create(Run);
    }

    public static async Task Run(string alias)
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
            Console.WriteLine($"'{alias}' is selected as default database");
        }
        else
        {
            ConsoleHelper.LogError("Invalid alias");
        }
    }
}
