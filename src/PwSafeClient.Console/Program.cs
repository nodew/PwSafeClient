using System.CommandLine;
using PwSafeClient.Console.Commands;

namespace PwSafeClient.Console
{
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
}
