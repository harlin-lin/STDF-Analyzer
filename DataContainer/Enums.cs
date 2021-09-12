using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer{
    public enum DeviceType{
        Fresh,
        RT_ID,
        RT_Cord
    }

    public enum ResultType{
        Pass,
        Fail,
        Abort,
        Null
    }

    public enum LoadingPhase {
        NotStart,
        Reading,
        Analysing,
        Done
    }
}
