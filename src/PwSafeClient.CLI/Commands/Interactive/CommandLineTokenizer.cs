using System;
using System.Collections.Generic;
using System.Text;

namespace PwSafeClient.Cli.Commands.Interactive;

internal static class CommandLineTokenizer
{
    public static string[] Tokenize(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return Array.Empty<string>();
        }

        var tokens = new List<string>();
        var current = new StringBuilder();

        var inQuotes = false;
        char quoteChar = '\0';
        var escaping = false;

        foreach (var ch in line)
        {
            if (escaping)
            {
                current.Append(ch);
                escaping = false;
                continue;
            }

            if (ch == '\\')
            {
                escaping = true;
                continue;
            }

            if (inQuotes)
            {
                if (ch == quoteChar)
                {
                    inQuotes = false;
                    quoteChar = '\0';
                    continue;
                }

                current.Append(ch);
                continue;
            }

            if (ch is '"' or '\'')
            {
                inQuotes = true;
                quoteChar = ch;
                continue;
            }

            if (char.IsWhiteSpace(ch))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(ch);
        }

        if (escaping)
        {
            current.Append('\\');
        }

        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }

        return tokens.ToArray();
    }
}
