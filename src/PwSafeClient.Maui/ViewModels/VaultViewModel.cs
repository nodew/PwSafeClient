using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.CloudSync;
using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Vault;
using PwSafeClient.AppCore.Vault.Editing;
using PwSafeClient.Maui.Models;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class VaultViewModel : ObservableObject
{
    private readonly IVaultSession _vaultSession;
    private readonly ICloudSyncService _cloudSyncService;

    private readonly List<string> _groupSegments = new();
    private IReadOnlyList<PwSafeClient.AppCore.Vault.Browsing.VaultEntrySnapshot> _entriesSnapshot = Array.Empty<PwSafeClient.AppCore.Vault.Browsing.VaultEntrySnapshot>();

    public VaultViewModel(IVaultSession vaultSession, ICloudSyncService cloudSyncService)
    {
        _vaultSession = vaultSession;
        _cloudSyncService = cloudSyncService;
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

    private bool _isReadOnly;
    public bool IsReadOnly
    {
        get => _isReadOnly;
        private set => SetProperty(ref _isReadOnly, value);
    }

    public ObservableCollection<VaultListItem> Items { get; } = new();

    private VaultListItem? _draggingItem;

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

    public bool IsInSubGroup => _groupSegments.Count > 0;

    public void Refresh()
    {
        IsUnlocked = _vaultSession.IsUnlocked;
        IsReadOnly = _vaultSession.IsReadOnly;
        DatabaseName = string.IsNullOrWhiteSpace(_vaultSession.CurrentFilePath)
            ? null
            : Path.GetFileNameWithoutExtension(_vaultSession.CurrentFilePath);

        if (!IsUnlocked)
        {
            _groupSegments.Clear();
            SearchText = null;
            Items.Clear();
            Breadcrumb = string.Empty;
            OnPropertyChanged(nameof(IsInSubGroup));
            return;
        }

        LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task CloseDatabaseAsync()
    {
        _vaultSession.Unload();
        await Shell.Current.GoToAsync($"//{Routes.Unlock}");
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
            return Task.CompletedTask;
        }

        _entriesSnapshot = _vaultSession.GetEntriesSnapshot();

        var prefixLen = _groupSegments.Count;
        Breadcrumb = prefixLen == 0 ? "Vault" : string.Join('.', _groupSegments);
        OnPropertyChanged(nameof(IsInSubGroup));

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

            foreach (var emptyGroup in _vaultSession.GetEmptyGroupPaths())
            {
                var segments = SplitGroup(emptyGroup);
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
                var groupPrefix = new List<string>(_groupSegments) { group };
                var count = _entriesSnapshot.Count(e => StartsWith(SplitGroup(e.GroupPath), groupPrefix));

                var hasEntries = count > 0;
                Items.Add(new VaultListItem
                {
                    Kind = VaultListItemKind.Group,
                    GroupSegment = group,
                    GroupPath = string.Join('.', groupPrefix),
                    Title = group,
                    Subtitle = hasEntries ? (count == 1 ? "1 item" : $"{count} items") : "Empty group",
                    Depth = prefixLen
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
                Subtitle = entry.UserName,
                Depth = prefixLen
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
    private async Task NewEntryAsync()
    {
        if (IsReadOnly)
        {
            return;
        }

        var action = await Shell.Current.DisplayActionSheetAsync("Create New", "Cancel", null, "Group", "Password Entry");

        if (action == "Password Entry")
        {
            var group = _groupSegments.Count == 0 ? string.Empty : string.Join('.', _groupSegments);
            var encoded = Uri.EscapeDataString(group);
            await Shell.Current.GoToAsync($"{Routes.EntryEdit}?group={encoded}");
        }
        else if (action == "Group")
        {
            await CreateGroupAsync();
        }
    }

    [RelayCommand]
    private Task GoUpAsync()
    {
        if (_groupSegments.Count == 0)
        {
            return Task.CompletedTask;
        }

        _groupSegments.RemoveAt(_groupSegments.Count - 1);
        LoadCommand.Execute(null);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task CreateGroupAsync()
    {
        if (Shell.Current == null || IsReadOnly)
        {
            return;
        }

        var name = await Shell.Current.DisplayPromptAsync(
            "Create Group",
            "Group name",
            accept: "Create",
            cancel: "Cancel");

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var trimmed = name.Trim();
        var fullPath = _groupSegments.Count == 0
            ? trimmed
            : $"{string.Join('.', _groupSegments)}.{trimmed}";

        var result = _vaultSession.CreateGroup(fullPath);
        if (!result.IsSuccess)
        {
            await Shell.Current.DisplayAlertAsync("Group", result.ErrorMessage ?? "Failed to create group.", "OK");
            return;
        }

        await _vaultSession.SaveAsync();
        await TriggerCloudSyncIfEnabledAsync();
        LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task RenameGroupAsync(VaultListItem? item)
    {
        if (Shell.Current == null || item?.GroupPath == null)
        {
            return;
        }

        var newName = await Shell.Current.DisplayPromptAsync(
            "Rename Group",
            "New name",
            accept: "Rename",
            cancel: "Cancel",
            initialValue: item.Title);

        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        var trimmed = newName.Trim();
        if (string.Equals(trimmed, item.Title, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var parentSegments = SplitGroup(item.GroupPath).ToList();
        if (parentSegments.Count == 0)
        {
            return;
        }

        parentSegments.RemoveAt(parentSegments.Count - 1);
        var newPath = parentSegments.Count == 0 ? trimmed : $"{string.Join('.', parentSegments)}.{trimmed}";

        var result = _vaultSession.RenameGroup(item.GroupPath, newPath);
        if (!result.IsSuccess)
        {
            await Shell.Current.DisplayAlertAsync("Group", result.ErrorMessage ?? "Failed to rename group.", "OK");
            return;
        }

        await _vaultSession.SaveAsync();
        await TriggerCloudSyncIfEnabledAsync();
        LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task DeleteGroupAsync(VaultListItem? item)
    {
        if (Shell.Current == null || item?.GroupPath == null)
        {
            return;
        }

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Delete Group",
            $"Delete empty group \"{item.Title}\"?",
            "Delete",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        var result = _vaultSession.DeleteEmptyGroup(item.GroupPath);
        if (!result.IsSuccess)
        {
            await Shell.Current.DisplayAlertAsync("Group", result.ErrorMessage ?? "Failed to delete group.", "OK");
            return;
        }

        await _vaultSession.SaveAsync();
        await TriggerCloudSyncIfEnabledAsync();
        LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task MoveItemAsync(VaultListItem? item)
    {
        if (Shell.Current == null || item == null)
        {
            return;
        }

        var destination = await Shell.Current.DisplayPromptAsync(
            "Move",
            "Destination group (dot-separated)",
            accept: "Move",
            cancel: "Cancel",
            placeholder: "e.g. Work.Personal");

        if (string.IsNullOrWhiteSpace(destination))
        {
            return;
        }

        await MoveItemToGroupAsync(item, destination.Trim(), "Move");
    }

    [RelayCommand]
    private Task StartDragAsync(VaultListItem? item)
    {
        _draggingItem = item;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DropOnItemAsync(VaultListItem? item)
    {
        if (Shell.Current == null || item?.Kind != VaultListItemKind.Group || _draggingItem == null)
        {
            return;
        }

        await MoveItemToGroupAsync(_draggingItem, item.GroupPath ?? string.Empty, "Drop");
        _draggingItem = null;
    }

    private async Task MoveItemToGroupAsync(VaultListItem item, string destination, string title)
    {
        if (Shell.Current == null)
        {
            return;
        }

        var target = NormalizeGroupPath(destination);
        VaultGroupOperationResult result;

        if (item.Kind == VaultListItemKind.Entry && item.EntryIndex.HasValue)
        {
            result = _vaultSession.MoveEntry(item.EntryIndex.Value, target);
        }
        else if (item.Kind == VaultListItemKind.Group && !string.IsNullOrWhiteSpace(item.GroupPath))
        {
            var groupSegment = item.GroupSegment ?? string.Empty;
            var newPath = string.IsNullOrWhiteSpace(target)
                ? groupSegment
                : $"{target}.{groupSegment}";

            result = _vaultSession.RenameGroup(item.GroupPath, newPath);
        }
        else
        {
            return;
        }

        if (!result.IsSuccess)
        {
            await Shell.Current.DisplayAlertAsync(title, result.ErrorMessage ?? "Move failed.", "OK");
            return;
        }

        await _vaultSession.SaveAsync();
        await TriggerCloudSyncIfEnabledAsync();
        LoadCommand.Execute(null);
    }

    private static string NormalizeGroupPath(string? groupPath)
    {
        if (string.IsNullOrWhiteSpace(groupPath))
        {
            return string.Empty;
        }

        var segments = groupPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join('.', segments);
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

    private Task TriggerCloudSyncIfEnabledAsync()
        => _cloudSyncService.TriggerSyncIfEnabledAsync(CloudSyncTrigger.Save);
}
