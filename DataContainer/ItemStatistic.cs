using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace DataContainer {
    public class NormalityDeviationResult
    {
        public int ModeCount { get; set; } = 0;
        public List<float> ModeLocations { get; set; } = new List<float>();
        public int OutlierCount { get; set; } = 0;
        //public float MAD { get; set; } = float.NaN;
        public float MAD_L { get; set; } = float.NaN;
        public float MAD_R { get; set; } = float.NaN;
        public float MadOutlierThRatio_Left { get; set; }
        public float MadOutlierThRatio_Right { get; set; }
        public bool IsConstantData { get; set; } = false; // 方差极小，数据近似常数
    }
    /// <summary>
    /// 分析参数
    /// </summary>
    public class DeviationAnalysisParams
    {
        /// <summary>核密度带宽，null 则用 BandwidthRatio * (USL-LSL) 准则自动计算</summary>
        public float BandwidthRatio { get; set; } = 0.05f;

        /// <summary>密度估计采样点数</summary>
        public int KernelSampleCount { get; set; } = 512;

        /// <summary>峰的相对显著度（峰高与邻近谷底的最小比值）</summary>
        public float PeakProminenceRatio { get; set; } = 0.5f;

        public float MadOutlierThRatio_Left { get; set; } = 4.0f;
        public float MadOutlierThRatio_Right { get; set; } = 4.0f;
        public float MadHalfLimitThRatio { get; set; } = 8.0f;
    }


    [Serializable]
    public class ItemStatistic {
        const double Epsilon = 1e-12;

        public float MeanValue { get; private set; } = float.NaN;
        public float MinValue { get; private set; } = float.NaN;
        public float MaxValue { get; private set; } = float.NaN;
        public float MedianValue { get; private set; } = float.NaN;
        public float Skewness { get; private set; } = float.NaN;
        public float Kurtosis { get; private set; } = float.NaN;

        public float Cp { get; private set; } = float.NaN;
        public float Cpk { get; private set; } = float.NaN;
        public float Sigma { get; private set; } = float.NaN;
        
        public int PassCount { get; private set; }
        public int FailCount { get; private set; }
        public int ValidCount { get; private set; }
        public float PassRate { get; private set; } //PassCount/validCount(invalid test result would make this value wrong)

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

            // 检查是否为常数数据（方差极小）
            if (Sigma < Epsilon)
            {
                Skewness = 0;
                Kurtosis = 0;
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
