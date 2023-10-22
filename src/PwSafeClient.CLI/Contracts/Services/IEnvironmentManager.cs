namespace PwSafeClient.CLI.Contracts.Services;

/// <summary>
/// Manage the environment of the CLI.
/// </summary>
public interface IEnvironmentManager
{
    /// <summary>
    /// Get the home directory of current user.
    /// </summary>
    /// <returns>The home directory of current user.</returns>
    string? GetHomeDirectory();
}
