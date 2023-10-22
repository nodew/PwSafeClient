namespace PwSafeClient.CLI.Contracts.Helpers;

/// <summary>
/// Console helper.
/// </summary>
public interface IConsoleHelper
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
    /// Log error message to console.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    void LogError(string errorMessage);
}
