using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Options;

namespace PwSafeClient.CLI.Commands;

public class GetPasswordCommand : Command
{
    public GetPasswordCommand() : base("get", "Get the password")
    {
        AddArgument(new Argument<Guid>("ID", "The ID of an entry"));

        AddOption(CommonOptions.AliasOption());

        AddOption(CommonOptions.FileOption());
    }

    public class GetPasswordCommandHandler : CommandHandler
    {
        private readonly IConsoleService consoleService;
        private readonly IDocumentHelper documentHelper;

        public GetPasswordCommandHandler(IConsoleService consoleService, IDocumentHelper documentHelper)
        {
            this.consoleService = consoleService;
            this.documentHelper = documentHelper;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public Guid Id { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            Document? document = await documentHelper.TryLoadDocumentAsync(Alias, File, true);
            if (document == null)
            {
                return 1;
            }

            try
            {
                var entry = document.Entries.Where(entry => entry.Uuid == Id).FirstOrDefault();

                if (entry != null)
                {
                    await TextCopy.ClipboardService.SetTextAsync(entry.Password);
                    Console.WriteLine("Copied password to your clipboard");
                    return 0;
                }
                else
                {
                    consoleService.LogError("Entry is not found");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                consoleService.LogError(ex.Message);
                return 1;
            }
        }
    }
}
