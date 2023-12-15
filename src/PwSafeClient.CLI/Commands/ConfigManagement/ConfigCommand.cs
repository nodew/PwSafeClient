using System.CommandLine;
using System.IO;

namespace PwSafeClient.CLI.Commands;

public class ConfigCommand : Command
{
    public ConfigCommand() : base("config", "Manage your pwsafe config file")
    {
        AddCommand(new InitConfigCommand());
        AddCommand(new SetAliasCommand());
        AddCommand(new RemoveAliasCommand());
    }
}
