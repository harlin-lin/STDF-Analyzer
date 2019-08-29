using StdfReader.Records.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public struct PinMapRecord {
        public UInt16 PinIndex;
        public string ChanName;
        public string PhyName;
        public string LogicalName;

        public PinMapRecord(Pmr pmr) {
            PinIndex = pmr.PinIndex;
            ChanName = pmr.ChannelName;
            PhyName = pmr.PhysicalName;
            LogicalName = pmr.LogicalName;
        }

    }
}
