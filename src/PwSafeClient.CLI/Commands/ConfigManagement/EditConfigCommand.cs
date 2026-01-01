using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Commands;

internal sealed class EditConfigCommand : AsyncCommand<EditConfigCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [Description("Create the config file first if it does not exist")]
        [CommandOption("--init")]
        public bool InitIfMissing { get; init; }
    }

    private readonly IConfigManager _configManager;

    public EditConfigCommand(IConfigManager configManager)
    {
        _configManager = configManager;
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, Settings settings)
    {
        try
        {
            var configPath = _configManager.GetConfigFilePath();

            if (!File.Exists(configPath))
            {
                if (!settings.InitIfMissing)
                {
                    AnsiConsole.MarkupLine($"[red]Config file not found:[/] {configPath}");
                    AnsiConsole.MarkupLine("Run 'config init' or 'config reset', or re-run with --init.");
                    return 1;
                }

                await _configManager.ResetConfigurationAsync();
            }

            var editor = Environment.GetEnvironmentVariable("VISUAL");
            if (string.IsNullOrWhiteSpace(editor))
            {
                editor = Environment.GetEnvironmentVariable("EDITOR");
            }

            ProcessStartInfo psi;

            if (!string.IsNullOrWhiteSpace(editor))
            {
                psi = new ProcessStartInfo
                {
                    FileName = editor,
                    Arguments = $"\"{configPath}\"",
                    UseShellExecute = false,
                };
            }
            else
            {
                psi = new ProcessStartInfo
                {
                    FileName = configPath,
                    UseShellExecute = true,
                };
            }

            using var proc = Process.Start(psi);
            proc?.WaitForExit();

            return 0;
        }
        catch (Exception ex)
        {
            CliError.WriteException(ex);
            return ExitCodes.Error;
        }
    }
}
