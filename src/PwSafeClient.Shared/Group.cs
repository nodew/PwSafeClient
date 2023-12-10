using Medo.Security.Cryptography.PasswordSafe;
using System.Collections.Generic;
using System.Linq;

namespace PwSafeClient.Shared;

public class Group
{
    public Group()
    {
        Name = string.Empty;
        Parent = null;
        Children = [];
    }
    public Group(string groupName, Group parent)
    {
        Name = groupName;
        Parent = parent;
        Children = [];
    }

    public string Name { get; set; }

    public Group? Parent { get; set; }

    public List<Group> Children { get; set; }

    public bool IsRoot => Parent == null;
    public string GetGroupPath()
    {
        if (Parent == null && string.IsNullOrEmpty(Name))
        {
            return string.Empty;
        }

        List<string> segments = new List<string> { Name };
        var node = this;

        while (node.Parent != null)
        {
            node = node.Parent;
            segments.Add(node.Name);
        }

        segments.Reverse();

        return new GroupPath(segments.ToArray());
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

        string[] nestedPath;

        if (segments.Length > 1)
        {
            nestedPath = segments[1..(segments.Length - 1)];
        }
        else
        {
            nestedPath = new string[0];
        }

        var child = Children.Where(item => item.Name == childGroupName).FirstOrDefault();

        if (child == null)
        {
            child = new Group(childGroupName, this);

            Children.Add(child);
        }

        if (nestedPath != null)
        {
            child.InsertBySegments(nestedPath);
        }
    }
}
