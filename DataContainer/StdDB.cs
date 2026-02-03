using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer
{
    public struct SubData : IEquatable<SubData> {
        public string StdFilePath { get; }
        public int FilterId { get; }
        public int FilterId_index { get; }

        public SubData(string filePath, int filterId,int filterIdx=0) {
            StdFilePath = filePath;
            FilterId = filterId;
            FilterId_index = filterIdx;
        }

        public bool Equals(SubData other) {
            return StdFilePath == other.StdFilePath && FilterId == other.FilterId;
        }
    }

    public static class StdDB{

        private static ConcurrentDictionary<string, SubContainer> _subContainers = new ConcurrentDictionary<string, SubContainer>();

        public static List<string> GetAllFiles() {
            return _subContainers.Keys.ToList();
        }

        public static List<IDataAcquire> GetAllDataAcquires() {
            return (from r in _subContainers select (IDataAcquire)r.Value).ToList();
        }

        public static List<SubData> GetAllSubData() {
            List<SubData> rst = new List<SubData>();
            int i;
            foreach(var v in _subContainers) {
                i = 0;
                foreach(var f in v.Value.GetAllFilterId()) {
                    rst.Add(new SubData(v.Key, f,i));
                    i++;
                }
            }

            return rst;
        }


        public static bool IfExsistFile(string filePath) {
            return _subContainers.ContainsKey(filePath);
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

        public static bool MergeFiles(string filePath, IEnumerable<string> files) {
            if (!_subContainers.TryAdd(filePath, new SubContainer(filePath))) return false;
            foreach (var f in files) {
                if (!IfExsistFile(f) || !_subContainers[filePath].MergeData(_subContainers[f])) {
                    RemoveFile(filePath);
                    return false;
                }
            }
            _subContainers[filePath].AnalyseData();
            return true;
        }

        public static bool MergeSubData(string filePath, IEnumerable<SubData> subDataList) {
            if (!_subContainers.TryAdd(filePath, new SubContainer(filePath))) return false;
            foreach (var f in subDataList) {
                if (!IfExsistFile(f.StdFilePath) || !_subContainers[filePath].MergeSubData(_subContainers[f.StdFilePath], f.FilterId)) {
                    RemoveFile(filePath);
                    return false;
                }
            }
            _subContainers[filePath].AnalyseData();
            return true;
        }

        public static bool RemoveFile(string filePath) {
            SubContainer tmp;
            var rst = _subContainers.TryRemove(filePath, out tmp);
            if(tmp != null)
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
