﻿using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SillyMonkey.Core;
using System.Threading.Tasks;
using DataContainer;
using FileReader;
using System.Windows;

namespace UI_DataList.ViewModels {

    public interface ISubNode {
        string NodeName { get; }
        string FilePath { get; }
        bool IsSelected { get; set; }
        FileNode ParentNode { get; }
        IEnumerable<SiteNode> SiteList { get; }
        Visibility EnableContextMenu { get; }
    }

    public class SiteNode {
        public string NodeName { get; private set; }
        public string FilePath { get; private set; }

        public byte Site { get; private set; }

        public FileNode ParentNode { get; private set; }

        public SiteNode(FileNode file, byte site, int cnt) {
            ParentNode = file;
            Site = site;
            NodeName = $"Site {site}: {cnt}";
            FilePath = ParentNode.FilePath;
        }
    }

    public class SiteCollectionNode : ISubNode {
        public string NodeName { get; private set; }
        public string FilePath { get; private set; }
        public bool IsSelected { get; set; }
        public IEnumerable<SiteNode> SiteList { get; private set; }

        public FileNode ParentNode { get; private set; }
        public Visibility EnableContextMenu { get { return  Visibility.Hidden; } }

        public SiteCollectionNode(FileNode file) {
            ParentNode = file;
            if (!file.ExtractedDone) 
                return;
            FilePath = file.FilePath;
            var dataAcquire = StdDB.GetDataAcquire(FilePath);
            SiteList = (from r in dataAcquire.GetSitesChipCount()
                        let l = new SiteNode(file, r.Key, r.Value)
                        select l);
            NodeName = "Site List";
            IsSelected = false;
        }
    }

    public class FilterNode : ISubNode {
        public string NodeName { get; private set; }

        public string FilePath { get; private set; }
        public bool IsSelected { get; set; }

        public IEnumerable<SiteNode> SiteList { get; private set; }

        public FileNode ParentNode { get; private set; }

        public Visibility EnableContextMenu { get { return Visibility.Visible; } }

        public int FilterId { get; }

        public int FilterIdx { get; }

        public SubData SubData { get; }

        public FilterNode(FileNode file, int filterId, int filterIdx) {
            ParentNode = file;
            FilterId = filterId;
            FilterIdx = filterIdx;
            NodeName = $"Filter_{filterIdx}";
            FilePath = file.FilePath;
            SubData = new SubData(FilePath, filterId);
            IsSelected = true;
        }
    }

    public class FileNode : BindableBase {
        public string NodeName { get; private set; }
        public string FilePath { get; private set; }
        public int FileIdx { get; }
        public bool ExtractedDone { get; private set; }
        public Visibility EnableContextMenu { get { return ExtractedDone? Visibility.Visible :Visibility.Hidden; } }
        public ObservableCollection<ISubNode> SubDataList { get; private set; }

        public FileNode(string path, int idx) {
            SubDataList = new ObservableCollection<ISubNode>();
            FilePath = path;
            FileIdx = idx;
            NodeName = $"File_{idx}:{System.IO.Path.GetFileName(path)}";
            ExtractedDone = false;
        }

        public void Update() {
            var dataAcquire = StdDB.GetDataAcquire(FilePath);
            ExtractedDone = dataAcquire.LoadingDone;
            RaisePropertyChanged("ExtractedDone");
            RaisePropertyChanged("EnableContextMenu");

            SubDataList.Clear();
            SubDataList.Add(new SiteCollectionNode(this));
            foreach(var f in dataAcquire.GetAllFilterId()) {
                SubDataList.Add(new FilterNode(this, f, dataAcquire.GetFilterIndex(f)));
            }
        }
    }

    public class DataManagementViewModel : BindableBase {
        IEventAggregator _ea;
        IRegionManager _regionManager;

        int _fileIdx = 1;

        private ObservableCollection<FileNode> _files;
        public ObservableCollection<FileNode> Files {
            get { return _files ?? (_files = new ObservableCollection<FileNode>()); }
            private set { SetProperty(ref _files, value); } 
        }

        public DataManagementViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_OpenFile>().Subscribe(OpenStdFile);
            _ea.GetEvent<Event_CloseData>().Subscribe(CloseSubData);
        }

        private async void OpenStdFile(string path) {
            if (StdDB.IfExsistFile(path)) {
                Log("Already Exist:" + path);
                return;
            } 
            var f = new FileNode(path, _fileIdx++);
            Files.Add(f);
            try {
                var dataAcquire = StdDB.CreateSubContainer(path);
                dataAcquire.PropertyChanged += DataAcquire_PropertyChanged;
                using (var data = new StdReader(path, StdFileType.STD)) {
                    await Task.Run(() => { data.ExtractStdf();});
                    LoadingDone(f);
                }
            }catch (Exception e) {
                Files.Remove(f);
                StdDB.RemoveFile(path);
                Log("File Open Failed:" + e);
                return;
            }

        }

        private void DataAcquire_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(e.PropertyName== "CurrentLoadingProgress" || e.PropertyName== "CurrentLoadingPhase") {
                var dataAcquire = sender as IDataAcquire;
                string info="";
                switch (dataAcquire.CurrentLoadingPhase) {
                    case LoadingPhase.NotStart:
                        info = "";
                        break;
                    case LoadingPhase.Analysing:
                        info = $"Analysing {dataAcquire.FileName}";
                        break;
                    case LoadingPhase.Reading:
                        info = $"Loading {dataAcquire.FileName}";
                        break;
                    case LoadingPhase.Done:
                        info = "";
                        break;
                }
                _ea.GetEvent<Event_Progress>().Publish(new Tuple<string, int>(info, dataAcquire.CurrentLoadingProgress));
            }
        }

        private void LoadingDone(FileNode fn) {
            var dataAcquire = StdDB.GetDataAcquire(fn.FilePath);
            var id = dataAcquire.CreateFilter();
            fn.Update();
            RaisePropertyChanged("Files");
            RequestRawTab(new SubData(fn.FilePath, id), fn.FileIdx, dataAcquire.GetFilterIndex(id));
        }

        private void RequestRawTab(SubData data, int fileIdx, int filterIdx) {
            var parameters = new NavigationParameters();
            parameters.Add("subData", data);
            parameters.Add("fileIdx", fileIdx);
            parameters.Add("filterIdx", filterIdx);
            _regionManager.RequestNavigate("Region_DataView", "DataRaw", parameters);
        }

        private void RemoveRawTab(SubData data) {
            var v =_regionManager.Regions["Region_DataView"].Views;
            var view = v.First(x => ((x as System.Windows.Controls.UserControl).DataContext as IDataView).CurrentData.Equals(data));
            _regionManager.Regions["Region_DataView"].Remove(view);
        }

        private void CloseSubData(SubData data) {
            RemoveRawTab(data);

            var dataAcquire = StdDB.GetDataAcquire(data.StdFilePath);
            dataAcquire.RemoveFilter(data.FilterId);
            Files.First((f) => {
                if (f.FilePath == dataAcquire.FilePath) return true;
                return false;
            }).Update();
            RaisePropertyChanged("Files");

        }

        private DelegateCommand<object> _selectData;
        public DelegateCommand<object> SelectData =>
            _selectData ?? (_selectData = new DelegateCommand<object>(ExecuteSelectData));

        void ExecuteSelectData(object x) {
            if (x.GetType().Name == "FileNode") {
                if((x as FileNode).ExtractedDone)
                    _ea.GetEvent<Event_DataSelected>().Publish((x as FileNode).FilePath);
            } else if (x.GetType().Name == "FilterNode") {
                _ea.GetEvent<Event_SubDataSelected>().Publish((x as FilterNode).SubData);
                var f = (x as FilterNode);
                RequestRawTab(f.SubData, f.ParentNode.FileIdx, f.FilterIdx);
            } else {
                _ea.GetEvent<Event_DataSelected>().Publish(null);
            }
        }

        private DelegateCommand<object> _openData;
        public DelegateCommand<object> OpenData =>
            _openData ?? (_openData = new DelegateCommand<object>(ExecuteOpenData));

        void ExecuteOpenData(object x) {
            if (x.GetType().Name == "FilterNode") {
                var f = (x as FilterNode);
                RequestRawTab(f.SubData, f.ParentNode.FileIdx, f.FilterIdx);
            } else if (x.GetType().Name == "SiteNode") {
                var s = (x as SiteNode);
                var f = StdDB.GetDataAcquire(s.FilePath);
                var id = f.CreateFilter(s.Site);

                s.ParentNode.Update();
                RaisePropertyChanged("Files");

                RequestRawTab(new SubData(s.FilePath, id), s.ParentNode.FileIdx, f.GetFilterIndex(id));
            }else if(x.GetType().Name == "FileNode") {
                var f = x as FileNode;
                var da = StdDB.GetDataAcquire(f.FilePath);
                var id = da.CreateFilter();
                f.Update();
                RaisePropertyChanged("Files");
                RequestRawTab(new SubData(f.FilePath, id), f.FileIdx, da.GetFilterIndex(id));

            }
        }

        private DelegateCommand<object> _cmdCloseFile;
        public DelegateCommand<object> CmdCloseFile =>
            _cmdCloseFile ?? (_cmdCloseFile = new DelegateCommand<object>(ExecuteCmdCloseFile));

        void ExecuteCmdCloseFile(object parameter) {
            var f = parameter as FileNode;
            foreach(var v in f.SubDataList) {
                if (v is FilterNode) {
                    RemoveRawTab((v as FilterNode).SubData);
                }
            }
            StdDB.RemoveFile(f.FilePath);
            Files.Remove(f);
        }

        private DelegateCommand<object> _cmdCloseData;
        public DelegateCommand<object> CmdCloseData =>
            _cmdCloseData ?? (_cmdCloseData = new DelegateCommand<object>(ExecuteCmdCloseData));

        void ExecuteCmdCloseData(object parameter) {
            CloseSubData((parameter as FilterNode).SubData);
        }

        private void Log(string s) {
            _ea.GetEvent<Event_Log>().Publish(s);
        }
    }
}
