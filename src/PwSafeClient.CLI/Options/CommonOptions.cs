using System.CommandLine;
using System.IO;

namespace PwSafeClient.CLI.Options;

public static class CommonOptions
{
    public static Option<string> AliasOption() => new(
        aliases: ["--alias", "-a"],
        description: "The alias of the database");

    public static Option<FileInfo> FileOption() => new(
        aliases: ["--file", "-f"],
        description: "The file path of your database file");

    public static Option<bool> ReadOnlyOption() => new(
        name: "--readonly",
        description: "Open database in read-only mode",
        getDefaultValue: () => true);
}
