using Medo.Security.Cryptography.PasswordSafe;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Options;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public class CreateDbCommand : Command
{
    public CreateDbCommand() : base("createdb", "Create an empty new PasswordSafe v3 database file")
    {
        AddArgument(new Argument<Guid>("FILE", "The file path of your psafe3 file"));

        AddOption(CommonOptions.AliasOption());

        AddOption(new Option<bool>(
            name: "--force",
            getDefaultValue: () => false,
            description: "Force to create new database if file exists"
        ));

        AddOption(new Option<bool>(
            name: "--default",
            getDefaultValue: () => false,
            description: "Whether to set the database as default"
        ));
    }


    public class CreateDbCommandHandler : CommandHandler
    {
        private readonly IConfigManager configManager;
        private readonly IConsoleService consoleService;

        public CreateDbCommandHandler(IConfigManager configManager, IConsoleService consoleService)
        {
            this.configManager = configManager;
            this.consoleService = consoleService;
        }

        public string Alias { get; set; } = null!;

        public FileInfo File { get; set; } = null!;

        public bool Force { get; set; }

        public bool Default { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            if (string.IsNullOrEmpty(Alias))
            {
                consoleService.LogError("The alias is required.");
                return 1;
            }

            if (File.Exists && !Force)
            {
                consoleService.LogError($"The file '{File.FullName}' has already existed.");
                return 1;
            }

            var password = consoleService.ReadPassword();

            try
            {
                Document document = new Document(password);
                document.Save(File.FullName);

                await configManager.AddDatabaseAsync(Alias, File.FullName, Default);
                return 0;
            }
            catch (Exception e)
            {
                consoleService.LogError(e.Message);
                return 1;
            }
        }
    }
}