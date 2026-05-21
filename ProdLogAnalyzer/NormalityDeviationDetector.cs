using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using Statistics = MathNet.Numerics.Statistics.Statistics;

namespace ProdLogAnalyzer
{
    /// <summary>
    /// 正态偏离分析结果
    /// </summary>
    public class NormalityDeviationResult
    {
        public double Mean { get; set; }
        public double StdDev { get; set; }
        public double Skewness { get; set; }
        public double ExcessKurtosis { get; set; }

        public double JarqueBeraStatistic { get; set; }
        public double JarqueBeraPValue { get; set; }
        public bool IsNormal { get; set; }

        public int ModeCount { get; set; }
        public List<double> ModeLocations { get; set; } = new List<double>();
        public List<List<double>> Clusters { get; set; } = new List<List<double>>();
        public List<int> ClusterSizes { get; set; } = new List<int>();
        public List<double> Outliers { get; set; } = new List<double>();

        /// <summary>
        /// 当数据为常数时，此标记为 true
        /// </summary>
        public bool IsConstantData { get; set; }
        public double Cpk { get; set; } = double.NaN;      // 新增
    }

    /// <summary>
    /// 分析参数
    /// </summary>
    public class DeviationAnalysisParams
    {
        /// <summary>正态检验显著性水平</summary>
        public double SignificanceLevel { get; set; } = 0.05;

        /// <summary>核密度带宽，null 则用 BandwidthRatio * (USL-LSL) 准则自动计算</summary>
        public double BandwidthRatio { get; set; } = 0.05;

        /// <summary>密度估计采样点数</summary>
        public int KernelSampleCount { get; set; } = 512;

        /// <summary>峰的相对显著度（峰高与邻近谷底的最小比值）</summary>
        public double PeakProminenceRatio { get; set; } = 0.5;

        /// <summary>DBSCAN 邻域半径，null 则自动取 0.5*StdDev</summary>
        public double? DbscanEpsilon { get; set; } = null;

        /// <summary>DBSCAN 核心点最小邻居数</summary>
        public int DbscanMinPts { get; set; } = 5;

        // 新增规格限
        public double? UpperSpecLimit { get; set; } = null;
        public double? LowerSpecLimit { get; set; } = null;

    }

    /// <summary>
    /// 正态偏离检测器（基于 MathNet.Numerics）
    /// </summary>
    public class NormalityDeviationDetector
    {
        private const double Epsilon = 1e-12;

        /// <summary>
        /// 执行分析，对各类异常输入具有高容错性
        /// </summary>
        public static NormalityDeviationResult Analyze(IEnumerable<float> rawData, DeviationAnalysisParams parameters = null)
        {
            if (rawData == null)
                throw new ArgumentNullException(nameof(rawData));

            // 1. 清洗数据：剔除 NaN 和无穷值
            var cleanData = rawData.Where(d => !float.IsNaN(d) && !float.IsInfinity(d)).Select(d => (double)d).ToArray();
            if (cleanData.Length < 4)
                throw new ArgumentException($"有效数据点不足（需要至少4个，当前 {cleanData.Length} 个）。");

            if (parameters == null) parameters = new DeviationAnalysisParams();
            var result = new NormalityDeviationResult();

            // 2. 基本统计量（利用 MathNet.Numerics）
            double mean = Statistics.Mean(cleanData);
            double stdDev = Statistics.StandardDeviation(cleanData);
            result.Mean = mean;
            result.StdDev = stdDev;

            // 检查是否为常数数据（方差极小）
            if (stdDev < Epsilon)
            {
                // 常数数据：偏度/峰度无意义，正态性不成立
                result.IsConstantData = true;
                result.Skewness = 0;
                result.ExcessKurtosis = 0;
                result.JarqueBeraStatistic = double.NaN;
                result.JarqueBeraPValue = 1.0;     // 形式上不拒绝，但从定义看不是正态
                result.IsNormal = false;
                result.ModeCount = 1;
                result.ModeLocations.Add(mean);
                result.Clusters.Add(new List<double>(cleanData));
                result.ClusterSizes.Add(cleanData.Length);
                result.Outliers.Clear();
                return result;
            }

            // 偏度与超值峰度（MathNet 的 Kurtosis 直接是 excess kurtosis）
            double skewness = Statistics.Skewness(cleanData);
            double excessKurtosis = Statistics.Kurtosis(cleanData);
            result.Skewness = skewness;
            result.ExcessKurtosis = excessKurtosis;

            // 3. Jarque–Bera 正态性检验（p值使用精确 χ²(2) 分布）
            int n = cleanData.Length;
            double jb = n / 6.0 * (skewness * skewness + excessKurtosis * excessKurtosis / 4.0);
            result.JarqueBeraStatistic = jb;
            var chiSquare = new ChiSquared(2);
            result.JarqueBeraPValue = 1.0 - chiSquare.CumulativeDistribution(jb);
            result.IsNormal = result.JarqueBeraPValue >= parameters.SignificanceLevel;

            // 4. 核密度估计 + 多峰检测
            //double bw = parameters.Bandwidth ?? SilvermanBandwidth(cleanData, stdDev);
            var usl = parameters.UpperSpecLimit ?? mean + 6 * stdDev;
            var lsl = parameters.LowerSpecLimit ?? mean - 6 * stdDev;
            double bw = parameters.BandwidthRatio * (usl - lsl);

            var (densityX, densityY) = KernelDensityEstimate(cleanData, bw, parameters.KernelSampleCount);
            var peakIndices = FindPeaks(densityY, parameters.PeakProminenceRatio);
            result.ModeCount = peakIndices.Count;
            result.ModeLocations = peakIndices.Select(i => densityX[i]).ToList();

            // 5. 根据密度谷底划分聚落
            if (result.ModeCount > 1)
            {
                var valleys = FindValleys(densityY, peakIndices);
                result.Clusters = AssignToClustersByDensityValleys(cleanData, densityX, valleys);
            } else
            {
                result.Clusters.Add(new List<double>(cleanData));
            }
            result.ClusterSizes = result.Clusters.Select(c => c.Count).ToList();

            // 6. DBSCAN 识别离群点及微小聚落
            bool dbScanNeeded = false;
            if (parameters.UpperSpecLimit.HasValue && parameters.UpperSpecLimit.Value > (mean + parameters.DbscanEpsilon * stdDev))
            {
                dbScanNeeded = true;
            }
            if (parameters.LowerSpecLimit.HasValue && parameters.LowerSpecLimit.Value < (mean - parameters.DbscanEpsilon * stdDev))
            {
                dbScanNeeded = true;
            }

            if (dbScanNeeded)
            {
                double eps = parameters.DbscanEpsilon ?? Math.Max(0.5 * stdDev, Epsilon);
                //var (dbClusters, noise) = Dbscan(cleanData, eps, parameters.DbscanMinPts);
                var (dbClusters, noise) = DbscanOptimized(cleanData, eps, parameters.DbscanMinPts);
                result.Outliers = noise;
            } else
            {
                Console.WriteLine("DBSCAN 识别离群点被跳过。");
                result.Outliers.Clear();
            }

            // ========== CPK 计算（新增）==========
            double? cpk = null;
            double? cpu = null; if (parameters.UpperSpecLimit.HasValue) cpu = (parameters.UpperSpecLimit.Value - mean) / (3.0 * stdDev);
            double? cpl = null; if (parameters.LowerSpecLimit.HasValue) cpl = (mean - parameters.LowerSpecLimit.Value) / (3.0 * stdDev);
            if (cpu.HasValue && cpl.HasValue)
                cpk = Math.Min(cpu.Value, cpl.Value);
            else if (cpu.HasValue)
                cpk = cpu.Value;
            else if (cpl.HasValue)
                cpk = cpl.Value;
            result.Cpk = cpk ?? double.NaN;


            return result;
        }

        // ---------- 内部方法（仅 KDE、多峰、DBSCAN 需自实现）----------

        /// <summary>
        /// Silverman 带宽计算（鲁棒处理方差/分位数为零）
        /// </summary>
        private static double SilvermanBandwidth(double[] data, double stdDev)
        {
            double iqr = data.Percentile(75) - data.Percentile(25);   // 利用 MathNet 扩展
            double a = Math.Min(stdDev, iqr / 1.34);
            double h = 0.9 * a * Math.Pow(data.Length, -0.2);

            // 防止零带宽（数据过于集中）
            if (h < Epsilon)
            {
                double range = data.Max() - data.Min();
                h = Math.Max(range / 4.0 * Math.Pow(data.Length, -0.2), Epsilon);
            }
            return h;
        }

        private static (double[] x, double[] y) KernelDensityEstimate(double[] data, double bandwidth, int sampleCount)
        {
            double min = data.Min() - 3 * bandwidth;
            double max = data.Max() + 3 * bandwidth;
            double[] x = new double[sampleCount];
            double[] y = new double[sampleCount];
            double n = data.Length;
            double coef = 1.0 / (n * bandwidth * Math.Sqrt(2 * Math.PI));

            for (int i = 0; i < sampleCount; i++)
            {
                x[i] = min + (max - min) * i / (sampleCount - 1);
                double sum = 0.0;
                foreach (double d in data)
                {
                    double z = (x[i] - d) / bandwidth;
                    sum += Math.Exp(-0.5 * z * z);
                }
                y[i] = coef * sum;
            }
            return (x, y);
        }

        private static List<int> FindPeaks(double[] y, double prominenceRatio)
        {
            var peaks = new List<int>();
            for (int i = 1; i < y.Length - 1; i++)
            {
                if (y[i] > y[i - 1] && y[i] > y[i + 1])
                {
                    // 简单显著度检查：与左右最近谷底比较
                    double leftValley = double.MaxValue;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (j == 0 || y[j] < y[j - 1]) { leftValley = y[j]; break; }
                    }
                    double rightValley = double.MaxValue;
                    for (int j = i + 1; j < y.Length; j++)
                    {
                        if (j == y.Length - 1 || y[j] < y[j + 1]) { rightValley = y[j]; break; }
                    }
                    double highestValley = Math.Max(leftValley, rightValley);
                    if (y[i] > highestValley * (1.0 + prominenceRatio))
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

        private static List<int> FindValleys(double[] y, List<int> peakIndices)
        {
            var valleys = new List<int>();
            for (int p = 0; p < peakIndices.Count - 1; p++)
            {
                int start = peakIndices[p];
                int end = peakIndices[p + 1];
                int minIdx = start;
                double minVal = y[start];
                for (int i = start + 1; i <= end; i++)
                {
                    if (y[i] < minVal)
                    {
                        minVal = y[i];
                        minIdx = i;
                    }
                }
                valleys.Add(minIdx);
            }
            return valleys;
        }

        private static List<List<double>> AssignToClustersByDensityValleys(
            double[] data, double[] densityX, List<int> valleyIndices)
        {
            var thresholds = valleyIndices.Select(i => densityX[i]).ToList();
            var sortedData = data.OrderBy(x => x).ToList();
            var clusters = new List<List<double>>();
            int startIdx = 0;

            foreach (double thresh in thresholds)
            {
                var cluster = new List<double>();
                while (startIdx < sortedData.Count && sortedData[startIdx] <= thresh)
                    cluster.Add(sortedData[startIdx++]);
                if (cluster.Count > 0)
                    clusters.Add(cluster);
            }

            // 剩余点归入最后一簇
            var lastCluster = new List<double>();
            while (startIdx < sortedData.Count)
                lastCluster.Add(sortedData[startIdx++]);
            if (lastCluster.Count > 0)
                clusters.Add(lastCluster);

            return clusters;
        }

        // ---------- DBSCAN (一维) ----------
        private static (List<List<double>> Clusters, List<double> Noise) Dbscan(
            double[] data, double eps, int minPts)
        {
            int n = data.Length;
            bool[] visited = new bool[n];
            bool[] isNoise = new bool[n];
            var clusters = new List<List<double>>();
            var noiseList = new List<double>();

            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;
                visited[i] = true;

                var neighbors = RegionQuery(data, i, eps);
                if (neighbors.Count < minPts)
                {
                    isNoise[i] = true;
                    noiseList.Add(data[i]);
                } else
                {
                    var cluster = new List<double>();
                    clusters.Add(cluster);
                    ExpandCluster(data, i, neighbors, cluster, visited, isNoise, noiseList, eps, minPts);
                }
            }

            // 清除噪声列表中后来被归入簇的点
            noiseList = noiseList.Where(p => !clusters.Any(c => c.Contains(p))).ToList();
            return (clusters, noiseList);
        }

        private static void ExpandCluster(double[] data, int pointIdx, List<int> neighbors,
            List<double> cluster, bool[] visited, bool[] isNoise, List<double> noiseList,
            double eps, int minPts)
        {
            cluster.Add(data[pointIdx]);
            for (int i = 0; i < neighbors.Count; i++)
            {
                int nb = neighbors[i];
                if (!visited[nb])
                {
                    visited[nb] = true;
                    var nbNeighbors = RegionQuery(data, nb, eps);
                    if (nbNeighbors.Count >= minPts)
                    {
                        foreach (var nn in nbNeighbors)
                            if (!neighbors.Contains(nn))
                                neighbors.Add(nn);
                    }
                }
                if (isNoise[nb])
                {
                    isNoise[nb] = false;
                    cluster.Add(data[nb]);
                    noiseList.Remove(data[nb]);
                } else if (!cluster.Contains(data[nb]))
                {
                    cluster.Add(data[nb]);
                }
            }
        }

        private static List<int> RegionQuery(double[] data, int pointIdx, double eps)
        {
            var neighbors = new List<int>();
            double value = data[pointIdx];
            for (int i = 0; i < data.Length; i++)
            {
                if (Math.Abs(data[i] - value) <= eps)
                    neighbors.Add(i);
            }
            return neighbors;
        }

        private static (List<List<double>> Clusters, List<double> Noise) DbscanOptimized(
            double[] data, double eps, int minPts)
        {
            int n = data.Length;
            // 带着原始索引排序，方便回溯
            var sorted = data
                .Select((val, idx) => (val, idx))
                .OrderBy(x => x.val)
                .ToArray();

            bool[] visited = new bool[n];
            bool[] isNoise = new bool[n];
            var clusters = new List<List<double>>();
            var noiseList = new List<double>();

            // 预计算每个点在排序数组中的位置，便于从原始索引映射到排序索引
            int[] posOfOriginalIdx = new int[n];
            for (int i = 0; i < n; i++)
                posOfOriginalIdx[sorted[i].idx] = i;

            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;
                visited[i] = true;

                var neighbors = SortedRegionQuery(sorted, posOfOriginalIdx[i], eps);
                if (neighbors.Count < minPts)
                {
                    isNoise[i] = true;
                    noiseList.Add(data[i]);
                } else
                {
                    var cluster = new List<double>();
                    clusters.Add(cluster);
                    ExpandClusterOptimized(data, sorted, posOfOriginalIdx, i, neighbors, cluster,
                                           visited, isNoise, noiseList, eps, minPts);
                }
            }

            // 清理噪声列表中已被归簇的点
            noiseList = noiseList.Where(p => !clusters.Any(c => c.Contains(p))).ToList();
            return (clusters, noiseList);
        }

        // 在有序数组中，以 sorted[pos].val 为中心，查找 [val-eps, val+eps] 范围内的所有点
        private static HashSet<int> SortedRegionQuery(
            (double val, int idx)[] sorted, int pos, double eps)
        {
            var neighbors = new HashSet<int>();
            double center = sorted[pos].val;
            double leftBound = center - eps;
            double rightBound = center + eps;

            int left = pos;
            while (left >= 0 && sorted[left].val >= leftBound)
            {
                neighbors.Add(sorted[left].idx);
                left--;
            }

            int right = pos + 1;
            while (right < sorted.Length && sorted[right].val <= rightBound)
            {
                neighbors.Add(sorted[right].idx);
                right++;
            }
            return neighbors;
        }

        private static void ExpandClusterOptimized(
            double[] data,
            (double val, int idx)[] sorted,
            int[] posOfOriginalIdx,
            int pointIdx,
            HashSet<int> neighbors,
            List<double> cluster,
            bool[] visited,
            bool[] isNoise,
            List<double> noiseList,
            double eps,
            int minPts)
        {
            cluster.Add(data[pointIdx]);

            // 遍历时邻居集合可能会动态增长，使用队列或复制遍历
            var toProcess = new Queue<int>(neighbors);
            while (toProcess.Count > 0)
            {
                int nb = toProcess.Dequeue();
                if (!visited[nb])
                {
                    visited[nb] = true;
                    var nbNeighbors = SortedRegionQuery(sorted, posOfOriginalIdx[nb], eps);
                    if (nbNeighbors.Count >= minPts)
                    {
                        foreach (var nn in nbNeighbors)
                        {
                            if (neighbors.Add(nn))   // Add 返回 true 表示是新元素
                            {
                                toProcess.Enqueue(nn);
                            }
                        }
                    }
                }

                if (isNoise[nb])
                {
                    isNoise[nb] = false;
                    cluster.Add(data[nb]);
                    noiseList.Remove(data[nb]);
                } else if (!cluster.Contains(data[nb]))
                {
                    cluster.Add(data[nb]);
                }
            }
        }
    }
}