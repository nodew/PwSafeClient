using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using PwSafeLib.Filesystem;
using PwSafeLib.Passwords;

namespace PwSafeClient.Console.Commands
{
    public static class CreateDbCommand
    {
        private static readonly string ext = "psafe3";

        public static RootCommand AddCreateDbCommand(this RootCommand rootCommand)
        {
            Command command = new Command("createdb", "Create a empty new PasswordSafe v3 database file");
            command.AddArgument(new Argument("FILENAME"));
            command.AddOption(new Option(new string[] { "--password", "-p" }, "The security password") { Argument = new Argument("PASSWORD") });
            command.AddOption(new Option(new string[] { "--output", "-o" }, "The output path") { Argument = new Argument("OUTPUT") });
            command.AddOption(new Option("--description", "The description for current database") { Argument = new Argument("DESCRIPTION") });

            command.Handler = CommandHandler.Create<string, string, string, string, IConsole>(HandleCreate);

            rootCommand.AddCommand(command);
            return rootCommand;
        }

        private static async void HandleCreate(string FILENAME, string PASSWORD, string OUTPUT, string DESCRIPTION, IConsole console)
        {
            if (string.IsNullOrEmpty(OUTPUT))
            {
                OUTPUT = Directory.GetCurrentDirectory();
            }

            string fullname = $"{FILENAME}.{ext}";
            string fullpath = Path.Combine(OUTPUT, fullname);

            if (Directory.Exists(fullpath))
            {
                System.Console.Error.WriteLine($"File ${fullname} already exists in ${OUTPUT}");
                return;
            }
            
            SecureString secureString = ConsoleHelper.GetSecureString(PASSWORD);

            try
            {
                using MemoryStream stream = new MemoryStream();
                
                PwsFile pwsFile = await PwsFile.CreateAsync(stream, secureString, PwsFileVersion.Version3);
                
                pwsFile.Header = new PwsFileHeader
                {
                    WhenLastSaved = DateTime.Now,
                    WhatLastSaved = "Initialized",
                    DbName = FILENAME,
                    DbDescription = DESCRIPTION
                };
                
                await pwsFile.OpenAsync();
                
                pwsFile.Dispose();

                using FileStream fileStream = File.Create(fullpath);
                stream.Position = 0;
                stream.CopyTo(fileStream);
                System.Console.WriteLine($"Successfully created {fullpath}");
            } 
            catch (Exception e)
            {
                System.Console.Error.WriteLine($"Failed to create {fullpath}, error: {e.Message}");
                throw;
            }
        }
    }
}
