using PwSafeClient.Core;
using PwSafeLib.Filesystem;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PwSafeClient.Console.Commands
{
    public static class ListCommand
    {
        public static RootCommand AddListCommand(this RootCommand rootCommand)
        {
            Command command = new Command("list", "List the items in database");
            command.AddOption(new Option(new string[] { "--alias", "-a" }, "Alias to the database, config at $HOME/.pwsafe")
            {
                Argument = new Argument("ALIAS")
            });
            command.AddOption(new Option(new string[] { "--file", "-f" }, "Path to your PasswordSafe file")
            {
                Argument = new Argument("FILE")
            });
            command.AddOption(new Option(new string[] { "--password", "-p" }, "Password for current database")
            {
                Argument = new Argument("PASSWORD")
            });
            command.AddOption(new Option(new string[] { "--group", "-g" }, "Show items in under a specific group")
            {
                Argument = new Argument("GROUP")
            });

            command.Handler = CommandHandler.Create<string, string, string, string, IConsole>(HandleListCommand);

            rootCommand.AddCommand(command);

            return rootCommand;
        }

        private static async Task HandleListCommand(string alias, string file, string password, string group, IConsole console)
        {
            string filepath = await ConsoleHelper.GetPwsFilePath(file, alias);

            if (!File.Exists(filepath))
            {
                System.Console.Error.WriteLine($"Can't locate a valid file, please check your command parameters or configuration in ~/pwsafe.json");
                return;
            }

            SecureString secureString = ConsoleHelper.GetSecureString(password);

            try
            {
                using FileStream stream = File.OpenRead(filepath);
                using MemoryStream ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                PwsFileV3 pwsFile = (PwsFileV3)await PwsFile.OpenAsync(ms, secureString);

                List<ItemData> items = await PwsFileHelper.ReadAllItemsFromPwsFileV3Async(pwsFile);
                Group rootGroup = Group.GroupItems(items);

                if (string.IsNullOrWhiteSpace(group))
                {
                    PrintGroup(rootGroup);
                }
                else
                {
                    Group? subGroup = rootGroup.FindSubGroup(item => item.Path == group);

                    if (subGroup is null)
                    {
                        System.Console.Error.WriteLine("No such group, {0}", group);
                    }
                    else
                    {
                        System.Console.WriteLine("Items ({0}):", subGroup.Items.Count);
                        subGroup.Items.ForEach((item) =>
                        {
                            System.Console.WriteLine(item.Title);
                        });
                        System.Console.WriteLine("Groups ({0}):", subGroup.Groups.Count);
                        subGroup.Groups.ForEach((item) =>
                        {
                            System.Console.WriteLine(item.Name);
                        });
                    }
                }

                pwsFile.Dispose();
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.Message);
                throw;
            }
        }

        private static void PrintGroup(Group group)
        {
            PrintGroup(group, 0);
        }

        private static void PrintGroup(Group group, int depth)
        {
            string indent = new string(' ', depth * 2);
            group.Items.ForEach((item) =>
            {
                System.Console.WriteLine("{0}|-- {1}", indent, item.Title);
            });
            group.Groups.ForEach((item) =>
            {
                System.Console.WriteLine("{0}|-- {1}", indent, item.Name);
                PrintGroup(item, depth + 1);
            });
            if (group.Items.Count == 0 && group.Groups.Count == 0)
            {
                System.Console.WriteLine("{0}|-- {1}", indent, "(empty)");
            }
        }
    }
}
