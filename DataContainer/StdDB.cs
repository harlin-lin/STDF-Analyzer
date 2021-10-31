using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer
{
    public static class StdDB{
        public static bool IfCmpTextInUid = false;

        private static ConcurrentDictionary<string, SubContainer> _subContainers = new ConcurrentDictionary<string, SubContainer>();

        public static List<string> GetAllFiles() {
            return (from r in _subContainers select r.Value.FilePath).ToList();
        }

        public static List<IDataAcquire> GetAllDataAcquires() {
            return (from r in _subContainers select (IDataAcquire)r.Value).ToList();
        }


        public static bool IfExsistFile(string filePath) {
            return _subContainers.Any(x=> x.Value.FilePath == filePath);
        }

        //public static IDataCollect CreateSubContainer(string filePath) {
        //    if (IfExsistFile(filePath)) throw new Exception("Already exsist this file");
        //    _subContainers.TryAdd(filePath, new SubContainer(filePath));
        //    return _subContainers[filePath];
        //}
        public static IDataCollect GetDataCollect(string filePath) {
            if (!IfExsistFile(filePath)) throw new Exception("No file in container");
            return _subContainers[filePath];
        }

        public static IDataAcquire CreateSubContainer(string filePath) {
            if (IfExsistFile(filePath)) throw new Exception("Already exsist this file");
            _subContainers.TryAdd(filePath, new SubContainer(filePath));
            return _subContainers[filePath];
        }


        public static IDataAcquire GetDataAcquire(string filePath) {
            if (!IfExsistFile(filePath)) throw new Exception("No file in container");
            return _subContainers[filePath];
        }

        public static string MergeFiles(List<string> files) {
            var filePath = "\\MergerFile_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            _subContainers.TryAdd(filePath, new SubContainer(filePath));
            foreach(var f in files) {
                if (!IfExsistFile(f)) throw new Exception("File not exsist");
            }
            foreach (var f in files) {
                _subContainers[filePath].MergeData(_subContainers[f]);
            }
            _subContainers[filePath].AnalyseData();

            return filePath;
        }

        public static bool RemoveFile(string filePath) {
            SubContainer tmp;
            var rst = _subContainers.TryRemove(filePath, out tmp);
            tmp.Dispose();
            GC.Collect();
            return rst;
        }

        public static void RemoveAllFiles() {
            _subContainers.Clear();
            GC.Collect();
        }
    }
}
