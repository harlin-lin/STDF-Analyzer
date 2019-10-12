using DataInterface;
using FileHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SillyMonkey.ViewModel {
    public class LoadedFile : INotifyPropertyChanged {
        #region private
        //private Dictionary<int, IDataAcquire> _files;
        private StdFileHelper _fileHelper;
        #endregion

        #region File Select property


        public ObservableCollection<FileInfo> FileInfos { get; private set; }
        public string SelectedSummary { get; private set; }

        public void AddFile(string path) {
            var val = _fileHelper.AddFile(path);
            val.ExtractDone += Val_ExtractDone;
            FileInfos.Add(new FileInfo(val));
        }
        public void RemoveFile(string path) {
            if (_fileHelper.RemoveFile(path)) {
                for (int i = 0; i < FileInfos.Count; i++)
                    if (FileInfos[i].FilePath == path)
                        FileInfos.RemoveAt(i);
            } else {
                ///////////////////////////
            }
        }

        public void ExtractFiles(List<string> paths) {
            _fileHelper.ExtractFiles(paths);
        }

        //public void ChangeFileSelected(int fileHash, byte? site) {
        //    StringBuilder sb = new StringBuilder();

        //    IChipSummary summary;
        //    IFileBasicInfo info = _files[fileHash].BasicInfo;

        //    if (site.HasValue) {
        //        summary = _files[fileHash].GetChipSummaryBySite()[site.Value];
        //    } else {
        //        summary = _files[fileHash].GetChipSummary();
        //    }

        //    if (site.HasValue)
        //        sb.AppendLine($"Site:{site}");
        //    sb.AppendLine("");
        //    sb.AppendLine("General Info");
        //    sb.AppendLine($"Total Count:{summary.TotalCount}");
        //    sb.AppendLine($"Pass Count:{summary.PassCount}\t\t{(summary.PassCount * 100 / summary.TotalCount).ToString("f2")}%");
        //    sb.AppendLine($"Total Count:{summary.FailCount}\t\t{(summary.FailCount * 100 / summary.TotalCount).ToString("f2")}%");
        //    sb.AppendLine($"Total Count:{summary.AbortCount}\t\t{(summary.AbortCount * 100 / summary.TotalCount).ToString("f2")}%");
        //    sb.AppendLine($"Total Count:{summary.NullCount}\t\t{(summary.NullCount * 100 / summary.TotalCount).ToString("f2")}%");
        //    sb.AppendLine("");
        //    sb.AppendLine("Re-Test Info");
        //    sb.AppendLine($"Total Count:{summary.FreshCount}");
        //    sb.AppendLine($"Total Count:{summary.RetestCount}");

        //    SelectedSummary = sb.ToString();

        //    OnPropertyChanged("SelectedSummary");
        //}


        #endregion


        #region RawData Display



        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        //OnPropertyChanged event handler to update property value in binding
        private void OnPropertyChanged(string info) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(info));
        }





        public LoadedFile() {
            _fileHelper = new StdFileHelper();
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
