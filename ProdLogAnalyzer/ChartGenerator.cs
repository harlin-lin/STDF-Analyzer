using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ProdLogAnalyzer
{

    public class BitMap
    {
        public Bitmap Data { get; set; }
        public string TestId { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }

        public BitMap(Bitmap data, string testId, string description = "")
        {
            Data = data;
            TestId = testId;
            Description = description;
        }

    }
    /// <summary>
    /// 使用 ScottPlot 生成数据可视化图表（适配 .NET Framework 4.6.2）
    /// </summary>
    public class ChartGenerator
    {
        /// <summary>
        /// 生成趋势图（清洗前）
        /// </summary>
        public static Bitmap GenerateTrendChart(IEnumerable<float> data, IEnumerable<int> dataxs, string title)
        {
            var dataList = data.ToList();
            var plt = new Plot(800, 600);

            if (dataList.Count == 0)
            {
                plt.Title(title);
                return plt.Render();
            }
            var idx = Enumerable.Range(0, data.Count()).Where(i => (!float.IsNaN(data.ElementAt(i))) && (!float.IsInfinity(data.ElementAt(i)))).Select(i => i).ToArray();

            var xs = idx.Select(i => (double)dataxs.ElementAt(i)).ToArray();
            var ys = idx.Select(i => (double)data.ElementAt(i)).ToArray();

            plt.AddSignalXY(xs, ys, color: Color.Blue);
            plt.Title(title, size: 12);
            plt.XLabel("序号");
            plt.YLabel("测量值");
            plt.Legend(true, ScottPlot.Alignment.UpperRight);
            plt.Grid(enable: true);

            //plt.SaveFig($"C:\\Users\\harlin\\Documents\\SillyMonkey\\stdfData\\M3\\output\\{title}_Trend.png");  

            return plt.Render();
        }

        /// <summary>
        /// 生成直方图（100个 bin）
        /// </summary>
        public static Bitmap GenerateHistogram(IEnumerable<float> data, float? loLimit, float? hiLimit, string title, float min, float max)
        {
            var dataList = data.Where(d => !float.IsInfinity(d) && !float.IsNaN(d)).ToList();
            var plt = new Plot(800, 600);

            plt.Title(title, size: 18);
            plt.XLabel("测量值");
            plt.YLabel("频数");
            plt.Grid(enable: true);

            if (dataList.Count == 0)
                return plt.Render();

            const int binCount = 100;
            double[] values = dataList.Select(d => (double)d).ToArray();

            // 计算直方图数据
            //double min = values.Min();
            //double max = values.Max();

            double binSize = (max - min) / binCount;
            double[] bins = new double[binCount];
            double[] counts = new double[binCount];

            for (int i = 0; i < values.Length; i++)
            {
                int bin = (int)((values[i] - min) / binSize);
                if (bin < 0) bin = 0;
                if (bin >= binCount) bin = binCount - 1;
                counts[bin]++;
            }
            for (int i = 0; i < binCount; i++)
            {
                bins[i] = min + binSize * (i + 0.5);
            }

            // 绘制直方图
            var bar = plt.AddBar(counts, bins, color: Color.Blue);
            bar.BarWidth = binSize > 0 ? binSize : 1;
            plt.AddScatter(bins, counts, color: Color.Black, lineWidth: 0.5f, markerSize: 0);

            // 添加限制线（垂直）
            if (loLimit.HasValue)
                plt.AddVerticalLine(loLimit.Value, color: Color.Red, width: 2, style: LineStyle.Dash).Label = $"下限: {loLimit:F4}";
            else
                plt.AddVerticalLine(min, color: Color.Green, width: 2, style: LineStyle.Dash).Label = $"6σ: {min:F4}";


            if (hiLimit.HasValue)
                plt.AddVerticalLine(hiLimit.Value, color: Color.Red, width: 2, style: LineStyle.Dash).Label = $"上限: {hiLimit:F4}";
            else
                plt.AddVerticalLine(max, color: Color.Green, width: 2, style: LineStyle.Dash).Label = $"6σ: {max:F4}";


            plt.Legend();

            //plt.SaveFig($"C:\\Users\\harlin\\Documents\\SillyMonkey\\stdfData\\M3\\output\\{title}_Histogram.png");

            return plt.Render();
        }

        /// <summary>
        /// 生成箱型图（盒须图）
        /// </summary>
        public static Bitmap GenerateBoxPlot(IEnumerable<float> data, float? loLimit, float? hiLimit, string title, float min, float max)
        {
            var dataList = data.Where(d => !float.IsInfinity(d) && !float.IsNaN(d)).Select(d => (double)d).ToArray();
            var plt = new Plot(800, 600);

            plt.Title(title, size: 18);
            plt.YLabel("测量值");
            plt.Grid(enable: true);

            if (dataList.Length == 0)
                return plt.Render();

            // ScottPlot 4.x 没有 AddBoxPlot，使用 AddPopulation 绘制单组箱型图
            var pop = new ScottPlot.Statistics.Population(dataList);
            plt.AddPopulation(pop);

            // 计算统计值用于标注
            var sorted = dataList.OrderBy(d => d).ToArray();
            double q1 = CalculatePercentile(sorted, 0.25);
            double median = CalculatePercentile(sorted, 0.5);
            double q3 = CalculatePercentile(sorted, 0.75);
            double iqr = q3 - q1;
            double lowerWhisker = Math.Max(sorted.First(), q1 - 1.5 * iqr);
            double upperWhisker = Math.Min(sorted.Last(), q3 + 1.5 * iqr);

            // 中位线注释
            plt.AddHorizontalLine(median, color: Color.Red, width: 2).Label = $"中位数: {median:F4}";

            // 限制线（水平）
            if (loLimit.HasValue)
                plt.AddHorizontalLine(loLimit.Value, color: Color.Orange, width: 2, style: LineStyle.Dash).Label = $"下限: {loLimit:F4}";
            else
                plt.AddHorizontalLine(min, color: Color.Green, width: 2, style: LineStyle.Dash).Label = $"6σ: {min:F4}";

            if (hiLimit.HasValue)
                plt.AddHorizontalLine(hiLimit.Value, color: Color.Orange, width: 2, style: LineStyle.Dash).Label = $"上限: {hiLimit:F4}";
            else
                plt.AddHorizontalLine(max, color: Color.Green, width: 2, style: LineStyle.Dash).Label = $"6σ: {max:F4}";

            // ScottPlot Population 默认 X 轴为类别，只有一个箱，范围为 -0.5~0.5
            plt.SetAxisLimits(xMin: -0.5, xMax: 0.5);

            plt.Legend();

            //plt.SaveFig($"C:\\Users\\harlin\\Documents\\SillyMonkey\\stdfData\\M3\\output\\{title}_BoxPlot.png");

            return plt.Render();
        }

        /// <summary>
        /// 生成对比趋势图（原始 vs 清洗）
        /// 标记被移除的数据点（红色 X）
        /// </summary>
        public static Bitmap GenerateComparisonTrendChart(IEnumerable<float> original, IEnumerable<float> cleaned, string title)
        {
            var origList = original.ToList();
            var cleanedList = cleaned.ToList();
            var plt = new Plot(800, 600);

            if (origList.Count == 0)
            {
                plt.Title(title);
                return plt.Render();
            }

            double[] xs = Enumerable.Range(0, origList.Count).Select(i => (double)i).ToArray();
            double[] ys = origList.Select(d => (double)d).ToArray();

            plt.AddScatter(xs, ys, color: Color.Blue, lineWidth: 1, markerSize: 2);

            if (cleanedList.Count > 0)
            {
                // 被移除的点 = 原始集合中不在 cleaned 集合中的元素（按值比较）
                var cleanedMultiset = BuildValueCounts(cleanedList);
                var removedXs = new List<double>();
                var removedYs = new List<double>();

                for (int i = 0; i < origList.Count; i++)
                {
                    double v = origList[i];
                    if (!ConsumeValueIfExists(cleanedMultiset, v))
                    {
                        removedXs.Add(i);
                        removedYs.Add(v);
                    }
                }

                if (removedXs.Count > 0)
                    plt.AddScatter(removedXs.ToArray(), removedYs.ToArray(), color: Color.Red, markerSize: 7, markerShape: MarkerShape.filledSquare, lineWidth: 0);
            }

            plt.Title(title, size: 18);
            plt.XLabel("样本序号");
            plt.YLabel("测量值");
            plt.Legend();
            plt.Grid(enable: true);

            //plt.SaveFig($"C:\\Users\\harlin\\Documents\\SillyMonkey\\stdfData\\M3\\output\\{title}_ComparisonTrend.png");

            return plt.Render();
        }

        // ==================== 辅助方法 ====================

        private static double CalculatePercentile(double[] sortedData, double percentile)
        {
            if (sortedData == null || sortedData.Length == 0)
                return 0;

            double index = percentile * (sortedData.Length - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);

            if (lower == upper)
                return sortedData[lower];

            double diff = index - lower;
            return sortedData[lower] * (1 - diff) + sortedData[upper] * diff;
        }

        private static Dictionary<double, int> BuildValueCounts(IEnumerable<float> values)
        {
            var dict = new Dictionary<double, int>();
            foreach (var v in values)
            {
                double d = v;
                if (dict.ContainsKey(d)) dict[d]++; else dict[d] = 1;
            }
            return dict;
        }

        private static bool ConsumeValueIfExists(Dictionary<double, int> counts, double v)
        {
            if (!counts.TryGetValue(v, out int c) || c <= 0) return false;
            counts[v] = c - 1;
            return true;
        }
    }
}