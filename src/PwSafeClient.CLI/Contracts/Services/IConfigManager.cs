using System.Threading.Tasks;

using PwSafeClient.Cli.Models;

namespace PwSafeClient.Cli.Contracts.Services;

internal interface IConfigManager
{
    bool IsConfigurationExists();

    Task InitConfigurationAsync();

    Task<Configuration> LoadConfigurationAsync();

    Task SaveConfigurationAsync(Configuration configuration);
}
