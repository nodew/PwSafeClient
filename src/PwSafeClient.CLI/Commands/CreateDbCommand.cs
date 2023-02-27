using Medo.Security.Cryptography.PasswordSafe;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public class CreateDbCommand : Command
{
    public CreateDbCommand() : base("createdb", "Create an empty new PasswordSafe v3 database file")
    {
        AddArgument(new Argument<Guid>("FILE", "The file path of your psafe3 file"));

        AddOption(new Option<string>(
            aliases: new string[] { "--alias", "-a" },
            description: "The alias of the database"
        ));

        AddOption(new Option<FileInfo>(
            aliases: new string[] { "--file", "-f" },
            description: "The file path of your database file"
        ));

        AddOption(new Option<FileInfo>(
            name: "--force",
            description: "Force to create new database if file exists"
        ));

        CommandHandler.Create(Run);
    }

    public static async Task Run(FileInfo file, string alias, bool isDefault, bool force)
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
