using System;
using System.IO;

using PwSafeClient.Cli.Contracts.Services;

namespace PwSafeClient.Cli.Services;

internal class EnvironmentManager : IEnvironmentManager
{
    public string? GetHomeDirectory()
    {
        var overrideHome = Environment.GetEnvironmentVariable("PWSAFE_HOME");
        if (!string.IsNullOrWhiteSpace(overrideHome))
        {
            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(overrideHome));
        }

        return (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            ? Environment.GetEnvironmentVariable("HOME")
            : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
    }
}
