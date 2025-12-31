using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Databases;
using PwSafeClient.Maui.Services;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class DatabaseListViewModel : ObservableObject
{
    private readonly IDatabaseRegistry _databaseRegistry;
    private readonly IFilePickerService _filePicker;
    private readonly IAppConfigurationStore _configStore;

    public DatabaseListViewModel(
        IDatabaseRegistry databaseRegistry,
        IFilePickerService filePicker,
        IAppConfigurationStore configStore)
    {
        _databaseRegistry = databaseRegistry;
        _filePicker = filePicker;
        _configStore = configStore;
    }

    public ObservableCollection<DatabaseRegistration> Databases { get; } = new();

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

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        ErrorMessage = null;
        IsBusy = true;

        try
        {
            Databases.Clear();
            var items = await _databaseRegistry.ListAsync(SearchText);
            foreach (var item in items)
            {
                Databases.Add(item);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenDatabaseAsync(DatabaseRegistration? database)
    {
        if (database == null)
        {
            return;
        }

        try
        {
            var config = await _configStore.LoadAsync();
            if (config.DefaultDatabase != database.Alias)
            {
                config.DefaultDatabase = database.Alias;
                await _configStore.SaveAsync(config);
            }
        }
        catch
        {
            // Ignore configuration save errors
        }

        var alias = Uri.EscapeDataString(database.Alias);
        await Shell.Current.GoToAsync($"//{Routes.Unlock}?alias={alias}");
    }

    [RelayCommand]
    private Task CreateDatabaseAsync()
    {
        return Shell.Current.GoToAsync(Routes.CreateDatabase);
    }

    [RelayCommand]
    private async Task ImportDatabaseAsync()
    {
        ErrorMessage = null;
        IsBusy = true;

        try
        {
            var filePath = await _filePicker.PickDatabaseFileAsync();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            var baseAlias = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrWhiteSpace(baseAlias))
            {
                baseAlias = "Database";
            }

            var existing = await _databaseRegistry.ListAsync();
            var alias = MakeUniqueAlias(baseAlias, existing);

            await _databaseRegistry.AddOrUpdateAsync(alias, filePath, makeDefault: false);

            Databases.Clear();
            foreach (var item in await _databaseRegistry.ListAsync(SearchText))
            {
                Databases.Add(item);
            }

            var encodedAlias = Uri.EscapeDataString(alias);
            await Shell.Current.GoToAsync($"{Routes.Unlock}?alias={encodedAlias}");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string MakeUniqueAlias(string baseAlias, System.Collections.Generic.IReadOnlyList<DatabaseRegistration> existing)
    {
        var alias = baseAlias.Trim();
        if (alias.Length == 0)
        {
            alias = "Database";
        }

        bool Exists(string candidate)
        {
            foreach (var db in existing)
            {
                if (string.Equals(db.Alias, candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        if (!Exists(alias))
        {
            return alias;
        }

        for (var i = 2; i < 1000; i++)
        {
            var candidate = $"{alias} ({i})";
            if (!Exists(candidate))
            {
                return candidate;
            }
        }

        return $"{alias} ({DateTime.UtcNow:yyyyMMddHHmmss})";
    }
}
