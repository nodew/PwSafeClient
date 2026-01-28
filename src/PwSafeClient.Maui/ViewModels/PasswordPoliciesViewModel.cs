using System.Collections.ObjectModel;
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.AppCore.CloudSync;
using PwSafeClient.AppCore.Vault;
using PwSafeClient.Maui.Services;
using PwSafeClient.Shared;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class PasswordPoliciesViewModel : ObservableObject
{
    private const string PolicySavedMessage = "Policy saved.";
    private const string PolicyDefaultMessage = "Default policy updated.";
    private const string PolicyDeletedMessage = "Policy deleted.";

    private readonly IVaultSession _vaultSession;
    private readonly ICloudSyncService _cloudSyncService;
    private readonly IFilePickerService _filePicker;
    private readonly ISaveFileService _saveFileService;

    private static readonly JsonSerializerOptions PolicySerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        MaxDepth = 32,
    };

    public PasswordPoliciesViewModel(
        IVaultSession vaultSession,
        ICloudSyncService cloudSyncService,
        IFilePickerService filePicker,
        ISaveFileService saveFileService)
    {
        _vaultSession = vaultSession;
        _cloudSyncService = cloudSyncService;
        _filePicker = filePicker;
        _saveFileService = saveFileService;
    }

    public ObservableCollection<VaultPasswordPolicySnapshot> Policies { get; } = new();

    private bool _isSheetVisible;
    public bool IsSheetVisible
    {
        get => _isSheetVisible;
        set => SetProperty(ref _isSheetVisible, value);
    }

    private string _sheetTitle = "Edit Policy";
    public string SheetTitle
    {
        get => _sheetTitle;
        set => SetProperty(ref _sheetTitle, value);
    }

    private string _policyName = string.Empty;
    public string PolicyName
    {
        get => _policyName;
        set => SetProperty(ref _policyName, value);
    }

    private string _passwordLength = "16";
    public string PasswordLength
    {
        get => _passwordLength;
        set => SetProperty(ref _passwordLength, value);
    }

    private string _uppercaseCount = "2";
    public string UppercaseCount
    {
        get => _uppercaseCount;
        set => SetProperty(ref _uppercaseCount, value);
    }

    private string _lowercaseCount = "6";
    public string LowercaseCount
    {
        get => _lowercaseCount;
        set => SetProperty(ref _lowercaseCount, value);
    }

    private string _numberCount = "4";
    public string NumberCount
    {
        get => _numberCount;
        set => SetProperty(ref _numberCount, value);
    }

    private string _symbolCount = "2";
    public string SymbolCount
    {
        get => _symbolCount;
        set => SetProperty(ref _symbolCount, value);
    }

    private string _customSymbols = string.Empty;
    public string CustomSymbols
    {
        get => _customSymbols;
        set => SetProperty(ref _customSymbols, value);
    }

    private bool _canDeletePolicy;
    public bool CanDeletePolicy
    {
        get => _canDeletePolicy;
        set => SetProperty(ref _canDeletePolicy, value);
    }

    private string? _originalPolicyName;

    private bool _isHexOnly;
    public bool IsHexOnly
    {
        get => _isHexOnly;
        set => SetProperty(ref _isHexOnly, value);
    }

    private bool _isPronounceable;
    public bool IsPronounceable
    {
        get => _isPronounceable;
        set => SetProperty(ref _isPronounceable, value);
    }

    private bool _useLowercase;
    public bool UseLowercase
    {
        get => _useLowercase;
        set => SetProperty(ref _useLowercase, value);
    }

    private bool _useUppercase;
    public bool UseUppercase
    {
        get => _useUppercase;
        set => SetProperty(ref _useUppercase, value);
    }

    private bool _useDigits;
    public bool UseDigits
    {
        get => _useDigits;
        set => SetProperty(ref _useDigits, value);
    }

    private bool _useSymbols;
    public bool UseSymbols
    {
        get => _useSymbols;
        set => SetProperty(ref _useSymbols, value);
    }

    private bool _useEasyVision;
    public bool UseEasyVision
    {
        get => _useEasyVision;
        set => SetProperty(ref _useEasyVision, value);
    }

    [RelayCommand]
    private Task CloseAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private void OpenEditPolicy(VaultPasswordPolicySnapshot? policy)
    {
        if (policy == null)
        {
            return;
        }

        SheetTitle = "Edit Policy";
        _originalPolicyName = policy.Name;
        PolicyName = policy.Name;
        PasswordLength = policy.TotalPasswordLength.ToString();
        UppercaseCount = policy.MinimumUppercaseCount.ToString();
        LowercaseCount = policy.MinimumLowercaseCount.ToString();
        NumberCount = policy.MinimumDigitCount.ToString();
        SymbolCount = policy.MinimumSymbolCount.ToString();
        CustomSymbols = policy.SymbolSet;
        IsHexOnly = policy.Style.HasFlag(PasswordPolicyStyle.UseHexDigits);
        IsPronounceable = policy.Style.HasFlag(PasswordPolicyStyle.MakePronounceable);
        UseLowercase = policy.Style.HasFlag(PasswordPolicyStyle.UseLowercase);
        UseUppercase = policy.Style.HasFlag(PasswordPolicyStyle.UseUppercase);
        UseDigits = policy.Style.HasFlag(PasswordPolicyStyle.UseDigits);
        UseSymbols = policy.Style.HasFlag(PasswordPolicyStyle.UseSymbols);
        UseEasyVision = policy.Style.HasFlag(PasswordPolicyStyle.UseEasyVision);
        CanDeletePolicy = true;
        IsSheetVisible = true;
    }

    [RelayCommand]
    private void OpenCreatePolicy()
    {
        SheetTitle = "Create Policy";
        _originalPolicyName = null;
        PolicyName = string.Empty;
        PasswordLength = "16";
        UppercaseCount = "2";
        LowercaseCount = "6";
        NumberCount = "4";
        SymbolCount = "2";
        CustomSymbols = string.Empty;
        IsHexOnly = false;
        IsPronounceable = false;
        UseLowercase = true;
        UseUppercase = true;
        UseDigits = true;
        UseSymbols = true;
        UseEasyVision = false;
        CanDeletePolicy = false;
        IsSheetVisible = true;
    }

    [RelayCommand]
    private void CloseSheet()
    {
        IsSheetVisible = false;
    }

    [RelayCommand]
    private async Task SaveSheetAsync()
    {
        IsSheetVisible = false;

        var shell = Shell.Current;
        if (shell == null)
        {
            return;
        }

        if (!_vaultSession.IsUnlocked)
        {
            await shell.DisplayAlertAsync("Password Policies", "Unlock the vault first.", "OK");
            return;
        }

        if (!TryBuildPolicySnapshot(out var policy, out var errorMessage) || policy == null)
        {
            await shell.DisplayAlertAsync("Password Policies", errorMessage ?? "Invalid policy settings.", "OK");
            return;
        }

        var normalizedPolicy = new VaultPasswordPolicySnapshot
        {
            Name = policy.Name,
            TotalPasswordLength = policy.TotalPasswordLength,
            MinimumLowercaseCount = policy.MinimumLowercaseCount,
            MinimumUppercaseCount = policy.MinimumUppercaseCount,
            MinimumDigitCount = policy.MinimumDigitCount,
            MinimumSymbolCount = policy.MinimumSymbolCount,
            Style = policy.Style,
            SymbolSet = string.Join("", ResolveSymbolSet(policy))
        };

        var result = _vaultSession.SavePasswordPolicy(normalizedPolicy, _originalPolicyName);
        if (!result.IsSuccess)
        {
            await shell.DisplayAlertAsync("Password Policies", result.ErrorMessage ?? "Failed to save policy.", "OK");
            return;
        }

        await _vaultSession.SaveAsync();
        await _cloudSyncService.TriggerSyncIfEnabledAsync(CloudSyncTrigger.Save);
        LoadPolicies();
        await shell.DisplayAlertAsync("Password Policies", PolicySavedMessage, "OK");
    }

    [RelayCommand]
    private async Task SaveAsDefaultAsync()
    {
        IsSheetVisible = false;

        var shell = Shell.Current;
        if (shell == null)
        {
            return;
        }

        if (!_vaultSession.IsUnlocked)
        {
            await shell.DisplayAlertAsync("Password Policies", "Unlock the vault first.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(PolicyName))
        {
            await shell.DisplayAlertAsync("Password Policies", "Select a policy first.", "OK");
            return;
        }

        var result = _vaultSession.SetDefaultPasswordPolicy(PolicyName);
        if (!result.IsSuccess)
        {
            await shell.DisplayAlertAsync("Password Policies", result.ErrorMessage ?? "Failed to update default policy.", "OK");
            return;
        }

        await _vaultSession.SaveAsync();
        await _cloudSyncService.TriggerSyncIfEnabledAsync(CloudSyncTrigger.Save);
        LoadPolicies();
        await shell.DisplayAlertAsync("Password Policies", PolicyDefaultMessage, "OK");
    }

    [RelayCommand]
    private async Task DeletePolicyAsync()
    {
        IsSheetVisible = false;

        var shell = Shell.Current;
        if (shell == null)
        {
            return;
        }

        if (!_vaultSession.IsUnlocked)
        {
            await shell.DisplayAlertAsync("Password Policies", "Unlock the vault first.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(PolicyName))
        {
            await shell.DisplayAlertAsync("Password Policies", "Select a policy first.", "OK");
            return;
        }

        var result = _vaultSession.DeletePasswordPolicy(PolicyName);
        if (!result.IsSuccess)
        {
            await shell.DisplayAlertAsync("Password Policies", result.ErrorMessage ?? "Failed to delete policy.", "OK");
            return;
        }

        await _vaultSession.SaveAsync();
        await _cloudSyncService.TriggerSyncIfEnabledAsync(CloudSyncTrigger.Save);
        LoadPolicies();
        await shell.DisplayAlertAsync("Password Policies", PolicyDeletedMessage, "OK");
    }

    [RelayCommand]
    private async Task ImportPoliciesAsync()
    {
        var shell = Shell.Current;
        if (shell == null)
        {
            return;
        }

        if (!_vaultSession.IsUnlocked)
        {
            await shell.DisplayAlertAsync("Password Policies", "Unlock the vault first.", "OK");
            return;
        }

        var path = await _filePicker.PickPolicyFileAsync();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(path);
            List<VaultPasswordPolicySnapshot> policies;
            try
            {
                policies = JsonSerializer.Deserialize<List<VaultPasswordPolicySnapshot>>(json, PolicySerializerOptions)
                    ?? new List<VaultPasswordPolicySnapshot>();
            }
            catch (JsonException)
            {
                await shell.DisplayAlertAsync("Password Policies", "Invalid policy file format.", "OK");
                return;
            }

            if (policies.Count == 0)
            {
                await shell.DisplayAlertAsync("Password Policies", "No policies found in file.", "OK");
                return;
            }

            var imported = 0;
            foreach (var policy in policies)
            {
                if (string.IsNullOrWhiteSpace(policy.Name))
                {
                    continue;
                }

                var result = _vaultSession.SavePasswordPolicy(policy);
                if (!result.IsSuccess)
                {
                    await shell.DisplayAlertAsync("Password Policies", result.ErrorMessage ?? "Failed to import policy.", "OK");
                    return;
                }

                imported++;
            }

            await _vaultSession.SaveAsync();
            await _cloudSyncService.TriggerSyncIfEnabledAsync(CloudSyncTrigger.Save);
            LoadPolicies();
            await shell.DisplayAlertAsync("Password Policies", $"Policies imported: {imported}.", "OK");
        }
        catch (Exception ex)
        {
            await shell.DisplayAlertAsync("Password Policies", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ExportPoliciesAsync()
    {
        var shell = Shell.Current;
        if (shell == null)
        {
            return;
        }

        if (!_vaultSession.IsUnlocked)
        {
            await shell.DisplayAlertAsync("Password Policies", "Unlock the vault first.", "OK");
            return;
        }

        var filename = $"pwsafe_policies_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var path = await _saveFileService.PickSaveFilePathAsync(filename);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            var policies = _vaultSession.GetPasswordPoliciesSnapshot();
            var json = JsonSerializer.Serialize(policies, PolicySerializerOptions);
            await File.WriteAllTextAsync(path, json);
            await shell.DisplayAlertAsync("Password Policies", $"Policies exported:\n{path}", "OK");
        }
        catch (Exception ex)
        {
            await shell.DisplayAlertAsync("Password Policies", ex.Message, "OK");
        }
    }

    public void Refresh()
    {
        LoadPolicies();
    }

    private void LoadPolicies()
    {
        Policies.Clear();
        foreach (var policy in _vaultSession.GetPasswordPoliciesSnapshot())
        {
            Policies.Add(policy);
        }

        if (_originalPolicyName != null && !Policies.Any(policy => string.Equals(policy.Name, _originalPolicyName, StringComparison.OrdinalIgnoreCase)))
        {
            _originalPolicyName = null;
        }
    }

    private bool TryBuildPolicySnapshot(out VaultPasswordPolicySnapshot? policy, out string? errorMessage)
    {
        policy = null;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(PolicyName))
        {
            errorMessage = "Policy name is required.";
            return false;
        }

        if (!int.TryParse(PasswordLength, out var length) || length < 4 || length > 64)
        {
            errorMessage = "Password length must be between 4 and 64.";
            return false;
        }

        if (!int.TryParse(UppercaseCount, out var uppercase) || uppercase < 0)
        {
            errorMessage = "Uppercase count must be 0 or higher.";
            return false;
        }

        if (!int.TryParse(LowercaseCount, out var lowercase) || lowercase < 0)
        {
            errorMessage = "Lowercase count must be 0 or higher.";
            return false;
        }

        if (!int.TryParse(NumberCount, out var digits) || digits < 0)
        {
            errorMessage = "Number count must be 0 or higher.";
            return false;
        }

        if (!int.TryParse(SymbolCount, out var symbols) || symbols < 0)
        {
            errorMessage = "Symbol count must be 0 or higher.";
            return false;
        }

        if (IsHexOnly && IsPronounceable)
        {
            errorMessage = "Hex-only and pronounceable options cannot be combined.";
            return false;
        }

        if (IsHexOnly && (UseLowercase || UseUppercase || UseDigits || UseSymbols))
        {
            errorMessage = "Hex-only policies cannot include lowercase, uppercase, digits, or symbols.";
            return false;
        }

        var style = BuildStyleFlags();
        var minimumSymbols = IsPronounceable ? 0 : symbols;
        var minimumDigits = IsPronounceable ? 0 : digits;
        var minimumUppercase = IsPronounceable ? 0 : uppercase;
        var minimumLowercase = IsPronounceable ? 0 : lowercase;

        if (minimumSymbols + minimumDigits + minimumLowercase + minimumUppercase > length)
        {
            errorMessage = "Sum of minimum counts exceeds password length.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(CustomSymbols) && !PwCharPool.IsValidSymbols(CustomSymbols))
        {
            errorMessage = "Custom symbol set contains invalid characters.";
            return false;
        }

        var symbolSet = CustomSymbols.Trim();
        if (string.IsNullOrWhiteSpace(symbolSet) && UseSymbols && !IsHexOnly && !IsPronounceable)
        {
            symbolSet = UseEasyVision ? new string(PwCharPool.EasyVisionSymbolChars) : new string(PwCharPool.StdSymbolChars);
        }

        policy = new VaultPasswordPolicySnapshot
        {
            Name = PolicyName.Trim(),
            TotalPasswordLength = length,
            MinimumLowercaseCount = minimumLowercase,
            MinimumUppercaseCount = minimumUppercase,
            MinimumDigitCount = minimumDigits,
            MinimumSymbolCount = minimumSymbols,
            Style = style,
            SymbolSet = symbolSet
        };

        return true;
    }

    private PasswordPolicyStyle BuildStyleFlags()
    {
        if (IsHexOnly)
        {
            return PasswordPolicyStyle.UseHexDigits;
        }

        PasswordPolicyStyle style = 0;
        if (IsPronounceable) style |= PasswordPolicyStyle.MakePronounceable;
        if (UseEasyVision) style |= PasswordPolicyStyle.UseEasyVision;
        if (UseLowercase) style |= PasswordPolicyStyle.UseLowercase;
        if (UseUppercase) style |= PasswordPolicyStyle.UseUppercase;
        if (UseDigits) style |= PasswordPolicyStyle.UseDigits;
        if (UseSymbols) style |= PasswordPolicyStyle.UseSymbols;
        return style;
    }

    private char[] ResolveSymbolSet(VaultPasswordPolicySnapshot policy)
    {
        if (policy.Style.HasFlag(PasswordPolicyStyle.UseHexDigits))
        {
            return Array.Empty<char>();
        }

        if (policy.Style.HasFlag(PasswordPolicyStyle.MakePronounceable))
        {
            return PwCharPool.PronounceableSymbolChars;
        }

        if (!string.IsNullOrWhiteSpace(policy.SymbolSet))
        {
            return policy.SymbolSet.ToCharArray();
        }

        if (policy.Style.HasFlag(PasswordPolicyStyle.UseEasyVision))
        {
            return PwCharPool.EasyVisionSymbolChars;
        }

        return PwCharPool.StdSymbolChars;
    }
}
