using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Medo.Security.Cryptography.PasswordSafe;
using PwSafeClient.CLI.Contracts.Services;

namespace PwSafeClient.CLI.Commands;

public class ShowDbCommand : Command
{
   public ShowDbCommand() : base("showdb", "Show the detail of PasswordSafe database")
   {
       AddOption(new Option<string>(
           aliases: new string[] { "--alias", "-a" },
           description: "The alias of the database"
       ));

       AddOption(new Option<FileInfo>(
           aliases: new string[] { "--file", "-f" },
           description: "The file path of your database file"
       ));
   }

    public class ShowDbCommandHandler : CommandHandler
    {
        private readonly IConfigManager configManager;
        private readonly IConsoleService consoleService;

        public ShowDbCommandHandler(IConfigManager configManager, IConsoleService consoleService)
        {
            this.configManager = configManager;
            this.consoleService = consoleService;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            string filepath;

            if (File != null)
            {
                filepath = File.FullName;
            }
            else
            {
                filepath = await configManager.GetDbPath(Alias);
            }

            if (!System.IO.File.Exists(filepath))
            {
                consoleService.LogError($"Can't locate a valid file, please check your command parameters or configuration in <HOMEDIR>/pwsafe.json");
                return 1;
            }

            string password = consoleService.ReadPassword();

            try
            {
                var doc = Document.Load(filepath, password);
                doc.IsReadOnly = true;

                string format = "{0, -30}{1}";
                Console.WriteLine(format, "Database UUID:", doc.Uuid);
                Console.WriteLine(format, "Name:", doc.Name ?? "-");
                Console.WriteLine(format, "Description:", doc.Description ?? "-");
                Console.WriteLine(format, "Version", doc.Version);
                Console.WriteLine(format, "Last saved by:", doc.LastSaveUser);
                Console.WriteLine(format, "Last saved on:", doc.LastSaveTime);
                Console.WriteLine(format, "Last saved application: ", doc.LastSaveApplication);
                Console.WriteLine(format, "Last saved machine: ", doc.LastSaveHost);
                Console.WriteLine(format, "Last saved user: ", doc.LastSaveUser);
                Console.WriteLine(format, "Item count: ", doc.Entries.Count);

                return 0;
            }
            catch (Exception ex)
            {
                consoleService.LogError(ex.Message);
                return 1;
            }
        }
    }
}
