using System;
using System.Threading.Tasks;

using PwSafeClient.Cli.Models;

namespace PwSafeClient.Cli.Services;

internal static class ConfigurationLoader
{
    public static async Task<Configuration?> TryLoadAsync()
    {
        try
        {
            var env = new EnvironmentManager();
            var configManager = new ConfigManager(env);
            return await configManager.LoadConfigurationAsync();
        }
        catch
        {
            return null;
        }
    }
}
