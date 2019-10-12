using DataInterface;
using DataParse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHelper {
    public class StdFileHelper {
        private Dictionary<int, IDataAcquire> _files;


        public StdFileHelper() {
            _files = new Dictionary<int, IDataAcquire>();
        }

        public IDataAcquire AddFile(string path) {
            int key = path.GetHashCode();
            IDataAcquire val = new StdfParse(path);
            if (!_files.ContainsKey(key))
                _files.Add(key, val);
            else {

            }
            return val;
        }
        public bool RemoveFile(string path) {
            return _files.Remove(path.GetHashCode());
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
    }
}
