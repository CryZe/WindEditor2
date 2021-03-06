﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WEditor;
using WEditor.WindWaker.Entities;

namespace WindEditor.UI.ViewModel
{
    public sealed class TabItem
    {
        public string Header { get; set; }
        public List<ObjectUIListEntry> Content { get; set; }

        public TabItem()
        {
            Content = new List<ObjectUIListEntry>();
        }
    }
    [System.Serializable]
    public sealed class ObjectUIListEntry : MapObjectSpawnDescriptor
    {
        public ImageSource DisplayImage
        {
            get { return m_imageSource; }
            set { m_imageSource = value; }
        }

        [NonSerialized]
        private ImageSource m_imageSource;
    }

    public class ObjectPlaceToolViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TabItem> Tabs { get; private set; }
        public CollectionViewSource FullList { get; private set; }
        public bool IsSearching
        {
            get { return SearchFilterText.Length > 0; }
        }
        public bool CanPlaceObject
        {
            get { return !IsSearching; }
        }

        public string SearchFilterText
        {
            get { return m_searchFilterText; }
            set
            {
                m_searchFilterText = value;

                if (!string.IsNullOrEmpty(m_searchFilterText))
                    AddFilter();

                FullList.View.Refresh();

                OnPropertyChanged("SearchFilterText");
                OnPropertyChanged("IsSearching");
                OnPropertyChanged("CanPlaceObject");
            }
        }

        private readonly MainWindowViewModel m_mainWindow;
        private string m_searchFilterText;

        public ObjectPlaceToolViewModel(MainWindowViewModel mainWindow)
        {
            m_mainWindow = mainWindow;
            Tabs = new ObservableCollection<TabItem>();
            FullList = new CollectionViewSource();

            LoadTemplatesFromDisk();

            // Set the search filter text to an empty string so it triggers the IsSearching/CanPlaceObject OnPropertyChanged
            // events so that the view sets the visibility of both controls correctly.
            SearchFilterText = string.Empty;
        }

        private void LoadTemplatesFromDisk()
        {
            string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = executionPath + "/WindWaker/Templates/ActorCategoryList.json";

            string fileContents = File.ReadAllText(filePath);

            try
            {
                var categoryData = JsonConvert.DeserializeObject<List<ObjectUIListEntry>>(fileContents);


                // Sort them by category
                var objByCategory = new Dictionary<string, List<ObjectUIListEntry>>();
                for (int i = 0; i < categoryData.Count; i++)
                {
                    if (!objByCategory.ContainsKey(categoryData[i].Category))
                        objByCategory[categoryData[i].Category] = new List<ObjectUIListEntry>();

                    objByCategory[categoryData[i].Category].Add(categoryData[i]);
                }

                var fullEntryList = new List<ObjectUIListEntry>();
                var tabList = new List<TabItem>();

                // Create tabs for each unique category
                foreach (var kvp in objByCategory)
                {
                    TabItem tab = new TabItem();
                    tab.Header = kvp.Key;

                    foreach (var entry in kvp.Value)
                    {
                        // Create a new BitmapImage to represent the icon - caching the icon on load so that it doesn't
                        // have atomic focus on the file and lock others from using the same icon.
                        using (FileStream fs = new FileStream(entry.IconPath, FileMode.Open))
                        {
                            BitmapImage entryIcon = new BitmapImage();
                            entryIcon.BeginInit();
                            entryIcon.CacheOption = BitmapCacheOption.OnLoad;
                            entryIcon.StreamSource = fs;
                            entryIcon.EndInit();

                            entry.DisplayImage = entryIcon;
                            tab.Content.Add(entry);
                            fullEntryList.Add(entry);
                        }
                    }

                    tabList.Add(tab);
                }

                // Use the flat-list of our entries and assign it as the source of the CollectionViewSource so we can filter it.
                FullList.Source = fullEntryList;

                // Sort the Tabs by header, A-Z (but put "Uncategorized" last)
                tabList.Sort(delegate(TabItem a, TabItem b) { return a.Header.CompareTo(b.Header); });
                TabItem uncatTab = tabList.Find(x => x.Header == "Uncategorized");
                if (uncatTab != null)
                {
                    tabList.Remove(uncatTab);
                    tabList.Add(uncatTab);
                }

                for (int i = 0; i < tabList.Count; i++)
                {
                    Tabs.Add(tabList[i]);
                }
            }
            catch (Exception ex)
            {
                WLog.Error(LogCategory.EditorCore, null, "Caught Exception while loading Actor Object List: {0}", ex);
            }
        }

        private void AddFilter()
        {
            FullList.Filter -= new FilterEventHandler(Filter);
            FullList.Filter += new FilterEventHandler(Filter);
        }

        private void Filter(object sender, FilterEventArgs e)
        {
            var src = e.Item as ObjectUIListEntry;
            e.Accepted = false;
            if (src == null)
                return;

            string searchTerm = SearchFilterText.ToLowerInvariant();

            // See if the keywords array for the object contains the search term
            for (int i = 0; i < src.Keywords.Length; i++)
            {
                if (src.Keywords[i].ToLowerInvariant().Contains(searchTerm))
                {
                    e.Accepted = true;
                    return;
                }
            }

            // See if the technical name contains the search term.
            if (src.TechnicalName.ToLowerInvariant().Contains(searchTerm))
            {
                e.Accepted = true;
                return;
            }

            // And finally, see if the display name contains the search term.
            if (src.DisplayName.ToLowerInvariant().Contains(searchTerm))
            {
                e.Accepted = true;
                return;
            }

            // See if the category it belongs to contains the search term.
            if (src.Category.ToLowerInvariant().Contains(searchTerm))
            {
                e.Accepted = true;
                return;
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
