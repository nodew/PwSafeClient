using System.CommandLine;

namespace PwSafeClient.CLI.Options;

internal static class PasswordPolicyOptions
{
    public const int DefaultPasswordLength = 12;

    public static Option<string> NameOption() => new Option<string>(
        aliases: ["--name", "-n"],
        description: "The password policy name");

    public static Option<int> DigitsOption() => new Option<int>(
        aliases: ["--digits", "-d"],
        description: "Use digits",
        getDefaultValue: () => 0);

    public static Option<int> UppercaseOption() => new Option<int>(
        aliases: ["--uppercase", "-u"],
        description: "Use upper case",
        getDefaultValue: () => 0);

    public static Option<int> LowercaseOption() => new Option<int>(
        aliases: ["--lowercase", "-l"],
        description: "Use lower case",
        getDefaultValue: () => 0);

    public static Option<int> SymbolsOption() => new Option<int>(
        aliases: ["--symbols", "-s"],
        description: "Use symbols",
        getDefaultValue: () => 0);

    public static Option<string> SymbolCharsOption() => new Option<string>(
        aliases: ["--symbol-chars", "-c"],
        description: "Specify the symbol characters");

    public static Option<bool> HexDigitsOnlyOption() => new Option<bool>(
        aliases: ["--hex-only"],
        description: "Use hex digits only");

    public static Option<bool> EasyVisionOption() => new Option<bool>(
        aliases: ["--easy-vision"],
        description: "Make password easy vision");

    public static Option<int> LengthOption() => new Option<int>(
        aliases: ["--length",],
        description: $"Password length, default to be {DefaultPasswordLength} characters",
        getDefaultValue: () => DefaultPasswordLength);
}
