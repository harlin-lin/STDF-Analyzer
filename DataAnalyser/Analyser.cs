using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParse;

namespace DataAnalyser
{
    public class Analyser
    {
        Dictionary<int, StdfParse> files = new Dictionary<int, StdfParse>();

        public Analyser() {
            
        }

        public int AddFile(string path) {
            int key = path.GetHashCode();

            throw new NotImplementedException();

            return key;
        }

    }
}
