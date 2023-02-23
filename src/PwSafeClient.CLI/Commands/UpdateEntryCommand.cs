using Medo.Security.Cryptography.PasswordSafe;
using PwSafeClient.Core;
using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Commands;

public static class UpdateEntryCommand
{
    public static RootCommand AddUpdateEntryCommand(this RootCommand rootCommand)
    {
        var command = new Command("update", "Update an entry");

        var entryIdArgument = new Argument<Guid>("GUID", "The ID of an entry");

        var aliasOption = new Option<string>("--alias", "The alias of the database");
        aliasOption.AddAlias("-a");

        var fileOption = new Option<FileInfo>("--file", "The file path of your psafe3 file");
        fileOption.AddAlias("-f");

        var titleOption = new Option<string>("--title", "The title of the entry");
        titleOption.AddAlias("-t");

        var usernameOption = new Option<string>("--username", "The username of the entry");
        usernameOption.AddAlias("-u");

        var groupOption = new Option<GroupPath>("--group", "The group of the entry");
        groupOption.AddAlias("-g");

        var renewPasswordOption = new Option<bool>("--newpass", "Whether to renew the password");
        renewPasswordOption.SetDefaultValue(false);
        renewPasswordOption.AddAlias("-n");

        var policyOption = new Option<string>("--policy", "The policy of the entry");
        policyOption.AddAlias("-p");

        command.AddOption(aliasOption);
        command.AddOption(fileOption);
        command.AddOption(titleOption);
        command.AddOption(usernameOption);
        command.AddOption(groupOption);
        command.AddOption(renewPasswordOption);
        command.AddOption(policyOption);

        command.AddArgument(entryIdArgument);
        command.SetHandler(
            HandleUpdateEntry,
            entryIdArgument,
            aliasOption,
            fileOption,
            titleOption,
            usernameOption,
            groupOption,
            renewPasswordOption,
            policyOption
        );

        rootCommand.Add(command);

        return rootCommand;
    }

    public static async Task HandleUpdateEntry(
        Guid id,
        string alias,
        FileInfo? file,
        string title,
        string username,
        GroupPath group,
        bool renewPassword,
        string policyName)
    {
        string filepath;
        if (file != null)
        {
            filepath = file.FullName;
        }
        else
        {
            filepath = await ConsoleHelper.GetPWSFilePathAsync(alias);
        }

        if (!File.Exists(filepath))
        {
            ConsoleHelper.LogError($"Can't locate a valid file, please check your command parameters or configuration in <HOMEDIR>/pwsafe.json");
            return;
        }

        string password = ConsoleHelper.ReadPassword();

        try
        {
            var doc = Document.Load(filepath, password);

            var entry = doc.Entries.Where(entry => entry.Uuid == id).FirstOrDefault();

            if (entry == null)
            {
                ConsoleHelper.LogError("Entry is not found");
                return;
            }

            if (!string.IsNullOrEmpty(title))
            {
                entry.Title = title;
            }

            if (!string.IsNullOrEmpty(username))
            {
                entry.UserName = username;
            }

            if (!string.IsNullOrEmpty(group))
            {
                entry.Group = group;
            }

            if (renewPassword)
            {
                string newPassword;
                if (!string.IsNullOrEmpty(policyName))
                {
                    var namedPolicy = doc.NamedPasswordPolicies.Where(policy => policy.Name == policyName).FirstOrDefault();
                    if (namedPolicy == null)
                    {
                        ConsoleHelper.LogError($"There's no a password policy named as '{policyName}'");
                        return;
                    }

                    entry.PasswordPolicyName = namedPolicy.Name;
                    entry.PasswordPolicy.TotalPasswordLength = namedPolicy.TotalPasswordLength;
                    entry.PasswordPolicy.MinimumDigitCount = namedPolicy.MinimumDigitCount;
                    entry.PasswordPolicy.MinimumLowercaseCount = namedPolicy.MinimumLowercaseCount;
                    entry.PasswordPolicy.MinimumUppercaseCount = namedPolicy.MinimumUppercaseCount;
                    entry.PasswordPolicy.MinimumSymbolCount = namedPolicy.MinimumSymbolCount;
                    entry.PasswordPolicy.SetSpecialSymbolSet(namedPolicy.GetSpecialSymbolSet());

                    newPassword = PwSafeClientHelper.GeneratePassword(entry.PasswordPolicy);
                }
                else
                {
                    newPassword = ConsoleHelper.ReadPassword();
                }

                entry.Password = newPassword;
            }

            doc.Save(filepath);

            if (renewPassword)
            {
                Console.WriteLine("Copied password to your clipboard");
                await TextCopy.ClipboardService.SetTextAsync(entry.Password);
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.LogError(ex.Message);
        }
    }
}
