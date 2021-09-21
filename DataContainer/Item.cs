﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public class Item{
        public string TestNumber { get; private set; }

        public string TestText { get; private set; }
        public float? LoLimit { get; private set; }
        public float? HiLimit { get; private set; }
        public string Unit { get; private set; }
        public float? MeanValue { get; private set; }
        public float? MinValue { get; private set; }
        public float? MaxValue { get; private set; }
        public float? Cp { get; private set; }
        public float? Cpk { get; private set; }
        public float? Sigma { get; private set; }
        public int PassCount { get; private set; }
        public int FailCount { get; private set; }

        public Item(string uid, ItemInfo info, ItemStatistic statistic) {
            TestNumber = uid;

            TestText = info.TestText;
            LoLimit = info.LoLimit;
            HiLimit = info.HiLimit;
            Unit = info.Unit;
            MeanValue = statistic.MeanValue;
            MinValue = statistic.MinValue;
            MaxValue = statistic.MaxValue;
            Cp = statistic.Cp;
            Cpk = statistic.Cpk;
            Sigma = statistic.Sigma;
            PassCount = statistic.PassCount;
            FailCount = statistic.FailCount;
        }

    }
}
