using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Options;

namespace PwSafeClient.CLI.Commands;

public class ShowDbCommand : Command
{
    public ShowDbCommand() : base("showdb", "Show the detail of PasswordSafe database")
    {
        AddOption(CommonOptions.AliasOption());

        AddOption(CommonOptions.FileOption());
    }

    public class ShowDbCommandHandler : CommandHandler
    {
        private readonly IDocumentHelper documentHelper;
        private readonly IConsoleService consoleService;

        public ShowDbCommandHandler(IDocumentHelper documentHelper, IConsoleService consoleService)
        {
            this.documentHelper = documentHelper;
            this.consoleService = consoleService;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            Document? doc = await documentHelper.TryLoadDocumentAsync(Alias, File, true);

            if (doc == null)
            {
                return 1;
            }

            string format = "{0, -30}{1}";
            Console.WriteLine(format, "Database UUID:", doc.Uuid);
            Console.WriteLine(format, "Name:", doc.Name ?? "-");
            Console.WriteLine(format, "Description:", doc.Description ?? "-");
            Console.WriteLine(format, "Version", doc.Version);
            Console.WriteLine(format, "Last saved by:", doc.LastSaveUser);
            Console.WriteLine(format, "Last saved on:", doc.LastSaveTime);
            Console.WriteLine(format, "Last saved application: ", doc.LastSaveApplication);
            Console.WriteLine(format, "Last saved machine: ", doc.LastSaveHost);
            Console.WriteLine(format, "Last saved user: ", doc.LastSaveUser);
            Console.WriteLine(format, "Item count: ", doc.Entries.Count);

            return 0;
        }
    }
}
