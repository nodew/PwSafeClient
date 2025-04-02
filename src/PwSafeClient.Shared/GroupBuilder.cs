using System.Collections.Generic;
using System.Linq;

using Medo.Security.Cryptography.PasswordSafe;

namespace PwSafeClient.Shared;

public class GroupBuilder
{
    private readonly List<Entry> entries;
    private Group root;

    public GroupBuilder()
    {
        entries = new List<Entry>();
        root = new Group();
    }

    public GroupBuilder(List<Entry> entries)
    {
        this.entries = entries;
        root = new Group();
    }

    public GroupBuilder SetEntries(List<Entry> entries)
    {
        this.entries.Clear();
        this.entries.AddRange(entries);

        return this;
    }

    public Group Root => root;

    public Group Build()
    {
        var groupList = new List<GroupPath>();

        foreach (var entry in entries)
        {
            if (!string.IsNullOrEmpty(entry.Group) && !groupList.Contains(entry.Group))
            {
                groupList.Add(entry.Group);
            }
        }

        var orderedGroupList = groupList.OrderBy(item => item.ToString());

        foreach (var path in orderedGroupList)
        {
            Root.InsertByGroupPath(path);
        }

        return Root;
    }
}
