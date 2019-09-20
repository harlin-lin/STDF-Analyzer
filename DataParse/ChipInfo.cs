using StdfReader.Records.V4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public enum DeviceType{
        Fresh,
        RT_ID,
        RT_Cord
    }

    public enum ResultType {
        Pass,
        Fail,
        Abort,
        Null
    }
    public class ChipInfo {
        public byte Site { get; }
        public UInt32? TestTime { get; }
        public UInt16 HardBin { get; }
        public UInt16 SoftBin { get; }
        public string PartId { get; }
        public CordType WaferCord { get; }
        public int InternalId { get; }
        public DeviceType ChipType { get; }        
        public ResultType Result { get; }

        public ChipInfo(Prr prr, int idAll) {
            this.Site = prr.SiteNumber;
            this.TestTime = prr.TestTime;
            this.HardBin = prr.HardBin;
            this.SoftBin = prr.SoftBin;
            this.PartId = prr.PartId;
            this.WaferCord = new CordType(prr.XCoordinate, prr.YCoordinate);
            if (prr.SupersedesPartId)
                this.ChipType = DeviceType.RT_ID;
            else if (prr.SupersedesCoords)
                this.ChipType = DeviceType.RT_Cord;
            else
                this.ChipType = DeviceType.Fresh;

            if (prr.AbnormalTest)
                this.Result = ResultType.Abort;
            else {
                if (prr.Failed.HasValue)
                    this.Result = (bool)prr.Failed ? ResultType.Fail : ResultType.Pass;
                else
                    this.Result = ResultType.Null;
            }
            this.InternalId = idAll;
        }


    }
}
