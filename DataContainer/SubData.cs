using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public struct SubData : IEquatable<SubData> {
        public string StdFilePath { get; }
        public int FilterId { get; }

        public SubData(string filePath, int filterId) {
            StdFilePath = filePath;
            FilterId = filterId;
        }

        public bool Equals(SubData other) {
            return StdFilePath== other.StdFilePath && FilterId==other.FilterId;
        }

        
    }
}
