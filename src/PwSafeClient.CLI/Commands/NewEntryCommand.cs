using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Medo.Security.Cryptography.PasswordSafe;
using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;

namespace PwSafeClient.CLI.Commands;

public class NewEntryCommand : Command
{
    public NewEntryCommand() : base("add", "Add a new entry")
    {
        AddOption(new Option<string>(
            aliases: ["--alias", "-a"],
            description: "The alias of the database"
        ));

        AddOption(new Option<FileInfo>(
            aliases: ["--file", "-f"],
            description: "The file path of your database file"
        ));

        AddOption(new Option<string>(
            aliases: ["--title", "-t"],
            description: "The title of the entry"
        ));

        AddOption(new Option<string>(
            aliases: ["--username", "-u"],
            description: "The username of the entry",
            getDefaultValue: () => string.Empty
        ));

        AddOption(new Option<string>(
            aliases: ["--password", "-p"],
            description: "The password of the entry",
            getDefaultValue: () => string.Empty
        ));

        AddOption(new Option<string>(
            aliases: ["--group", "-g"],
            description: "The group of the entry",
            getDefaultValue: () => string.Empty
        ));

        AddOption(new Option<string>(
            name: "--policy",
            description: "The password policy of the entry",
            getDefaultValue: () => string.Empty
        ));

        AddOption(new Option<string>(
            name: "--url",
            description: "The url of the entry",
            getDefaultValue: () => string.Empty
        ));

        AddOption(new Option<string>(
            name: "--email",
            description: "The email of the entry",
            getDefaultValue: () => string.Empty
        ));

        AddOption(new Option<string>(
            name: "--notes",
            description: "The notes of the entry",
            getDefaultValue: () => string.Empty
        ));
    }

    public class NewEntryCommandHandler : CommandHandler {
        private readonly IConsoleService consoleService;
        private readonly IDocumentHelper documentHelper;

        public NewEntryCommandHandler(IConsoleService consoleService, IDocumentHelper documentHelper)
        {
            this.consoleService = consoleService;
            this.documentHelper = documentHelper;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Policy { get; set; } = string.Empty;

        public string Group { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            Document? document = await documentHelper.TryLoadDocumentAsync(Alias, File, false);
            if (document == null)
            {
                return 1;
            }

            NamedPasswordPolicy? namedPasswordPolicy = document.NamedPasswordPolicies.FirstOrDefault(p => p.Name == Policy);

            try
            {
                var entry = new Entry
                {
                    Title = Title,
                    UserName = Username,
                    Password = Password,
                    Group = Group,
                    Url = Url,
                    Email = Email,
                    Notes = Notes
                };

                if (namedPasswordPolicy != null)
                {
                    entry.PasswordPolicy.TotalPasswordLength = namedPasswordPolicy.TotalPasswordLength;
                    entry.PasswordPolicy.MinimumLowercaseCount = namedPasswordPolicy.MinimumLowercaseCount;
                    entry.PasswordPolicy.MinimumUppercaseCount = namedPasswordPolicy.MinimumUppercaseCount;
                    entry.PasswordPolicy.MinimumDigitCount = namedPasswordPolicy.MinimumDigitCount;
                    entry.PasswordPolicy.MinimumSymbolCount = namedPasswordPolicy.MinimumSymbolCount;
                    entry.PasswordPolicy.Style = namedPasswordPolicy.Style;
                    entry.PasswordPolicy.SetSpecialSymbolSet(namedPasswordPolicy.GetSpecialSymbolSet());
                }

                document.Entries.Add(entry);
                document.Save(File!.FullName);

                Console.WriteLine($"Entry {Title} is added to the database");
                return 0;
            }
            catch (Exception ex)
            {
                consoleService.LogError(ex.Message);
                return 1;
            }
        }
    }
}
