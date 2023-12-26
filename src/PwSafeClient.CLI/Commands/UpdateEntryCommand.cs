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

public class UpdateEntryCommand : Command
{
    public UpdateEntryCommand() : base("update", "Update the properties of an entry")
    {
        AddArgument(new Argument<Guid>("ID", "The ID of an entry"));

        AddOption(CommonOptions.AliasOption());

        AddOption(CommonOptions.FileOption());

        AddOption(new Option<string?>(
            aliases: ["--title", "-t"],
            description: "The title of the entry"));

        AddOption(new Option<string?>(
            aliases: ["--username", "-u"],
            description: "The username of the entry"));

        AddOption(new Option<string?>(
            aliases: ["--group", "-g"],
            description: "The group of the entry"));

        AddOption(new Option<string?>(
            name: "--url",
            description: "The url of the entry"));

        AddOption(new Option<string?>(
            name: "--email",
            description: "The email of the entry"));

        AddOption(new Option<string?>(
            name: "--notes",
            description: "The notes of the entry"));
    }

    public class UpdateEntryCommandHandler : CommandHandler
    {
        private readonly IConsoleService consoleService;
        private readonly IDocumentHelper documentHelper;

        public UpdateEntryCommandHandler(IConsoleService consoleService, IDocumentHelper documentHelper)
        {
            this.consoleService = consoleService;
            this.documentHelper = documentHelper;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Username { get; set; }

        public string? Group { get; set; }

        public string? Url { get; set; }

        public string? Email { get; set; }

        public string? Notes { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            Document? document = await documentHelper.TryLoadDocumentAsync(Alias, File, true);
            bool isUpdated = false;

            if (document == null)
            {
                return 1;
            }

            if (document.IsReadOnly)
            {
                consoleService.LogError("The database is readonly.");
                return 1;
            }

            var entry = document.Entries.Where(entry => entry.Uuid == Id).FirstOrDefault();

            if (entry == null)
            {
                consoleService.LogError("Entry is not found.");
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(Title))
            {
                entry.Title = Title;
                isUpdated = true;
            }

            if (Username != null)
            {
                entry.UserName = Username;
                isUpdated = true;
            }

            if (Group != null)
            {
                entry.Group = Group;
                isUpdated = true;
            }

            if (Url != null)
            {
                entry.Url = Url;
                isUpdated = true;
            }

            if (Email != null)
            {
                entry.Email = Email;
                isUpdated = true;
            }

            if (Notes != null)
            {
                entry.Notes = Notes;
                isUpdated = true;
            }

            if (isUpdated)
            {
                await documentHelper.SaveDocumentAsync(Alias, File);
                consoleService.LogSuccess("Entry is updated.");
            }
            else
            {
                Console.WriteLine("Nothing is updated.");
            }

            return 0;
        }
    }
}
