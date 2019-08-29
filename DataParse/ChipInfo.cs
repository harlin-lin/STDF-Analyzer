using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public class ChipInfo {
        public byte Site { get; }
        public UInt32? TestTime { get; }
        public UInt16 HardBin { get; }
        public UInt16? SoftBin { get; }
        public string PartId { get; }
        public CordType WaferCord { get; }
        public bool IfPassed { get; }
        public bool IfReTest { get; }
        public int InternalId { get; }


        public ChipInfo(byte site, UInt32? tTime, UInt16 hb, UInt16? sb, string PId,
            CordType cord, bool passed, bool retest, int idAll) {
            this.Site = site;
            this.TestTime = tTime;
            this.HardBin = hb;
            this.SoftBin = sb;
            this.PartId = PId;
            this.WaferCord = cord;
            this.IfPassed = passed;
            this.IfReTest = retest;
            this.InternalId = idAll;
        }

    }
}
