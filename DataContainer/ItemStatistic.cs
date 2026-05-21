using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace DataContainer {
    [Serializable]
    public class ItemStatistic {
        public float MeanValue { get; private set; } = float.NaN;
        public float MinValue { get; private set; } = float.NaN;
        public float MaxValue { get; private set; } = float.NaN;
        public float MedianValue { get; private set; } = float.NaN;
        public float Skewness { get; private set; } = float.NaN;
        public float Kurtosis { get; private set; } = float.NaN;
        //public float Q1 { get; private set; } = float.NaN;
        //public float Q3 { get; private set; } = float.NaN;
        public float MAD { get; private set; } = float.NaN;

        public float Cp { get; private set; } = float.NaN;
        public float Cpk { get; private set; } = float.NaN;
        public float Sigma { get; private set; } = float.NaN;
        
        public int PassCount { get; private set; }
        public int FailCount { get; private set; }
        public int ValidCount { get; private set; }
        public float PassRate { get; private set; } //PassCount/validCount(invalid test result would make this value wrong)
        //public int OutlierCount { get; private set; }

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
            if (listUnNullItems.Count > 0) {

                var statistics = new DescriptiveStatistics(listUnNullItems);
                MeanValue = (float)statistics.Mean;
                MinValue = (float)statistics.Minimum;
                MaxValue = (float)statistics.Maximum;
                MedianValue = (float)Statistics.Median(listUnNullItems);
                //Q1 = (float)Statistics.LowerQuartile(listUnNullItems);
                //Q3 = (float)Statistics.UpperQuartile(listUnNullItems);
                //var lsl = Q1 - 1.5 * (Q3 - Q1);
                //var usl = Q3 + 1.5 * (Q3 - Q1);
                //OutlierCount = listUnNullItems.Count(v => v < lsl || v > usl);

                if (listUnNullItems.Count > 1)
                {
                    Sigma = (float)statistics.StandardDeviation;
                }

                if (listUnNullItems.Count > 2)
                {
                    Skewness = (float)statistics.Skewness;
                }

                if (listUnNullItems.Count > 3)
                {
                    MAD = (float)Statistics.Median(listUnNullItems.Select(v => Math.Abs(v - MeanValue)));

                    Kurtosis = (float)statistics.Kurtosis;

                    if (hl != null && ll != null)
                    {
                        var T = ((float)hl - (float)ll);
                        var U = ((float)hl + (float)ll) / 2;
                        var Ca = (MeanValue - U) / (T / 2);
                        //Cp= (Hlimit-Llimit)/(6*Sigma)
                        Cp = (float)(T / (Sigma * 6));
                        //Cpk = Cp*(1-|Ca|)
                        Cpk = Cp * (1 - Math.Abs((float)Ca));
                    } else if (hl != null && ll == null)
                    {
                        var T = ((float)hl - MeanValue);
                        Cp = (float)(T / (Sigma * 3));

                        Cpk = Cp;
                    } else if (hl == null && ll != null)
                    {
                        var T = (MeanValue - (float)ll);
                        Cp = (float)(T / (Sigma * 3));

                        Cpk = Cp;
                    } else
                    {
                        Cp = float.NaN;
                        Cpk = float.NaN;
                    }
                }
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

            PassRate = ValidCount > 0 ? (float)PassCount / ValidCount : 0.0f;
        }
    }
}
