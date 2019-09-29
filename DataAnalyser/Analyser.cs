using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParse;
using System.Collections.ObjectModel;


namespace DataAnalyser
{
    public class FileInfo {
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public bool FileStatus { get; private set; }
        public int FileDeviceCount { get; private set; }
        public Dictionary<byte, int> Sites { get; private set; }

        public FileInfo(IDataAcquire stdfParse) {
            FileName = stdfParse.FileName;
            FilePath = stdfParse.FilePath;
            FileStatus = stdfParse.ParseDone;
            FileDeviceCount = stdfParse.ChipsCount;
            Sites = stdfParse.GetSitesChipCount();
        }

        public void UpdateFileInfo(IDataAcquire stdfParse) {
            FileName = stdfParse.FileName;
            FilePath = stdfParse.FilePath;
            FileStatus = stdfParse.ParseDone;
            FileDeviceCount = stdfParse.ChipsCount;
            Sites = stdfParse.GetSitesChipCount();
        }

    }

    public class Analyser: INotifyPropertyChanged {
        #region private
        private Dictionary<int, StdfParse> _files;


        #endregion

        #region File Select property


        public ObservableCollection<FileInfo> FileInfos { get; private set; }
        //public List<string> FileNames { get {
        //        return (from f in _files.Values
        //                let s = f.FileName
        //                select s).ToList();
        //               } }

        //public List<string> FilePaths {
        //    get {
        //        return (from f in _files.Values
        //                let s = f.FilePath
        //                select s).ToList();
        //    }
        //}

        //public List<bool> FilesStatus {
        //    get {
        //        return (from f in _files.Values
        //                let s = f.ParseDone
        //                select s).ToList();
        //    }
        //}
        //public List<int> FilesStatistic {
        //    get {
        //        return (from f in _files.Values
        //                let s = f.ChipsCount
        //                select s).ToList();
        //    }
        //}

        //public List<Dictionary<byte, int>> FilesSites {
        //    get {
        //        return (from f in _files.Values
        //                let s = f.GetSitesChipCount()
        //                select s).ToList();
        //    }
        //}


        public int AddFile(string path) {
            int key = path.GetHashCode();
            var val = new StdfParse(path);
            FileInfos.Add(new FileInfo(val));
            val.ExtractDone += Val_ExtractDone;

            _files.Add(key, val);


            OnPropertyChanged("FileInfos");
            return key;
        }
        public void RemoveFile(string path) {
            if (_files.Remove(path.GetHashCode())) {
                //FileInfos.Remove(FileInfos.FindLast((x)=>x.FilePath==path));
                OnPropertyChanged("FileInfos");
            } else {

            }
        }

        public void ExtractFiles() {
            Parallel.ForEach(_files, (x) => {
                x.Value.ExtractStdf();
            });
        }

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
        }


        private void Val_ExtractDone(IDataAcquire data) {
            //FileInfos.FindLast((x) => x.FilePath == data.FilePath).UpdateFileInfo(data);
            OnPropertyChanged("FileInfos");
        }

    }
}
