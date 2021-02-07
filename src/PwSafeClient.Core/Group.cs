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
        private readonly List<Group> groups = new List<Group>();
        private List<ItemData> items = new List<ItemData>();
        private readonly string name;
        private readonly Group? parent;
        private string? path;

        private Group()
        {
            this.name = "";
        }

        public Group(string name, Group? parent = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("Invalid group name");
            }
            this.name = name;
            this.parent = parent;
        }

        public List<Group> Groups
        {
            get { return groups; }
        }

        public List<ItemData> Items
        {
            get { return items; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Path
        {
            get {
                if (parent is null || parent.IsRoot)
                {
                    return name;
                }
                if (string.IsNullOrEmpty(path))
                {
                    path = $"{parent.Path}.{name}";
                }

                return path;
            }
        }

        public int ItemSize
        {
            get
            {
                return groups.Sum(item => item.ItemSize) + items.Count;
            }
        }

        public bool IsRoot
        {
            get { return string.IsNullOrEmpty(name); }
        }

        public Group InsertGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new Exception("Invalid group name");
            }

            string currentName = group.Split('.').First();
            string subGroupId = "";

            if (currentName != group)
            {
                subGroupId = group[(currentName.Length + 1)..];
            }

            Group? item = groups.Find(item => item.name == currentName);

            if (item is null)
            {
                item = new Group(currentName, this);
                groups.Add(item);
            }
            
            if (!string.IsNullOrWhiteSpace(subGroupId))
            {
                item.InsertGroup(subGroupId);
            }

            return this;
        }

        public IEnumerable<string> GetFirstLevelGroups()
        {
            IEnumerable<string> names = new List<string>();
            groups.ForEach(item =>
            {
                names = names.Append(item.Path);
            });
            return names;
        }

        public IEnumerable<string> GetAllNestedGroups()
        {
            IEnumerable<string> names = new List<string>();
            groups.ForEach(group =>
            {
                names = names.Append(group.Path).Concat(group.GetAllNestedGroups());
            });

            return names;
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
            var query = from item in unGroupItems
                        group item by item.Group;

            List<string> groupNames = query.Select((item) => item.Key).ToList();

            Group rootGroup = FromList(groupNames);

            rootGroup.Traverse((group) =>
            {
                var _items = query.Where(item => item.Key == group.Path).FirstOrDefault();
                if (_items is not null)
                {
                    group.items = new List<ItemData>(_items);
                }
            });
            return rootGroup;
        }

        public static Group FromList(List<string> records)
        {
            // root group
            Group rootGroup = RootGroup();

            records.ForEach(record =>
            {
                if (!string.IsNullOrWhiteSpace(record))
                {
                    rootGroup.InsertGroup(record);
                }
            });

            return rootGroup;
        }

        public static Group RootGroup()
        {
            Group rootGroup = new Group();
            return rootGroup;
        }
    }
}
