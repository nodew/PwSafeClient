using Medo.Security.Cryptography.PasswordSafe;
using System;
using System.IO;

namespace PwSafeClient.CLI.Commands;

public class UpdateEntryOption
{
    public Guid Guid { get; set; }

    public string? Alias { get; set; }

    public FileInfo? File { get; set; }

    public string? Title { get; set; }

    public string? Username { get; set; }

    public GroupPath? Group { get; set; }

    public bool NewPass { get; set; }

    public string? Policy { get; set; }
}

//public class UpdateEntryCommand : Command
//{
//    public UpdateEntryCommand() : base("update", "Update the entry")
//    {
//        AddArgument(new Argument<Guid>("GUID", "The ID of an entry"));

//        AddOption(new Option<string>(
//            aliases: new string[] { "--alias", "-a" },
//            description: "The alias of the database"
//        ));

//        AddOption(new Option<FileInfo>(
//            aliases: new string[] { "--file", "-f" },
//            description: "The file path of your database file"
//        ));

//        AddOption(new Option<string>(
//            aliases: new string[] { "--title", "-t" },
//            description: "The title of the entry"
//        ));

//        AddOption(new Option<string>(
//            aliases: new string[] { "--username", "-u" },
//            description: "The username of the entry"
//        ));

//        AddOption(new Option<string>(
//            aliases: new string[] { "--group", "-g" },
//            description: "The group path of the entry"
//        ));

//        AddOption(new Option<bool>(
//            aliases: new string[] { "--newpass", "-n" },
//            description: "Whether to renew the password",
//            getDefaultValue: () => false
//        ));

//        AddOption(new Option<string>(
//            aliases: new string[] { "--policy", "-p" },
//            description: "The name of password policy"
//        ));

//        Handler = CommandHandler.Create<UpdateEntryOption, IHost>(Run);
//    }

//    public static async Task Run(
//        UpdateEntryOption option,
//        IHost host)
//    {
//        string filepath;
//        if (option.File != null)
//        {
//            filepath = option.File.FullName;
//        }
//        else
//        {
//            filepath = await ConsoleService.GetPWSFilePathAsync(option.Alias);
//        }

//        if (!File.Exists(filepath))
//        {
//            ConsoleService.LogError($"Can't locate a valid file, please check your command parameters or configuration in <HOMEDIR>/pwsafe.json");
//            return;
//        }

//        string password = ConsoleService.ReadPassword();

//        try
//        {
//            var doc = Document.Load(filepath, password);

//            var entry = doc.Entries.Where(entry => entry.Uuid == option.Guid).FirstOrDefault();

//            if (entry == null)
//            {
//                ConsoleService.LogError("Entry is not found");
//                return;
//            }

//            if (!string.IsNullOrEmpty(option.Title))
//            {
//                entry.Title = option.Title;
//            }

//            if (!string.IsNullOrEmpty(option.Username))
//            {
//                entry.UserName = option.Username;
//            }

//            if (option.Group != null && !string.IsNullOrEmpty(option.Group))
//            {
//                entry.Group = option.Group;
//            }

//            if (option.NewPass)
//            {
//                string newPassword;
//                if (!string.IsNullOrEmpty(option.Policy))
//                {
//                    var namedPolicy = doc.NamedPasswordPolicies.Where(policy => policy.Name == option.Policy).FirstOrDefault();
//                    if (namedPolicy == null)
//                    {
//                        ConsoleService.LogError($"There's no a password policy named as '{option.Policy}'");
//                        return;
//                    }

//                    entry.PasswordPolicyName = namedPolicy.Name;
//                    entry.PasswordPolicy.TotalPasswordLength = namedPolicy.TotalPasswordLength;
//                    entry.PasswordPolicy.MinimumDigitCount = namedPolicy.MinimumDigitCount;
//                    entry.PasswordPolicy.MinimumLowercaseCount = namedPolicy.MinimumLowercaseCount;
//                    entry.PasswordPolicy.MinimumUppercaseCount = namedPolicy.MinimumUppercaseCount;
//                    entry.PasswordPolicy.MinimumSymbolCount = namedPolicy.MinimumSymbolCount;
//                    entry.PasswordPolicy.SetSpecialSymbolSet(namedPolicy.GetSpecialSymbolSet());

//                    newPassword = PwSafeClientHelper.GeneratePassword(entry.PasswordPolicy);
//                }
//                else
//                {
//                    newPassword = ConsoleService.ReadPassword();
//                }

//                entry.Password = newPassword;
//            }

//            doc.Save(filepath);

//            if (option.NewPass)
//            {
//                Console.WriteLine("Copied password to your clipboard");
//                await TextCopy.ClipboardService.SetTextAsync(entry.Password);
//            }
//        }
//        catch (Exception ex)
//        {
//            ConsoleService.LogError(ex.Message);
//        }
//    }
//}
