using System;
using System.Linq;

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
        Console.WriteLine($"Generating password...");
        char[] password = new char[passwordPolicy.TotalPasswordLength];

        bool useEasyVision = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseEasyVision);
        bool useHexDigits = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseHexDigits);
        bool useDigits = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseDigits);
        bool useSymbols = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseSymbols);
        bool useUppercase = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseUppercase);
        bool useLowercase = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.UseLowercase);
        bool makePronounceable = passwordPolicy.Style.HasFlag(PasswordPolicyStyle.MakePronounceable);

        if (useHexDigits)
        {
            for (var i = 0; i < passwordPolicy.TotalPasswordLength; i++)
            {
                password[i] = GetRandomChar(PwCharPool.StdHexDigitChars);
            }

            return string.Join("", password);
        }

        if (makePronounceable)
        {
            return MakePronounceablePassword();
        }

        char[] uppercaseChars = useEasyVision ? PwCharPool.EasyVisionUppercase_chars : PwCharPool.StdUppercaseChars;
        char[] lowercaseChars = useEasyVision ? PwCharPool.EasyVisionLowercaseChars : PwCharPool.StdLowercaseChars;
        char[] digitChars = useEasyVision ? PwCharPool.EasyVisionDigitChars : PwCharPool.StdDigitChars;
        char[] symbolChars = useEasyVision ? PwCharPool.EasyVisionSymbolChars : PwCharPool.StdSymbolChars;

        int n = 0;

        if (useUppercase)
        {
            for (var i = 0; i < passwordPolicy.MinimumUppercaseCount; i++)
            {
                password[n + i] = GetRandomChar(uppercaseChars);
                n++;
            }
        }

        if (useLowercase)
        {
            for (var i = 0; i < passwordPolicy.MinimumLowercaseCount; i++)
            {
                password[n + i] = GetRandomChar(lowercaseChars);
                n++;
            }
        }

        if (useDigits)
        {
            for (var i = 0; i < passwordPolicy.MinimumDigitCount; i++)
            {
                password[n + i] = GetRandomChar(digitChars);
                n++;
            }
        }

        if (useSymbols)
        {
            for (var i = 0; i < passwordPolicy.MinimumSymbolCount; i++)
            {
                password[n + i] = GetRandomChar(symbolChars);
                n++;
            }
        }

        for (var i = n; i < passwordPolicy.TotalPasswordLength; i++)
        {
            password[i] = GetRandomChar(uppercaseChars.Concat(lowercaseChars).Concat(digitChars).Concat(symbolChars).ToArray());
        }

        password = Shuffle(password);
        return string.Join("", password);
    }

    /// <summary>
    /// TODO: Generate a pronounceable password.
    /// </summary>
    /// <returns>Pronounceable password</returns>
    private string MakePronounceablePassword()
    {
        char[] password = new char[passwordPolicy.TotalPasswordLength];
        return string.Join("", password);
    }

    private static char GetRandomChar(char[] chars)
    {
        var random = new Random();
        var randomChar = chars[random.Next(chars.Length)];
        return randomChar;
    }

    private static char[] Shuffle(char[] password)
    {
        var length = password.Length;
        var count = length * 3;

        while (count > 0)
        {
            var index1 = new Random().Next(length);
            var index2 = new Random().Next(length);

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
