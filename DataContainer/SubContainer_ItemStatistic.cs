using MathNet.Numerics.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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

        const float Epsilon = 1e-12f;
        const float ConsistencyFactor = 1.4826f;

        public NormalityDeviationResult GetFilteredNormalityDeviationResult(int filterId, string uid, DeviationAnalysisParams parameters = null)
        {
            if(parameters == null) parameters = new DeviationAnalysisParams();
            
            var result = new NormalityDeviationResult();

            var itemVals = GetItemVal(uid, _filterContainer[filterId]);

            // 1. 清洗数据：剔除 NaN 和无穷值
            var cleanData = itemVals.Where(d => !float.IsNaN(d) && !float.IsInfinity(d)).Select(d => d).ToArray();

            if (cleanData.Length < 4)
                return null;

            var statistic = _filterContainer[filterId].FilterItemStatistics[uid];
            var info = _itemContainer[uid];
            result.MadOutlierThRatio_Left = info.HiLimit != null ? parameters.MadOutlierThRatio_Left : parameters.MadHalfLimitThRatio;
            result.MadOutlierThRatio_Right = info.LoLimit != null ? parameters.MadOutlierThRatio_Right : parameters.MadHalfLimitThRatio;

            // 检查是否为常数数据（方差极小）
            if (statistic.Sigma < Epsilon)
            {
                // 常数数据：偏度/峰度无意义，正态性不成立
                result.IsConstantData = true;
                //result.MAD = 0;
                result.MAD_L = 0;
                result.MAD_R = 0;
                result.OutlierCount = 0;
                result.ModeCount = 1;
                result.ModeLocations.Add(statistic.MeanValue);
                return result;
            }

            // 4. 核密度估计 + 多峰检测
            //double bw = parameters.Bandwidth ?? SilvermanBandwidth(cleanData, stdDev);
            var usl = info.HiLimit ?? statistic.MeanValue + 6 * statistic.Sigma;
            var lsl = info.LoLimit ?? statistic.MeanValue - 6 * statistic.Sigma;
            var bw = parameters.BandwidthRatio * (usl - lsl);

            var (densityX, densityY) = KernelDensityEstimate(cleanData, bw, parameters.KernelSampleCount);
            var peakIndices = FindPeaks(densityY, parameters.PeakProminenceRatio);
            result.ModeCount = peakIndices.Count;
            result.ModeLocations = peakIndices.Select(i => densityX[i]).ToList();

            //计算MAD及其左右分界
            var (median, madLeft, madRight) = CalculateMAD(itemVals);
            result.MAD_L = madLeft;
            result.MAD_R = madRight;


            var madlsl = median - parameters.MadOutlierThRatio_Left * ConsistencyFactor * madLeft;
            var madusl = median + parameters.MadOutlierThRatio_Right * ConsistencyFactor * madRight;
            //var outlierCnt = itemVals.Count(v => !float.IsNaN(v) && ( float.IsInfinity(v) || v < madlsl || v > madusl));
            int outlierCnt_right = 0;
            int outlierCnt_left = 0;
            if (info.HiLimit != null)
            {
                outlierCnt_right += itemVals.Count(v => !float.IsNaN(v) && v > madusl);
            }
            
            if (info.LoLimit != null)
            {
                outlierCnt_left += itemVals.Count(v => !float.IsNaN(v) && v < madlsl);
            }

            if (peakIndices.Count == 1 && outlierCnt_left > 0)
            {
                float eps = parameters.DbscanEpsilon ?? Math.Max(1.0f * statistic.Sigma, Epsilon);
                var dboutlier_left = ExtractNoiseByDbscan(cleanData.Where(v=> v<=(madlsl+2.0*eps)).ToArray(), eps, parameters.DbscanMinPts).Count;
                outlierCnt_left = dboutlier_left < outlierCnt_left ? dboutlier_left : outlierCnt_left;
            }

            if (peakIndices.Count == 1 && outlierCnt_right > 0)
            {
                float eps = parameters.DbscanEpsilon ?? Math.Max(1.0f * statistic.Sigma, Epsilon);
                var dboutlier_right = ExtractNoiseByDbscan(cleanData.Where(v => v >= (madusl - 2.0*eps)).ToArray(), eps, parameters.DbscanMinPts).Count;
                outlierCnt_right = dboutlier_right < outlierCnt_right ? dboutlier_right : outlierCnt_right;
            }

            result.OutlierCount = outlierCnt_right + outlierCnt_left;

            return result;

        }

        private (float, float, float) CalculateMAD(IEnumerable<float> data)
        {
            var listUnNullItems = (from r in data
                                   where !float.IsNaN(r)
                                   select r);
            
            var median = Statistics.Median(listUnNullItems);

            var leftDeviations = new List<float>();
            var rightDeviations = new List<float>();

            foreach (float value in listUnNullItems)
            {
                if (value < median)
                    leftDeviations.Add(median - value);
                else if (value > median)
                    rightDeviations.Add(value - median);
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
            
            return (median, madLeft, madRight);
        }

        public (float, float, float, int) GetFilteredMAD_BySite(int filterId, string uid, byte site)
        {
            var itemVal = GetItemValBySite(uid, _filterContainer[filterId], site);
            var (median, madLeft, madRight) = CalculateMAD(itemVal);
            
            return (median, madLeft, madRight, itemVal.Count());
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
                float maxY = y.Max();
                int maxIdx = Array.IndexOf(y, maxY);
                peaks.Add(maxIdx);
            }
            return peaks;
        }

        /// <summary>
        /// 使用一维优化 DBSCAN 提取噪声点（离群点）。
        /// 自动剔除 NaN / Inf，适合大数据量。
        /// </summary>
        /// <param name="data">原始数据数组</param>
        /// <param name="eps">邻域半径</param>
        /// <param name="minPts">核心点最小邻居数</param>
        /// <returns>噪声点列表（未被任何簇包含的孤立点）</returns>
        private static List<float> ExtractNoiseByDbscan(float[] clean, float eps, int minPts)
        {
            if (clean == null || clean.Length == 0)
                return new List<float>();

            int n = clean.Length;
            if (n == 0) return new List<float>();

            // 排序并保留原始索引
            var sorted = clean
                .Select((val, idx) => (val, idx))
                .OrderBy(x => x.val)
                .ToArray();

            int[] posOfOriginalIdx = new int[n];
            for (int i = 0; i < n; i++)
                posOfOriginalIdx[sorted[i].idx] = i;

            bool[] visited = new bool[n];
            bool[] isNoise = new bool[n];   // 标记噪声，最终据此收集

            // 邻域查询（双指针，利用有序性）
            Func<int, HashSet<int>> regionQuery = pos =>
            {
                var neighbors = new HashSet<int>();
                float center = sorted[pos].val;
                float leftBound = center - eps;
                float rightBound = center + eps;

                int left = pos;
                while (left >= 0 && sorted[left].val >= leftBound)
                {
                    neighbors.Add(sorted[left].idx);
                    left--;
                }
                int right = pos + 1;
                while (right < n && sorted[right].val <= rightBound)
                {
                    neighbors.Add(sorted[right].idx);
                    right++;
                }
                return neighbors;
            };

            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;
                visited[i] = true;

                var neighbors = regionQuery(posOfOriginalIdx[i]);
                if (neighbors.Count < minPts)
                {
                    isNoise[i] = true;           // 暂标为噪声
                } else
                {
                    var queue = new Queue<int>(neighbors);
                    while (queue.Count > 0)
                    {
                        int nb = queue.Dequeue();
                        if (!visited[nb])
                        {
                            visited[nb] = true;
                            var nbNeighbors = regionQuery(posOfOriginalIdx[nb]);
                            if (nbNeighbors.Count >= minPts)
                            {
                                foreach (var nn in nbNeighbors)
                                {
                                    if (neighbors.Add(nn))
                                        queue.Enqueue(nn);
                                }
                            }
                        }

                        if (isNoise[nb])
                        {
                            isNoise[nb] = false;   // 吸收到簇中，不再是噪声
                        }
                    }
                }
            }

            // 一次性收集所有仍为噪声的点
            var noiseList = new List<float>();
            for (int i = 0; i < n; i++)
            {
                if (isNoise[i])
                    noiseList.Add(clean[i]);
            }
            return noiseList;
        }

    }
}
