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
        double? Cp { get; }
        double? Cpk { get; }
        double? Sigma { get; }
        int PassCount { get; }
        int FailCount { get; }
    }
}
