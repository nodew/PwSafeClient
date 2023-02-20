using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public static class ConfigCommand
{
    public static RootCommand AddConfigCommand(this RootCommand rootCommand)
    {
        var aliasOption = new Option<string>("--alias", "The alias of the database");
        aliasOption.AddAlias("-a");
        aliasOption.IsRequired = true;

        var fileOption = new Option<FileInfo>("--file", "The file path of your psafe3 file");
        fileOption.AddAlias("-f");
        fileOption.IsRequired = true;

        var IsDefaultOption = new Option<bool>("--isDefault", "If default database");
        IsDefaultOption.SetDefaultValue(false);

        Command configCommand = new Command("config", "Manage your pwsafe config file");

        Command initConfigCommand = new Command("init", "Init your pwsafe config if it doesn't exist");
        initConfigCommand.SetHandler(InitConfigAsync);

        Command setConfigCommand = new Command("set", "Set alias for your psafe3 files");
        setConfigCommand.AddOption(aliasOption);
        setConfigCommand.AddOption(fileOption);
        setConfigCommand.AddOption(IsDefaultOption);
        setConfigCommand.SetHandler(SetConfig, aliasOption, fileOption, IsDefaultOption);

        Command removeConfigCommand = new Command("rm", "Set alias for your psafe3 files");
        var aliasArgument = new Argument<string>("ALIAS", "The alias of database");
        removeConfigCommand.AddArgument(aliasArgument);
        removeConfigCommand.SetHandler(RemoveConfig, aliasArgument);

        configCommand.AddCommand(initConfigCommand);
        configCommand.AddCommand(setConfigCommand);
        configCommand.AddCommand(removeConfigCommand);

        rootCommand.AddCommand(configCommand);

        return rootCommand;
    }

    public static async Task InitConfigAsync()
    {
        var configPath = ConsoleHelper.GetConfigPath();
        if (File.Exists(configPath))
        {
            ConsoleHelper.LogError("The pwsafe.json has already existed");
            return;
        }

        await ConsoleHelper.UpdateConfigAsync(new Config());
    }

    public static async Task SetConfig(string alias, FileInfo file, bool isDefault)
    {
        if (!file.Exists)
        {
            ConsoleHelper.LogError($"{file.FullName} does not exist");
            return;
        }

        var config = await ConsoleHelper.LoadConfigAsync();

        config.Databases.Add(alias, file.FullName);

        if (isDefault)
        {
            config.DefaultDatabase = alias;
        }

        await ConsoleHelper.UpdateConfigAsync(config);
    }

    public static async Task RemoveConfig(string alias)
    {
        var config = await ConsoleHelper.LoadConfigAsync();

        config.Databases.Remove(alias);

        if (config.DefaultDatabase == alias)
        {
            config.DefaultDatabase = string.Empty;
        }

        await ConsoleHelper.UpdateConfigAsync(config);
    }
}
