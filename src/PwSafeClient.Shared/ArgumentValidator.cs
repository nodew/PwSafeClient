using System;

namespace PwSafeClient.Shared;

/// <summary>
/// The argument validator.
/// </summary>
public static class ArgumentValidator
{
    /// <summary>
    /// Check if argument is null 
    /// </summary>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="obj">The parameter object.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ThrowIfNull(string paramName, object obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(paramName, $"{paramName} is null");
        }
    }

    /// <summary>
    /// Check if the input string is null, empty or contains only white-space characters.
    /// </summary>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="input">The input string.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ThrowIfNullOrWhiteSpace(string paramName, string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentNullException(paramName, $"{paramName} is null, empty or contains only white-space characters.");
        }
    }
}
