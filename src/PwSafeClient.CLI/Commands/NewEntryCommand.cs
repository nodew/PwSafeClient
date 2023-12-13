using Medo.Security.Cryptography.PasswordSafe;
using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Options;
using PwSafeClient.Shared;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public class NewEntryCommand : Command
{
    public NewEntryCommand() : base("add", "Add a new entry")
    {
        AddOption(CommonOptions.AliasOption());

        AddOption(CommonOptions.FileOption());

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

    public class NewEntryCommandHandler : CommandHandler
    {
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

            if (document.IsReadOnly)
            {
                consoleService.LogError("The database is readonly");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                consoleService.LogError("The title is required");
                return 1;
            }

            if (document.Entries.Any(e => e.Title == Title && e.Group.ToString() == Group))
            {
                consoleService.LogError($"The entry {Title} already exists under the group {Group}");
                return 1;
            }

            try
            {
                var entry = new Entry()
                {
                    Title = Title,
                    UserName = Username,
                    Password = Password,
                    Group = new GroupPath(Group),
                    Url = Url,
                    Email = Email,
                    Notes = Notes
                };

                if (string.IsNullOrWhiteSpace(Password))
                {
                    NamedPasswordPolicy? namedPasswordPolicy = null;

                    if (!string.IsNullOrEmpty(Policy))
                    {
                        namedPasswordPolicy = document.NamedPasswordPolicies.FirstOrDefault(p => p.Name == Policy);
                        if (namedPasswordPolicy == null)
                        {
                            consoleService.LogError($"The password policy {Policy} is not found");
                            return 1;
                        }
                    }

                    Console.WriteLine(entry.PasswordPolicy.TotalPasswordLength);
                    Console.WriteLine(entry.PasswordPolicy.MinimumLowercaseCount);
                    Console.WriteLine(entry.PasswordPolicy.MinimumUppercaseCount);
                    Console.WriteLine(entry.PasswordPolicy.MinimumDigitCount);
                    Console.WriteLine(entry.PasswordPolicy.MinimumSymbolCount);
                    Console.WriteLine(entry.PasswordPolicy.Style);
                    Console.WriteLine(entry.PasswordPolicy.GetSpecialSymbolSet());

                    if (namedPasswordPolicy != null)
                    {
                        entry.PasswordPolicy.TotalPasswordLength = namedPasswordPolicy.TotalPasswordLength;
                        entry.PasswordPolicy.MinimumLowercaseCount = namedPasswordPolicy.MinimumLowercaseCount;
                        entry.PasswordPolicy.MinimumUppercaseCount = namedPasswordPolicy.MinimumUppercaseCount;
                        entry.PasswordPolicy.MinimumDigitCount = namedPasswordPolicy.MinimumDigitCount;
                        entry.PasswordPolicy.MinimumSymbolCount = namedPasswordPolicy.MinimumSymbolCount;
                        entry.PasswordPolicy.Style = namedPasswordPolicy.Style;
                        entry.PasswordPolicy.SetSpecialSymbolSet(namedPasswordPolicy.GetSpecialSymbolSet());

                        entry.PasswordPolicyName = namedPasswordPolicy.Name;
                        entry.Password = new PasswordGenerator(entry.PasswordPolicy).GeneratePassword();
                        Console.WriteLine($"Generated password!");
                    }

                    Console.WriteLine(entry.PasswordPolicy.TotalPasswordLength);
                    Console.WriteLine(entry.PasswordPolicy.MinimumLowercaseCount);
                    Console.WriteLine(entry.PasswordPolicy.MinimumUppercaseCount);
                    Console.WriteLine(entry.PasswordPolicy.MinimumDigitCount);
                    Console.WriteLine(entry.PasswordPolicy.MinimumSymbolCount);
                    Console.WriteLine(entry.PasswordPolicy.Style);
                    Console.WriteLine(entry.PasswordPolicy.GetSpecialSymbolSet());
                }

                document.Entries.Add(entry);

                await documentHelper.SaveDocumentAsync(Alias, File);

                Console.WriteLine($"Entry {Title} is added to the database");
                return 0;
            }
            catch (Exception ex)
            {
                consoleService.LogError(ex.Message);
                throw;
            }
        }
    }
}
