using Medo.Security.Cryptography.PasswordSafe;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public static class CreateDbCommand
{
    private static readonly string ext = "psafe3";

    public static RootCommand AddCreateDbCommand(this RootCommand rootCommand)
    {
        Command command = new Command("createdb", "Create an empty new PasswordSafe v3 database file");

        var aliasOption = new Option<string>("--alias", "The alias of the database");
        aliasOption.AddAlias("-a");

        var isDefaultOption = new Option<bool>("--isDefault", "The alias of the database");
        isDefaultOption.SetDefaultValue(false);

        var forceOption = new Option<bool>("--force", "The alias of the database");
        forceOption.SetDefaultValue(false);

        var fileArg = new Argument<FileInfo>("FILE", "The file path of your psafe3 file");

        command.AddArgument(fileArg);
        command.AddOption(aliasOption);
        command.AddOption(isDefaultOption);
        command.AddOption(forceOption);
        command.SetHandler(HandleCreateDbAsync, fileArg, aliasOption, isDefaultOption, forceOption);

        rootCommand.AddCommand(command);
        return rootCommand;
    }

    private static async Task HandleCreateDbAsync(FileInfo file, string alias, bool isDefault, bool force)
    {
        if (file.Exists && !force)
        {
            ConsoleHelper.LogError($"The file '{file.FullName}' has already existed.");
            return;
        }

        var config = await ConsoleHelper.LoadConfigAsync();

        if (config == null)
        {
            ConsoleHelper.LogError($"Not able to load config");
            return;
        }

        if (string.IsNullOrEmpty(alias))
        {
            alias = file.Name.Replace(".psafe3", "");
        }

        if (config.Databases.ContainsKey(alias))
        {
            ConsoleHelper.LogError($"The alias '{alias}' has already existed, please try with another alias");
            return;
        }

        var password = ConsoleHelper.ReadPassword();

        Document document = new Document(password);

        document.Save(file.FullName);

        config.Databases.Add(alias, file.FullName);

        if (isDefault)
        {
            config.DefaultDatabase = alias;
        }

        await ConsoleHelper.UpdateConfigAsync(config);
    }
}
