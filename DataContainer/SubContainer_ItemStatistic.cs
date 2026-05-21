using MathNet.Numerics.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public partial class SubContainer {
        private ConcurrentDictionary<string, ItemStatistic> _itemStatistics;


        private void Initialize_ItemStatistic() {
            //_itemStatistics = new ConcurrentDictionary<string, ItemStatistic>(from r in _itemContainer
            //                                                                  let v = new KeyValuePair<string, ItemStatistic>(r.Key, null)
            //                                                                  select v);
        }

        private void AnalyseItems() {
            _itemStatistics = new ConcurrentDictionary<string, ItemStatistic>(from r in _itemContainer
                                                                              let v = new KeyValuePair<string, ItemStatistic>(r.Key, null)
                                                                              select v);

            Parallel.For(0, _itemStatistics.Count, (x) => {
                var key = _itemStatistics.ElementAt((int)x).Key;
                _itemStatistics[key] = new ItemStatistic(GetItemVal(key), _itemContainer[key].LoLimit, _itemContainer[key].HiLimit);
            });
        }

        private void AnalyseItems_Filtered(Filter filter) {
            filter.FilterItemStatistics = new ConcurrentDictionary<string, ItemStatistic>(from i in Enumerable.Range(0, _itemContainer.Count)
                                                                                          let v = new KeyValuePair<string, ItemStatistic>(_itemContainer.ElementAt(i).Key, null)
                                                                                          select v);

            Parallel.For(0, filter.FilterItemStatistics.Count, (x) => {
                var key = filter.FilterItemStatistics.ElementAt((int)x).Key;
                filter.FilterItemStatistics[key] = new ItemStatistic(GetItemVal(key, filter), _itemContainer[key].LoLimit, _itemContainer[key].HiLimit);
            });
        }

        const double Epsilon = 1e-12;
        const float ConsistencyFactor = 1.4826f;

        public NormalityDeviationResult GetFilteredNormalityDeviationResult(int filterId, string uid, DeviationAnalysisParams parameters = null)
        {
            if(parameters == null) parameters = new DeviationAnalysisParams();
            
            var result = new NormalityDeviationResult();

            // 1. 清洗数据：剔除 NaN 和无穷值
            var cleanData = GetItemVal(uid, _filterContainer[filterId]).Where(d => !float.IsNaN(d) && !float.IsInfinity(d)).Select(d => d).ToArray();

            if (cleanData.Length < 4)
                return null;

            if (parameters == null) parameters = new DeviationAnalysisParams();

            var statistic = _filterContainer[filterId].FilterItemStatistics[uid];
            var info = _itemContainer[uid];

            // 检查是否为常数数据（方差极小）
            if (statistic.Sigma < Epsilon)
            {
                // 常数数据：偏度/峰度无意义，正态性不成立
                result.IsConstantData = true;
                result.MAD = 0;
                result.MAD_L = 0;
                result.MAD_R = 0;
                result.OutlierCount = 0;
                result.ModeCount = 1;
                result.ModeLocations.Add(statistic.MeanValue);
                return result;
            }

            var mad = (float)Statistics.Median(cleanData.Select(v => Math.Abs(v - statistic.MeanValue)));
            result.MAD = mad;

            var leftDeviations = new List<float>();
            var rightDeviations = new List<float>();

            foreach (float value in cleanData)
            {
                if (value < statistic.MedianValue)
                    leftDeviations.Add(statistic.MedianValue - value);
                else if (value > statistic.MedianValue)
                    rightDeviations.Add(value - statistic.MedianValue);
                // 与中位数相等的值不参与偏差计算，避免人为压低MAD
            }

            // 处理极端情况：一侧无数据时，使用另一侧的MAD作为对称边界（或设为无穷）
            float madLeft = 0.0f;
            float madRight = 0.0f;

            if (leftDeviations.Count > 0)
                madLeft = Statistics.Median(leftDeviations);
            else
                madLeft = (rightDeviations.Count > 0) ? Statistics.Median(rightDeviations) : 1.0f;

            if (rightDeviations.Count > 0)
                madRight = Statistics.Median(rightDeviations);
            else
                madRight = (leftDeviations.Count > 0) ? Statistics.Median(leftDeviations) : 1.0f;

            result.MAD_L = madLeft;
            result.MAD_R = madRight;

            // 4. 核密度估计 + 多峰检测
            //double bw = parameters.Bandwidth ?? SilvermanBandwidth(cleanData, stdDev);
            var usl = info.HiLimit ?? statistic.MeanValue + 6 * statistic.Sigma;
            var lsl = info.LoLimit ?? statistic.MeanValue - 6 * statistic.Sigma;
            var bw = parameters.BandwidthRatio * (usl - lsl);

            var (densityX, densityY) = KernelDensityEstimate(cleanData, bw, parameters.KernelSampleCount);
            var peakIndices = FindPeaks(densityY, parameters.PeakProminenceRatio);
            result.ModeCount = peakIndices.Count;
            result.ModeLocations = peakIndices.Select(i => densityX[i]).ToList();

            var listUnNullItems = (from r in GetItemVal(uid, _filterContainer[filterId])
                                   where !float.IsNaN(r)
                                   select r);

            var madlsl = statistic.MedianValue - parameters.MadOutlierRatio * ConsistencyFactor * madLeft;
            var madusl = statistic.MedianValue + parameters.MadOutlierRatio * ConsistencyFactor * madRight;
            var outlierCnt = listUnNullItems.Count(v => float.IsInfinity(v) || v < madlsl || v > madusl);


            return result;

        }

        private static (float[] x, float[] y) KernelDensityEstimate(float[] data, float bandwidth, int sampleCount)
        {
            float min = data.Min() - 3 * bandwidth;
            float max = data.Max() + 3 * bandwidth;
            float[] x = new float[sampleCount];
            float[] y = new float[sampleCount];
            float n = data.Length;
            float coef = 1.0f / (n * bandwidth * (float)Math.Sqrt(2 * Math.PI));

            for (int i = 0; i < sampleCount; i++)
            {
                x[i] = min + (max - min) * i / (sampleCount - 1);
                float sum = 0.0f;
                foreach (float d in data)
                {
                    float z = (x[i] - d) / bandwidth;
                    sum += (float)Math.Exp(-0.5 * z * z);
                }
                y[i] = coef * sum;
            }
            return (x, y);
        }

        private static List<int> FindPeaks(float[] y, float prominenceRatio)
        {
            var peaks = new List<int>();
            for (int i = 1; i < y.Length - 1; i++)
            {
                if (y[i] > y[i - 1] && y[i] > y[i + 1])
                {
                    // 简单显著度检查：与左右最近谷底比较
                    float leftValley = float.MaxValue;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (j == 0 || y[j] < y[j - 1]) { leftValley = y[j]; break; }
                    }
                    float rightValley = float.MaxValue;
                    for (int j = i + 1; j < y.Length; j++)
                    {
                        if (j == y.Length - 1 || y[j] < y[j + 1]) { rightValley = y[j]; break; }
                    }
                    float highestValley = Math.Max(leftValley, rightValley);
                    if (y[i] > highestValley * (1.0f + prominenceRatio))
                        peaks.Add(i);
                }
            }

            // 若未发现任何峰，至少保留全局最高点
            if (peaks.Count == 0)
            {
                double maxY = y.Max();
                int maxIdx = Array.IndexOf(y, maxY);
                peaks.Add(maxIdx);
            }
            return peaks;
        }



    }
}
