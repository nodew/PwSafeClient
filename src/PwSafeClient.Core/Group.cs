using PwSafeLib.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwSafeClient.Core
{
    public class Group
    {
        private readonly List<Group> groups;
        private List<ItemData> items;
        private readonly string displayName;
        private readonly string id;

        private static readonly string rootID = "c4362666-5c42-4400-8bd3-266dd1138b4a";

        public Group(string id)
        {
            this.id = id;
            groups = new List<Group>();
            items = new List<ItemData>();

            var idx = id.LastIndexOf('.');
            if (idx >= 0)
            {
                displayName = id[(idx + 1)..];
            }
            else
            {
                displayName = id;
            }
        }

        public List<Group> Groups
        {
            get { return groups; }
        }

        public List<ItemData> Items
        {
            get { return items; }
        }

        public string Value
        {
            get { return displayName; }
        }

        public string ID
        {
            get { return id; }
        }

        public int ItemSize
        {
            get
            {
                return groups.Sum(item => item.ItemSize) + items.Count;
            }
        }

        public Group InsertGroup(string groupId)
        {
            if (id == rootID || (groupId != id && groupId.StartsWith(id)))
            {
                string _groupId = "";
                string subGroupName = "";

                if (string.IsNullOrEmpty(groupId)) {
                    if (id != rootID) {
                        throw new Exception("Invalid group ID");
                    }
                    
                }

                if (id == rootID) {
                    _groupId = groupId.Split('.').First();
                } 
                else 
                {
                    subGroupName = groupId.Substring(id.Length + 1).Split(".").First();
                }


                if (id != rootID) {
                    _groupId = id + "." + subGroupName;
                }

                Group? item = groups.Find(item => item.id == _groupId);

                if (item is null)
                {
                    var _group = new Group(_groupId);
                    _group.InsertGroup(groupId);
                    groups.Add(_group);
                }
                else if (item.id.Length < groupId.Length)
                {
                    item.InsertGroup(groupId);
                }
            }

            return this;
        }

        public List<string> GetGroupNames()
        {
            List<string> groupNames = new List<string>();
            groupNames.Add(id);
            groups.ForEach(group =>
            {
                groupNames = groupNames.Concat(group.GetGroupNames()).ToList();
            });

            return groupNames;
        }

        public void Traverse(Action<Group> handle)
        {
            handle(this);
            groups.ForEach(group => group.Traverse(handle));
        }

        public static async Task<Group> ReadFromPwsFileV3Async(PwsFileV3 pwsFile)
        {
            List<ItemData> _items = new List<ItemData>();
            ItemData item;

            do
            {
                item = await pwsFile.ReadRecordAsync();
                if (item is not null)
                {
                    _items.Add(item);
                }
            } while (item is not null);

            return GroupItems(_items);
        }

        public static Group GroupItems(List<ItemData> unGroupItems)
        {
            var query = unGroupItems.GroupBy(
                item => item.Group,
                (key, items) => Tuple.Create(key, items.ToList()));

            List<string> groupNames = query.Select((item) => item.Item1).ToList();
            
            Group rootGroup = FromList(groupNames);
            
            rootGroup.Traverse((group) =>
            {
                var _items = query.Where(item => item.Item1 == group.ID)
                                .Select(item => item.Item2)
                                .FirstOrDefault();
                if (_items is not null)
                {
                    group.items = _items;
                }
            });
            return rootGroup;
        }

        public static Group FromList(List<string> groups)
        {
            Group group = new Group(rootID);
            group.groups.Add(new Group(""));

            groups.ForEach(record =>
            {
                group.InsertGroup(record);
            });

            return group;
        }
    }
}
