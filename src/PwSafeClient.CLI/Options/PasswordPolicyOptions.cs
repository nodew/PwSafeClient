using System.CommandLine;

namespace PwSafeClient.CLI.Options;

internal static class PasswordPolicyOptions
{
    public const int DefaultPasswordLength = 12;

    public static Option<string> NameOption() => new(
        aliases: ["--name", "-n"],
        description: "The password policy name");

    public static Option<int> DigitsOption() => new(
        aliases: ["--digits", "-d"],
        description: "Use digits and specify the minimum requirement, pass '-1' to disable the option",
        getDefaultValue: () => 0);

    public static Option<int> UppercaseOption() => new(
        aliases: ["--uppercase", "-u"],
        description: "Use uppercase and specify the minimum requirement, pass '-1' to disable the option",
        getDefaultValue: () => 0);

    public static Option<int> LowercaseOption() => new(
        aliases: ["--lowercase", "-l"],
        description: "Use lowercase and specify the minimum requirement, pass '-1' to disable the option",
        getDefaultValue: () => 0);

    public static Option<int> SymbolsOption() => new(
        aliases: ["--symbols", "-s"],
        description: "Use symbols and specify the minimum requirement, pass '-1' to disable the option",
        getDefaultValue: () => 0);

    public static Option<string> SymbolCharsOption() => new(
        aliases: ["--symbol-chars", "-c"],
        description: "Specify the symbol characters");

    public static Option<bool> HexDigitsOnlyOption() => new(
        aliases: ["--hex-only"],
        description: "Use hex digits only characters in the password",
        getDefaultValue: () => false);

    public static Option<bool> EasyVisionOption() => new(
        aliases: ["--easy-vision"],
        description: "Make password easy vision",
        getDefaultValue: () => false);

    public static Option<bool> PronounceableOption() => new(
        aliases: ["--pronounceable"],
        description: "Make password pronouncable",
        getDefaultValue: () => false);

    public static Option<int> LengthOption() => new(
        aliases: ["--length",],
        description: $"Password length, default to be {DefaultPasswordLength} characters",
        getDefaultValue: () => DefaultPasswordLength);
}
