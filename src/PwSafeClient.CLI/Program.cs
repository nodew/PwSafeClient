using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PwSafeClient.CLI.Commands;
using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Services;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;

namespace PwSafeClient.CLI;

class Program
{
    static void Main(string[] args)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        BuildCommandLine()
            .UseHost(_ => Host.CreateDefaultBuilder(),
                host =>
                {
                    host.ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<IConfigManager, ConfigManager>();
                        services.AddSingleton<IEnvironmentManager, EnvironmentManager>();
                        services.AddSingleton<IConsoleService, ConsoleService>();
                        services.AddSingleton<IDocumentHelper, DocumentHelper>();
                    });

                    host.UseCommandHandler<InitConfigCommand, InitConfigCommand.InitConfigCommandHandler>();
                    host.UseCommandHandler<SetAliasCommand, SetAliasCommand.SetAliasCommandHandler>();
                    host.UseCommandHandler<RemoveAliasCommand, RemoveAliasCommand.RemoveAliasCommandHandler>();

                    host.UseCommandHandler<ChooseDbCommand, ChooseDbCommand.ChooseDbCommandHandler>();
                    host.UseCommandHandler<ListDbCommand, ListDbCommand.ListDbCommandHandler>();
                    host.UseCommandHandler<ShowDbCommand, ShowDbCommand.ShowDbCommandHandler>();
                    host.UseCommandHandler<CreateDbCommand, CreateDbCommand.CreateDbCommandHandler>();

                    host.UseCommandHandler<ListEntriesCommand, ListEntriesCommand.ListEntriesCommandHandler>();
                    host.UseCommandHandler<GetPasswordCommand, GetPasswordCommand.GetPasswordCommandHandler>();
                    host.UseCommandHandler<NewEntryCommand, NewEntryCommand.NewEntryCommandHandler>();
                })
            .UseDefaults()
            .Build()
            .Invoke(args);
    }

    private static CommandLineBuilder BuildCommandLine()
    {
        var root = new RootCommand()
        {
            Description = "PasswordSafe CLI"
        };

        root.AddCommand(new ConfigCommand());

        root.AddCommand(new ChooseDbCommand());
        root.AddCommand(new ListDbCommand());
        root.AddCommand(new ShowDbCommand());
        root.AddCommand(new CreateDbCommand());

        root.AddCommand(new ListEntriesCommand());
        root.AddCommand(new GetPasswordCommand());
        root.AddCommand(new NewEntryCommand());
        //root.AddCommand(new UpdateEntryCommand());

        return new CommandLineBuilder(root);
    }
}
