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
using UI_Data.Views;

namespace UI_DataList.ViewModels {

    public interface ISubNode {
        string NodeName { get; }
        string FilePath { get; }
        //bool IsSelected { get; set; }
        FileNode ParentNode { get; }
        //IEnumerable<SiteNode> SiteList { get; }
        Visibility EnableContextMenu { get; }
    }

    //public class SiteNode : ISubNode {
    //    public string NodeName { get; private set; }
    //    public string FilePath { get; private set; }
    //    public bool IsSelected { get; set; }
    //    public byte Site { get; private set; }

    //    public FileNode ParentNode { get; private set; }

    //    public IEnumerable<SiteNode> SiteList { get { return null; } }

    //    public Visibility EnableContextMenu { get { return Visibility.Hidden; } }

    //    public SiteNode(FileNode file, byte site, int cnt) {
    //        ParentNode = file;
    //        Site = site;
    //        NodeName = $"Site {site}: {cnt}";
    //        FilePath = ParentNode.FilePath;
    //    }
    //}

    //public class SiteCollectionNode : ISubNode {
    //    public string NodeName { get; private set; }
    //    public string FilePath { get; private set; }
    //    public bool IsSelected { get; set; }
    //    public IEnumerable<SiteNode> SiteList { get; private set; }

    //    public FileNode ParentNode { get; private set; }
    //    public Visibility EnableContextMenu { get { return  Visibility.Hidden; } }

    //    public SiteCollectionNode(FileNode file) {
    //        ParentNode = file;
    //        if (!file.ExtractedDone) 
    //            return;
    //        FilePath = file.FilePath;
    //        var dataAcquire = StdDB.GetDataAcquire(FilePath);
    //        SiteList = (from r in dataAcquire.GetSitesChipCount()
    //                    let l = new SiteNode(file, r.Key, r.Value)
    //                    select l);
    //        NodeName = "Site List";
    //        IsSelected = false;
    //    }
    //}

    public class FilterNode : BindableBase, ISubNode {
        public string NodeName { get; private set; }

        public string FilePath { get; private set; }
        //public bool IsSelected { get; set; }
        //private bool isSelected = true;
        //public bool IsSelected {
        //    get { return isSelected; }
        //    set { SetProperty(ref isSelected, value); }
        //}

        //public IEnumerable<SiteNode> SiteList { get; private set; }

        public FileNode ParentNode { get; private set; }

        public Visibility EnableContextMenu { get { return Visibility.Visible; } }

        public int FilterId { get; }

        public int FilterIdx { get; }

        public SubData SubData { get; }

        public FilterNode(FileNode file, int filterId, int filterIdx) {
            ParentNode = file;
            FilterId = filterId;
            FilterIdx = filterIdx;
            NodeName = $"Filter_{filterIdx}:{filterId:X8}";
            FilePath = file.FilePath;
            SubData = new SubData(FilePath, filterId);
            //IsSelected = true;
        }
    }

    public class FileNode : BindableBase, ISubNode {
        public string NodeName { get; private set; }
        public string FilePath { get; private set; }
        public int FileIdx { get; }
        public bool ExtractedDone { get; private set; }
        public Visibility EnableContextMenu { get { return ExtractedDone? Visibility.Visible :Visibility.Hidden; } }
        public ObservableCollection<ISubNode> SubDataList { get; private set; }
        
        public bool IsSelected { get; set; }

        public FileNode ParentNode { get { return null; } }

        //public IEnumerable<SiteNode> SiteList { get { return null; } }

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
            //SubDataList.Add(new SiteCollectionNode(this));
            foreach(var f in dataAcquire.GetAllFilterId()) {
                SubDataList.Add(new FilterNode(this, f, dataAcquire.GetFilterIndex(f)));
            }
        }
    }

    public class DataManagementViewModel : BindableBase {
        IEventAggregator _ea;
        IRegionManager _regionManager;

        int _fileIdx = 1;
        int _corrDataIdx = 1;

        private ObservableCollection<FileNode> _files;
        public ObservableCollection<FileNode> Files {
            get { return _files ?? (_files = new ObservableCollection<FileNode>()); }
            private set { SetProperty(ref _files, value); } 
        }

        private ISubNode selectedItem;
        public ISubNode SelectedItem {
            get { return selectedItem; }
            set { SetProperty(ref selectedItem, value); }
        }

        public DataManagementViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_OpenFile>().Subscribe(OpenStdFile);
            _ea.GetEvent<Event_CvtFile>().Subscribe(CvtStdFile);
            _ea.GetEvent<Event_CloseData>().Subscribe(CloseSubData);
            _ea.GetEvent<Event_MergeFiles>().Subscribe(MergeFiles);
            _ea.GetEvent<Event_MergeSubData>().Subscribe(MergeSubData);
            _ea.GetEvent<Event_CorrData>().Subscribe(RequestCorrTab);
            _ea.GetEvent<Event_CloseAllFiles>().Subscribe(CloseAllFiles);
            _ea.GetEvent<Event_SubDataTabSelected>().Subscribe(SubDataTabSelected);
            //_ea.GetEvent<Event_AddRtFile>().Subscribe(MergeRtFile);
        }

        private void SubDataTabSelected(SubData subData) {
            try {
                var view = _regionManager.Regions["Region_DataList"].Views.ElementAt(0) as Views.DataManagement;
                //change the selected treeview sub data
                foreach (var f in view.dataList.Items) {
                    foreach (var sn in ((FileNode)f).SubDataList) {
                        if (sn is FilterNode) {
                            if ((sn as FilterNode).SubData.Equals(subData)) {
                                var filenode = view.dataList.ItemContainerGenerator.ContainerFromItem(f) as System.Windows.Controls.TreeViewItem;
                                //(filenode.ItemContainerGenerator.ContainerFromItem(sn) as System.Windows.Controls.TreeViewItem).IsSelected = true;
                                var oo = filenode.ItemContainerGenerator.ContainerFromItem(sn);
                                if (oo is null) return;
                                var tvi = oo as System.Windows.Controls.TreeViewItem;
                                tvi.IsSelected = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e){
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

        }

        private void RequestCorrTab(IEnumerable<SubData> data) {
            var parameters = new NavigationParameters();
            parameters.Add("subDataList", data);
            parameters.Add("corrDataIdx", _corrDataIdx++);
            _regionManager.RequestNavigate("Region_DataView", "DataCorrelation", parameters);
        }

        private async void MergeFiles(IEnumerable<string> files) {
            string ss = "";
            foreach(var s in files) {
                ss += Files.First((x)=>x.FilePath.Equals(s)).FileIdx;
                ss += "_";
            }
            var filePath = "MergerFile_" + ss + DateTime.Now.ToString("HH-mm-ss");
            var f = new FileNode(filePath, _fileIdx++);
            Files.Add(f);

            try {
                var rst = await Task.Run<bool>(() => {
                    return StdDB.MergeFiles(filePath, files);
                });
                if (!rst) {
                    System.Windows.Forms.MessageBox.Show("Files on loading, please try later");
                }
                LoadingDone(f);

                Log($"Merge Done!");
            }
            catch (Exception e) {
                Files.Remove(f);
                StdDB.RemoveFile(filePath);
                Log("Merge Failed:" + e);
                return;
            }

        }

        private async void MergeSubData(IEnumerable<SubData> subDataList) {
            var filePath = "MergerSubData_" + DateTime.Now.ToString("HH-mm-ss");
            var f = new FileNode(filePath, _fileIdx++);
            Files.Add(f);

            try {
                var rst = await Task.Run<bool>(() => {
                    return StdDB.MergeSubData(filePath, subDataList);
                });
                if (!rst) {
                    System.Windows.Forms.MessageBox.Show("Files on loading, please try later");
                }
                LoadingDone(f);

                Log($"Merge Done!");
            } catch (Exception e) {
                Files.Remove(f);
                StdDB.RemoveFile(filePath);
                Log("Merge Failed:" + e);
                return;
            }
        }

        private async void OpenStdFile(string path) {
            try {
                var info = new System.IO.FileInfo(path);
                if (!info.Exists) {
                    System.Windows.Forms.MessageBox.Show("File Invalid:" + path);
                    return;
                }
            }
            catch {
                System.Windows.Forms.MessageBox.Show("File Invalid:" + path);
                return;
            }

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
                System.Windows.Forms.MessageBox.Show("File Open Failed:" + e.Message);
                return;
            }

        }

        //CvtStdFile
        private async void CvtStdFile(string path) {
            try {
                var info = new System.IO.FileInfo(path);
                if (!info.Exists) {
                    System.Windows.Forms.MessageBox.Show("File Invalid:" + path);
                    return;
                }
            } catch {
                System.Windows.Forms.MessageBox.Show("File Invalid:" + path);
                return;
            }

            if (StdDB.IfExsistFile(path)) {
                Log("Already Exist:" + path);
                return;
            }
            try {
                var dataAcquire = StdDB.CreateSubContainer(path);
                dataAcquire.PropertyChanged += DataAcquire_PropertyChanged;
                using (var data = new StdReader(path, StdFileType.STD)) {
                    await Task.Run(() => { data.ExtractStdf(); });
                    var id = dataAcquire.CreateFilter();

                    await Task.Run(() => { dataAcquire.ExportToExcel(path+".csv", id); });

                    StdDB.RemoveFile(path);
                }
            } catch (Exception e) {
                StdDB.RemoveFile(path);
                System.Windows.Forms.MessageBox.Show("File Open Failed:" + e.Message);
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
            RequestRawTab(new SubData(fn.FilePath, id), fn.FileIdx);
        }

        private void RequestRawTab(SubData data, int fileIdx) {
            var parameters = new NavigationParameters();
            parameters.Add("subData", data);
            parameters.Add("fileIdx", fileIdx);
            _regionManager.RequestNavigate("Region_DataView", "DataRaw", parameters);
        }

        private void RemoveRawTab(SubData data) {
            var views =_regionManager.Regions["Region_DataView"].Views;
            foreach(var v in views) {
                var x = v as IDataView;
                if(x.CurrentTabType == TabType.RawDataTab) {
                    if (x.CurrentData.Value.Equals(data)) {
                        _regionManager.Regions["Region_DataView"].Remove(v);
                    }
                } else {
                    foreach(var s in x.SubDataList) {
                        if (s.Equals(data)) {
                            _regionManager.Regions["Region_DataView"].Remove(v);
                            break;
                        }
                    }
                }
            }

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

        private void CloseFile(FileNode f) {
            foreach (var v in f.SubDataList) {
                if (v is FilterNode) {
                    RemoveRawTab((v as FilterNode).SubData);
                }
            }
            StdDB.RemoveFile(f.FilePath);
            Files.Remove(f);

        }

        private void CloseAllFiles() {
            for (int i = _files.Count - 1; i >= 0; i--) {
                CloseFile(_files[i]);
            }
        }

        private DelegateCommand<object> _selectData;
        public DelegateCommand<object> SelectData =>
            _selectData ?? (_selectData = new DelegateCommand<object>(ExecuteSelectData));

        void ExecuteSelectData(object x) {
            if (x is null) {
                // all files closed, publish event
                _ea.GetEvent<Event_DataSelected>().Publish(string.Empty);
                return;
            }
            if (x.GetType().Name == "FileNode") {
                if((x as FileNode).ExtractedDone)
                    _ea.GetEvent<Event_DataSelected>().Publish((x as FileNode).FilePath);
            } else if (x.GetType().Name == "FilterNode") {
                _ea.GetEvent<Event_SubDataSelected>().Publish((x as FilterNode).SubData);
                var f = (x as FilterNode);
                RequestRawTab(f.SubData, f.ParentNode.FileIdx);
            } else {
                _ea.GetEvent<Event_DataSelected>().Publish(string.Empty);
            }
        }

        private DelegateCommand<object> _openData;
        public DelegateCommand<object> OpenData =>
            _openData ?? (_openData = new DelegateCommand<object>(ExecuteOpenData));

        void ExecuteOpenData(object x) {
            //if (x.GetType().Name == "FilterNode") {
            //    var f = (x as FilterNode);
            //    RequestRawTab(f.SubData, f.ParentNode.FileIdx, f.FilterIdx);
            //} else if (x.GetType().Name == "SiteNode") {
            //    var s = (x as SiteNode);
            //    var f = StdDB.GetDataAcquire(s.FilePath);
            //    var id = f.CreateFilter(s.Site);

            //    s.ParentNode.Update();
            //    RaisePropertyChanged("Files");

            //    RequestRawTab(new SubData(s.FilePath, id), s.ParentNode.FileIdx);
            //}else 
            if(x.GetType().Name == "FileNode") {
                var f = x as FileNode;
                var da = StdDB.GetDataAcquire(f.FilePath);
                var id = da.CreateFilter();
                f.Update();
                RaisePropertyChanged("Files");
                RequestRawTab(new SubData(f.FilePath, id), f.FileIdx);

            }
        }

        private DelegateCommand<object> _cmdCreateSitesFilter;
        public DelegateCommand<object> CmdCreateSitesFilter =>
            _cmdCreateSitesFilter ?? (_cmdCreateSitesFilter = new DelegateCommand<object>(ExecuteCmdCreateSitesFilter));

        void ExecuteCmdCreateSitesFilter(object parameter) {
            var file = parameter as FileNode;
            if (!file.ExtractedDone) return;
            var da = StdDB.GetDataAcquire(file.FilePath);
            foreach(var s in da.GetSites()) {
                var id = da.CreateFilter(s);

                file.Update();
                RaisePropertyChanged("Files");

                RequestRawTab(new SubData(file.FilePath, id), file.FileIdx);

            }

        }

        private DelegateCommand<object> _cmdCloseFile;
        public DelegateCommand<object> CmdCloseFile =>
            _cmdCloseFile ?? (_cmdCloseFile = new DelegateCommand<object>(ExecuteCmdCloseFile));

        void ExecuteCmdCloseFile(object parameter) {
            CloseFile(parameter as FileNode);
        }

        private DelegateCommand<object> _cmdCloseData;
        public DelegateCommand<object> CmdCloseData =>
            _cmdCloseData ?? (_cmdCloseData = new DelegateCommand<object>(ExecuteCmdCloseData));

        void ExecuteCmdCloseData(object parameter) {
            CloseSubData((parameter as FilterNode).SubData);
        }

        private DelegateCommand<object> _cmdSiteDataCorr;
        public DelegateCommand<object> CmdSiteDataCorr =>
            _cmdSiteDataCorr ?? (_cmdSiteDataCorr = new DelegateCommand<object>(ExecuteCmdSiteDataCorr));

        void ExecuteCmdSiteDataCorr(object parameter) {
            var f = (parameter as FilterNode);

            var parameters = new NavigationParameters();
            parameters.Add("subData", f.SubData);
            //parameters.Add("fileIdx", f.ParentNode.FileIdx);
            //parameters.Add("filterIdx", f.FilterIdx);
            _regionManager.RequestNavigate("Region_DataView", "SiteDataCorrelation", parameters);

        }

        private DelegateCommand<object> _cmdCreateNewFilter;
        public DelegateCommand<object> CmdCreateNewFilter =>
            _cmdCreateNewFilter ?? (_cmdCreateNewFilter = new DelegateCommand<object>(ExecuteCmdCreateNewFilter));

        void ExecuteCmdCreateNewFilter(object parameter) {
            var file = parameter as FileNode;
            if (!file.ExtractedDone) return;
            var da = StdDB.GetDataAcquire(file.FilePath);
            var id = da.CreateFilter();
            file.Update();
            RaisePropertyChanged("Files");
            RequestRawTab(new SubData(file.FilePath, id), file.FileIdx);
        }

        private void Log(string s) {
            _ea.GetEvent<Event_Log>().Publish(s);
        }
    }
}
