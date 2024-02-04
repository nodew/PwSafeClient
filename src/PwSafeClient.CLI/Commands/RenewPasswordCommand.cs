using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Options;
using PwSafeClient.Shared;

namespace PwSafeClient.CLI.Commands;

public class RenewPasswordCommand : Command
{
    private static readonly Option<string?> PasswordOption = new Option<string?>(
        aliases: ["--password", "-p"],
        description: "The password of the entry");

    public RenewPasswordCommand() : base("renew", "Renew the password of an entry")
    {
        AddArgument(new Argument<Guid>("ID", "The ID of an entry"));

        AddOption(CommonOptions.AliasOption());

        AddOption(CommonOptions.FileOption());

        AddOption(PasswordOption);

        AddOption(new Option<string?>(
                      name: "--policy",
                      description: "The password policy of the entry"));
    }

    public class RenewPasswordCommandHandler : CommandHandler
    {
        private readonly IConsoleService consoleService;
        private readonly IDocumentHelper documentHelper;

        public RenewPasswordCommandHandler(IConsoleService consoleService, IDocumentHelper documentHelper)
        {
            this.consoleService = consoleService;
            this.documentHelper = documentHelper;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public Guid Id { get; set; }

        public string? Password { get; set; }

        public string? Policy { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            var document = await documentHelper.TryLoadDocumentAsync(Alias, File, true);
            var newPassword = string.Empty;

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

            if (context.ParseResult.HasOption(PasswordOption))
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    newPassword = consoleService.ReadPassword();
                }
                else
                {
                    newPassword = Password;
                }
            }

            if (string.IsNullOrEmpty(newPassword) && !string.IsNullOrWhiteSpace(Policy))
            {
                var namedPasswordPolicy = document.NamedPasswordPolicies.FirstOrDefault(p => p.Name == Policy);

                if (namedPasswordPolicy == null)
                {
                    consoleService.LogError($"The password policy {Policy} is not found.");
                    return 1;
                }

                entry.PasswordPolicy.TotalPasswordLength = namedPasswordPolicy.TotalPasswordLength;
                entry.PasswordPolicy.MinimumLowercaseCount = namedPasswordPolicy.MinimumLowercaseCount;
                entry.PasswordPolicy.MinimumUppercaseCount = namedPasswordPolicy.MinimumUppercaseCount;
                entry.PasswordPolicy.MinimumDigitCount = namedPasswordPolicy.MinimumDigitCount;
                entry.PasswordPolicy.MinimumSymbolCount = namedPasswordPolicy.MinimumSymbolCount;
                entry.PasswordPolicy.Style = namedPasswordPolicy.Style;
                entry.PasswordPolicy.SetSpecialSymbolSet(namedPasswordPolicy.GetSpecialSymbolSet());
                entry.PasswordPolicyName = namedPasswordPolicy.Name;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                newPassword = new PasswordGenerator(entry.PasswordPolicy).GeneratePassword();
            }

            entry.Password = newPassword;

            await documentHelper.SaveDocumentAsync(Alias, File);
            return 0;
        }
    }
}
