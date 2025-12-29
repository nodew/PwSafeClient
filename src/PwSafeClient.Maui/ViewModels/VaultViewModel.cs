using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.Vault;
using PwSafeClient.Maui.Models;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class VaultViewModel : ObservableObject
{
    private readonly IVaultSession _vaultSession;

    private readonly List<string> _groupSegments = new();
    private IReadOnlyList<PwSafeClient.AppCore.Vault.Browsing.VaultEntrySnapshot> _entriesSnapshot = Array.Empty<PwSafeClient.AppCore.Vault.Browsing.VaultEntrySnapshot>();

    public VaultViewModel(IVaultSession vaultSession)
    {
        _vaultSession = vaultSession;
        Refresh();
    }

    private string? _databaseName;
    public string? DatabaseName
    {
        get => _databaseName;
        private set => SetProperty(ref _databaseName, value);
    }

    private bool _isUnlocked;
    public bool IsUnlocked
    {
        get => _isUnlocked;
        private set => SetProperty(ref _isUnlocked, value);
    }

    public ObservableCollection<VaultListItem> Items { get; } = new();

    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                LoadCommand.Execute(null);
            }
        }
    }

    private string _breadcrumb = string.Empty;
    public string Breadcrumb
    {
        get => _breadcrumb;
        private set => SetProperty(ref _breadcrumb, value);
    }

    private bool _canGoUp;
    public bool CanGoUp
    {
        get => _canGoUp;
        private set => SetProperty(ref _canGoUp, value);
    }

    public void Refresh()
    {
        IsUnlocked = _vaultSession.IsUnlocked;
        DatabaseName = string.IsNullOrWhiteSpace(_vaultSession.CurrentFilePath)
            ? null
            : Path.GetFileName(_vaultSession.CurrentFilePath);

        if (!IsUnlocked)
        {
            _groupSegments.Clear();
            SearchText = null;
            Items.Clear();
            Breadcrumb = string.Empty;
            CanGoUp = false;
            return;
        }

        LoadCommand.Execute(null);
    }

    [RelayCommand]
    private Task BackAsync()
    {
        if (CanGoUp)
        {
            return GoUpAsync();
        }

        return Shell.Current.GoToAsync($"//{Routes.DatabaseList}");
    }

    [RelayCommand]
    private Task OpenSettingsAsync()
    {
        return Shell.Current.GoToAsync(Routes.Settings);
    }

    [RelayCommand]
    private Task LoadAsync()
    {
        Items.Clear();

        if (!_vaultSession.IsUnlocked)
        {
            Breadcrumb = string.Empty;
            CanGoUp = false;
            return Task.CompletedTask;
        }

        _entriesSnapshot = _vaultSession.GetEntriesSnapshot();

        var prefixLen = _groupSegments.Count;
        Breadcrumb = prefixLen == 0 ? "Personal Vault" : string.Join('.', _groupSegments);
        CanGoUp = prefixLen > 0;

        var search = (SearchText ?? string.Empty).Trim();
        var hasSearch = !string.IsNullOrWhiteSpace(search);

        var matchingEntries = new List<(int index, PwSafeClient.AppCore.Vault.Browsing.VaultEntrySnapshot entry)>();
        for (var i = 0; i < _entriesSnapshot.Count; i++)
        {
            var entry = _entriesSnapshot[i];
            var segments = SplitGroup(entry.GroupPath);

            // When searching, include entries within current prefix (including subgroups).
            // When not searching, only include entries directly in the current group.
            if (!StartsWith(segments, _groupSegments))
            {
                continue;
            }

            if (!hasSearch)
            {
                if (segments.Length != prefixLen)
                {
                    continue;
                }
            }

            if (hasSearch)
            {
                if (!ContainsIgnoreCase(entry.Title, search) && !ContainsIgnoreCase(entry.UserName, search))
                {
                    continue;
                }
            }

            matchingEntries.Add((i, entry));
        }

        if (!hasSearch)
        {
            var childGroups = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in _entriesSnapshot)
            {
                var segments = SplitGroup(entry.GroupPath);
                if (!StartsWith(segments, _groupSegments))
                {
                    continue;
                }

                if (segments.Length > prefixLen)
                {
                    childGroups.Add(segments[prefixLen]);
                }
            }

            foreach (var group in childGroups)
            {
                Items.Add(new VaultListItem
                {
                    Kind = VaultListItemKind.Group,
                    GroupSegment = group,
                    Title = group,
                    Subtitle = null
                });
            }
        }

        foreach (var (index, entry) in matchingEntries.OrderBy(e => e.entry.Title, StringComparer.OrdinalIgnoreCase))
        {
            Items.Add(new VaultListItem
            {
                Kind = VaultListItemKind.Entry,
                EntryIndex = index,
                Title = entry.Title,
                Subtitle = entry.UserName
            });
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenItemAsync(VaultListItem? item)
    {
        if (item == null)
        {
            return;
        }

        if (item.Kind == VaultListItemKind.Group && !string.IsNullOrWhiteSpace(item.GroupSegment))
        {
            _groupSegments.Add(item.GroupSegment);
            LoadCommand.Execute(null);
            return;
        }

        if (item.Kind == VaultListItemKind.Entry && item.EntryIndex.HasValue)
        {
            await Shell.Current.GoToAsync($"{Routes.EntryDetails}?index={item.EntryIndex.Value}");
        }
    }

    [RelayCommand]
    private Task GoUpAsync()
    {
        if (_groupSegments.Count > 0)
        {
            _groupSegments.RemoveAt(_groupSegments.Count - 1);
            LoadCommand.Execute(null);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task NewEntryAsync()
    {
        var group = _groupSegments.Count == 0 ? string.Empty : string.Join('.', _groupSegments);
        var encoded = Uri.EscapeDataString(group);
        return Shell.Current.GoToAsync($"{Routes.EntryEdit}?group={encoded}");
    }

    private static string[] SplitGroup(string? groupPath)
    {
        return string.IsNullOrWhiteSpace(groupPath)
            ? Array.Empty<string>()
            : groupPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool StartsWith(string[] segments, List<string> prefix)
    {
        if (prefix.Count == 0)
        {
            return true;
        }

        if (segments.Length < prefix.Count)
        {
            return false;
        }

        for (var i = 0; i < prefix.Count; i++)
        {
            if (!string.Equals(segments[i], prefix[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsIgnoreCase(string? text, string search)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        return text.Contains(search, StringComparison.OrdinalIgnoreCase);
    }
}
