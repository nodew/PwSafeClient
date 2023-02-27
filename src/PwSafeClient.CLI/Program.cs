using Microsoft.Extensions.Hosting;
using PwSafeClient.CLI.Commands;
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
                        // TODO: Add services here
                    });
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
        root.AddCommand(new UpdateEntryCommand());
        root.AddCommand(new GetPasswordCommand());

        return new CommandLineBuilder(root);
    }
}
