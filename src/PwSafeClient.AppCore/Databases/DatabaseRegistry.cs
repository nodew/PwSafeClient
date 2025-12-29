using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PwSafeClient.AppCore.Configuration;

namespace PwSafeClient.AppCore.Databases;

public sealed class DatabaseRegistry : IDatabaseRegistry
{
    private readonly IAppConfigurationStore _store;

    public DatabaseRegistry(IAppConfigurationStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<DatabaseRegistration>> ListAsync(string? searchText = null, CancellationToken cancellationToken = default)
    {
        var config = await _store.LoadAsync(cancellationToken);

        var normalizedSearch = string.IsNullOrWhiteSpace(searchText)
            ? null
            : searchText.Trim();

        var results = new List<DatabaseRegistration>();

        foreach (var pair in config.Databases)
        {
            var alias = pair.Key;
            var path = pair.Value;

            if (normalizedSearch != null && !alias.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var isDefault = !string.IsNullOrWhiteSpace(config.DefaultDatabase)
                && alias.Equals(config.DefaultDatabase, StringComparison.OrdinalIgnoreCase);

            string? lastUpdatedText = null;
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                var lastWrite = File.GetLastWriteTime(path);
                lastUpdatedText = $"Last updated {lastWrite:g}";
            }

            results.Add(new DatabaseRegistration(alias, path, isDefault, lastUpdatedText));
        }

        return results
            .OrderByDescending(r => r.IsDefault)
            .ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<string?> TryGetPathAsync(string alias, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return null;
        }

        var config = await _store.LoadAsync(cancellationToken);
        return config.Databases.TryGetValue(alias, out var path) ? path : null;
    }

    public async Task AddOrUpdateAsync(string alias, string filePath, bool makeDefault, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException("Alias is required.", nameof(alias));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is required.", nameof(filePath));
        }

        var config = await _store.LoadAsync(cancellationToken);
        config.Databases[alias] = filePath;

        if (makeDefault)
        {
            config.DefaultDatabase = alias;
        }

        await _store.SaveAsync(config, cancellationToken);
    }

    public async Task RemoveAsync(string alias, CancellationToken cancellationToken = default)
    {
        var config = await _store.LoadAsync(cancellationToken);

        if (config.Databases.Remove(alias) && config.DefaultDatabase?.Equals(alias, StringComparison.OrdinalIgnoreCase) == true)
        {
            config.DefaultDatabase = null;
        }

        await _store.SaveAsync(config, cancellationToken);
    }

    public async Task SetDefaultAsync(string alias, CancellationToken cancellationToken = default)
    {
        var config = await _store.LoadAsync(cancellationToken);

        if (!config.Databases.ContainsKey(alias))
        {
            throw new InvalidOperationException($"Database '{alias}' not found.");
        }

        config.DefaultDatabase = alias;
        await _store.SaveAsync(config, cancellationToken);
    }
}
