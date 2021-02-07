using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
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
                .AddShowDbCommand();

            rootCommand.Invoke(args);
        }
    }
}
