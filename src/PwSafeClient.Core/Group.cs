using Medo.Security.Cryptography.PasswordSafe;
using System.Collections.Generic;
using System.Linq;

namespace PwSafeClient.Core;

public class Group
{
    public Group(string groupName)
    {
        Name = groupName;
        Parent = null;
        Children = new List<Group>();
    }

    public string Name { get; set; }

    public Group? Parent { get; set; }

    public List<Group> Children { get; set; }

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
            child = new Group(childGroupName)
            {
                Parent = this,
            };

            Children.Add(child);
        }

        if (nestedPath != null)
        {
            child.InsertBySegments(nestedPath);
        }
    }
}
