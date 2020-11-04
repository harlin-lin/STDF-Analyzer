using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader {
    public class ChipInfo : IChipInfo {
        public byte Site { get; }
        public UInt32? TestTime { get; }
        public UInt16 HardBin { get; }
        public UInt16 SoftBin { get; }
        public string PartId { get; }
        public CordType WaferCord { get; }
        public int InternalId { get; }
        public DeviceType ChipType { get; }        
        public ResultType Result { get; }

        public ChipInfo(byte site,UInt32? testTime, UInt16 hb, UInt16 sb, string partId, CordType cord, DeviceType deviceType, ResultType result, int idAll) {
            this.Site = site;
            this.TestTime = testTime;
            this.HardBin = hb;
            this.SoftBin = sb;
            this.PartId = partId;
            this.WaferCord = cord;
            this.ChipType = deviceType;
            this.Result = result;
            this.InternalId = idAll;
        }


    }
}
