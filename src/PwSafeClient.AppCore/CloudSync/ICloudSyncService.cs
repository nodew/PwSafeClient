using System.Threading;
using System.Threading.Tasks;

namespace PwSafeClient.AppCore.CloudSync;

public interface ICloudSyncService
{
    Task<CloudSyncState> GetStateAsync(CancellationToken cancellationToken = default);
    Task UpdateOptionsAsync(CloudSyncState state, CancellationToken cancellationToken = default);
    Task<CloudSyncResult> TriggerSyncAsync(CloudSyncTrigger trigger, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
