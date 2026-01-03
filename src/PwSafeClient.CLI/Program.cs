using System;
using System.Text;
using System.Linq;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using PwSafeClient.Cli.Commands;
using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Infrastructure;
using PwSafeClient.Cli.Services;

using Spectre.Console;
using Spectre.Console.Cli;

if (OperatingSystem.IsWindows())
{
    try
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }
    catch
    {
        // Ignore encoding failures (some hosts disallow changes).
    }
}

var debug = args.Any(a => string.Equals(a, "--debug", StringComparison.OrdinalIgnoreCase));
if (debug)
{
    CliRuntime.DebugEnabled = true;
    args = args.Where(a => !string.Equals(a, "--debug", StringComparison.OrdinalIgnoreCase)).ToArray();
}

var noColor = args.Any(a => string.Equals(a, "--no-color", StringComparison.OrdinalIgnoreCase));
if (noColor)
{
    args = args.Where(a => !string.Equals(a, "--no-color", StringComparison.OrdinalIgnoreCase)).ToArray();
    AnsiConsole.Console = AnsiConsole.Create(new AnsiConsoleSettings
    {
        Ansi = AnsiSupport.No,
    });
}

var registrations = new ServiceCollection();

var session = new CliSession();
registrations.AddSingleton<ICliSession>(session);

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

    if (debug)
    {
        config.PropagateExceptions();
    }

    config.SetExceptionHandler((ex, _) =>
    {
        CliError.WriteException(ex);
        return ExitCodes.Error;
    });

    config.AddBranch("config", cmd =>
    {
        cmd.SetDescription("Manage the configuration");

        cmd.AddCommand<InitConfigCommand>("init")
            .WithDescription("Initialize the configuration file");

        cmd.AddCommand<ConfigPathCommand>("path")
            .WithDescription("Print the configuration file path");

        cmd.AddCommand<ShowConfigCommand>("show")
            .WithDescription("Show current configuration");

        cmd.AddCommand<SetConfigCommand>("set")
            .WithDescription("Update configuration values");

        cmd.AddCommand<EditConfigCommand>("edit")
            .WithDescription("Open the configuration file in an editor");

        cmd.AddCommand<ResetConfigCommand>("reset")
            .WithDescription("Reset the configuration file (overwrite)");
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

        cmd.AddCommand<SearchEntriesCommand>("search")
            .WithDescription("Search entries by title/username/url/notes");

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

if (args.Length > 0 && string.Equals(args[0], "interactive", StringComparison.OrdinalIgnoreCase))
{
    var (alias, filePath, remainingArgs) = InteractiveMode.ParseInteractiveArgs(args.Skip(1).ToArray());
    if (remainingArgs.Length > 0)
    {
        AnsiConsole.MarkupLine("[red]interactive mode does not accept extra arguments. Use -a/--alias or -f/--file only.[/]");
        return ExitCodes.Error;
    }

    if (!string.IsNullOrWhiteSpace(alias) && !string.IsNullOrWhiteSpace(filePath))
    {
        AnsiConsole.MarkupLine("[red]Use only one of -a/--alias or -f/--file.[/]");
        return ExitCodes.Error;
    }

    session.DefaultAlias = string.IsNullOrWhiteSpace(alias) ? null : alias;
    session.DefaultFilePath = string.IsNullOrWhiteSpace(filePath) ? null : filePath;

    session.UnlockedPassword = InteractiveMode.ReadPasswordFromConsoleOrStdin();
    if (string.IsNullOrEmpty(session.UnlockedPassword))
    {
        AnsiConsole.MarkupLine("[red]No password provided.[/]");
        return ExitCodes.Error;
    }

    var loadedConfig = await ConfigurationLoader.TryLoadAsync();
    var idleTimeMinutes = loadedConfig?.IdleTime ?? 5;

    string promptDisplayName = "pwsafe";
    var resolver = registrar.Build();
    try
    {
        var docService = resolver.Resolve(typeof(IDocumentService)) as IDocumentService;
        if (docService != null)
        {
            promptDisplayName = await docService.GetDocumentDisplayNameAsync(session.DefaultAlias, session.DefaultFilePath);
        }
    }
    finally
    {
        if (resolver is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    return await InteractiveMode.RunAsync(app, session, promptDisplayName, idleTimeMinutes, cts.Token);
}

return app.Run(args);
