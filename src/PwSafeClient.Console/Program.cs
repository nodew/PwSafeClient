using PwSafeClient.CLI.Commands;
using System.CommandLine;

namespace PwSafeClient.CLI;

class Program
{
    static void Main(string[] args)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        RootCommand rootCommand = new RootCommand()
        {
            Description = "The best PasswordSafe CLI"
        };

        rootCommand
            .AddCreateDbCommand()
            .AddShowDbCommand()
            .AddListDbCommand()
            .AddListEntriesCommand();

        rootCommand.Invoke(args);
    }
}
