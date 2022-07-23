using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public class Item{
        public int Idx { get; private set; }
        public string TNumber { get; private set; }

        public string TestText { get; private set; }
        public float? LoLimit { get; private set; }
        public float? HiLimit { get; private set; }
        public string Unit { get; private set; }
        public int PassCnt { get; private set; }
        public int FailCnt { get; private set; }
        public string FailPer { get; private set; }
        public float? MeanValue { get; private set; }
        public float? MedianValue { get; private set; }
        public float? MinValue { get; private set; }
        public float? MaxValue { get; private set; }
        public float? Cp { get; private set; }
        public float? Cpk { get; private set; }
        public float? Sigma { get; private set; }

        public Item(int idx, string uid, ItemInfo info, ItemStatistic statistic) {
            Idx = idx;
            TNumber = uid;

            TestText = info.TestText;
            LoLimit = info.LoLimit;
            HiLimit = info.HiLimit;
            Unit = info.Unit;
            MeanValue = statistic.MeanValue;
            MedianValue = statistic.MedianValue;
            MinValue = statistic.MinValue;
            MaxValue = statistic.MaxValue;
            Cp = statistic.Cp;
            Cpk = statistic.Cpk;
            Sigma = statistic.Sigma;
            PassCnt = statistic.PassCount;
            FailCnt = statistic.FailCount;
            FailPer = (FailCnt * 100.0 / (FailCnt + PassCnt)).ToString("f2")+"%";
        }

    }
}
