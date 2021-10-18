using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public class Filter {
        public bool[] FilterIdxFlag;
        public bool[] FilterItemFlag;
        public bool IfNoChanged;
        public PartStatistic FilterPartStatistic;
        public ConcurrentDictionary<string, ItemStatistic> FilterItemStatistics;
        public IEnumerable<int> FilteredPartIdx;
        public IEnumerable<string> FilteredUid;

        public Filter(int partCnt, int itemCnt) {

            FilterIdxFlag = new bool[partCnt];
            FilterItemFlag = new bool[itemCnt];
            IfNoChanged = true;
        }

        public void ResetFilter() {
            if (IfNoChanged) return;
            Array.Clear(FilterIdxFlag, 0, FilterIdxFlag.Length);
            Array.Clear(FilterItemFlag, 0, FilterItemFlag.Length);
            IfNoChanged = true;
        }

    }

}
