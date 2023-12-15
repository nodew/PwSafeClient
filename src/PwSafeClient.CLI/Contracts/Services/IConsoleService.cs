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
    /// <param name="message">The question to end user.</param>
    /// <returns>If confirm or cancel an action</returns>
    bool DoConfirm(string message);

    /// <summary>
    /// Read a line from console with given symbol.
    /// </summary>
    string ReadLine(string symbol = ">");

    /// <summary>
    /// Log error message to console.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    void LogError(string errorMessage);

    /// <summary>
    /// Log success message to console.
    /// </summary>
    /// <param name="message">The success message.</param>
    void LogSuccess(string message);
}
