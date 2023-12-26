using System.CommandLine;

namespace PwSafeClient.CLI.Commands;

public class UpdateEntryCommand : Command
{
    public UpdateEntryCommand(): base("update", "Update an entry")
    {
    }
}
