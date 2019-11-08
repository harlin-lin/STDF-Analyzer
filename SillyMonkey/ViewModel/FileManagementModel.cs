using C1.WPF;
using DataInterface;
using FileHelper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SillyMonkey.ViewModel {
    public delegate void OpenDetailHandler(int fileHash, byte? site, int tag);
    public delegate void RemoveHandler(int tag);
    public delegate void RemoveFileHandler(string filePath);

    public class FileInfo : ViewModelBase {
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public bool FileStatus { get; private set; }
        public int FileDeviceCount { get; private set; }
        public Dictionary<byte, KeyValuePair<int, string>> Sites { get; private set; }


        public FileInfo(IDataAcquire stdfParse) {
            FileName = stdfParse.FileName;
            FilePath = stdfParse.FilePath;
            FileStatus = stdfParse.ParseDone;
            FileDeviceCount = stdfParse.ChipsCount;
            Sites = new Dictionary<byte, KeyValuePair<int, string>>();
        }

        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public void UpdateFileInfo(IDataAcquire stdfParse) {
            FileName = stdfParse.FileName;
            FilePath = stdfParse.FilePath;
            FileStatus = stdfParse.ParseDone;
            FileDeviceCount = stdfParse.ChipsCount;
            _dispatcher.Invoke(new Action(() => {
                Sites = (from f in stdfParse.GetSitesChipCount()
                         let x = new KeyValuePair<byte, KeyValuePair<int, string>>(f.Key, new KeyValuePair<int, string>(f.Value, FilePath))
                         select x).ToDictionary(x => x.Key, x => x.Value);
            }));

            RaisePropertyChanged("FileStatus");
            RaisePropertyChanged("FileDeviceCount");
            RaisePropertyChanged("Sites");
        }
    }
    public class OpenedItemsInfo : ViewModelBase {
        public string ItemName { get; private set; }
        public string ItemPath { get; private set; }
        public int FileHash { get; private set; }
        public int Tag { get; private set; }

        public ObservableCollection<KeyValuePair<int, string>> Items { get; private set; }

        public OpenedItemsInfo(IDataAcquire stdfParse, int tag) {
            FileHash = stdfParse.FilePath.GetHashCode();
            ItemName = stdfParse.FileName;
            ItemPath = stdfParse.FilePath;
            Items = new ObservableCollection<KeyValuePair<int, string>>();
            Tag = tag;
        }

        public void AddSubItem(string itemName, int tag) {

            Items.Add(new KeyValuePair<int, string>(tag, itemName));
            //RaisePropertyChanged("Items");
        }
    }
    public class FileManagementModel : ViewModelBase {

        private StdFileHelper _fileHelper;

        public ObservableCollection<FileInfo> FileInfos { get; private set; }
        public ObservableCollection<OpenedItemsInfo> OpenedItems { get; private set; }

        public string SelectedSummary { get; private set; }

        public RelayCommand<SelectionChangedEventArgs> SelectedItemChangedCommand { get; private set; }
        public RelayCommand<System.Windows.Input.MouseEventArgs> DoubleClickCommand { get; private set; }
        public RelayCommand<RoutedEventArgs> CloseCommand { get; private set; }
        public RelayCommand<RoutedEventArgs> FileCloseCommand { get; private set; }

        public event OpenDetailHandler OpenDetailEvent;
        public event RemoveHandler RemoveTabEvent;
        public event RemoveFileHandler RemoveFileEvent;

        public FileManagementModel(StdFileHelper stdFileHelper) {
            SelectedItemChangedCommand = new RelayCommand<SelectionChangedEventArgs>((e) => {
                var v = (C1TreeViewItem)(e.AddedItems[0]);
                if(v.DataContext.GetType().Name == "FileInfo") {
                    var s = v.DataContext as FileInfo;
                    if (!s.FileStatus) return;
                    SelectedSummary = _fileHelper.GetBriefSummary(s.FilePath.GetHashCode(), null);
                } else {
                    var s = (KeyValuePair<byte, KeyValuePair<int, string>>)v.DataContext;
                    SelectedSummary = _fileHelper.GetBriefSummary(s.Value.Value.GetHashCode(), s.Key);
                }
                RaisePropertyChanged("SelectedSummary");
            });

            DoubleClickCommand = new RelayCommand<System.Windows.Input.MouseEventArgs>((e) => {
                var v=((C1TreeView)e.Source).GetNode(e.GetPosition(null));
                if (v == null) return;
                if (v.DataContext.GetType().Name == "FileInfo") {
                    var s = v.DataContext as FileInfo;
                    if (!s.FileStatus) return;
                    int hash = s.FilePath.GetHashCode();
                    int tag = hash ^ System.DateTime.UtcNow.Ticks.GetHashCode();
                    AddOpenedItem(hash, s.FileName, tag);
                    OpenDetailEvent?.Invoke(hash, null, tag);
                } else {
                    var s = (KeyValuePair<byte, KeyValuePair<int, string>>)v.DataContext;

                    int hash = s.Value.Value.GetHashCode();
                    int tag = hash ^ System.DateTime.UtcNow.Ticks.GetHashCode();

                    AddOpenedItem(hash, $"{s.Key}: Detail", tag);
                    OpenDetailEvent?.Invoke(s.Value.Value.GetHashCode(), s.Key, tag);
                }
            });

            CloseCommand = new RelayCommand<RoutedEventArgs>((e) => {
                bool b = ((Button)e.Source).DataContext.GetType().Name == "OpenedItemsInfo";
                RemoveOpenedItem((int)((Button)e.Source).Tag, b);
            });
            FileCloseCommand = new RelayCommand<RoutedEventArgs>((e) => {
                var path = (((FileInfo)((Button)e.Source).DataContext)).FilePath;

                foreach (var v in OpenedItems) {
                    if (v.ItemPath == path) {
                        foreach (var t in v.Items) {
                            RemoveTabEvent?.Invoke(t.Key);
                        }
                        OpenedItems.Remove(v);
                        break;
                    }
                }

                RemoveFileEvent?.Invoke(path);

                for (int i = 0; i < FileInfos.Count; i++)
                    if (FileInfos[i].FilePath == path)
                        FileInfos.RemoveAt(i);

                GC.Collect();
            });

            _fileHelper = stdFileHelper;
            FileInfos = new ObservableCollection<FileInfo>();
            OpenedItems = new ObservableCollection<OpenedItemsInfo>();
            _fileHelper.UpdateFileInfo += UpdateFileInfo;
            _fileHelper.AddFileEvent += AddFileEvent;
            //_fileHelper.RemoveFileEvent += RemoveFile;

            SelectedSummary = "";
        }

        //private void RemoveFile(string path) {
        //    for (int i = 0; i < FileInfos.Count; i++)
        //        if (FileInfos[i].FilePath == path)
        //            FileInfos.RemoveAt(i);
        //}

        private void AddFileEvent(IDataAcquire fileInfo) {
            FileInfos.Add(new FileInfo(fileInfo));
        }

        private void UpdateFileInfo(IDataAcquire data) {
            for (int i = 0; i < FileInfos.Count; i++)
                if (FileInfos[i].FilePath == data.FilePath)
                    FileInfos[i].UpdateFileInfo(data);
        }

        private void AddOpenedItem(int fileHash, string itemName, int tag) {
            bool found = false;
            for (int i = 0; i < OpenedItems.Count; i++) {
                if (OpenedItems[i].FileHash == fileHash) {
                    OpenedItems[i].AddSubItem(itemName, tag);
                    found = true;
                }
            }
            if (!found) {
                OpenedItemsInfo o = new OpenedItemsInfo(_fileHelper.GetFile(fileHash), tag);
                o.AddSubItem(itemName, tag);
                OpenedItems.Add(o);
            }
        }

        private void RemoveOpenedItem(int tag, bool ifParent) {
            bool search = false;

            if (ifParent) {
                foreach (var v in OpenedItems) {
                    if (v.Tag == tag) {
                        foreach (var t in v.Items) {
                            RemoveTabEvent?.Invoke(t.Key);
                            search = true;
                        }                        
                        OpenedItems.Remove(v);
                        break;
                    }
                }
            } else {
                foreach (var v in OpenedItems) {
                    foreach (var t in v.Items) {
                        if (t.Key == tag) {
                            RemoveTabEvent?.Invoke(t.Key);
                            v.Items.Remove(t);
                            search = true;
                            break;
                        }
                    }
                    if (v.Items.Count == 0) {
                        OpenedItems.Remove(v);
                        break;
                    }
                    if (search) break;
                }
            }
        }

    }
}
