namespace PwSafeClient.AppCore.Databases;

public sealed record DatabaseRegistration(
    string Alias,
    string FilePath,
    bool IsDefault,
    string? LastUpdatedText);
