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

public class AddPolicyCommand : Command
{
    public AddPolicyCommand() : base("add", "Add new password policy")
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

    public class AddPolicyCommandHandler : CommandHandler
    {
        private readonly IConsoleService consoleService;
        private readonly IDocumentHelper documentHelper;

        public AddPolicyCommandHandler(IConsoleService consoleService, IDocumentHelper documentHelper)
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

            if (HexOnly && (Digits > 0 || Uppercase > 0 || Lowercase > 0 || Symbols > 0))
            {
                if (!consoleService.DoConfirm("When --hex-ony is set, all other options will be ignored, continue?"))
                {
                    return 0;
                }
            }

            var document = await documentHelper.TryLoadDocumentAsync(Alias, File, false);
            if (document == null)
            {
                return 1;
            }

            try
            {
                if (document.NamedPasswordPolicies.Any(policy => policy.Name == Name))
                {
                    consoleService.LogError($"The password policy with name '{Name}' already exists.");
                }

                var policy = new NamedPasswordPolicy(Name, Length);

                PasswordPolicyStyle style = 0;
                var minimumDigitCount = 0;
                var minimumUppercaseCount = 0;
                var minimumLowercaseCount = 0;
                var minimumSymbolCount = 0;

                if (HexOnly)
                {
                    style |= PasswordPolicyStyle.UseHexDigits;
                }
                else
                {
                    if (Pronounceable)
                    {
                        style |= PasswordPolicyStyle.MakePronounceable;
                    }

                    if (EasyVision)
                    {
                        style |= PasswordPolicyStyle.UseEasyVision;
                    }

                    if (Digits >= 0)
                    {
                        style |= PasswordPolicyStyle.UseDigits;
                        minimumDigitCount = Pronounceable ? 0 : Digits;
                    }

                    if (Uppercase >= 0)
                    {
                        style |= PasswordPolicyStyle.UseUppercase;
                        minimumUppercaseCount = Pronounceable ? 0 : Uppercase;
                    }

                    if (Lowercase >= 0)
                    {
                        style |= PasswordPolicyStyle.UseLowercase;
                        minimumLowercaseCount = Pronounceable ? 0 : Lowercase;
                    }

                    if (Symbols >= 0)
                    {
                        style |= PasswordPolicyStyle.UseSymbols;
                        minimumSymbolCount = Pronounceable ? 0 : Symbols;
                    }
                }

                policy.Style = style;
                policy.MinimumDigitCount = minimumDigitCount;
                policy.MinimumUppercaseCount = minimumUppercaseCount;
                policy.MinimumLowercaseCount = minimumLowercaseCount;
                policy.MinimumSymbolCount = minimumSymbolCount;

                if (Pronounceable)
                {
                    policy.SetSpecialSymbolSet(PwCharPool.PronounceableSymbolChars);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(SymbolChars))
                    {
                        policy.SetSpecialSymbolSet(SymbolChars.ToArray());
                    }
                    else
                    {
                        if (EasyVision)
                        {
                            policy.SetSpecialSymbolSet(PwCharPool.EasyVisionSymbolChars);
                        }
                        else
                        {
                            policy.SetSpecialSymbolSet(PwCharPool.StdSymbolChars);
                        }
                    }
                }

                document.NamedPasswordPolicies.Add(policy);

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

            var constraintsLength = FilterNegativeValue(Digits) + FilterNegativeValue(Uppercase) + FilterNegativeValue(Lowercase) + FilterNegativeValue(Symbols);

            if (constraintsLength > Length)
            {
                consoleService.LogError("The password length is less than the sum of 'at least' constraints.");
                return false;
            }

            if (EasyVision && Pronounceable)
            {
                Console.WriteLine("The options '--easy-vision' and '--pronounceable' cannot be used together.");
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
