using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PwSafeClient.CLI.Commands;
using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Services;

namespace PwSafeClient.CLI;

class Program
{
    public static Parser? Parser;

    static void Main(string[] args)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        Parser = BuildCommandLine()
            .UseHost(_ => Host.CreateDefaultBuilder(),
                host =>
                {
                    host.ConfigureLogging(logger =>
                    {
                        #if DEBUG
                        logger.SetMinimumLevel(LogLevel.Debug);
                        #else
                        logger.SetMinimumLevel(LogLevel.Warning);
                        #endif
                    });

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
                    host.UseCommandHandler<NewEntryCommand, NewEntryCommand.NewEntryCommandHandler>();
                    host.UseCommandHandler<GetPasswordCommand, GetPasswordCommand.GetPasswordCommandHandler>();
                    host.UseCommandHandler<RenewPasswordCommand, RenewPasswordCommand.RenewPasswordCommandHandler>();
                    host.UseCommandHandler<UpdateEntryCommand, UpdateEntryCommand.UpdateEntryCommandHandler>();
                    host.UseCommandHandler<RemoveEntryCommand, RemoveEntryCommand.RemoveEntryCommandHandler>();

                    host.UseCommandHandler<ListPoliciesCommand, ListPoliciesCommand.ListPoliciesCommandHandler>();
                    host.UseCommandHandler<AddPolicyCommand, AddPolicyCommand.AddPolicyCommandHandler>();
                    host.UseCommandHandler<RemovePolicyCommand, RemovePolicyCommand.RemovePolicyCommandHandler>();
                    host.UseCommandHandler<UpdatePolicyCommand, UpdatePolicyCommand.UpdatePolicyCommandHandler>();

                    host.UseCommandHandler<UnlockCommand, UnlockCommand.UnlockCommandHandler>();
                })
            .UseDefaults()
            .Build();

        Parser.Invoke(args);
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
        root.AddCommand(new NewEntryCommand());
        root.AddCommand(new GetPasswordCommand());
        root.AddCommand(new RenewPasswordCommand());
        root.AddCommand(new UpdateEntryCommand());
        root.AddCommand(new RemoveEntryCommand());

        root.AddCommand(new PolicyCommand());

        root.AddCommand(new UnlockCommand());

        return new CommandLineBuilder(root);
    }
}
