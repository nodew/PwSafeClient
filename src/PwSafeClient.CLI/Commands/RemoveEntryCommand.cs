using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.CLI.Contracts.Helpers;
using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Options;
using PwSafeClient.Shared;

namespace PwSafeClient.CLI.Commands;

public class RemoveEntryCommand : Command
{
    public RemoveEntryCommand() : base("rm", "Remove an entry or group from the database")
    {
        AddArgument(new Argument<Guid>("ID", getDefaultValue: () => Guid.Empty, "The ID of an entry"));

        AddOption(CommonOptions.AliasOption());

        AddOption(CommonOptions.FileOption());

        AddOption(new Option<string>(
            aliases: ["--group", "-g"],
            description: "remove a group and all items under it"
        ));
    }

    public class RemoveEntryCommandHandler : CommandHandler
    {
        private readonly IConsoleService consoleService;
        private readonly IDocumentHelper documentHelper;

        public RemoveEntryCommandHandler(IConsoleService consoleService, IDocumentHelper documentHelper)
        {
            this.consoleService = consoleService;
            this.documentHelper = documentHelper;
        }

        public string? Alias { get; set; }

        public FileInfo? File { get; set; }

        public Guid Id { get; set; }

        public string? Group { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            Document? document = await documentHelper.TryLoadDocumentAsync(Alias, File, true);
            int totalRemovedEntries = 0;

            if (document == null)
            {
                return 1;
            }

            if (document.IsReadOnly)
            {
                consoleService.LogError("The database is readonly");
                return 1;
            }

            if (Id == Guid.Empty && string.IsNullOrWhiteSpace(Group))
            {
                consoleService.LogError("Either ID or group must be specified");
                return 1;
            }

            if (Id != Guid.Empty)
            {
                Entry? entry = document.Entries.Where(entry => entry.Uuid == Id).FirstOrDefault();

                if (entry != null)
                {
                    if (consoleService.DoConfirm($"Are you sure to remove entry '{entry.Title}[{entry.UserName}]'?"))
                    {
                        totalRemovedEntries = 1;
                        document.Entries.Remove(entry);
                    }
                }
                else
                {
                    consoleService.LogError($"Entry '{Id}' is not found");
                    return 1;
                }
            }
            else if (!string.IsNullOrWhiteSpace(Group))
            {
                Group root = new GroupBuilder([.. document.Entries]).Build();

                string[] groupSegments = [];
                if (!string.IsNullOrWhiteSpace(Group))
                {
                    groupSegments = Group.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < groupSegments.Length; i++)
                    {
                        groupSegments[i] = groupSegments[i].Trim();
                    }
                }

                Group? targetGroup = root.GetChildGroupBySegments(groupSegments);

                if (targetGroup == null)
                {
                    consoleService.LogError($"Group '{Group}' is not found");
                    return 1;
                }

                List<Entry> targetItems = [];

                List<Group> queue = [targetGroup];
                while (queue.Count > 0)
                {
                    Group currentGroup = queue[0];
                    queue.RemoveAt(0);
                    queue.AddRange(currentGroup.Children);
                    targetItems.AddRange(document.Entries.Where(entry => entry.Group.Equals(currentGroup.GetGroupPath())));
                }

                if (targetItems.Count > 0)
                {
                    if (consoleService.DoConfirm($"Are you sure to remove {targetItems.Count} entries under group '{Group}'?"))
                    {
                        totalRemovedEntries = targetItems.Count;
                        foreach (Entry entry in targetItems)
                        {
                            document.Entries.Remove(entry);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"No entries found under group '{Group}'");
                }
            }

            await documentHelper.SaveDocumentAsync(Alias, File);

            if (totalRemovedEntries == 1)
            {
                consoleService.LogSuccess("1 entry removed");
            }
            else if (totalRemovedEntries > 1)
            {
                consoleService.LogSuccess($"{totalRemovedEntries} entries removed");
            }

            return 0;
        }
    }
}
