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

public class ListPoliciesCommand : Command
{
    public ListPoliciesCommand() : base("list", "List all password policies")
    {
        AddOption(CommonOptions.AliasOption());

        AddOption(CommonOptions.FileOption());
    }

    public class ListPoliciesCommandHandler : CommandHandler
    {
        private readonly IConsoleService consoleService;
        private readonly IDocumentHelper documentHelper;

        public ListPoliciesCommandHandler(IConsoleService consoleService, IDocumentHelper documentHelper)
        {
            this.consoleService = consoleService;
            this.documentHelper = documentHelper;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            Document? document = await documentHelper.TryLoadDocumentAsync(Alias, File, true);
            if (document == null)
            {
                return 1;
            }

            try
            {
                if (document.NamedPasswordPolicies.Any())
                {
                    foreach (NamedPasswordPolicy policy in document.NamedPasswordPolicies)
                    {
                        PrintPasswordPolicy(policy);
                    }

                    return 0;
                }
                else
                {
                    Console.WriteLine("No password policies found.");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                consoleService.LogError(ex.Message);
                return 1;
            }
        }

        private void PrintPasswordPolicy(NamedPasswordPolicy policy)
        {
            bool isPronounceable = policy.Style.HasFlag(PasswordPolicyStyle.MakePronounceable);
            bool useLowercase = policy.Style.HasFlag(PasswordPolicyStyle.UseLowercase);
            bool useUppercase = policy.Style.HasFlag(PasswordPolicyStyle.UseUppercase);
            bool useDigits = policy.Style.HasFlag(PasswordPolicyStyle.UseDigits);
            bool useSymbols = policy.Style.HasFlag(PasswordPolicyStyle.UseSymbols);

            Console.WriteLine($"{policy.Name}:");
            Console.WriteLine($"    Password length: {policy.TotalPasswordLength}.");

            if (policy.Style.HasFlag(PasswordPolicyStyle.UseHexDigits))
            {
                Console.WriteLine($"    Hex digits only.");
                return;
            }

            Console.WriteLine($"    Pronounceable: {isPronounceable}.");
            Console.WriteLine($"    Easy vision: {policy.Style.HasFlag(PasswordPolicyStyle.UseEasyVision)}.");

            Console.WriteLine();
            Console.WriteLine($"    Use lowercase: {useLowercase}.");
            if (useLowercase && !isPronounceable)
            {
                Console.WriteLine($"    Minimum lowercase: {policy.MinimumLowercaseCount}.");
            }

            Console.WriteLine();
            Console.WriteLine($"    Use uppercase: {useUppercase}.");
            if (useUppercase && !isPronounceable)
            {
                Console.WriteLine($"    Minimum uppercase: {policy.MinimumUppercaseCount}.");
            }

            Console.WriteLine();
            Console.WriteLine($"    Use digits: {useDigits}.");
            if (useDigits && !isPronounceable)
            {
                Console.WriteLine($"    Minimum digits: {policy.MinimumDigitCount}.");
            }

            Console.WriteLine();
            if (!isPronounceable)
            {
                Console.WriteLine($"    Use symbols: {useSymbols}.");

                if (useSymbols)
                {
                    Console.WriteLine($"    Minimum symbols: {policy.MinimumSymbolCount}.");
                    Console.WriteLine($"    Symbols: {policy.GetSpecialSymbolSet()}.");
                }
            }
        }
    }
}
