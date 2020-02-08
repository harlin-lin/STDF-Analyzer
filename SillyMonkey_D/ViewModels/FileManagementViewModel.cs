using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DataInterface;
using DevExpress.Mvvm;
using FileHelper;

namespace SillyMonkey_D.ViewModels {
    public delegate void OpenDetailHandler(IDataAcquire dataAcquire, int? filterId, int tag);
    public delegate void RemoveHandler(int tag);
    public delegate void ChangeViewTabHandler(int hash);

    public class LoadedInfoBasic : ViewModelBase {
        public string FileName { get { return GetProperty(() => FileName); } set { SetProperty(() => FileName, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } set { SetProperty(() => FilePath, value); } }
        public bool FileStatus { get { return GetProperty(() => FileStatus); } set { SetProperty(() => FileStatus, value); } }
        public int FileDeviceCount { get { return GetProperty(() => FileDeviceCount); } set { SetProperty(() => FileDeviceCount, value); } }
        public byte? Site { get { return GetProperty(() => Site); } set { SetProperty(() => Site, value); } }

        public LoadedInfoBasic() {
            FileName = string.Empty;
            FilePath = string.Empty;
            FileStatus = false;
            FileDeviceCount = 0;
            Site = null;
        }
    }
    public class FileInfo : LoadedInfoBasic {
        public ObservableCollection<SiteInfo> Sites { get; set; }

        public FileInfo(IDataAcquire stdfParse) {
            FileName = stdfParse.FileName;
            FilePath = stdfParse.FilePath;
            FileStatus = stdfParse.ParseDone;
            FileDeviceCount = stdfParse.ChipsCount;
            //Sites = new Dictionary<byte, KeyValuePair<int, string>>();
            Sites = new ObservableCollection<SiteInfo>();
        }

        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public void UpdateFileInfo(IDataAcquire stdfParse) {
            _dispatcher.Invoke(new Action(() => {
                var g = from f in stdfParse.GetSitesChipCount()
                        let x = new SiteInfo(this, f.Key, f.Value)
                        select x;
                foreach (var v in g)
                    Sites.Add(v);
                FileStatus = stdfParse.ParseDone;
                FileDeviceCount = stdfParse.ChipsCount;
            }));
        }
    }
    public class SiteInfo : LoadedInfoBasic {
        public SiteInfo(FileInfo file, byte site, int count) {
            FileName = file.FileName;
            FilePath = file.FilePath;

            Site = site;
            FileDeviceCount = count;
            FileStatus = true;

        }
    }

    public class OpenedInfoBasic : ViewModelBase {
        public string FileName { get { return GetProperty(() => FileName); } set { SetProperty(() => FileName, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } set { SetProperty(() => FilePath, value); } }
        public int FileHash { get { return GetProperty(() => FileHash); } set { SetProperty(() => FileHash, value); } }
        public int Tag { get { return GetProperty(() => Tag); } set { SetProperty(() => Tag, value); } }

        public OpenedInfoBasic() {
            FileHash = 0;
            FileName = string.Empty;
            FilePath = string.Empty;
            Tag = 0;
        }
    }
    public class OpenedFile : OpenedInfoBasic {
        public ObservableCollection<OpenedItem> Items { get; private set; }

        public OpenedFile(IDataAcquire stdfParse, int tag) {
            FileHash = stdfParse.FilePath.GetHashCode();
            FileName = stdfParse.FileName;
            FilePath = stdfParse.FilePath;
            Items = new ObservableCollection<OpenedItem>();
            Tag = tag;
        }

        public void AddSubItem(string itemName, int tag) {

            Items.Add(new OpenedItem(this, itemName, tag));
        }
    }
    public class OpenedItem : OpenedInfoBasic {
        public string ItemName { get; set; }
        public OpenedItem(OpenedFile openedFile, string itemName, int tag) {
            ItemName = itemName;
            FileHash = openedFile.FileHash;
            FileName = openedFile.FileName;
            FilePath = openedFile.FilePath;
            Tag = tag;
        }

    }
    public class FileManagementViewModel : ViewModelBase {
        private StdFileHelper _fileHelper;

        public ObservableCollection<FileInfo> FileInfos { get; private set; }
        public ObservableCollection<OpenedFile> OpenedItems { get; private set; }

        public string SelectedSummary { get { return GetProperty(() => SelectedSummary); } private set { SetProperty(() => SelectedSummary, value); } }
        public object SelectedOpenedItem { get { return GetProperty(() => SelectedOpenedItem); } set { SetProperty(() => SelectedOpenedItem, value); } }

        //public ICommand<DevExpress.Xpf.Grid.SelectedItemChangedEventArgs> SelectedItemChangedCommand { get; private set; }
        //public ICommand<DevExpress.Xpf.Grid.SelectedItemChangedEventArgs> ChangeDisplayCommand { get; private set; }
        public ICommand<object> DoubleClickCommand { get; private set; }
        public ICommand<object> CloseCommand { get; private set; }
        public ICommand<RoutedEventArgs> FileCloseCommand { get; private set; }
        public ICommand CopySummary { get; private set; }

        public event OpenDetailHandler OpenDetailEvent;
        public event RemoveHandler RemoveTabEvent;
        public event ChangeViewTabHandler ChangeViewTabEvent;

        public FileManagementViewModel() {
            _fileHelper = new StdFileHelper();
            FileInfos = new ObservableCollection<FileInfo>();
            OpenedItems = new ObservableCollection<OpenedFile>();
            _fileHelper.UpdateFileInfo += UpdateFileInfo;
            _fileHelper.AddFileEvent += AddFileEvent;

            SelectedSummary = "";
            SelectedOpenedItem = string.Empty;

            //SelectedItemChangedCommand = new DelegateCommand<DevExpress.Xpf.Grid.SelectedItemChangedEventArgs>((e) => {
            //    if (!(e.NewItem is LoadedInfoBasic)) return;
            //    LoadedInfoBasic s = e.NewItem as LoadedInfoBasic;
            //    if (!s.FileStatus) return;
            //    SelectedSummary = _fileHelper.GetBriefSummary(s.FilePath.GetHashCode(), s.Site);
            //});

            //ChangeDisplayCommand = new DelegateCommand<DevExpress.Xpf.Grid.SelectedItemChangedEventArgs>((e) => {
            //    if (!(e.NewItem is OpenedInfoBasic)) return;
            //    var s = e.NewItem as OpenedInfoBasic;
            //    ChangeViewTabEvent?.Invoke(s.Tag);
            //});

            DoubleClickCommand = new DelegateCommand<object>((e) => {
                if (!(e is LoadedInfoBasic)) return;
                var s = e as LoadedInfoBasic;
                if (!s.FileStatus) return;
                int hash = s.FilePath.GetHashCode();
                int tag = hash ^ System.DateTime.UtcNow.Ticks.GetHashCode();
                AddOpenedItem(hash, $"[RAW] SITE:{s.Site.ToString()} {s.FileName}", tag);
                var id = _fileHelper.CreateFilterDataHandler(hash, s.Site);
                OpenDetailEvent?.Invoke(_fileHelper.GetFile(hash), id, tag);
            });

            CloseCommand = new DelegateCommand<object>((e) => {
                var s = e as OpenedInfoBasic;
                bool b = s.GetType().Name == "OpenedFile";
                RemoveOpenedItem(s.Tag, b);
            });
            FileCloseCommand = new DelegateCommand<RoutedEventArgs>((e) => {
                var path = (((FileInfo)((Button)e.Source).DataContext)).FilePath;

                foreach (var v in OpenedItems) {
                    if (v.FilePath == path) {
                        foreach (var t in v.Items) {
                            RemoveTabEvent?.Invoke(t.Tag);
                        }
                        OpenedItems.Remove(v);
                        break;
                    }
                }

                RemoveFile(path);

                for (int i = 0; i < FileInfos.Count; i++)
                    if (FileInfos[i].FilePath == path)
                        FileInfos.RemoveAt(i);

                GC.Collect();
            });

            CopySummary = new DelegateCommand(() => {
                Clipboard.SetDataObject(SelectedSummary);
            });


        }

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
                OpenedFile o = new OpenedFile(_fileHelper.GetFile(fileHash), tag);
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
                            RemoveTabEvent?.Invoke(t.Tag);
                            search = true;
                        }
                        OpenedItems.Remove(v);
                        break;
                    }
                }
            } else {
                foreach (var v in OpenedItems) {
                    foreach (var t in v.Items) {
                        if (t.Tag == tag) {
                            RemoveTabEvent?.Invoke(t.Tag);
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


        public async void AddFile(string path) {
            var val = _fileHelper.AddFile(path);
            //extract the files
            await Task.Run(new Action(() => val?.ExtractStdf()));
        }
        private void RemoveFile(string path) {
            _fileHelper.RemoveFile(path);
            GC.Collect();
        }

        public void ChangeTab(int tag) {
            for (int i = 0; i < OpenedItems.Count; i++) {
                for (int j = 0; j < OpenedItems[i].Items.Count; j++) {
                    if (OpenedItems[i].Items[j].Tag == tag)
                        SelectedOpenedItem = OpenedItems[i].Items[j];
                }
            }

        }

    }
}