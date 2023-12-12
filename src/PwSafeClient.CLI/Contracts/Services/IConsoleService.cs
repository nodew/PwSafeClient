using System.IO;
using Medo.Security.Cryptography.PasswordSafe;

namespace PwSafeClient.CLI.Contracts.Services;

/// <summary>
/// Console helper.
/// </summary>
public interface IConsoleService
{
    /// <summary>
    /// Read a password from console.
    /// </summary>
    /// <returns>The password.</returns>
    string ReadPassword();

    /// <summary>
    /// Read a answer of given question from console.
    /// </summary>
    /// <param name="question">The question to end user.</param>
    /// <returns>The answer from end user.</returns>
    string ReadQA(string question);

    /// <summary>
    /// Read a line from console.
    /// </summary>
    string ReadLine();

    /// <summary>
    /// Read a line from console with given symbol.
    /// </summary>
    string ReadLine(string symbol);

    /// <summary>
    /// Log error message to console.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    void LogError(string errorMessage);
}
