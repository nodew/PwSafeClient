using Microsoft.Extensions.DependencyInjection;

using PwSafeClient.Cli.Commands;
using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Infrastructure;
using PwSafeClient.Cli.Services;

using Spectre.Console.Cli;

var registrations = new ServiceCollection();

registrations.AddSingleton<IEnvironmentManager, EnvironmentManager>();
registrations.AddSingleton<IConfigManager, ConfigManager>();
registrations.AddSingleton<IDatabaseManager, DatabaseManager>();
registrations.AddSingleton<IDocumentService, DocumentService>();

var registrar = new TypeRegistrar(registrations);

var app = new CommandApp(registrar);

app.Configure(config =>
{
#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif

    config.AddBranch("config", cmd =>
    {
        cmd.SetDescription("Manage the configuration");

        cmd.AddCommand<InitConfigCommand>("init")
            .WithDescription("Initialize the configuration file");
    });

    config.AddBranch("db", cmd =>
    {
        cmd.SetDescription("Manage the pwsafe databases");

        cmd.AddCommand<AddDatabaseCommand>("add")
            .WithDescription("Add a new database to configuration");

        cmd.AddCommand<ListDatabaseCommand>("list")
            .WithAlias("ls")
            .WithDescription("List all configured databases");

        cmd.AddCommand<RemoveDatabaseCommand>("remove")
            .WithAlias("rm")
            .WithDescription("Remove a database from configuration");

        cmd.AddCommand<SetDatabaseCommand>("set")
            .WithDescription("Set the default database");

        cmd.AddCommand<CreateDatabaseCommand>("create")
            .WithDescription("Create an empty database");

        cmd.AddCommand<ShowDatabaseCommand>("show")
            .WithDescription("Show the database information");
    });

    config.AddBranch("entry", cmd =>
    {
        cmd.SetDescription("Manage password safe entries");

        cmd.AddCommand<GetPasswordCommand>("get")
            .WithDescription("Get the password of an entry");

        cmd.AddCommand<ShowEntryCommand>("show")
            .WithDescription("Show the details of an entry");

        cmd.AddCommand<ListEntriesCommand>("list")
            .WithAlias("ls")
            .WithDescription("List all entries in the database");

        cmd.AddCommand<NewEntryCommand>("new")
            .WithDescription("Create a new entry in the database");

        cmd.AddCommand<RemoveEntryCommand>("remove")
            .WithAlias("rm")
            .WithDescription("Remove an entry from the database");

        cmd.AddCommand<UpdateEntryCommand>("update")
            .WithDescription("Update an existing entry in the database");

        cmd.AddCommand<RenewPasswordCommand>("renew")
            .WithDescription("Generate a new password for an existing entry");
    });

    config.AddBranch("policy", cmd =>
    {
        cmd.SetDescription("Manage password policies");

        cmd.AddCommand<ListPoliciesCommand>("list")
            .WithAlias("ls")
            .WithDescription("List all password policies");

        cmd.AddCommand<AddPolicyCommand>("add")
            .WithDescription("Add a new password policy");

        cmd.AddCommand<UpdatePolicyCommand>("update")
            .WithDescription("Update an existing password policy");

        cmd.AddCommand<RemovePolicyCommand>("rm")
            .WithDescription("Remove a password policy");

        cmd.AddCommand<GeneratePasswordCommand>("genpass")
            .WithDescription("Generate a password using a policy");
    });
});

return app.Run(args);
