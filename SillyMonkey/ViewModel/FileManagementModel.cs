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
using System.Windows.Threading;

namespace SillyMonkey.ViewModel {
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

    public class FileManagementModel : ViewModelBase {

        private StdFileHelper _fileHelper;

        public ObservableCollection<FileInfo> FileInfos { get; private set; }

        public string SelectedSummary { get; private set; }

        public RelayCommand<RoutedPropertyChangedEventArgs<object>> SelectedItemChangedCommand { get; private set; }
        public RelayCommand<System.Windows.Input.MouseEventArgs> DoubleClickCommand { get; private set; }

        public FileManagementModel(StdFileHelper stdFileHelper) {
            SelectedItemChangedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>((e) => {
                if (e.NewValue is FileInfo) {
                    var s = e.NewValue as FileInfo;
                    SelectedSummary = _fileHelper.GetBriefSummary(s.FilePath.GetHashCode(), null);
                } else {

                    var s = (KeyValuePair<byte, KeyValuePair<int, string>>)e.NewValue;
                    SelectedSummary = _fileHelper.GetBriefSummary(s.Value.Value.GetHashCode(), s.Key);
                }
                RaisePropertyChanged("SelectedSummary");
            });

            DoubleClickCommand = new RelayCommand<System.Windows.Input.MouseEventArgs>((e) => {
                ;
            });

            _fileHelper = stdFileHelper;
            FileInfos = new ObservableCollection<FileInfo>();
            _fileHelper.UpdateFileInfo += UpdateFileInfo;
            _fileHelper.AddFileEvent += AddFileEvent;
            _fileHelper.RemoveFileEvent += RemoveFileEvent;

            SelectedSummary = "";
        }

        private void RemoveFileEvent(string path) {
            for (int i = 0; i < FileInfos.Count; i++)
                if (FileInfos[i].FilePath == path)
                    FileInfos.RemoveAt(i);
        }

        private void AddFileEvent(IDataAcquire fileInfo) {
            FileInfos.Add(new FileInfo(fileInfo));
        }

        private void UpdateFileInfo(IDataAcquire data) {
            for (int i = 0; i < FileInfos.Count; i++)
                if (FileInfos[i].FilePath == data.FilePath)
                    FileInfos[i].UpdateFileInfo(data);
        }
    }
}
