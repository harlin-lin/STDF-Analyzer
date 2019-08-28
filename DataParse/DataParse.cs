using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StdfReader;
using System.IO;

namespace DataParse{

    public class DataParse{
        private StdfFile _stdfFile;
        private Filter _filter;
        private RawData _rawData;

        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public bool ParseDone { get; private set; }
        
        public DataParse(String filePath) {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            ParseDone = false;
            _stdfFile = new StdfFile(filePath);

            //...
        }

        public void ExtractStdf() {



        }
    }
}
