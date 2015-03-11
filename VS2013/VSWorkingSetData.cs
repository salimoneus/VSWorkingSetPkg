using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Company.VSWorkingSetPkg
{
    [Serializable]
    public class ItemData
    {
        public ItemData()
        {
        }

        public ItemData(string itemName, string fullPath)
        {
            name = itemName;
            itemFullPath = fullPath;
        }

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(Object obj)
        {
            ItemData other = obj as ItemData;
            if (other == null)
            {
                return false;
            }
            else
            {
                return ((name == other.name) && (itemFullPath == other.itemFullPath));
            }
        }

        public override int GetHashCode()
        {
            return itemFullPath.GetHashCode();
        }

        protected string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        protected string itemFullPath;
        public string FullPath
        {
            get { return itemFullPath; }
            set { itemFullPath = value; }
        }

        protected int count = 0;
        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        protected int position = 0;
        public int Position
        {
            get { return position; }
            set { position = value; }
        }

        public void Increment()
        {
            count++;
        }
    }

    [Serializable]
    public class ItemList
    {
        protected List<ItemData> items = new List<ItemData>();
        public List<ItemData> Items
        {
            get { return items; }
            set { items = value; }
        }

        public void AddItem(ItemData item)
        {
            items.Add(item);
        }
    }

    [Serializable]
    public class WorkingSet
    {
        protected ItemList recentItems = new ItemList();
        public ItemList RecentItems
        {
            get { return recentItems; }
            set { recentItems = value; }
        }

        protected int selectedTab = 0;
        public int SelectedTab
        {
            get { return selectedTab; }
            set { selectedTab = value; }
        }
    }
}
