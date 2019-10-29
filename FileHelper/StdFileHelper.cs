using DataInterface;
using DataParse;
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
            IDataAcquire val = new StdfParse(path);
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

        public bool RemoveFile(string path) {
            if (_files.Remove(path.GetHashCode())) {
                RemoveFileEvent?.Invoke(path);
                return true;
            } else {
                return false;
            }
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

        public string GetBriefSummary(int fileHash, byte? site) {
            StringBuilder sb = new StringBuilder();

            IChipSummary summary;
            IFileBasicInfo info = _files[fileHash].BasicInfo;

            if (site.HasValue) {
                summary = _files[fileHash].GetChipSummaryBySite()[site.Value];
            } else {
                summary = _files[fileHash].GetChipSummary();
            }

            if (site.HasValue)
                sb.AppendLine($"Site:{site}");
            sb.AppendLine("");
            sb.AppendLine("General Info");
            sb.AppendLine($"Total QTY:{summary.TotalCount}");
            sb.AppendLine($"Pass QTY:{summary.PassCount}\t\t{(summary.PassCount * 100 / summary.TotalCount).ToString("f4")}%");
            sb.AppendLine($"Fail QTY:{summary.FailCount}\t\t{(summary.FailCount * 100 / summary.TotalCount).ToString("f4")}%");
            sb.AppendLine($"Abort QTY:{summary.AbortCount}\t\t{(summary.AbortCount * 100 / summary.TotalCount).ToString("f4")}%");
            sb.AppendLine($"Null QTY:{summary.NullCount}\t\t{(summary.NullCount * 100 / summary.TotalCount).ToString("f4")}%");
            sb.AppendLine("");
            sb.AppendLine("Re-Test Info");
            sb.AppendLine($"Fresh QTY:{summary.FreshCount}");
            sb.AppendLine($"Retest QTY:{summary.RetestCount}");


            return sb.ToString();
        }

        public int? CreateFilterDataHandler(int fileHash, byte? site) {
            if (!_files.ContainsKey(fileHash)) return null;

            var id = _files[fileHash].CreateFilterCopy(_files[fileHash].GetFilterID(site));

            return id;
        }

        public FilterSetup GetFilterSetup(int fileHash, int filterId) {
            if (!_files.ContainsKey(fileHash)) return null;

            return _files[fileHash].GetFilterSetup(filterId);
        }
    }
}
