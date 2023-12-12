namespace PwSafeClient.Shared;

public static class PwCharPool {
    public static readonly char[] StdLowercaseChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

    public static readonly char[] StdUppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public static readonly char[] StdDigitChars = "0123456789".ToCharArray();

    public static readonly char[] StdHexDigitChars = "0123456789abcdef".ToCharArray();

    public static readonly char[] StdSymbolChars = "+-=_@#$%^&;:,.<>/~\\[](){}?!|*".ToCharArray();

    public static readonly char[] EasyVisionLowercaseChars = "abcdefghijkmnopqrstuvwxyz".ToCharArray();

    public static readonly char[] EasyVisionUppercase_chars = "ABCDEFGHJKLMNPQRTUVWXY".ToCharArray();

    public static readonly char[] EasyVisionDigitChars = "346789".ToCharArray();

    public static readonly char[] EasyVisionSymbolChars = "+-=_@#$%^&<>/~\\?*".ToCharArray();

    public static readonly char[] EasyVisionHexDigitChars = "0123456789abcdef".ToCharArray();

    public static readonly char[] PronounceableSymbolChars = "@&(#!|$+".ToCharArray();
}