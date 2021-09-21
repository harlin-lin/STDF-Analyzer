using DataContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SillyMonkey.Core {
    public enum TabType {
        RawDataTab,
        RawDataCorTab
    }
    public struct SubData : IEquatable<SubData> {
        public string StdFilePath { get; }
        public int FilterId { get; }

        public SubData(string filePath, int filterId) {
            StdFilePath = filePath;
            FilterId = filterId;
        }

        public bool Equals(SubData other) {
            return StdFilePath == other.StdFilePath && FilterId == other.FilterId;
        }


    }

    public class DataViewItem {
        string TabTitle { get; }
        List<SubData> DataList { get; }
        TabType TabType { get; }
    }
}
