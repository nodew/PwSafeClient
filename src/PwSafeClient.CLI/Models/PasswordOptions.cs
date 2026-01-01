namespace PwSafeClient.Cli.Models;

internal sealed class PasswordOptions
{
    public bool PasswordStdin { get; init; }

    public string? PasswordEnvVar { get; init; }
}
