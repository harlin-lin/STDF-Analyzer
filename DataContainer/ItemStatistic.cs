﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace DataContainer {
    [Serializable]
    public class ItemStatistic {
        public float MeanValue { get; private set; }
        public float MedianValue { get; private set; }
        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }
        public float Cp { get; private set; }
        public float Cpk { get; private set; }
        public float Sigma { get; private set; }
        public int PassCount { get; private set; }
        public int FailCount { get; private set; }
        public int ValidCount { get; private set; }

        public float GetSigmaRangeLow(int times) {
            try {
                return MeanValue - Sigma * times;
            }
            catch {
                return float.NaN;
            }
        }

        public float GetSigmaRangeHigh(int times) {
            try {
                return MeanValue + Sigma * times;
            }
            catch {
                return float.NaN;
            }
        }

        public ItemStatistic(IEnumerable<float> data, float? ll, float? hl) {
            List<double> listUnNullItems = (from r in data
                                           where !float.IsNaN(r) && !float.IsInfinity(r)
                                           select (double)r).ToList();
            if (listUnNullItems.Count != 0) {

                var statistics = new DescriptiveStatistics(listUnNullItems);
                MeanValue = (float)statistics.Mean;
                MinValue = (float)statistics.Minimum;
                MaxValue = (float)statistics.Maximum;
                Sigma = (float)statistics.StandardDeviation;
                MedianValue = (float)Statistics.Median(listUnNullItems);

                if (hl != null && ll != null) {
                    var T = ((float)hl - (float)ll);
                    var U = ((float)hl + (float)ll) / 2;
                    var Ca = (MeanValue - U) / (T / 2);
                    //Cp= (Hlimit-Llimit)/(6*Sigma)
                    Cp = (float)(T / (Sigma * 6));
                    //Cpk = Cp*(1-|Ca|)
                    Cpk = Cp * (1 - Math.Abs((float)Ca));
                } else if (hl != null && ll == null) {
                    var T = ((float)hl - MeanValue);
                    Cp = (float)(T / (Sigma * 3));

                    Cpk = Cp;
                } else if (hl == null && ll != null) {
                    var T = (MeanValue - (float)ll);
                    Cp = (float)(T / (Sigma * 3));

                    Cpk = Cp;
                } else {
                    Cp = float.NaN;
                    Cpk = float.NaN;
                }
            } else {
                MeanValue = float.NaN;
                MinValue = float.NaN;
                MaxValue = float.NaN;
                Sigma = float.NaN;
                MedianValue = float.NaN;
                Cp = float.NaN;
                Cpk = float.NaN;
            }

            ValidCount = listUnNullItems.Count;
            PassCount = 0;
            FailCount = 0;

            if(!ll.HasValue && !hl.HasValue) {
                PassCount = ValidCount;
                FailCount = ValidCount - PassCount;
            } else {
                foreach(var v in listUnNullItems) {
                    if (ll.HasValue && !hl.HasValue){
                        if (v >= ll)
                            PassCount++;
                    }else if(!ll.HasValue && hl.HasValue) {
                        if (v <= hl)
                            PassCount++;
                    } else {
                        if (v >= ll && v<=hl)
                            PassCount++;
                    }
                }
                FailCount = ValidCount - PassCount;
            }


        }
    }
}
