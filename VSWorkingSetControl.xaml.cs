using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Company.VSWorkingSetPkg
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class MyControl : UserControl
    {
        public delegate void OpenItemDelegate(string item, int position);
        public event OpenItemDelegate OpenItem;

        public MyControl()
        {
            InitializeComponent();
            listBoxRecentItems.MouseDoubleClick += new MouseButtonEventHandler(listBoxRecentItems_MouseDoubleClick);
            listBoxFrequentItems.MouseDoubleClick += new MouseButtonEventHandler(listBoxFrequentItems_MouseDoubleClick);
            this.SizeChanged += new SizeChangedEventHandler(MyControl_SizeChanged);
            tabControl.SizeChanged += new SizeChangedEventHandler(tabControl_SizeChanged);
        }

        void tabControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MyControl_SizeChanged(sender, e);
        }

        private void MyControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            const int sizeMargin = 20;
            const int heightFactor = 3;
            const int widthFactor = 2;
            tabControl.Height = this.ActualHeight - sizeMargin;
            tabControl.Width = this.ActualWidth - sizeMargin;

            listBoxRecentItems.Height = tabControl.ActualHeight - sizeMargin * heightFactor;
            listBoxRecentItems.Width = tabControl.ActualWidth - sizeMargin * widthFactor;

            listBoxFrequentItems.Height = tabControl.ActualHeight - sizeMargin * heightFactor;
            listBoxFrequentItems.Width = tabControl.ActualWidth - sizeMargin * widthFactor;

            listBoxBookmarks.Height = tabControl.ActualHeight - sizeMargin * heightFactor;
            listBoxBookmarks.Width = tabControl.ActualWidth - sizeMargin * widthFactor;

            listBoxBreakpoints.Height = tabControl.ActualHeight - sizeMargin * heightFactor;
            listBoxBreakpoints.Width = tabControl.ActualWidth - sizeMargin * widthFactor;
        }

        private void HandleDoubleClick(ListBox listBox)
        {
            if (OpenItem != null && (listBox.SelectedItem != null))
            {
                ItemData data = listBox.SelectedItem as ItemData;
                OpenItem(data.FullPath, data.Position);
            }
        }

        private void listBoxFrequentItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HandleDoubleClick(listBoxFrequentItems);
        }

        private void listBoxRecentItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HandleDoubleClick(listBoxRecentItems);
        }

        private void RemoveItem(ItemData data)
        {
            int index = listBoxFrequentItems.Items.IndexOf(data);
            if (index != -1)
            {
                listBoxFrequentItems.Items.RemoveAt(index);
            }
            index = listBoxRecentItems.Items.IndexOf(data);
            if (index != -1)
            {
                listBoxRecentItems.Items.RemoveAt(index);
            }
        }

        private void DeleteSelectedItem(ListBox listbox)
        {
            if (listbox.SelectedItem != null)
            {
                RemoveItem(listbox.SelectedItem as ItemData);
            }
        }

        private void listBoxRecentItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteSelectedItem(listBoxRecentItems);
            }
        }

        private void listBoxFrequentItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteSelectedItem(listBoxFrequentItems);
            }
        }

        public void AddItemToRecent(ref ItemData data)
        {
            int index = listBoxRecentItems.Items.IndexOf(data);
            if (index == -1)
            {
                listBoxRecentItems.Items.Insert(0, data);
            }
            else
            {
                ItemData foundItem = listBoxRecentItems.Items[index] as ItemData;
                listBoxRecentItems.Items.RemoveAt(index);
                listBoxRecentItems.Items.Insert(0, foundItem);
            }
        }

        public void AddItemToFrequent(ref ItemData data)
        {
            int index = listBoxFrequentItems.Items.IndexOf(data);
            if (index == -1)
            {
                listBoxFrequentItems.Items.Add(data);
            }
            else
            {
                ItemData foundItem = listBoxFrequentItems.Items[index] as ItemData;
                foundItem.Increment();
                while (--index >= 0)
                {
                    ItemData curItem = listBoxFrequentItems.Items[index] as ItemData;
                    if (foundItem.Count > curItem.Count)
                    {
                        listBoxFrequentItems.Items.RemoveAt(index + 1);
                        listBoxFrequentItems.Items.Insert(index, foundItem);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public void AddItem(string item)
        {
            ItemData data = new ItemData(System.IO.Path.GetFileName(item), item);

            AddItemToRecent(ref data);
            AddItemToFrequent(ref data);
        }

        public WorkingSet GetWorkingSet()
        {
            WorkingSet workingSetItems = new WorkingSet();

            foreach (ItemData item in listBoxRecentItems.Items)
            {
                workingSetItems.RecentItems.AddItem(item);
            }

            workingSetItems.SelectedTab = tabControl.SelectedIndex;

            return workingSetItems;
        }

        public void SetWorkingSet(WorkingSet workingSet)
        {
            listBoxFrequentItems.Items.Clear();
            listBoxRecentItems.Items.Clear();

            if (workingSet == null)
            {
                return;
            }

            foreach (ItemData item in workingSet.RecentItems.Items)
            {
                listBoxRecentItems.Items.Add(item);
                listBoxFrequentItems.Items.Add(item);
            }

            var list = listBoxFrequentItems.Items.Cast<ItemData>().OrderByDescending(item => item.Count).ToList();
            listBoxFrequentItems.Items.Clear();
            foreach (ItemData itemData in list)
            {
                listBoxFrequentItems.Items.Add(itemData);
            }

            tabControl.SelectedIndex = workingSet.SelectedTab;
        }

        public void UpdateItemPosition(string item, int position)
        {
            ItemData data = new ItemData(System.IO.Path.GetFileName(item), item);

            int index = listBoxRecentItems.Items.IndexOf(data);
            if (index != -1)
            {
                ItemData existing = listBoxRecentItems.Items[index] as ItemData;
                existing.Position = position;
            }
        }

        public void RemoveItem(string item)
        {
            ItemData data = new ItemData(System.IO.Path.GetFileName(item), item);
            RemoveItem(data);           
        }
    }
}