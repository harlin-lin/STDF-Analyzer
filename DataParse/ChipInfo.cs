using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    class ChipInfo {
        public byte Site { get; }
        public UInt32 TestTime { get; }
        public UInt16 HardBin { get; }
        public UInt16? SoftBin { get; }
        public string PartId { get; }
        public CordType WaferCord { get; }
        public bool IfPassed { get; }
        public bool IfReTest { get; }
        public int InternalId { get; }
        public int InternalIdSite { get; }

    }
}
