using System.Threading;
using System.Threading.Tasks;

using PwSafeClient.AppCore.Vault.Browsing;
using PwSafeClient.AppCore.Vault.Editing;

namespace PwSafeClient.AppCore.Vault;

public interface IVaultSession
{
    bool IsUnlocked { get; }
    bool IsReadOnly { get; }
    string? CurrentFilePath { get; }

    IReadOnlyList<VaultEntrySnapshot> GetEntriesSnapshot();

    VaultEntryDetailsSnapshot? GetEntryDetailsSnapshot(int entryIndex, bool includePassword);

    VaultEntryUpsertResult CreateEntry(VaultEntryEditRequest request);
    VaultEntryUpsertResult UpdateEntry(int entryIndex, VaultEntryEditRequest request);
    VaultEntryDeleteResult DeleteEntry(int entryIndex);

    IReadOnlyList<VaultPasswordPolicySnapshot> GetPasswordPoliciesSnapshot();
    VaultEntryUpsertResult SavePasswordPolicy(VaultPasswordPolicySnapshot policy, string? originalName = null);
    VaultEntryUpsertResult DeletePasswordPolicy(string name);
    VaultEntryUpsertResult SetDefaultPasswordPolicy(string name);
    string? GetDefaultPasswordPolicyName();

    VaultGroupOperationResult CreateGroup(string groupPath);
    VaultGroupOperationResult RenameGroup(string groupPath, string newGroupPath);
    VaultGroupOperationResult DeleteEmptyGroup(string groupPath);
    VaultGroupOperationResult MoveEntry(int entryIndex, string newGroupPath);

    IReadOnlyList<string> GetEmptyGroupPaths();

    Task<VaultLoadResult> LoadAsync(string filePath, string password, bool readOnly, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);

    Task<VaultBackupResult> CreateBackupAsync(CancellationToken cancellationToken = default);

    Task<VaultChangePassphraseResult> ChangePassphraseAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    void Unload();
}
