using System;
using System.Threading;
using System.Threading.Tasks;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Vault;

namespace PwSafeClient.Maui.Services;

public sealed class AutoLockService
{
    private readonly IVaultSession _vaultSession;
    private readonly IAppConfigurationStore _configStore;

    private readonly object _gate = new();
    private CancellationTokenSource? _timeoutCts;

    private string? _lastKnownFilePath;

    public AutoLockService(IVaultSession vaultSession, IAppConfigurationStore configStore)
    {
        _vaultSession = vaultSession;
        _configStore = configStore;
    }

    public void NotifyVaultUnlocked(string? filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            _lastKnownFilePath = filePath;
        }

        NotifyActivity();
    }

    public void NotifyActivity()
    {
        _ = RestartTimeoutAsync();
    }

    public Task NotifyBackgroundedAsync()
    {
        return LockAsync();
    }

    private async Task RestartTimeoutAsync()
    {
        CancelTimeout();

        if (!_vaultSession.IsUnlocked)
        {
            return;
        }

        int minutes;
        try
        {
            var cfg = await _configStore.LoadAsync();
            minutes = cfg.AutoLockMinutes;
        }
        catch
        {
            return;
        }

        if (minutes <= 0)
        {
            return;
        }

        var cts = new CancellationTokenSource();
        lock (_gate)
        {
            _timeoutCts = cts;
        }

        try
        {
            await Task.Delay(TimeSpan.FromMinutes(minutes), cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await LockAsync();
    }

    private void CancelTimeout()
    {
        lock (_gate)
        {
            _timeoutCts?.Cancel();
            _timeoutCts?.Dispose();
            _timeoutCts = null;
        }
    }

    private Task LockAsync()
    {
        CancelTimeout();

        if (!_vaultSession.IsUnlocked)
        {
            return Task.CompletedTask;
        }

        var filePath = _vaultSession.CurrentFilePath;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = _lastKnownFilePath;
        }
        else
        {
            _lastKnownFilePath = filePath;
        }

        _vaultSession.Unload();

        var encoded = string.IsNullOrWhiteSpace(filePath) ? null : Uri.EscapeDataString(filePath);

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Shell.Current == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(encoded))
            {
                await Shell.Current.GoToAsync($"//{Routes.DatabaseList}/{Routes.Unlock}?filePath={encoded}");
            }
            else
            {
                await Shell.Current.GoToAsync($"//{Routes.DatabaseList}");
            }
        });
    }
}
