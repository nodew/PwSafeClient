using PwSafeLib.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PwSafeClient.Core
{
    public static class PwsFileHelper
    {
        public static async Task<List<ItemData>> ReadAllItemsFromPwsFileV3Async(PwsFileV3 pwsFile)
        {
            List<ItemData> items = new List<ItemData>();
            ItemData item;

            do
            {
                item = await pwsFile.ReadRecordAsync();
                if (item is not null)
                {
                    items.Add(item);
                }
            } while (item is not null);

            return items;
        }
    }
}
