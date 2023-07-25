using System;

namespace ReadTest {
    public struct PinMapRecord {
        public UInt16 PinIndex;
        public string ChanName;
        //public string PhyName;
        //public string LogicalName;

        //public PinMapRecord(UInt16 idx, string chan, string phy, string logic) {
        //    PinIndex = idx;
        //    ChanName = chan;
        //    PhyName = phy;
        //    LogicalName = logic;
        //}
        public PinMapRecord(UInt16 idx, string chan) {
            PinIndex = idx;
            ChanName = chan;
        }
    }
}
