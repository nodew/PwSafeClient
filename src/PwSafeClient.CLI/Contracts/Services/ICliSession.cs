namespace PwSafeClient.Cli.Contracts.Services;

internal interface ICliSession
{
    string? DefaultAlias { get; set; }

    string? DefaultFilePath { get; set; }

    string? UnlockedPassword { get; set; }
}
