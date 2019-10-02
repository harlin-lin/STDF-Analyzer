using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParse;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace DataAnalyser
{
    public class FileInfo : INotifyPropertyChanged {
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
            //Sites = new List<byte>();
            //SitesCount = new List<int>();
            Sites = new Dictionary<byte, KeyValuePair<int, string>>();
        }

        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public void UpdateFileInfo(IDataAcquire stdfParse) {
            FileName = stdfParse.FileName;
            FilePath = stdfParse.FilePath;
            FileStatus = stdfParse.ParseDone;
            FileDeviceCount = stdfParse.ChipsCount;
            _dispatcher.Invoke(new Action(() => {
                //Sites = stdfParse.GetSites();
                //SitesCount=stdfParse.GetSitesChipCount().Values.ToList();
                Sites = (from f in stdfParse.GetSitesChipCount()
                         let x = new KeyValuePair<byte, KeyValuePair<int, string>>(f.Key, new KeyValuePair<int, string>(f.Value, FilePath))
                         select x).ToDictionary(x=>x.Key, x=>x.Value);
            }));

            OnPropertyChanged("FileStatus");
            OnPropertyChanged("FileDeviceCount");
            OnPropertyChanged("Sites");
            //OnPropertyChanged("SitesCount");

        }

        public event PropertyChangedEventHandler PropertyChanged;
        //OnPropertyChanged event handler to update property value in binding
        private void OnPropertyChanged(string info) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }


    public class Analyser: INotifyPropertyChanged {
        #region private
        private Dictionary<int, StdfParse> _files;

        #endregion

        #region File Select property


        public ObservableCollection<FileInfo> FileInfos { get; private set; }
        public string SelectedSummary { get; private set; }

        public int AddFile(string path) {
            int key = path.GetHashCode();
            var val = new StdfParse(path);
            FileInfos.Add(new FileInfo(val));
            val.ExtractDone += Val_ExtractDone;
            if(!_files.ContainsKey(key))
                _files.Add(key, val);
            else {

            }

            //OnPropertyChanged("FileInfos");
            return key;
        }
        public void RemoveFile(string path) {
            if (_files.Remove(path.GetHashCode())) {
                for (int i = 0; i < FileInfos.Count; i++)
                    if (FileInfos[i].FilePath == path)
                        FileInfos.RemoveAt(i);
                //OnPropertyChanged("FileInfos");
            } else {

            }
        }

        public void ExtractFiles(List<string> paths) {
            var ll=(from f in _files
             where paths.Contains(f.Value.FilePath)
             let x = f.Value
             select x).ToList();
            Parallel.ForEach(ll, (x) => {
                x.ExtractStdf();
            });
        }

        public void ChangeFileSelected(int fileHash, byte? site) {
            StringBuilder sb = new StringBuilder();

            ChipSummary summary;
            FileBasicInfo info = _files[fileHash].BasicInfo;

            if (site.HasValue) {
                summary = _files[fileHash].GetChipSummaryBySite()[site.Value];
            } else {
                summary = _files[fileHash].GetChipSummary();
            }

            if (site.HasValue)
                sb.AppendLine($"Site:{site}");
            sb.AppendLine("");
            sb.AppendLine("General Info");
            sb.AppendLine($"Total Count:{summary.TotalCount}");
            sb.AppendLine($"Pass Count:{summary.PassCount}\t\t{(summary.PassCount*100/summary.TotalCount).ToString("f2")}%");
            sb.AppendLine($"Total Count:{summary.FailCount}\t\t{(summary.FailCount * 100 / summary.TotalCount).ToString("f2")}%");
            sb.AppendLine($"Total Count:{summary.AbortCount}\t\t{(summary.AbortCount * 100 / summary.TotalCount).ToString("f2")}%");
            sb.AppendLine($"Total Count:{summary.NullCount}\t\t{(summary.NullCount * 100 / summary.TotalCount).ToString("f2")}%");
            sb.AppendLine("");
            sb.AppendLine("Re-Test Info");
            sb.AppendLine($"Total Count:{summary.FreshCount}");
            sb.AppendLine($"Total Count:{summary.RetestCount}");

            SelectedSummary = sb.ToString();

            OnPropertyChanged("SelectedSummary");
        }


        #endregion


        #region RawData Display



        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        //OnPropertyChanged event handler to update property value in binding
        private void OnPropertyChanged(string info) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(info));
        }





        public Analyser() {
            _files = new Dictionary<int, StdfParse>();
            FileInfos = new ObservableCollection<FileInfo>();
            SelectedSummary = "";
        }


        private void Val_ExtractDone(IDataAcquire data) {
            for (int i = 0; i < FileInfos.Count; i++)
                if (FileInfos[i].FilePath == data.FilePath)
                    FileInfos[i].UpdateFileInfo(data);
        }

    }
}
