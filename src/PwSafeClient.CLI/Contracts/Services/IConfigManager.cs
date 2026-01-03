using System.Threading.Tasks;

using PwSafeClient.Cli.Models;

namespace PwSafeClient.Cli.Contracts.Services;

internal interface IConfigManager
{
    string GetConfigFilePath();

    string GetConfigFolderPath();

    bool IsConfigurationExists();

    Task InitConfigurationAsync();

    Task ResetConfigurationAsync();

    Task<Configuration> LoadConfigurationAsync();

    Task SaveConfigurationAsync(Configuration configuration);
}
