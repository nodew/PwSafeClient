using Medo.Security.Cryptography.PasswordSafe;
using System.Collections.Generic;

namespace PwSafeClient.Core;

public static class PwSafeClientHelper
{
    public static List<string> ListGroupInfo(Document document)
    {
        var list = new List<string>();
        foreach (Entry entry in document.Entries)
        {
            if (!string.IsNullOrEmpty(entry.Group) && !list.Contains(entry.Group))
            {
                list.Add(entry.Group);
            }
        }

        return list;
    }
}
