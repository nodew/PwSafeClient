using Medo.Security.Cryptography.PasswordSafe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PwSafeClient.Core;

public static class PwSafeClientHelper
{
    public static Group GetGroupInfo(List<Entry> entries)
    {
        var groupList = new List<GroupPath>();

        foreach (Entry entry in entries)
        {
            if (!string.IsNullOrEmpty(entry.Group) && !groupList.Contains(entry.Group))
            {
                groupList.Add(entry.Group);
            }
        }

        var oderedGroupList = groupList.OrderBy(item => item.ToString());

        var group = new Group(string.Empty);

        foreach (GroupPath path in oderedGroupList)
        {
            group.InsertByGroupPath(path);
        }

        return group;
    }

    public static string GeneratePassword(PasswordPolicy policy)
    {
        throw new NotImplementedException();
    }
}
