using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PwSafeClient.Cli.Commands.Interactive;
using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal static class InteractiveMode
{
    public static async Task<int> RunAsync(CommandApp app, ICliSession session, string promptDisplayName, int idleTimeMinutes, CancellationToken cancellationToken)
    {
        var idleTimeout = idleTimeMinutes < 0
            ? Timeout.InfiniteTimeSpan
            : TimeSpan.FromMinutes(idleTimeMinutes);

        while (!cancellationToken.IsCancellationRequested)
        {
            var prompt = string.IsNullOrWhiteSpace(promptDisplayName) ? "pwsafe" : promptDisplayName;
            AnsiConsole.Markup($"[green]{Markup.Escape(prompt)}>[/] ");

            string? line;
            if (idleTimeout == Timeout.InfiniteTimeSpan)
            {
                line = Console.ReadLine();
            }
            else
            {
                var result = await ReadLineWithTimeoutAsync(idleTimeout, cancellationToken);
                if (result.TimedOut)
                {
                    AnsiConsole.MarkupLine("[yellow]Idle timeout reached. Exiting.[/]");
                    return 0;
                }

                line = result.Line;
            }

            if (line == null)
            {
                return 0; // EOF
            }

            var tokens = CommandLineTokenizer.Tokenize(line);
            if (tokens.Length == 0)
            {
                continue;
            }

            var exitCode = app.Run(tokens);
            if (exitCode != 0)
            {
                // Keep the loop running; this matches typical shell behavior.
            }
        }

        return 0;
    }

    private sealed record ReadLineResult(bool TimedOut, string? Line);

    private static async Task<ReadLineResult> ReadLineWithTimeoutAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        var readTask = Task.Run(Console.ReadLine, cancellationToken);
        var timeoutTask = Task.Delay(timeout, cancellationToken);

        try
        {
            var completed = await Task.WhenAny(readTask, timeoutTask);
            if (completed == timeoutTask)
            {
                return new ReadLineResult(TimedOut: true, Line: null);
            }

            return new ReadLineResult(TimedOut: false, Line: await readTask);
        }
        catch (OperationCanceledException)
        {
            return new ReadLineResult(TimedOut: false, Line: null);
        }
    }

    public static string? ReadPasswordFromConsoleOrStdin()
    {
        if (Console.IsInputRedirected)
        {
            // In tests/CI there's no TTY; fall back to plain stdin.
            return Console.In.ReadLine();
        }

        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter password:")
                .PromptStyle("green")
                .Secret());
    }

    public static (string? alias, string? filePath, string[] remainingArgs) ParseInteractiveArgs(string[] args)
    {
        string? alias = null;
        string? filePath = null;

        var remaining = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg is "-a" or "--alias")
            {
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("Missing value for --alias");
                }

                alias = args[++i];
                continue;
            }

            if (arg is "-f" or "--file")
            {
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("Missing value for --file");
                }

                filePath = args[++i];
                continue;
            }

            remaining.Add(arg);
        }

        return (alias, filePath, remaining.ToArray());
    }
}
