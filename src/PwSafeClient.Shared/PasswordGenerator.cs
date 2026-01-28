using System;
using System.Collections.Generic;

using Medo.Security.Cryptography.PasswordSafe;

namespace PwSafeClient.Shared;

public class PasswordGenerator
{
    private readonly PasswordPolicy passwordPolicy;

    /// <summary>
    /// Initialize a new instance of the <see cref="PasswordGenerator"/>.
    /// </summary>
    /// <param name="passwordPolicy"></param>
    public PasswordGenerator(PasswordPolicy passwordPolicy)
    {
        this.passwordPolicy = passwordPolicy;
    }

    /// <summary>
    /// Generate a password based on the password policy.
    /// </summary>
    /// <returns></returns>
    public string GeneratePassword()
    {
        var useHexDigits = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseHexDigits);
        var makePronounceable = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.MakePronounceable);

        if (passwordPolicy.TotalPasswordLength <= 0)
        {
            return string.Empty;
        }

        if (useHexDigits)
        {
            return GenerateHexDigitsOnlyPassword();
        }

        if (makePronounceable)
        {
            return GeneratePronounceablePassword();
        }

        return GenerateClassicPassword();
    }

    private string GenerateHexDigitsOnlyPassword()
    {
        var password = new char[passwordPolicy.TotalPasswordLength];

        for (var i = 0; i < passwordPolicy.TotalPasswordLength; i++)
        {
            password[i] = GetRandomChar(PwCharPool.StdHexDigitChars);
        }

        return string.Join("", password);
    }

    private string GeneratePronounceablePassword()
    {
        var password = new char[passwordPolicy.TotalPasswordLength];

        var useDigits = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseDigits);
        var useSymbols = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseSymbols);
        var useUppercase = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseUppercase);
        var useLowercase = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseLowercase);

        // If we don't have any character types, we can't generate a password.
        if (!useDigits && !useSymbols && !useUppercase && !useLowercase)
        {
            return string.Empty;
        }

        var index = 0;
        var useVowel = false;
        var indicators = new List<char>();

        if (useUppercase)
        {
            indicators.AddRange("uuuuuu");
        }

        if (useLowercase)
        {
            indicators.AddRange("llllll");
        }

        if (useDigits)
        {
            indicators.AddRange("dd");
        }

        if (useSymbols)
        {
            indicators.AddRange("ss");
        }

        var indicatorsArray = indicators.ToArray();

        while (index < passwordPolicy.TotalPasswordLength)
        {
            var indicator = GetRandomChar(indicatorsArray);

            if (indicator == 'u')
            {
                password[index] = GetRandomChar(useVowel ? PwCharPool.UppercaseVowels : PwCharPool.UppercaseConsonants);
                useVowel = !useVowel;
            }

            if (indicator == 'l')
            {
                password[index] = GetRandomChar(useVowel ? PwCharPool.LowercaseVowels : PwCharPool.LowercaseConsonants);
                useVowel = !useVowel;
            }

            if (indicator == 'd')
            {
                password[index] = GetRandomChar(PwCharPool.StdDigitChars);
                useVowel = false; // Treat digits as vowels
            }

            if (indicator == 's')
            {
                password[index] = GetRandomChar(PwCharPool.PronounceableSymbolChars);
                useVowel = false; // Treat symbols as vowels
            }

            index++;
        }

        return string.Join("", password);
    }

    private string GenerateClassicPassword()
    {
        var password = new char[passwordPolicy.TotalPasswordLength];

        var useEasyVision = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseEasyVision);
        var useDigits = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseDigits);
        var useSymbols = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseSymbols);
        var useUppercase = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseUppercase);
        var useLowercase = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseLowercase);

        var uppercaseChars = useEasyVision ? PwCharPool.EasyVisionUppercaseChars : PwCharPool.StdUppercaseChars;
        var lowercaseChars = useEasyVision ? PwCharPool.EasyVisionLowercaseChars : PwCharPool.StdLowercaseChars;
        var digitChars = useEasyVision ? PwCharPool.EasyVisionDigitChars : PwCharPool.StdDigitChars;
        var symbolChars = passwordPolicy.GetSpecialSymbolSet();

        if (symbolChars.Length == 0)
        {
            symbolChars = useEasyVision ? PwCharPool.EasyVisionSymbolChars : PwCharPool.StdSymbolChars;
        }

        var allChars = new List<char>();

        // If we don't have any character types, we can't generate a password.
        if (!useDigits && !useSymbols && !useUppercase && !useLowercase)
        {
            return string.Empty;
        }

        // If the sum of the minimum counts is greater than the total length, then we can't generate a password.
        if (passwordPolicy.MinimumSymbolCount + passwordPolicy.MinimumDigitCount + passwordPolicy.MinimumLowercaseCount + passwordPolicy.MinimumUppercaseCount > passwordPolicy.TotalPasswordLength)
        {
            return string.Empty;
        }

        var n = 0;

        if (useUppercase)
        {
            for (var i = 0; i < passwordPolicy.MinimumUppercaseCount; i++)
            {
                password[n + i] = GetRandomChar(uppercaseChars);
                n++;
            }

            allChars.AddRange(uppercaseChars);
        }

        if (useLowercase)
        {
            for (var i = 0; i < passwordPolicy.MinimumLowercaseCount; i++)
            {
                password[n + i] = GetRandomChar(lowercaseChars);
                n++;
            }

            allChars.AddRange(lowercaseChars);
        }

        if (useDigits)
        {
            for (var i = 0; i < passwordPolicy.MinimumDigitCount; i++)
            {
                password[n + i] = GetRandomChar(digitChars);
                n++;
            }

            allChars.AddRange(digitChars);
        }

        if (useSymbols)
        {
            for (var i = 0; i < passwordPolicy.MinimumSymbolCount; i++)
            {
                password[n + i] = GetRandomChar(symbolChars);
                n++;
            }

            allChars.AddRange(symbolChars);
        }

        var allCharsArray = allChars.ToArray();

        for (var i = n; i < passwordPolicy.TotalPasswordLength; i++)
        {
            password[i] = GetRandomChar(allCharsArray);
        }

        password = Shuffle(password);
        return string.Join("", password);
    }

    private static char GetRandomChar(char[] chars)
    {
        var randomChar = chars[GetRandomInt(chars.Length)];
        return randomChar;
    }

    private static int GetRandomInt(int max)
    {
        var random = new Random();
        return random.Next(max);
    }

    private static char[] Shuffle(char[] password)
    {
        var length = password.Length;
        var count = length * 3;

        while (count > 0)
        {
            var index1 = GetRandomInt(length);
            var index2 = GetRandomInt(length);

            if (index1 == index2)
            {
                continue;
            }

            (password[index2], password[index1]) = (password[index1], password[index2]);

            count--;
        }

        return password;
    }
}
