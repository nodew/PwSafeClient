using System.Collections.Generic;
using System.Linq;

using Medo.Security.Cryptography.PasswordSafe;

namespace PwSafeClient.Shared;

public class Group
{
    private string name;

    private Group? parent;

    private List<Group> children;

    public Group()
    {
        name = string.Empty;
        parent = null;
        children = [];
    }

    public Group(string groupName, Group parent)
    {
        this.parent = parent;
        name = groupName;
        children = [];
    }

    public string Name
    {
        get => name;
        set => name = value;
    }

    public Group? Parent
    {
        get => parent;
        set => parent = value;
    }

    public List<Group> Children
    {
        get => children;
        set => children = value;
    }

    public bool IsRoot => Parent == null;

    public GroupPath GetGroupPath()
    {
        if (IsRoot)
        {
            return new GroupPath();
        }

        List<string> segments = [Name];
        var node = this;

        while (node.Parent != null)
        {
            node = node.Parent;
            segments.Add(node.Name);
        }

        segments.Reverse();

        return new GroupPath([.. segments]);
    }

    public void InsertByGroupPath(GroupPath path)
    {
        var segments = path.GetSegments();
        InsertBySegments(segments);
    }

    public void InsertBySegments(string[] segments)
    {
        if (segments.Length == 0) return;

        var childGroupName = segments[0];

        string[] subSegments;

        if (segments.Length > 1)
        {
            subSegments = segments[1..segments.Length];
        }
        else
        {
            subSegments = [];
        }

        var child = Children.Where(item => item.Name == childGroupName).FirstOrDefault();

        if (child == null)
        {
            child = new Group(childGroupName, this);

            Children.Add(child);
        }

        if (subSegments.Length > 0)
        {
            child.InsertBySegments(subSegments);
        }
    }

    public Group? GetSubGroupByGroupPath(GroupPath path)
    {
        var segments = path.GetSegments();
        return GetSubGroupBySegments(segments);
    }

    public Group? GetSubGroupBySegments(string[] segments)
    {
        if (segments.Length == 0) return this;

        var childGroupName = segments[0];

        var child = Children.Where(item => item.Name == childGroupName).FirstOrDefault();

        if (child == null)
        {
            return null;
        }

        return child.GetSubGroupBySegments(segments[1..segments.Length]);
    }
}
