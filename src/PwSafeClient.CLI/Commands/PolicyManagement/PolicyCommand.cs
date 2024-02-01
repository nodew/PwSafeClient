using System.CommandLine;

namespace PwSafeClient.CLI.Commands;

public class PolicyCommand : Command
{
    public PolicyCommand() : base("policy", "Manage your password policies")
    {
        AddCommand(new ListPoliciesCommand());
        AddCommand(new AddPolicyCommand());
        AddCommand(new RemovePolicyCommand());
        AddCommand(new UpdatePolicyCommand());
    }
}
