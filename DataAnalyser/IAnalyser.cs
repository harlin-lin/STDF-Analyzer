using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalyser {
    interface IAnalyser {
        int AddFile(string path);
        void RemoveFile(int fileId);
        List<string> GetFileNameList();
        List<string> GetFilePathList();

        

    }
}
