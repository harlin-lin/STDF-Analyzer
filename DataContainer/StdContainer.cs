using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer
{
    public class StdContainer{
        public static bool IfCmpTextInUid = false;

        private ConcurrentDictionary<string, SubContainer> _subContainers;


        public StdContainer(){
            _subContainers = new ConcurrentDictionary<string, SubContainer>();
        }

        public List<string> GetAllFiles() {
            return (from r in _subContainers select r.Value.FilePath).ToList();
        }

        public List<IDataAcquire> GetAllDataAcquires() {
            return (from r in _subContainers select (IDataAcquire)r.Value).ToList();
        }


        public bool IfExsistFile(string filePath) {
            return _subContainers.Any(x=> x.Value.FilePath == filePath);
        }

        public IDataCollect CreateSubContainer(string filePath) {
            if (IfExsistFile(filePath)) throw new Exception("Already exsist this file");
            _subContainers.TryAdd(filePath, new SubContainer(filePath));
            return _subContainers[filePath];
        }

        public IDataAcquire GetDataAcquire(string filePath) {
            if (!IfExsistFile(filePath)) throw new Exception("No file in container");
            return _subContainers[filePath];
        }

        public bool RemoveFile(string filePath) {
            SubContainer tmp;
            var rst = _subContainers.TryRemove(filePath, out tmp);
            tmp.Dispose();
            tmp = null;
            GC.Collect();
            return rst;
        }

        public void RemoveAllFiles() {
            _subContainers.Clear();
            GC.Collect();
        }
    }
}
