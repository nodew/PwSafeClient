using System;

using PwSafeClient.CLI.Contracts.Services;

namespace PwSafeClient.CLI.Services;

/// <summary>
/// Implement <see cref="IEnvironmentManager"/>.
/// </summary>
internal class EnvironmentManager : IEnvironmentManager
{
    /// <inheritdoc/>
    public string? GetHomeDirectory()
    {
        return (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            ? Environment.GetEnvironmentVariable("HOME")
            : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
    }
}
