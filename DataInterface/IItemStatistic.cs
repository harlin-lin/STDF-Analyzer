using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public interface IItemStatistic {
        float? MeanValue { get; }
        float? MinValue { get; }
        float? MaxValue { get; }
        float? Cp { get; }
        float? Cpk { get; }
        float? Sigma { get; }
        int PassCount { get; }
        int FailCount { get; }
    }
}
