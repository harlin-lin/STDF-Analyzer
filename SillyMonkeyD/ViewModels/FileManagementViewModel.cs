using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DataInterface;
using DataParse;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using FileHelper;
using Microsoft.Win32;
using SillyMonkeyD.Views;

namespace SillyMonkeyD.ViewModels {

    public class FileManagementViewModel : ViewModelBase {

        public FileManagementViewModel() {
            Files = new ObservableCollection<IDataAcquire>();
            Sites = null;
            SelectedFile = null;
            //SelectedSite = new KeyValuePair<byte, int>();

            _tabList = new List<DXTabItem>();

            InitUiCtr();
        }

        private FilterEditor _filterWindow;
        private Data _dataWindow;
        private List<DXTabItem> _tabList;


        public ObservableCollection<IDataAcquire> Files { get; private set; }
        public Dictionary<byte, int> Sites { get { return GetProperty(() => Sites); } private set { SetProperty(() => Sites, value); } }
        public IDataAcquire SelectedFile { get { return GetProperty(() => SelectedFile); } set { SetProperty(() => SelectedFile, value); } }
        public KeyValuePair<byte, int> SelectedSite { get { return GetProperty(() => SelectedSite); } set { SetProperty(() => SelectedSite, value); } }

        public string FileInfo { get { return GetProperty(() => FileInfo); } private set { SetProperty(() => FileInfo, value); } }


        public async void AddFile(string path) {
            IDataAcquire data = new StdfParse(path);
            Files.Add(data);
            //extract the files
            await System.Threading.Tasks.Task.Run(new Action(() => data.ExtractStdf()));
        }
        private void RemoveFile(IDataAcquire data) {
            Files.Remove(data);
            GC.Collect();
        }

        private DXTabItem AddTab(IDataAcquire dataAcquire, int filterId) {
            RawGridTab rawGridTab = new RawGridTab(dataAcquire, filterId);
            _tabList.Add(rawGridTab);
            return rawGridTab;
        }


        #region UI
        public ICommand<DragEventArgs> DropCommand { get; private set; }
        public ICommand<IDataAcquire> FileCloseCommand { get; private set; }
        public ICommand OpenFileRawData { get; private set; }
        public ICommand OpenSiteRawData { get; private set; }
        public ICommand UpdateInfo { get; private set; }
        public ICommand OpenFileDiag { get; private set; }

        private void InitUiCtr() {
            DropCommand = new DelegateCommand<DragEventArgs>((e) => {
                var paths = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
                foreach (string path in paths) {
                    var ext = System.IO.Path.GetExtension(path).ToLower();
                    if (ext == ".stdf" || ext == ".std") {
                        AddFile(path);
                    } else {
                        //log message not supported
                    }
                }
            });

            OpenFileDiag = new DelegateCommand(()=> {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "选择数据源文件";
                openFileDialog.Filter = "All|*.*|STDF|*.stdf|STD|*.std";
                openFileDialog.FileName = string.Empty;
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = true;
                openFileDialog.RestoreDirectory = false;
                openFileDialog.DefaultExt = "stdf";
                if (openFileDialog.ShowDialog() == false) {
                    return;
                }
                var paths = openFileDialog.FileNames;
                foreach (string path in paths) {
                    var ext = System.IO.Path.GetExtension(path).ToLower();
                    if (ext == ".stdf" || ext == ".std") {
                        AddFile(path);
                    } else {
                        //log message not supported
                    }
                }

            });

            FileCloseCommand = new DelegateCommand<IDataAcquire>((e) => {
                RemoveFile(e);
            });

            OpenFileRawData = new DelegateCommand(() => {
                ShowDataWindow(AddTab(SelectedFile, SelectedFile.GetFilterID(null)));
            });
            OpenSiteRawData = new DelegateCommand(() => {
                ShowDataWindow(AddTab(SelectedFile, SelectedFile.GetFilterID(SelectedSite.Key)));
            });

            UpdateInfo = new DelegateCommand(() => {
                if (SelectedFile is null)
                    Sites = null;
                else if (SelectedFile.ParseDone) {
                    Sites = SelectedFile.GetSitesChipCount();
                    FileInfo = StdFileHelper.GetBriefSummary(SelectedFile);
                }
                else
                    Sites = null;
            });


        }
        
        private void ShowFilterWindow(IDataAcquire dataAcquire, int filterId) {
            if(_filterWindow is null) {
                _filterWindow = new FilterEditor(dataAcquire, filterId);
            } else {
                _filterWindow.filter.UpdateFilter(dataAcquire, filterId);
            }
            _filterWindow.Show();
        }

        private void ShowDataWindow(DXTabItem tabItem) {
            if(_dataWindow is null) {
                _dataWindow = new Data();
            }
            ((DataViewModel)_dataWindow.DataContext).AddTab(tabItem);
            _dataWindow.Show();
        }

        #endregion

        //private StdFileHelper _fileHelper;

        //public ObservableCollection<FileInfo> FileInfos { get; private set; }
        //public ObservableCollection<OpenedFile> OpenedItems { get; private set; }

        //public string SelectedSummary { get { return GetProperty(() => SelectedSummary); } private set { SetProperty(() => SelectedSummary, value); } }
        //public object SelectedOpenedItem { get { return GetProperty(() => SelectedOpenedItem); } set { SetProperty(() => SelectedOpenedItem, value); } }

        ////public ICommand<DevExpress.Xpf.Grid.SelectedItemChangedEventArgs> SelectedItemChangedCommand { get; private set; }
        ////public ICommand<DevExpress.Xpf.Grid.SelectedItemChangedEventArgs> ChangeDisplayCommand { get; private set; }
        //public ICommand<object> DoubleClickCommand { get; private set; }
        //public ICommand<object> CloseCommand { get; private set; }
        ////public ICommand<RoutedEventArgs> FileCloseCommand { get; private set; }
        //public ICommand CopySummary { get; private set; }

        //public event OpenDetailHandler OpenDetailEvent;
        //public event RemoveHandler RemoveTabEvent;
        //public event ChangeViewTabHandler ChangeViewTabEvent;

        //public FileManagementViewModel() {
        //    _fileHelper = new StdFileHelper();
        //    FileInfos = new ObservableCollection<FileInfo>();
        //    OpenedItems = new ObservableCollection<OpenedFile>();
        //    _fileHelper.UpdateFileInfo += UpdateFileInfo;
        //    _fileHelper.AddFileEvent += AddFileEvent;

        //    SelectedSummary = "";
        //    SelectedOpenedItem = string.Empty;

        //    //SelectedItemChangedCommand = new DelegateCommand<DevExpress.Xpf.Grid.SelectedItemChangedEventArgs>((e) => {
        //    //    if (!(e.NewItem is LoadedInfoBasic)) return;
        //    //    LoadedInfoBasic s = e.NewItem as LoadedInfoBasic;
        //    //    if (!s.FileStatus) return;
        //    //    SelectedSummary = _fileHelper.GetBriefSummary(s.FilePath.GetHashCode(), s.Site);
        //    //});

        //    //ChangeDisplayCommand = new DelegateCommand<DevExpress.Xpf.Grid.SelectedItemChangedEventArgs>((e) => {
        //    //    if (!(e.NewItem is OpenedInfoBasic)) return;
        //    //    var s = e.NewItem as OpenedInfoBasic;
        //    //    ChangeViewTabEvent?.Invoke(s.Tag);
        //    //});

        //    DoubleClickCommand = new DelegateCommand<object>((e) => {
        //        if (!(e is LoadedInfoBasic)) return;
        //        var s = e as LoadedInfoBasic;
        //        if (!s.FileStatus) return;
        //        int hash = s.FilePath.GetHashCode();
        //        int tag = hash ^ System.DateTime.UtcNow.Ticks.GetHashCode();
        //        AddOpenedItem(hash, $"[RAW] SITE:{s.Site.ToString()} {s.FileName}", tag);
        //        var id = _fileHelper.CreateFilterDataHandler(hash, s.Site);
        //        OpenDetailEvent?.Invoke(_fileHelper.GetFile(hash), id, tag);
        //    });

        //    CloseCommand = new DelegateCommand<object>((e) => {
        //        var s = e as OpenedInfoBasic;
        //        bool b = s.GetType().Name == "OpenedFile";
        //        RemoveOpenedItem(s.Tag, b);
        //    });
        //    FileCloseCommand = new DelegateCommand<RoutedEventArgs>((e) => {
        //        var path = (((FileInfo)((Button)e.Source).DataContext)).FilePath;

        //        foreach (var v in OpenedItems) {
        //            if (v.FilePath == path) {
        //                foreach (var t in v.Items) {
        //                    RemoveTabEvent?.Invoke(t.Tag);
        //                }
        //                OpenedItems.Remove(v);
        //                break;
        //            }
        //        }

        //        RemoveFile(path);

        //        for (int i = 0; i < FileInfos.Count; i++)
        //            if (FileInfos[i].FilePath == path)
        //                FileInfos.RemoveAt(i);

        //        GC.Collect();
        //    });

        //    CopySummary = new DelegateCommand(() => {
        //        Clipboard.SetDataObject(SelectedSummary);
        //    });


        //}






        //private void AddFileEvent(IDataAcquire fileInfo) {
        //    FileInfos.Add(new FileInfo(fileInfo));
        //}

        //private void UpdateFileInfo(IDataAcquire data) {
        //    for (int i = 0; i < FileInfos.Count; i++)
        //        if (FileInfos[i].FilePath == data.FilePath)
        //            FileInfos[i].UpdateFileInfo(data);
        //}

        //private void AddOpenedItem(int fileHash, string itemName, int tag) {
        //    bool found = false;
        //    for (int i = 0; i < OpenedItems.Count; i++) {
        //        if (OpenedItems[i].FileHash == fileHash) {
        //            OpenedItems[i].AddSubItem(itemName, tag);
        //            found = true;
        //        }
        //    }
        //    if (!found) {
        //        OpenedFile o = new OpenedFile(_fileHelper.GetFile(fileHash), tag);
        //        o.AddSubItem(itemName, tag);
        //        OpenedItems.Add(o);
        //    }
        //}

        //private void RemoveOpenedItem(int tag, bool ifParent) {
        //    bool search = false;

        //    if (ifParent) {
        //        foreach (var v in OpenedItems) {
        //            if (v.Tag == tag) {
        //                foreach (var t in v.Items) {
        //                    RemoveTabEvent?.Invoke(t.Tag);
        //                    search = true;
        //                }
        //                OpenedItems.Remove(v);
        //                break;
        //            }
        //        }
        //    } else {
        //        foreach (var v in OpenedItems) {
        //            foreach (var t in v.Items) {
        //                if (t.Tag == tag) {
        //                    RemoveTabEvent?.Invoke(t.Tag);
        //                    v.Items.Remove(t);
        //                    search = true;
        //                    break;
        //                }
        //            }
        //            if (v.Items.Count == 0) {
        //                OpenedItems.Remove(v);
        //                break;
        //            }
        //            if (search) break;
        //        }
        //    }
        //}


        //public void ChangeTab(int tag) {
        //    for (int i = 0; i < OpenedItems.Count; i++) {
        //        for (int j = 0; j < OpenedItems[i].Items.Count; j++) {
        //            if (OpenedItems[i].Items[j].Tag == tag)
        //                SelectedOpenedItem = OpenedItems[i].Items[j];
        //        }
        //    }

        //}

    }
}