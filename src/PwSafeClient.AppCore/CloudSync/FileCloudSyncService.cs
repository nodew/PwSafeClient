using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Vault;

namespace PwSafeClient.AppCore.CloudSync;

public sealed class FileCloudSyncService : ICloudSyncService
{
    private readonly IAppConfigurationStore _configStore;
    private readonly IVaultSession _vaultSession;
    private readonly IAppPaths _paths;
    private readonly object _gate = new();
    private CloudSyncStatus _status = CloudSyncStatus.NotConfigured;
    private DateTimeOffset? _lastSynced;
    private DateTimeOffset? _nextScheduled;

    public FileCloudSyncService(IAppConfigurationStore configStore, IVaultSession vaultSession, IAppPaths paths)
    {
        _configStore = configStore;
        _vaultSession = vaultSession;
        _paths = paths;
    }

    public async Task<CloudSyncState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configStore.LoadAsync(cancellationToken);

        lock (_gate)
        {
            if (config.CloudSyncProvider == CloudSyncProvider.None)
            {
                _status = CloudSyncStatus.NotConfigured;
            }
            else if (_status == CloudSyncStatus.NotConfigured)
            {
                _status = CloudSyncStatus.Ready;
            }
        }

        return new CloudSyncState(
            config.CloudSyncProvider,
            config.CloudSyncAccount,
            _status,
            config.CloudSyncLastSyncedAt,
            config.CloudSyncSchedule,
            config.CloudSyncOnSave,
            config.CloudSyncOnCellular);
    }

    public async Task UpdateOptionsAsync(CloudSyncState state, CancellationToken cancellationToken = default)
    {
        var config = await _configStore.LoadAsync(cancellationToken);
        config.CloudSyncProvider = state.Provider;
        config.CloudSyncAccount = state.AccountName;
        config.CloudSyncSchedule = state.Schedule;
        config.CloudSyncOnSave = state.SyncOnSave;
        config.CloudSyncOnCellular = state.SyncOnCellular;
        await _configStore.SaveAsync(config, cancellationToken);

        lock (_gate)
        {
            if (config.CloudSyncProvider == CloudSyncProvider.None)
            {
                _status = CloudSyncStatus.NotConfigured;
            }
            else if (_status == CloudSyncStatus.NotConfigured)
            {
                _status = CloudSyncStatus.Ready;
            }
        }
    }

    public async Task<CloudSyncResult> TriggerSyncAsync(CloudSyncTrigger trigger, CancellationToken cancellationToken = default)
    {
        var config = await _configStore.LoadAsync(cancellationToken);
        if (config.CloudSyncProvider == CloudSyncProvider.None)
        {
            return CloudSyncResult.Fail("Select a sync provider first.");
        }

        if (!_vaultSession.IsUnlocked || string.IsNullOrWhiteSpace(_vaultSession.CurrentFilePath))
        {
            return CloudSyncResult.Fail("Unlock a vault to sync.");
        }

        var vaultPath = _vaultSession.CurrentFilePath!;
        if (!File.Exists(vaultPath))
        {
            return CloudSyncResult.Fail("Vault file not found.");
        }

        lock (_gate)
        {
            _status = CloudSyncStatus.Syncing;
        }

        try
        {
            if (trigger != CloudSyncTrigger.ConflictResolution)
            {
                await _vaultSession.SaveAsync(cancellationToken);
            }

            var cloudPath = GetProviderFilePath(config.CloudSyncProvider, vaultPath);

            var localModified = File.GetLastWriteTimeUtc(vaultPath);
            var cloudModified = File.Exists(cloudPath) ? File.GetLastWriteTimeUtc(cloudPath) : (DateTime?)null;

            var hadConflict = cloudModified.HasValue && cloudModified.Value > localModified.AddSeconds(2);

            if (hadConflict && trigger != CloudSyncTrigger.ConflictResolution)
            {
                lock (_gate)
                {
                    _status = CloudSyncStatus.SyncFailed;
                }

                return CloudSyncResult.Success(hadConflict: true);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(cloudPath)!);
            File.Copy(vaultPath, cloudPath, true);

            config.CloudSyncLastSyncedAt = DateTimeOffset.UtcNow;
            await _configStore.SaveAsync(config, cancellationToken);

            lock (_gate)
            {
                _status = CloudSyncStatus.Ready;
                _lastSynced = config.CloudSyncLastSyncedAt;
                _nextScheduled = CalculateNextScheduled(config.CloudSyncSchedule, _lastSynced);
            }

            return CloudSyncResult.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            lock (_gate)
            {
                _status = CloudSyncStatus.SyncFailed;
            }

            return CloudSyncResult.Fail(ex.Message);
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configStore.LoadAsync(cancellationToken);
        config.CloudSyncProvider = CloudSyncProvider.None;
        config.CloudSyncAccount = null;
        config.CloudSyncLastSyncedAt = null;
        config.CloudSyncSchedule = CloudSyncSchedule.Daily;
        config.CloudSyncOnSave = true;
        config.CloudSyncOnCellular = false;
        await _configStore.SaveAsync(config, cancellationToken);

        lock (_gate)
        {
            _status = CloudSyncStatus.Disconnected;
            _lastSynced = null;
            _nextScheduled = null;
        }
    }

    public DateTimeOffset? GetNextScheduledSync() => _nextScheduled;

    private string GetProviderFilePath(CloudSyncProvider provider, string vaultPath)
    {
        var providerFolder = provider switch
        {
            CloudSyncProvider.ICloudDrive => "icloud",
            CloudSyncProvider.GoogleDrive => "gdrive",
            CloudSyncProvider.Dropbox => "dropbox",
            CloudSyncProvider.OneDrive => "onedrive",
            _ => "unknown"
        };

        var fileName = Path.GetFileName(vaultPath);
        var root = string.IsNullOrWhiteSpace(_paths.AppDataDirectory)
            ? Path.GetTempPath()
            : Path.Combine(_paths.AppDataDirectory, "pwsafe", "cloudsync", providerFolder);

        return Path.Combine(root, fileName);
    }

    private static DateTimeOffset? CalculateNextScheduled(CloudSyncSchedule schedule, DateTimeOffset? lastSynced)
    {
        var baseTime = lastSynced ?? DateTimeOffset.UtcNow;

        return schedule switch
        {
            CloudSyncSchedule.Hourly => baseTime.AddHours(1),
            CloudSyncSchedule.Daily => baseTime.AddDays(1),
            CloudSyncSchedule.Weekly => baseTime.AddDays(7),
            _ => null
        };
    }
}
