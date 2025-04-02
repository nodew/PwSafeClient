using System;
using System.Collections.Generic;
using System.Linq;

namespace PwSafeClient.Shared;

public static class PwCharPool
{
    public static readonly char[] StdLowercaseChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

    public static readonly char[] StdUppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public static readonly char[] LowercaseVowels = "aeiouy".ToCharArray();

    public static readonly char[] UppercaseVowels = "AEIOUY".ToCharArray();

    public static readonly char[] LowercaseConsonants = "bcdfghjklmnpqrstvwxz".ToCharArray();

    public static readonly char[] UppercaseConsonants = "BCDFGHJKLMNPQRSTVWXZ".ToCharArray();

    public static readonly char[] StdDigitChars = "0123456789".ToCharArray();

    public static readonly char[] StdHexDigitChars = "0123456789abcdef".ToCharArray();

    public static readonly char[] StdSymbolChars = "+-=_@#$%^&;:,.<>/~\\[](){}?!|*".ToCharArray();

    public static readonly char[] EasyVisionLowercaseChars = "abcdefghijkmnopqrstuvwxyz".ToCharArray();

    public static readonly char[] EasyVisionUppercaseChars = "ABCDEFGHJKLMNPQRTUVWXY".ToCharArray();

    public static readonly char[] EasyVisionDigitChars = "346789".ToCharArray();

    public static readonly char[] EasyVisionSymbolChars = "+-=_@#$%^&<>/~\\?*".ToCharArray();

    public static readonly char[] EasyVisionHexDigitChars = "0123456789abcdef".ToCharArray();

    public static readonly char[] PronounceableSymbolChars = "@&(#!|$+".ToCharArray();

    public static bool IsInCharPool(char c, char[] charPool)
    {
        ArgumentNullException.ThrowIfNull(charPool);

        return Array.IndexOf(charPool, c) >= 0;
    }

    public static bool IsValidSymbolChar(char c)
    {
        return IsInCharPool(c, StdSymbolChars);
    }

    public static bool HasDuplicatedCharacters(string content)
    {
        var set = new HashSet<char>(content);
        return set.Count != content.Length;
    }

    public static bool IsValidSymbols(string symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);

        if (HasDuplicatedCharacters(symbols))
        {
            return false;
        }

        return symbols.All(IsValidSymbolChar);
    }
}
