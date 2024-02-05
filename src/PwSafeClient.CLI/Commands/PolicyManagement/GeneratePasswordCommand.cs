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
using PwSafeClient.Shared;

namespace PwSafeClient.CLI.Commands;

internal class GeneratePasswordCommand : Command
{
    public GeneratePasswordCommand() : base("genpass", "Generate a new password")
    {
        AddOption(new Option<string>(new[] { "--name", "-n" }, "Name of the policy to use"));

        AddOption(CommonOptions.AliasOption());
        AddOption(CommonOptions.FileOption());
    }

    public class GeneratePasswordCommandHandler : CommandHandler
    {
        private readonly IConsoleService consoleService;
        private readonly IDocumentHelper documentHelper;

        public GeneratePasswordCommandHandler(IConsoleService consoleService, IDocumentHelper documentHelper)
        {
            this.consoleService = consoleService;
            this.documentHelper = documentHelper;
        }

        public string? Alias { get; set; }
        public string? Name { get; set; }
        public FileInfo? File { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            var document = await documentHelper.TryLoadDocumentAsync(Alias, File, true);
            if (document == null)
            {
                return 1;
            }

            var namedPolicy = document.NamedPasswordPolicies.FirstOrDefault(p => p.Name == Name);
            if (namedPolicy == null)
            {
                consoleService.LogError($"Policy '{Name}' not found.");
                return 1;
            }

            var policy = new PasswordPolicy(namedPolicy.TotalPasswordLength);
            policy.Style = namedPolicy.Style;
            policy.MinimumLowercaseCount = namedPolicy.MinimumLowercaseCount;
            policy.MinimumUppercaseCount = namedPolicy.MinimumUppercaseCount;
            policy.MinimumDigitCount = namedPolicy.MinimumDigitCount;
            policy.MinimumSymbolCount = namedPolicy.MinimumSymbolCount;
            policy.SetSpecialSymbolSet(namedPolicy.GetSpecialSymbolSet());

            var generator = new PasswordGenerator(policy);
            var password = generator.GeneratePassword();

            if (string.IsNullOrEmpty(password))
            {
                consoleService.LogError("Failed to generate a password.");
                return 1;
            }
            else
            {
                consoleService.LogSuccess(password);
                await TextCopy.ClipboardService.SetTextAsync(password);
                Console.WriteLine("Password copied to clipboard.");
            }

            return 0;
        }
    }
}
