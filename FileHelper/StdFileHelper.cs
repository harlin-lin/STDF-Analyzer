using DataInterface;
using FileReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHelper {
    public enum FileChangeType{
        AddFile,
        RemoveFile
    }
    public delegate void UpdateFileInfoHandler(IDataAcquire fileInfo);
    public delegate void AddFileHandler(IDataAcquire fileInfo);
    public delegate void RemoveFileHandler(string path);


    public class StdFileHelper {
        private Dictionary<int, IDataAcquire> _files;

        public event UpdateFileInfoHandler UpdateFileInfo;
        public event AddFileHandler AddFileEvent;
        public event RemoveFileHandler RemoveFileEvent;

        public StdFileHelper() {
            _files = new Dictionary<int, IDataAcquire>();
        }

        public IDataAcquire GetFile(int fileHash) {
            if (_files.ContainsKey(fileHash))
                return _files[fileHash];
            else
                return null;
        }
        public IDataAcquire GetFile(string path){
            return GetFile(path.GetHashCode());
        }

        public IDataAcquire AddFile(string path) {
            int key = path.GetHashCode();
            IDataAcquire val = new StdReader(path, StdFileType.STD);
            if (!_files.ContainsKey(key)) { 
                _files.Add(key, val);
                val.ExtractDone += stdFile_ExtractDone;

                AddFileEvent?.Invoke(val);
                return val;
            }else {
                return null;
            }
        }

        private void stdFile_ExtractDone(IDataAcquire data) {
            UpdateFileInfo?.Invoke(data);
        }

        public void RemoveFile(string path) {
            _files[path.GetHashCode()].CleanUp();
            _files[path.GetHashCode()] = null;
            _files.Remove(path.GetHashCode());
            RemoveFileEvent?.Invoke(path);
            GC.Collect();
        }

        public void ExtractFiles(List<string> paths) {
            var ll = (from f in _files
                      where paths.Contains(f.Value.FilePath)
                      let x = f.Value
                      select x).ToList();
            Parallel.ForEach(ll, (x) => {
                x.ExtractStdf();
            });
        }

        public static string GetBriefSummary(IDataAcquire data) {
            StringBuilder sb = new StringBuilder();

            IChipSummary summary;
            IFileBasicInfo info = data.BasicInfo;

            summary = data.GetChipSummary();

            sb.AppendLine("General Info");
            sb.AppendLine($"Path:{data.FilePath}");
            sb.AppendLine($"Total QTY:{summary.TotalCount}");
            sb.AppendLine($"Pass QTY:{summary.PassCount}\t\t{((double)summary.PassCount * 100 / summary.TotalCount).ToString("f4")}%");
            sb.AppendLine($"Fail QTY:{summary.FailCount}\t\t{((double)summary.FailCount * 100 / summary.TotalCount).ToString("f4")}%");
            sb.AppendLine($"Abort QTY:{summary.AbortCount}\t\t{((double)summary.AbortCount * 100 / summary.TotalCount).ToString("f4")}%");
            sb.AppendLine($"Null QTY:{summary.NullCount}\t\t{((double)summary.NullCount * 100 / summary.TotalCount).ToString("f4")}%");
            sb.AppendLine("");
            sb.AppendLine("Re-Test Info");
            sb.AppendLine($"Fresh QTY:{summary.FreshCount}");
            sb.AppendLine($"Retest QTY:{summary.RetestCount}");


            //var filterId = _files[fileHash].GetAllFilter().Keys.ToList()[0];
            //var waferId = _files[fileHash].GetFilteredItemData(new TestID(20108), filterId);
            //var x = _files[fileHash].GetFilteredItemData(new TestID(20109), filterId);
            //var y = _files[fileHash].GetFilteredItemData(new TestID(20110), filterId);

            //StringBuilder sbb = new StringBuilder();

            //for(int i=0; i<waferId.Count;i++) {
            //    if(waferId[i].HasValue && waferId[i].Value == 11.0) {
            //        sbb.AppendLine($"{x[i]}\t\t{y[i]}");
            //    }
            //}

            //System.IO.File.WriteAllText(@"E:\Data\550x\mapraw.txt", sbb.ToString());


            return sb.ToString();
        }

        //public int? CreateFilterDataHandler(int fileHash, byte? site) {
        //    if (!_files.ContainsKey(fileHash)) return null;

        //    var id = _files[fileHash].CreateFilterCopy(_files[fileHash].GetFilterID(site));

        //    return id;
        //}

        public FilterSetup GetFilterSetup(int fileHash, int filterId) {
            if (!_files.ContainsKey(fileHash)) return null;

            return _files[fileHash].GetFilterSetup(filterId);
        }
    }
}
