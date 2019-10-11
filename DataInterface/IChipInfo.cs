using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public enum DeviceType {
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

    public interface IChipInfo {
        byte Site { get; }
        UInt32? TestTime { get; }
        UInt16 HardBin { get; }
        UInt16 SoftBin { get; }
        string PartId { get; }
        CordType WaferCord { get; }
        int InternalId { get; }
        DeviceType ChipType { get; }
        ResultType Result { get; }

    }
}
