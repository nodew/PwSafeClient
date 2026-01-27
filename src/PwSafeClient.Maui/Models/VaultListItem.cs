namespace PwSafeClient.Maui.Models;

public sealed class VaultListItem
{
    public required VaultListItemKind Kind { get; init; }

    // For groups: segment name at the current level.
    public string? GroupSegment { get; init; }

    // For groups: full path.
    public string? GroupPath { get; init; }

    // For entries: index into the current snapshot list.
    public int? EntryIndex { get; init; }

    public required string Title { get; init; }
    public string? Subtitle { get; init; }

    public int Depth { get; init; }

    public bool IsGroup => Kind == VaultListItemKind.Group;
}
