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

public class UpdatePolicyCommand : Command
{
    public UpdatePolicyCommand() : base("update", "Update password policy")
    {
        AddOption(CommonOptions.AliasOption());

        AddOption(CommonOptions.FileOption());

        AddOption(PasswordPolicyOptions.NameOption());

        AddOption(PasswordPolicyOptions.DigitsOption());

        AddOption(PasswordPolicyOptions.UppercaseOption());

        AddOption(PasswordPolicyOptions.LowercaseOption());

        AddOption(PasswordPolicyOptions.SymbolsOption());

        AddOption(PasswordPolicyOptions.SymbolCharsOption());

        AddOption(PasswordPolicyOptions.HexDigitsOnlyOption());

        AddOption(PasswordPolicyOptions.EasyVisionOption());

        AddOption(PasswordPolicyOptions.PronounceableOption());

        AddOption(PasswordPolicyOptions.LengthOption());
    }

    public class UpdatePolicyCommandHandler : CommandHandler
    {
        private readonly IConsoleService consoleService;
        private readonly IDocumentHelper documentHelper;

        public UpdatePolicyCommandHandler(IConsoleService consoleService, IDocumentHelper documentHelper)
        {
            this.consoleService = consoleService;
            this.documentHelper = documentHelper;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool HexOnly { get; set; }

        public bool EasyVision { get; set; }

        public bool Pronounceable { get; set; }

        public int Digits { get; set; }

        public int Uppercase { get; set; }

        public int Lowercase { get; set; }

        public int Symbols { get; set; }

        public int Length { get; set; }

        public string? SymbolChars { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            if (!HasValidatedInput())
            {
                return 1;
            }

            Document? document = await documentHelper.TryLoadDocumentAsync(Alias, File, false);
            if (document == null)
            {
                return 1;
            }

            try
            {
                var policy = document.NamedPasswordPolicies.Where(policy => policy.Name == Name).FirstOrDefault();

                if (policy == null)
                {
                    consoleService.LogError($"Password policy '{Name}' is not found");
                    return 1;
                }

                policy.TotalPasswordLength = Length;

                PasswordPolicyStyle style = 0;

                if (HexOnly)
                {
                    style |= PasswordPolicyStyle.UseHexDigits;
                }

                if (EasyVision)
                {
                    style |= PasswordPolicyStyle.UseEasyVision;
                }

                if (Pronounceable)
                {
                    style |= PasswordPolicyStyle.MakePronounceable;
                }

                if (Digits >= 0)
                {
                    style |= PasswordPolicyStyle.UseDigits;
                }

                if (Uppercase >= 0)
                {
                    style |= PasswordPolicyStyle.UseUppercase;
                }

                if (Lowercase >= 0)
                {
                    style |= PasswordPolicyStyle.UseLowercase;
                }

                if (Symbols >= 0)
                {
                    style |= PasswordPolicyStyle.UseSymbols;
                }

                policy.Style = style;
                policy.MinimumDigitCount = FilterNegativeValue(Digits);
                policy.MinimumUppercaseCount = FilterNegativeValue(Uppercase);
                policy.MinimumLowercaseCount = FilterNegativeValue(Lowercase);
                policy.MinimumSymbolCount = FilterNegativeValue(Symbols);

                if (!string.IsNullOrWhiteSpace(SymbolChars))
                {
                    policy.SetSpecialSymbolSet(SymbolChars.ToArray());
                }
                else
                {
                    if (policy.Style.HasFlag(PasswordPolicyStyle.UseEasyVision))
                    {
                        policy.SetSpecialSymbolSet(PwCharPool.EasyVisionSymbolChars);
                    }
                    else
                    {
                        policy.SetSpecialSymbolSet(PwCharPool.StdSymbolChars);
                    }
                }

                await documentHelper.SaveDocumentAsync(Alias, File);
                consoleService.LogSuccess($"Password policy '{policy.Name}' is added");
            }
            catch (Exception ex)
            {
                consoleService.LogError($"Error: {ex.Message}");
                return 1;
            }

            return 0;
        }

        private static int FilterNegativeValue(int value)
        {
            return value < 0 ? 0 : value;
        }

        private bool HasValidatedInput()
        {
            if (Length < 6)
            {
                consoleService.LogError("The password must contain no less than 6 characters.");
                return false;
            }

            int constraintsLength = FilterNegativeValue(Digits) + FilterNegativeValue(Uppercase) + FilterNegativeValue(Lowercase) + FilterNegativeValue(Symbols);

            if (constraintsLength > Length)
            {
                consoleService.LogError("The password length is less than the sum of 'at least' constraints.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(SymbolChars))
            {
                if (PwCharPool.HasDuplicatedCharacters(SymbolChars))
                {
                    consoleService.LogError("The symbol characters contain duplicated characters.");
                    return false;
                }

                if (!PwCharPool.IsValidSymbols(SymbolChars))
                {
                    consoleService.LogError("The symbol characters contain invalid characters.");
                    return false;
                }
            }

            return true;
        }
    }
}
