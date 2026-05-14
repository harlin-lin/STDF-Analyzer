using System;
using System.Collections.Generic;
using System.Linq;

namespace ProdLogAnalyzer
{
    /// <summary>
    /// 统计数据分析类
    /// </summary>
    public class StatisticsAnalyzer
    {
        /// <summary>
        /// 数据统计信息
        /// </summary>
        public class StatisticResult
        {
            // 基本统计指标
            public int TotalCount { get; set; }
            public float Mean { get; set; }
            public float Median { get; set; }
            public float StandardDeviation { get; set; }
            public float Min { get; set; }
            public float Max { get; set; }
            public float Range { get; set; }

            // 过程能力指数
            public float Cp { get; set; }
            public float Cpk { get; set; }
            public float Pp { get; set; }
            public float Ppk { get; set; }

            // 良率与失效
            public float PassRate { get; set; }
            public float FailRate { get; set; }
            public int FailCount { get; set; }
            public int OutlierCount { get; set; }

            // 分布信息
            public float SkewFactor { get; set; }
            public float KurtosisFactor { get; set; }
            public float QuartileOne { get; set; }
            public float QuartileThree { get; set; }

            // 异常标记
            public bool HasInfinityValues { get; set; }
            public bool HasOutliers { get; set; }
            public bool LimitLikelyTooWide { get; set; }
            public bool LimitLikelyTooNarrow { get; set; }
        }

        /// <summary>
        /// 异常分析结果
        /// </summary>
        public class AnomalyAnalysis
        {
            public bool IsAnomaly { get; set; }
            public List<string> AnomalyReasons { get; set; } = new List<string>();
            public float ExpectedPassRate { get; set; }
            public float ActualPassRate { get; set; }
            public float PassRateDifference { get; set; }
            public string Severity { get; set; } // Low, Medium, High, Critical
        }

        /// <summary>
        /// 计算数据统计信息
        /// </summary>
        public static StatisticResult CalculateStatistics(IEnumerable<float> data)
        {
            var result = new StatisticResult();
            var validData = data.Where(d => !float.IsInfinity(d) && !float.IsNaN(d)).ToList();

            if (validData.Count == 0)
            {
                result.TotalCount = 0;
                return result;
            }

            result.TotalCount = validData.Count;
            result.Mean = validData.Average();
            result.Min = validData.Min();
            result.Max = validData.Max();
            result.Range = result.Max - result.Min;

            // 计算中位数
            result.Median = CalculateMedian(validData);

            // 计算标准差
            result.StandardDeviation = CalculateStandardDeviation(validData, result.Mean);

            // 计算四分位数
            result.QuartileOne = CalculatePercentile(validData, 0.25f);
            result.QuartileThree = CalculatePercentile(validData, 0.75f);

            // 计算偏度和峰度
            result.SkewFactor = CalculateSkewness(validData, result.Mean, result.StandardDeviation);
            result.KurtosisFactor = CalculateKurtosis(validData, result.Mean, result.StandardDeviation);

            // 检查异常值（使用IQR方法）
            var outliers = DetectOutliers(validData, result.QuartileOne, result.QuartileThree);
            result.OutlierCount = outliers.Count;

            return result;
        }

        /// <summary>
        /// 计算带限制条件的统计信息和过程能力指数
        /// </summary>
        public static StatisticResult CalculateStatisticsWithLimit(IEnumerable<float> data, float? loLimit, float? hiLimit)
        {
            var result = CalculateStatistics(data);
            var validData = data.Where(d => !float.IsInfinity(d) && !float.IsNaN(d)).ToList();

            if (validData.Count == 0 || (loLimit == null && hiLimit == null))
            {
                return result;
            }

            // 计算Cp和Cpk（单侧或双侧）
            if (loLimit.HasValue && hiLimit.HasValue)
            {
                result.Cp = CalculateCp(result.StandardDeviation, loLimit.Value, hiLimit.Value);
                result.Cpk = CalculateCpk(result.Mean, result.StandardDeviation, loLimit.Value, hiLimit.Value);
            }
            else if (hiLimit.HasValue)
            {
                result.Cpk = (hiLimit.Value - result.Mean) / (3 * result.StandardDeviation);
            }
            else if (loLimit.HasValue)
            {
                result.Cpk = (result.Mean - loLimit.Value) / (3 * result.StandardDeviation);
            }

            // 计算Pp和Ppk（总体）
            var variance = validData.Sum(x => (x - result.Mean) * (x - result.Mean)) / validData.Count;
            var ppSigma = (float)Math.Sqrt(variance);
            if (loLimit.HasValue && hiLimit.HasValue)
            {
                result.Pp = (hiLimit.Value - loLimit.Value) / (6 * ppSigma);
                result.Ppk = Math.Min(
                    (hiLimit.Value - result.Mean) / (3 * ppSigma),
                    (result.Mean - loLimit.Value) / (3 * ppSigma));
            }

            // 计算良率
            var passCount = validData.Count(x => (!loLimit.HasValue || x >= loLimit.Value) && 
                                                   (!hiLimit.HasValue || x <= hiLimit.Value));
            result.FailCount = validData.Count - passCount;
            result.PassRate = (float)passCount / validData.Count;
            result.FailRate = 1 - result.PassRate;

            // 检查无穷值
            result.HasInfinityValues = data.Any(d => float.IsInfinity(d));

            return result;
        }

        /// <summary>
        /// 数据清洗：使用修正Z-score（MAD方法）移除异常值
        /// MAD: Median Absolute Deviation 中位数绝对偏差
        /// 相比标准Z-score方法，对极端异常值更加鲁棒
        /// </summary>
        /// <param name="data">输入数据</param>
        /// <param name="madThreshold">修正Z-score阈值，默认3.5（对应约99.3%的数据保留率）</param>
        /// <returns>清洗后的数据列表</returns>
        public static (List<float>, List<int>) CleanData(IEnumerable<float> data, IEnumerable<int> xs, float madThreshold = 3.5f)
        {
            var idx = Enumerable.Range(0, data.Count()).Where(i => (!float.IsNaN(data.ElementAt(i))) && (!float.IsInfinity(data.ElementAt(i)))).Select(i => i).ToArray();

            var validData = idx.Select(i => data.ElementAt(i)).ToList();
            var validXs = idx.Select(i => xs.ElementAt(i)).ToList();

            if (validData.Count == 0)
                return (new List<float>(), new List<int>());

            if (validData.Count == 1)
                return (validData, validXs);

            // 第一步：计算中位数
            float median = CalculateMedian(validData);

            // 第二步：计算绝对偏差（每个数据点与中位数的绝对差值）
            var absoluteDeviations = validData.Select(x => Math.Abs(x - median)).ToList();

            // 第三步：计算MAD（中位数绝对偏差）
            float mad = CalculateMedian(absoluteDeviations);

            // 第四步：使用修正Z-score过滤异常值
            // 修正Z-score公式：Mi = 0.6745 * (xi - median) / MAD
            // 0.6745是正态分布下的常数因子
            const float MAD_CONSTANT = 0.6745f;
            var cleanedData = new List<float>();
            var cleanedXs = new List<int>();

            if (Math.Abs(mad) < float.Epsilon)
            {
                // 如果MAD为0，说明所有数据相同或高度集中，保留所有数据
                cleanedData = validData;
                cleanedXs = validXs;
            }
            else
            {
                //foreach (var value in validData)
                for(int i = 0; i < validData.Count; i++)
                {
                    float modifiedZScore = MAD_CONSTANT * (validData[i] - median) / mad;
                    if (Math.Abs(modifiedZScore) <= madThreshold)
                    {
                        cleanedData.Add(validData[i]);
                        cleanedXs.Add(validXs[i]);
                    }
                }
            }

            return (cleanedData, cleanedXs);
        }

        /// <summary>
        /// 异常分析
        /// </summary>
        public static AnomalyAnalysis AnalyzeAnomalies(IEnumerable<float> data, StatisticResult stats, 
            StatisticResult cleanedStats, float? loLimit, float? hiLimit, float expectedPassRate = 0.99f)
        {
            var analysis = new AnomalyAnalysis
            {
                ExpectedPassRate = expectedPassRate,
                ActualPassRate = stats.PassRate,
                PassRateDifference = expectedPassRate - stats.PassRate
            };

            // 检测无穷值
            if (data.Any(d => float.IsInfinity(d)))
            {
                analysis.AnomalyReasons.Add("数据中存在无穷值（正无穷或负无穷）");
            }

            // 检测limit设置过宽
            if (loLimit.HasValue && hiLimit.HasValue)
            {
                float limitRange = hiLimit.Value - loLimit.Value;
                float dataRange = stats.Max - stats.Min;

                // 如果数据范围远小于限制范围，可能limit设置过宽
                if (dataRange > 0 && limitRange / dataRange > 5)
                {
                    analysis.AnomalyReasons.Add($"测试限制设置过宽：限制范围({limitRange:F4})是数据范围({dataRange:F4})的{limitRange / dataRange:F2}倍");
                }

                // 如果limit范围内存在outlier
                if (stats.OutlierCount > 0)
                {
                    var outliers = DetectOutliers(data.Where(d => 
                        !float.IsInfinity(d) && !float.IsNaN(d)).ToList(), 
                        stats.QuartileOne, stats.QuartileThree);

                    var outliersWithinLimit = outliers.Count(x => x >= loLimit.Value && x <= hiLimit.Value);
                    if (outliersWithinLimit > 0)
                    {
                        analysis.AnomalyReasons.Add($"限制范围内存在{outliersWithinLimit}个异常值");
                    }
                }
            }

            // 检测limit设置过窄
            if (stats.FailRate > 0.05 && expectedPassRate > 0.95)
            {
                if (loLimit.HasValue && hiLimit.HasValue)
                {
                    // 计算理论失效率与实际失效率差异
                    float failRateDifference = stats.FailRate - (1 - expectedPassRate);
                    if (failRateDifference > 0.03)
                    {
                        analysis.AnomalyReasons.Add($"测试限制可能设置过窄：实际失效率({stats.FailRate:P})远高于预期({1 - expectedPassRate:P})");
                    }
                }
            }

            // 检测清洗前后差异过大
            if (cleanedStats.TotalCount > 0)
            {
                float cleanDataRate = (float)cleanedStats.TotalCount / stats.TotalCount;
                if (cleanDataRate < 0.9)
                {
                    analysis.AnomalyReasons.Add($"异常数据占比较大：清洗后仅保留{cleanDataRate:P}的数据");
                }
            }

            // 检测分布异常
            if (Math.Abs(cleanedStats.SkewFactor) > 1.0)
            {
                analysis.AnomalyReasons.Add($"数据分布偏斜明显：偏度因子为{cleanedStats.SkewFactor:F4}");
            }

            // 判断是否为异常
            analysis.IsAnomaly = analysis.AnomalyReasons.Count > 0;

            // 评估严重程度
            if (analysis.AnomalyReasons.Count == 0)
            {
                analysis.Severity = "None";
            }
            else if (stats.FailRate > 0.1 || stats.HasInfinityValues)
            {
                analysis.Severity = "Critical";
            }
            else if (analysis.PassRateDifference < -0.05 || analysis.AnomalyReasons.Count >= 3)
            {
                analysis.Severity = "High";
            }
            else if (analysis.PassRateDifference < -0.02)
            {
                analysis.Severity = "Medium";
            }
            else
            {
                analysis.Severity = "Low";
            }

            return analysis;
        }

        // ==================== 辅助计算方法 ====================

        private static float CalculateMedian(List<float> data)
        {
            var sorted = data.OrderBy(x => x).ToList();
            int count = sorted.Count;
            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
            return sorted[count / 2];
        }

        private static float CalculateStandardDeviation(List<float> data, float mean)
        {
            if (data.Count < 2)
                return 0;

            float variance = data.Sum(x => (x - mean) * (x - mean)) / (data.Count - 1);
            return (float)Math.Sqrt(variance);
        }

        private static float CalculatePercentile(List<float> data, float percentile)
        {
            var sorted = data.OrderBy(x => x).ToList();
            float index = percentile * (sorted.Count - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);

            if (lower == upper)
                return sorted[lower];

            float diff = index - lower;
            return sorted[lower] * (1 - diff) + sorted[upper] * diff;
        }

        private static float CalculateSkewness(List<float> data, float mean, float stdDev)
        {
            if (data.Count < 3 || stdDev == 0)
                return 0;

            float sum = data.Sum(x => (float)Math.Pow((x - mean) / stdDev, 3));
            return sum / data.Count;
        }

        private static float CalculateKurtosis(List<float> data, float mean, float stdDev)
        {
            if (data.Count < 4 || stdDev == 0)
                return 0;

            float sum = data.Sum(x => (float)Math.Pow((x - mean) / stdDev, 4));
            return sum / data.Count - 3; // 超余峰度
        }

        private static List<float> DetectOutliers(List<float> data, float q1, float q3)
        {
            float iqr = q3 - q1;
            float lowerBound = q1 - 1.5f * iqr;
            float upperBound = q3 + 1.5f * iqr;

            return data.Where(x => x < lowerBound || x > upperBound).ToList();
        }

        private static float CalculateCp(float stdDev, float loLimit, float hiLimit)
        {
            if (stdDev == 0)
                return 0;
            return (hiLimit - loLimit) / (6 * stdDev);
        }

        private static float CalculateCpk(float mean, float stdDev, float loLimit, float hiLimit)
        {
            if (stdDev == 0)
                return 0;

            float cpuUpper = (hiLimit - mean) / (3 * stdDev);
            float cpuLower = (mean - loLimit) / (3 * stdDev);
            return Math.Min(cpuUpper, cpuLower);
        }
    }
}