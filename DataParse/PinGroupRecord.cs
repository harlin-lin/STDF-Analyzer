using StdfReader.Records.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    class PinGroupRecord {
        public UInt16 GroupIndex;
        public string GroupName;
        public List<PinMapRecord> listPins;

        public PinGroupRecord(Pgr pgr, List<PinMapRecord> listPinMaps) {
            GroupIndex = pgr.GroupIndex;
            GroupName = pgr.GroupName;

            if (listPinMaps == null)
                throw new Exception("PinMaps in Null!");

            listPins = new List<PinMapRecord>();
            foreach (var v in pgr.PinIndexes) {
                if (v < listPinMaps.Count)
                    listPins.Add(listPinMaps.ElementAt(v));
                else
                    throw new Exception("PGR Cannot get pins from the PMRs!");
            }

        }

    }
}
