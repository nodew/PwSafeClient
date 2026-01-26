using System.Threading;
using System.Threading.Tasks;

namespace PwSafeClient.AppCore.Configuration;

public interface IAppConfigurationStore
{
    Task<AppConfiguration> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppConfiguration configuration, CancellationToken cancellationToken = default);
}
