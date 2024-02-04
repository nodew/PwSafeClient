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
            var document = await documentHelper.TryLoadDocumentAsync(Alias, File, true);
            if (document == null)
            {
                return 1;
            }

            try
            {
                if (document.NamedPasswordPolicies.Any())
                {
                    foreach (var policy in document.NamedPasswordPolicies)
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

        private static void PrintPasswordPolicy(NamedPasswordPolicy policy)
        {
            var isPronounceable = policy.Style.HasFlag(PasswordPolicyStyle.MakePronounceable);
            var useLowercase = policy.Style.HasFlag(PasswordPolicyStyle.UseLowercase);
            var useUppercase = policy.Style.HasFlag(PasswordPolicyStyle.UseUppercase);
            var useDigits = policy.Style.HasFlag(PasswordPolicyStyle.UseDigits);
            var useSymbols = policy.Style.HasFlag(PasswordPolicyStyle.UseSymbols);
            var useHexDigits = policy.Style.HasFlag(PasswordPolicyStyle.UseHexDigits);
            var useEasyVision = policy.Style.HasFlag(PasswordPolicyStyle.UseEasyVision);
            var symbols = policy.GetSpecialSymbolSet();

            if (symbols.Length == 0 && !isPronounceable)
            {
                symbols = useEasyVision ? PwCharPool.EasyVisionDigitChars : PwCharPool.StdDigitChars;
            }

            var format = "{0,-30}: {1}";

            Console.WriteLine($"---------------- {policy.Name} ----------------");
            Console.WriteLine(format, $"Password length", policy.TotalPasswordLength);
            Console.WriteLine(format, "Use lowercase characters", RenderPolicyValue(useLowercase, isPronounceable ? 0 : policy.MinimumLowercaseCount));
            Console.WriteLine(format, "Use uppercase characters", RenderPolicyValue(useUppercase, isPronounceable ? 0 : policy.MinimumUppercaseCount));
            Console.WriteLine(format, "Use digits", RenderPolicyValue(useDigits, isPronounceable ? 0 : policy.MinimumDigitCount));
            Console.WriteLine(format, "Use symbols", RenderPolicyValue(useSymbols, isPronounceable ? 0 : policy.MinimumSymbolCount, string.Join("", symbols)));
            Console.WriteLine(format, "Use easy vision characters", RenderPolicyValue(useHexDigits, 0));
            Console.WriteLine(format, "Pronounceable passwords", RenderPolicyValue(isPronounceable, 0));
            Console.WriteLine(format, "Hexadecimal characters", RenderPolicyValue(useHexDigits, 0));
            Console.WriteLine();
        }

        private static string RenderPolicyValue(bool enabled, int minimumCount, string? symbols = null)
        {
            if (enabled)
            {
                if (minimumCount > 0)
                {
                    if (!string.IsNullOrWhiteSpace(symbols))
                    {
                        return $"Yes - (At least {minimumCount}) from set '{symbols}";
                    }

                    return $"Yes - (At least {minimumCount})";
                }
                else
                {
                    return "Yes";
                }
            }
            else
            {
                return "No";
            }
        }
    }
}
