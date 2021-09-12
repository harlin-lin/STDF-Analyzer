using System;
using System.Collections.Generic;
using System.Linq;

namespace FileReader {
    class PinGroupRecord {
        public UInt16 GroupIndex;
        public string GroupName;
        public List<PinMapRecord> listPins;

        public PinGroupRecord(UInt16 idx, string name, UInt16[] idxes, List<PinMapRecord> listPinMaps) {
            GroupIndex = idx;
            GroupName = name;

            if (listPinMaps == null)
                throw new Exception("PinMaps in Null!");

            listPins = new List<PinMapRecord>();
            foreach (var v in idxes) {
                if (v < listPinMaps.Count)
                    listPins.Add(listPinMaps.ElementAt(v));
                else
                    throw new Exception("PGR Cannot get pins from the PMRs!");
            }

        }

    }
}
