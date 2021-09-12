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
        public bool IfNoChanged { get; private set; }
        public PartStatistic FilterPartStatistic;
        public ConcurrentDictionary<string, ItemStatistic> FilterItemStatistics;

        public Filter(int partCnt, int itemCnt) {

            FilterIdxFlag = new bool[partCnt];
            FilterItemFlag = new bool[itemCnt];

            IfNoChanged = true;
        }

        public void ResetFilter() {
            if (IfNoChanged) return;
            FilterIdxFlag.Initialize();
            FilterItemFlag.Initialize();
            IfNoChanged = true;
        }

    }

}
