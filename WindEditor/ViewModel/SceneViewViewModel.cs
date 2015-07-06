﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor;
using WEditor.FileSystem;
using WEditor.WindWaker;
using WEditor.WindWaker.MapEntities;

namespace WindEditor.UI
{
    public class SceneViewViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public BindingList<Scene> ArchiveList
        {
            get { return m_archiveList; }
            set
            {
                m_archiveList = value;
                OnPropertyChanged("ArchiveList");
            }
        }

        private BindingList<Scene> m_archiveList;
        private MainWindowViewModel m_mainView;

        public SceneViewViewModel(MainWindowViewModel mainView)
        {
            m_mainView = mainView;
            ArchiveList = new BindingList<Scene>();
        }

        public void SetMap(Map map)
        {
            ArchiveList.Clear();
            if (map == null)
                return;

            for (int i = 0; i < map.NewRooms.Count; i++)
                ArchiveList.Add(map.NewRooms[i]);
            ArchiveList.Add(map.NewStage);
        }

        internal void OnSceneViewSelectObject(object newObject)
        {
            m_mainView.SetSelectedSceneFile((Scene)newObject);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
