using System;

using Spectre.Console;

namespace PwSafeClient.Cli.Commands;

internal static class CliError
{
    public static void WriteException(Exception ex)
    {
        if (CliRuntime.DebugEnabled)
        {
            AnsiConsole.WriteException(ex);
            return;
        }

        AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
    }
}
