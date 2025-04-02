using System;

using PwSafeClient.Cli.Contracts.Services;

namespace PwSafeClient.Cli.Services;

internal class EnvironmentManager : IEnvironmentManager
{
    public string? GetHomeDirectory()
    {
        return (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            ? Environment.GetEnvironmentVariable("HOME")
            : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
    }
}
