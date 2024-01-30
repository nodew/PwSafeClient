using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Options;

namespace PwSafeClient.CLI.Commands;

public class RemovePolicyCommand : Command
{
    public RemovePolicyCommand() : base("rm", "Remove password policy")
    {
        AddArgument(new Argument<string>("Name", "The password policy name"));

        AddOption(CommonOptions.AliasOption());

        AddOption(CommonOptions.FileOption());
    }

    public class RemovePolicyCommandHandler : CommandHandler
    {
        private readonly IConsoleService consoleService;
        private readonly IDocumentHelper documentHelper;

        public RemovePolicyCommandHandler(IConsoleService consoleService, IDocumentHelper documentHelper)
        {
            this.consoleService = consoleService;
            this.documentHelper = documentHelper;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public string? Name { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            Document? document = await documentHelper.TryLoadDocumentAsync(Alias, File, false);

            if (document == null)
            {
                return 1;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                consoleService.LogError("Password policy name is not specified");
                return 1;
            }

            try
            {
                NamedPasswordPolicy? policy = document.NamedPasswordPolicies.Where(policy => policy.Name == Name).FirstOrDefault();

                if (policy == null)
                {
                    consoleService.LogError($"Password policy '{Name}' is not found");
                    return 1;
                }
                else
                {
                    document.NamedPasswordPolicies.Remove(policy);

                    await documentHelper.SaveDocumentAsync(Alias, File);
                    consoleService.LogSuccess($"Password policy '{Name}' is removed");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                consoleService.LogError($"Error: {ex.Message}");
                return 1;
            }
        }
    }
}
