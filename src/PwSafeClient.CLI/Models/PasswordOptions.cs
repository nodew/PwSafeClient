namespace PwSafeClient.Cli.Models;

internal sealed class PasswordOptions
{
    public string? Password { get; init; }

    public bool PasswordStdin { get; init; }

    public string? PasswordEnvVar { get; init; }
}
