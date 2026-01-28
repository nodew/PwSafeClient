using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Vault.Browsing;
using PwSafeClient.AppCore.Vault.Editing;

namespace PwSafeClient.AppCore.Vault;

public sealed class VaultSession : IVaultSession
{
    private readonly IAppConfigurationStore _configStore;
    private Document? _document;

    public VaultSession(IAppConfigurationStore configStore)
    {
        _configStore = configStore;
    }

    public bool IsUnlocked => _document != null;
    public bool IsReadOnly { get; private set; }
    public string? CurrentFilePath { get; private set; }

    public IReadOnlyList<VaultEntrySnapshot> GetEntriesSnapshot()
    {
        if (_document == null)
        {
            return Array.Empty<VaultEntrySnapshot>();
        }

        var entries = _document.Entries;
        if (entries == null)
        {
            return Array.Empty<VaultEntrySnapshot>();
        }

        // Do NOT include secrets (password) in the snapshot.
        return entries
            .Select(e => new VaultEntrySnapshot
            {
                Title = string.IsNullOrWhiteSpace(e.Title) ? "(untitled)" : e.Title,
                UserName = e.UserName,
                GroupPath = e.Group?.ToString()
            })
            .ToList();
    }

    public VaultEntryDetailsSnapshot? GetEntryDetailsSnapshot(int entryIndex, bool includePassword)
    {
        if (_document == null)
        {
            return null;
        }

        var entries = _document.Entries;
        if (entries == null || entryIndex < 0 || entryIndex >= entries.Count)
        {
            return null;
        }

        var e = entries[entryIndex];

        return new VaultEntryDetailsSnapshot
        {
            Title = string.IsNullOrWhiteSpace(e.Title) ? "(untitled)" : e.Title,
            UserName = e.UserName,
            Password = includePassword ? e.Password : null,
            Url = e.Url,
            Notes = e.Notes,
            GroupPath = e.Group?.ToString(),
            PasswordPolicyName = e.PasswordPolicyName
        };
    }

    public VaultEntryUpsertResult CreateEntry(VaultEntryEditRequest request)
    {
        if (_document == null)
        {
            return VaultEntryUpsertResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultEntryUpsertResult.Fail("Vault is read-only.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return VaultEntryUpsertResult.Fail("Title is required.");
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            return VaultEntryUpsertResult.Fail("Password is required.");
        }

        if (!TryApplyPasswordPolicy(request, out var policyError))
        {
            return VaultEntryUpsertResult.Fail(policyError ?? "Password does not meet the policy requirements.");
        }

        var entry = new Entry
        {
            Title = request.Title.Trim(),
            UserName = request.UserName ?? string.Empty,
            Password = request.Password,
            Url = request.Url ?? string.Empty,
            Notes = request.Notes ?? string.Empty,
            Group = ToGroupPath(request.GroupPath)
        };

        ApplyPasswordPolicyToEntry(entry, request.PasswordPolicyName ?? GetDefaultPasswordPolicyName());

        var entries = _document.Entries;
        if (entries == null)
        {
            return VaultEntryUpsertResult.Fail("Vault entries are unavailable.");
        }

        entries.Add(entry);
        RemoveEmptyGroupHeader(request.GroupPath);
        return VaultEntryUpsertResult.Success(entries.Count - 1);
    }

    public VaultEntryUpsertResult UpdateEntry(int entryIndex, VaultEntryEditRequest request)
    {
        if (_document == null)
        {
            return VaultEntryUpsertResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultEntryUpsertResult.Fail("Vault is read-only.");
        }

        var entries = _document.Entries;
        if (entries == null || entryIndex < 0 || entryIndex >= entries.Count)
        {
            return VaultEntryUpsertResult.Fail("Entry not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return VaultEntryUpsertResult.Fail("Title is required.");
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            return VaultEntryUpsertResult.Fail("Password is required.");
        }

        if (!TryApplyPasswordPolicy(request, out var policyError))
        {
            return VaultEntryUpsertResult.Fail(policyError ?? "Password does not meet the policy requirements.");
        }

        var entry = entries[entryIndex];
        entry.Title = request.Title.Trim();
        entry.UserName = request.UserName ?? string.Empty;
        entry.Password = request.Password;
        entry.Url = request.Url ?? string.Empty;
        entry.Notes = request.Notes ?? string.Empty;
        entry.Group = ToGroupPath(request.GroupPath);

        ApplyPasswordPolicyToEntry(entry, request.PasswordPolicyName ?? GetDefaultPasswordPolicyName());

        RemoveEmptyGroupHeader(request.GroupPath);

        return VaultEntryUpsertResult.Success(entryIndex);
    }

    public IReadOnlyList<VaultPasswordPolicySnapshot> GetPasswordPoliciesSnapshot()
    {
        if (_document == null)
        {
            return Array.Empty<VaultPasswordPolicySnapshot>();
        }

        return _document.NamedPasswordPolicies
            .Select(policy =>
            {
                var symbols = policy.GetSpecialSymbolSet();
                var symbolSet = symbols.Length == 0 ? string.Empty : string.Join("", symbols);

                return new VaultPasswordPolicySnapshot
                {
                    Name = policy.Name,
                    TotalPasswordLength = policy.TotalPasswordLength,
                    MinimumLowercaseCount = policy.MinimumLowercaseCount,
                    MinimumUppercaseCount = policy.MinimumUppercaseCount,
                    MinimumDigitCount = policy.MinimumDigitCount,
                    MinimumSymbolCount = policy.MinimumSymbolCount,
                    Style = policy.Style,
                    SymbolSet = symbolSet
                };
            })
            .OrderBy(policy => policy.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public VaultEntryUpsertResult SavePasswordPolicy(VaultPasswordPolicySnapshot policy, string? originalName = null)
    {
        if (_document == null)
        {
            return VaultEntryUpsertResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultEntryUpsertResult.Fail("Vault is read-only.");
        }

        if (string.IsNullOrWhiteSpace(policy.Name))
        {
            return VaultEntryUpsertResult.Fail("Policy name is required.");
        }

        var policies = _document.NamedPasswordPolicies;
        var existing = policies.FirstOrDefault(p => string.Equals(p.Name, originalName ?? policy.Name, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            if (policies.Any(p => string.Equals(p.Name, policy.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return VaultEntryUpsertResult.Fail("A policy with this name already exists.");
            }

            existing = new NamedPasswordPolicy(policy.Name, policy.TotalPasswordLength);
            policies.Add(existing);
        }
        else if (!string.Equals(existing.Name, policy.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (policies.Any(p => string.Equals(p.Name, policy.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return VaultEntryUpsertResult.Fail("A policy with this name already exists.");
            }

            existing.Name = policy.Name;
        }

        existing.TotalPasswordLength = policy.TotalPasswordLength;
        existing.MinimumLowercaseCount = policy.MinimumLowercaseCount;
        existing.MinimumUppercaseCount = policy.MinimumUppercaseCount;
        existing.MinimumDigitCount = policy.MinimumDigitCount;
        existing.MinimumSymbolCount = policy.MinimumSymbolCount;
        existing.Style = policy.Style;

        var symbolChars = policy.SymbolSet.Length == 0
            ? Array.Empty<char>()
            : policy.SymbolSet.ToCharArray();
        existing.SetSpecialSymbolSet(symbolChars);

        return VaultEntryUpsertResult.Success(0);
    }

    public VaultEntryUpsertResult DeletePasswordPolicy(string name)
    {
        if (_document == null)
        {
            return VaultEntryUpsertResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultEntryUpsertResult.Fail("Vault is read-only.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return VaultEntryUpsertResult.Fail("Policy name is required.");
        }

        var policies = _document.NamedPasswordPolicies;
        var policy = policies.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (policy == null)
        {
            return VaultEntryUpsertResult.Fail("Policy not found.");
        }

        policies.Remove(policy);
        return VaultEntryUpsertResult.Success(0);
    }

    public VaultEntryUpsertResult SetDefaultPasswordPolicy(string name)
    {
        if (_document == null)
        {
            return VaultEntryUpsertResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultEntryUpsertResult.Fail("Vault is read-only.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return VaultEntryUpsertResult.Fail("Policy name is required.");
        }

        var policy = _document.NamedPasswordPolicies.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (policy == null)
        {
            return VaultEntryUpsertResult.Fail("Policy not found.");
        }

        var headers = _document.Headers
            .Where(header => header.HeaderType == HeaderType.NamedPasswordPolicies)
            .ToList();

        var header = headers.FirstOrDefault() ?? new Header(HeaderType.NamedPasswordPolicies);
        if (!headers.Contains(header))
        {
            _document.Headers.Add(header);
        }

        header.Text = name;
        return VaultEntryUpsertResult.Success(0);
    }

    public string? GetDefaultPasswordPolicyName()
    {
        if (_document == null)
        {
            return null;
        }

        var header = _document.Headers.FirstOrDefault(h => h.HeaderType == HeaderType.NamedPasswordPolicies);
        if (header == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(header.Text))
        {
            return header.Text;
        }

        return header.Uuid != Guid.Empty ? header.Uuid.ToString() : null;
    }

    private bool TryApplyPasswordPolicy(VaultEntryEditRequest request, out string? errorMessage)
    {
        errorMessage = null;

        if (_document == null)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return true;
        }

        var policyName = request.PasswordPolicyName;
        if (string.IsNullOrWhiteSpace(policyName))
        {
            policyName = GetDefaultPasswordPolicyName();
        }

        if (string.IsNullOrWhiteSpace(policyName))
        {
            return true;
        }

        var policy = _document.NamedPasswordPolicies.FirstOrDefault(p => string.Equals(p.Name, policyName, StringComparison.OrdinalIgnoreCase));
        if (policy == null)
        {
            errorMessage = "Password policy not found.";
            return false;
        }

        if (!DoesPasswordMeetPolicy(request.Password, policy, out var validationError))
        {
            errorMessage = validationError;
            return false;
        }

        return true;
    }

    private void ApplyPasswordPolicyToEntry(Entry entry, string? policyName)
    {
        if (_document == null || string.IsNullOrWhiteSpace(policyName))
        {
            return;
        }

        var policy = _document.NamedPasswordPolicies.FirstOrDefault(p => string.Equals(p.Name, policyName, StringComparison.OrdinalIgnoreCase));
        if (policy == null)
        {
            return;
        }

        entry.PasswordPolicy.TotalPasswordLength = policy.TotalPasswordLength;
        entry.PasswordPolicy.MinimumLowercaseCount = policy.MinimumLowercaseCount;
        entry.PasswordPolicy.MinimumUppercaseCount = policy.MinimumUppercaseCount;
        entry.PasswordPolicy.MinimumDigitCount = policy.MinimumDigitCount;
        entry.PasswordPolicy.MinimumSymbolCount = policy.MinimumSymbolCount;
        entry.PasswordPolicy.Style = policy.Style;
        entry.PasswordPolicy.SetSpecialSymbolSet(policy.GetSpecialSymbolSet());
        entry.PasswordPolicyName = policy.Name;
    }

    private static bool DoesPasswordMeetPolicy(string password, NamedPasswordPolicy policy, out string? errorMessage)
    {
        errorMessage = null;

        if (policy.TotalPasswordLength > 0 && password.Length < policy.TotalPasswordLength)
        {
            errorMessage = $"Password must be at least {policy.TotalPasswordLength} characters.";
            return false;
        }

        if (policy.Style.HasFlag(PasswordPolicyStyle.UseHexDigits))
        {
            var invalidHex = password.Any(ch => !Uri.IsHexDigit(ch));
            if (invalidHex)
            {
                errorMessage = "Password must contain only hexadecimal characters.";
                return false;
            }

            return true;
        }

        var lowercaseCount = password.Count(char.IsLower);
        var uppercaseCount = password.Count(char.IsUpper);
        var digitCount = password.Count(char.IsDigit);
        var symbolSet = policy.GetSpecialSymbolSet();

        var symbolCount = symbolSet.Length == 0
            ? password.Count(ch => !char.IsLetterOrDigit(ch))
            : password.Count(ch => symbolSet.Contains(ch));

        if (policy.Style.HasFlag(PasswordPolicyStyle.UseLowercase) && lowercaseCount < policy.MinimumLowercaseCount)
        {
            errorMessage = $"Password must include at least {policy.MinimumLowercaseCount} lowercase characters.";
            return false;
        }

        if (policy.Style.HasFlag(PasswordPolicyStyle.UseUppercase) && uppercaseCount < policy.MinimumUppercaseCount)
        {
            errorMessage = $"Password must include at least {policy.MinimumUppercaseCount} uppercase characters.";
            return false;
        }

        if (policy.Style.HasFlag(PasswordPolicyStyle.UseDigits) && digitCount < policy.MinimumDigitCount)
        {
            errorMessage = $"Password must include at least {policy.MinimumDigitCount} digits.";
            return false;
        }

        if (policy.Style.HasFlag(PasswordPolicyStyle.UseSymbols) && symbolCount < policy.MinimumSymbolCount)
        {
            errorMessage = $"Password must include at least {policy.MinimumSymbolCount} symbols.";
            return false;
        }

        if (policy.Style.HasFlag(PasswordPolicyStyle.UseSymbols))
        {
            var invalidSymbols = password.Any(ch => !char.IsLetterOrDigit(ch) && symbolSet.Length > 0 && !symbolSet.Contains(ch));
            if (invalidSymbols)
            {
                errorMessage = "Password contains symbols outside the allowed symbol set.";
                return false;
            }
        }

        return true;
    }

    public VaultEntryDeleteResult DeleteEntry(int entryIndex)
    {
        if (_document == null)
        {
            return VaultEntryDeleteResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultEntryDeleteResult.Fail("Vault is read-only.");
        }

        var entries = _document.Entries;
        if (entries == null || entryIndex < 0 || entryIndex >= entries.Count)
        {
            return VaultEntryDeleteResult.Fail("Entry not found.");
        }

        entries.RemoveAt(entryIndex);
        return VaultEntryDeleteResult.Success();
    }

    public VaultGroupOperationResult CreateGroup(string groupPath)
    {
        if (_document == null)
        {
            return VaultGroupOperationResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultGroupOperationResult.Fail("Vault is read-only.");
        }

        var normalized = NormalizeGroupPath(groupPath);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return VaultGroupOperationResult.Fail("Group name is required.");
        }

        if (GroupExists(normalized))
        {
            return VaultGroupOperationResult.Fail("Group already exists.");
        }

        _document.Headers.Add(new Header(HeaderType.EmptyGroups) { Text = normalized });
        return VaultGroupOperationResult.Success();
    }

    public VaultGroupOperationResult RenameGroup(string groupPath, string newGroupPath)
    {
        if (_document == null)
        {
            return VaultGroupOperationResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultGroupOperationResult.Fail("Vault is read-only.");
        }

        var oldPath = NormalizeGroupPath(groupPath);
        var updatedPath = NormalizeGroupPath(newGroupPath);

        if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(updatedPath))
        {
            return VaultGroupOperationResult.Fail("Group name is required.");
        }

        if (string.Equals(oldPath, updatedPath, StringComparison.OrdinalIgnoreCase))
        {
            return VaultGroupOperationResult.Success();
        }

        if (updatedPath.StartsWith(oldPath + ".", StringComparison.OrdinalIgnoreCase))
        {
            return VaultGroupOperationResult.Fail("Cannot move a group into itself.");
        }

        var entries = _document.Entries;
        if (entries == null)
        {
            return VaultGroupOperationResult.Fail("Vault entries are unavailable.");
        }

        var affected = 0;
        foreach (var entry in entries)
        {
            var current = NormalizeGroupPath(entry.Group?.ToString());
            if (!IsSameOrChild(current, oldPath))
            {
                continue;
            }

            var suffix = current.Length == oldPath.Length
                ? string.Empty
                : current[(oldPath.Length + 1)..];

            var newPath = string.IsNullOrEmpty(suffix)
                ? updatedPath
                : $"{updatedPath}.{suffix}";

            entry.Group = ToGroupPath(newPath);
            affected++;
        }

        UpdateEmptyGroupHeaders(oldPath, updatedPath);
        RemoveEmptyGroupHeader(updatedPath, entries);

        return VaultGroupOperationResult.Success(affected);
    }

    public VaultGroupOperationResult DeleteEmptyGroup(string groupPath)
    {
        if (_document == null)
        {
            return VaultGroupOperationResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultGroupOperationResult.Fail("Vault is read-only.");
        }

        var normalized = NormalizeGroupPath(groupPath);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return VaultGroupOperationResult.Fail("Group name is required.");
        }

        var emptyGroups = GetEmptyGroupHeaders();
        if (!emptyGroups.Any(g => string.Equals(g, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return VaultGroupOperationResult.Fail("Group not found.");
        }

        if (HasEntriesInGroup(normalized))
        {
            return VaultGroupOperationResult.Fail("Group is not empty.");
        }

        if (emptyGroups.Any(g => IsChildPath(g, normalized)))
        {
            return VaultGroupOperationResult.Fail("Group contains subgroups.");
        }

        RemoveEmptyGroupHeader(normalized);
        return VaultGroupOperationResult.Success();
    }

    public VaultGroupOperationResult MoveEntry(int entryIndex, string newGroupPath)
    {
        if (_document == null)
        {
            return VaultGroupOperationResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultGroupOperationResult.Fail("Vault is read-only.");
        }

        var entries = _document.Entries;
        if (entries == null || entryIndex < 0 || entryIndex >= entries.Count)
        {
            return VaultGroupOperationResult.Fail("Entry not found.");
        }

        var normalized = NormalizeGroupPath(newGroupPath);
        var entry = entries[entryIndex];
        var current = NormalizeGroupPath(entry.Group?.ToString());

        if (string.Equals(current, normalized, StringComparison.OrdinalIgnoreCase))
        {
            return VaultGroupOperationResult.Success();
        }

        entry.Group = ToGroupPath(normalized);
        RemoveEmptyGroupHeader(normalized);

        return VaultGroupOperationResult.Success(1);
    }

    private static GroupPath ToGroupPath(string? groupPath)
    {
        if (string.IsNullOrWhiteSpace(groupPath))
        {
            return new GroupPath();
        }

        var segments = groupPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return segments.Length == 0 ? new GroupPath() : new GroupPath(segments);
    }

    public IReadOnlyList<string> GetEmptyGroupPaths()
    {
        if (_document == null)
        {
            return Array.Empty<string>();
        }

        return GetEmptyGroupHeaders();
    }

    private List<string> GetEmptyGroupHeaders()
    {
        if (_document == null)
        {
            return new List<string>();
        }

        return _document.Headers
            .Where(header => header.HeaderType == HeaderType.EmptyGroups && !string.IsNullOrWhiteSpace(header.Text))
            .Select(header => NormalizeGroupPath(header.Text))
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private bool GroupExists(string groupPath)
    {
        if (string.IsNullOrWhiteSpace(groupPath))
        {
            return false;
        }

        if (HasEntriesInGroup(groupPath))
        {
            return true;
        }

        return GetEmptyGroupHeaders()
            .Any(existing => string.Equals(existing, groupPath, StringComparison.OrdinalIgnoreCase));
    }

    private bool HasEntriesInGroup(string groupPath)
    {
        if (_document?.Entries == null || string.IsNullOrWhiteSpace(groupPath))
        {
            return false;
        }

        foreach (var entry in _document.Entries)
        {
            var current = NormalizeGroupPath(entry.Group?.ToString());
            if (IsSameOrChild(current, groupPath))
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateEmptyGroupHeaders(string oldPath, string newPath)
    {
        if (_document == null)
        {
            return;
        }

        var headers = _document.Headers
            .Where(header => header.HeaderType == HeaderType.EmptyGroups && !string.IsNullOrWhiteSpace(header.Text))
            .ToList();

        foreach (var header in headers)
        {
            var current = NormalizeGroupPath(header.Text);
            if (!IsSameOrChild(current, oldPath))
            {
                continue;
            }

            var suffix = current.Length == oldPath.Length
                ? string.Empty
                : current[(oldPath.Length + 1)..];

            var updated = string.IsNullOrEmpty(suffix)
                ? newPath
                : $"{newPath}.{suffix}";

            header.Text = updated;
        }
    }

    private void RemoveEmptyGroupHeader(string? groupPath)
    {
        if (_document == null)
        {
            return;
        }

        var normalized = NormalizeGroupPath(groupPath);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        var headers = _document.Headers
            .Where(header => header.HeaderType == HeaderType.EmptyGroups && !string.IsNullOrWhiteSpace(header.Text))
            .ToList();

        foreach (var header in headers)
        {
            if (string.Equals(NormalizeGroupPath(header.Text), normalized, StringComparison.OrdinalIgnoreCase))
            {
                _document.Headers.Remove(header);
            }
        }
    }

    private void RemoveEmptyGroupHeader(string groupPath, EntryCollection entries)
    {
        if (entries == null)
        {
            return;
        }

        if (!entries.Any(entry => IsSameOrChild(NormalizeGroupPath(entry.Group?.ToString()), groupPath)))
        {
            return;
        }

        RemoveEmptyGroupHeader(groupPath);
    }

    private static bool IsSameOrChild(string currentPath, string groupPath)
    {
        if (string.IsNullOrWhiteSpace(groupPath))
        {
            return false;
        }

        if (string.Equals(currentPath, groupPath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return currentPath.StartsWith(groupPath + ".", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsChildPath(string currentPath, string groupPath)
    {
        if (string.IsNullOrWhiteSpace(groupPath))
        {
            return false;
        }

        return currentPath.StartsWith(groupPath + ".", StringComparison.OrdinalIgnoreCase);
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

    public Task<VaultLoadResult> LoadAsync(string filePath, string password, bool readOnly, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return Task.FromResult(VaultLoadResult.Fail(VaultLoadError.FileNotFound, $"File not found: {filePath}"));
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = Document.Load(filePath, password);
            document.IsReadOnly = readOnly;

            _document = document;
            IsReadOnly = readOnly;
            CurrentFilePath = filePath;

            return Task.FromResult(VaultLoadResult.Success());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (FormatException)
        {
            return Task.FromResult(VaultLoadResult.Fail(VaultLoadError.InvalidPasswordOrInvalidFormat, "Invalid Password Safe file or password mismatch."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(VaultLoadResult.Fail(VaultLoadError.Unknown, $"Error loading file: {ex.Message}"));
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_document == null || IsReadOnly)
        {
            return;
        }

        if (!_document.HasChanged)
        {
            return;
        }

        var path = CurrentFilePath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        await BackupDocumentAsync(path, cancellationToken);
        _document.Save(path);
    }

    public async Task<VaultBackupResult> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_document == null)
        {
            return VaultBackupResult.Fail("Vault is locked.");
        }

        var path = CurrentFilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return VaultBackupResult.Fail("Vault file not found.");
        }

        try
        {
            var backupPath = await BackupDocumentAsync(path, cancellationToken);
            return string.IsNullOrWhiteSpace(backupPath)
                ? VaultBackupResult.Fail("Backup was not created.")
                : VaultBackupResult.Success(backupPath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return VaultBackupResult.Fail(ex.Message);
        }
    }

    public async Task<VaultChangePassphraseResult> ChangePassphraseAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_document == null)
        {
            return VaultChangePassphraseResult.Fail("Vault is locked.");
        }

        if (IsReadOnly)
        {
            return VaultChangePassphraseResult.Fail("Vault is read-only.");
        }

        if (string.IsNullOrEmpty(currentPassword))
        {
            return VaultChangePassphraseResult.Fail("Current password is required.");
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            return VaultChangePassphraseResult.Fail("New password is required.");
        }

        var path = CurrentFilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return VaultChangePassphraseResult.Fail("Vault file not found.");
        }

        try
        {
            // Verify current password against the on-disk file.
            _ = Document.Load(path, currentPassword);

            await BackupDocumentAsync(path, cancellationToken);

            _document.ChangePassphrase(newPassword);
            _document.Save(path);

            return VaultChangePassphraseResult.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (FormatException)
        {
            return VaultChangePassphraseResult.Fail("Invalid Password Safe file or password mismatch.");
        }
        catch (Exception ex)
        {
            return VaultChangePassphraseResult.Fail(ex.Message);
        }
    }

    private async Task<string?> BackupDocumentAsync(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(filePath))
        {
            return null;
        }

        var maxBackupCount = 10;
        try
        {
            var config = await _configStore.LoadAsync(cancellationToken);
            maxBackupCount = config.MaxBackupCount;
        }
        catch
        {
            // ignore config failures; default is fine
        }

        if (maxBackupCount <= 0)
        {
            return null;
        }

        var targetFolder = Path.GetDirectoryName(filePath);
        var filename = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrWhiteSpace(targetFolder) || string.IsNullOrWhiteSpace(filename))
        {
            return null;
        }

        var backupFiles = Directory.GetFiles(targetFolder, $"{filename}_*.ibak")
            .OrderByDescending(GetBackupVersion)
            .ToArray();

        var backupVersion = backupFiles.Length > 0 ? GetBackupVersion(backupFiles[0]) + 1 : 1;

        var backupPath = Path.Combine(targetFolder, $"{filename}_{backupVersion}.ibak");
        File.Copy(filePath, backupPath, true);

        if (backupFiles.Length >= maxBackupCount)
        {
            var start = Math.Max(0, maxBackupCount - 1);
            for (var i = start; i < backupFiles.Length; i++)
            {
                File.Delete(backupFiles[i]);
            }
        }

        return backupPath;
    }

    private static int GetBackupVersion(string backupFilePath)
    {
        var name = Path.GetFileNameWithoutExtension(backupFilePath);
        if (string.IsNullOrWhiteSpace(name))
        {
            return 0;
        }

        var underscore = name.LastIndexOf('_');
        if (underscore <= 0 || underscore == name.Length - 1)
        {
            return 0;
        }

        var versionText = name[(underscore + 1)..];
        return int.TryParse(versionText, out var version) ? version : 0;
    }

    public void Unload()
    {
        _document = null;
        IsReadOnly = false;
        CurrentFilePath = null;
    }
}
