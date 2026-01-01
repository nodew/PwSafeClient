using PwSafeClient.Cli.Contracts.Services;

namespace PwSafeClient.Cli.Services;

internal sealed class CliSession : ICliSession
{
    public string? DefaultAlias { get; set; }

    public string? DefaultFilePath { get; set; }

    public string? UnlockedPassword { get; set; }
}
