using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public interface IChipSummary {
        int TotalCount { get; }
        int RetestCount { get; }
        int FreshCount { get; }

        int NullCount { get; }
        int AbortCount { get; }
        int PassCount { get; }
        int FailCount { get; }

        Dictionary<UInt16, int> GetHardBins();
        Dictionary<UInt16, int> GetSoftBins();

    }
}
