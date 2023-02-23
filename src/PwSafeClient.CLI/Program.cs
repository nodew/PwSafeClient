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
            Description = "PasswordSafe CLI"
        };

        rootCommand
            .AddConfigCommand()
            .AddChooseDbCommand()
            .AddCreateDbCommand()
            .AddShowDbCommand()
            .AddListDbCommand()
            .AddListEntriesCommand()
            .AddNewEntryCommand()
            .AddUpdateEntryCommand()
            .AddRemoveEntryCommand()
            .AddGetPasswordCommand();

        rootCommand.Invoke(args);
    }
}
