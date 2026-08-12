using MathNet.Numerics.Statistics;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProdLogAnalyzer
{

    public class BitMap
    {
        public Image Data { get; set; }
        public string TestId { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }

        public BitMap(Image data, string testId, string description = "")
        {
            Data = data;
            TestId = testId;
            Description = description;
        }

    }

    public class BoxPlotPara 
    {
        public double WhiskerMin { get; set; }
        public double BoxMin { get; set; }
        public double BoxMiddle { get; set; }
        public double BoxMax { get; set; }
        public double WhiskerMax { get; set; }
        public double Width { get; set; }

        public BoxPlotPara(double whiskerMin, double boxMin, double boxMiddle, double boxMax, double whiskerMax, double width)
        {
            WhiskerMin = whiskerMin;
            BoxMin = boxMin;
            BoxMiddle = boxMiddle;
            BoxMax = boxMax;
            WhiskerMax = whiskerMax;
            Width = width;
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
        public static Image GenerateTrendChart(IEnumerable<float> data, IEnumerable<int> dataxs, BoxPlotPara boxpara, string title, float min, float max)
        {
            var plt = new Plot();

            if (data.Count() == 0)
            {
                plt.Title(title);
                return plt.GetImage(800, 600);
            }
            plt.Title(title, size: 18);
            plt.XLabel("Part Index");
            plt.YLabel("Measurement Value");
            plt.Legend.IsVisible = true;
            plt.Legend.Alignment = Alignment.UpperRight;
            plt.Grid.IsVisible = true;

            // 优化后：一次性物化并单次遍历构建 xs/ys，避免 ElementAt/Count 的重复枚举
            var dataArray = data as float[] ?? data.ToArray();
            var dataXsArray = dataxs as int[] ?? dataxs.ToArray();

            int n = dataArray.Length;
            var xsList = new List<double>(n);
            var ysList = new List<double>(n);

            for (int i = 0; i < n; i++)
            {
                float v = dataArray[i];
                if (!float.IsNaN(v) && !float.IsInfinity(v))
                {
                    xsList.Add((double)dataXsArray[i]);
                    ysList.Add((double)v);
                }
            }

            var xs = xsList.ToArray();
            var ys = ysList.ToArray();

            var signalxy1 = plt.Add.SignalXY(xs, ys, Colors.Blue);
            signalxy1.LineWidth = 1;

            Box box = new Box
            {
                Position = xs.Length / 2,
                WhiskerMin = boxpara.WhiskerMin,//线的最低位置
                BoxMin = boxpara.BoxMin,//箱体的最低位置
                BoxMiddle = boxpara.BoxMiddle,//箱体的中间位置
                BoxMax = boxpara.BoxMax,//箱体的最高位置
                WhiskerMax = boxpara.WhiskerMax,//线的最高位置
                Width = xs.Length * 0.7,
                FillColor = Colors.Orange.WithOpacity(0.2),
                LineColor = Colors.Black.WithOpacity(0.5),
            };
            var boxPlot = plt.Add.Box(box);


            plt.Font.Automatic();

            plt.Axes.SetLimitsY(min - (max - min) * 0.1, max + (max - min) * 0.1);
            //plt.SaveFig($"C:\\Users\\harlin\\Documents\\SillyMonkey\\stdfData\\M3\\output\\{title}_Trend.png");  

            return plt.GetImage(800, 600);
        }

        /// <summary>
        /// 生成直方图（100个 bin）
        /// </summary>
        public static Image GenerateHistogram(IEnumerable<float> data, BoxPlotPara boxpara, float? loLimit, float? hiLimit, string title, float min, float max)
        {
            var plt = new Plot();
            
            plt.Title(title, size: 18);
            plt.XLabel("Measurement Value");
            plt.YLabel("Frequency");
            plt.Grid.IsVisible = true;

            if (data.Count() == 0)
                return plt.GetImage(800,600);

            const int binCount = 100;
            //double[] values = dataList.Select(d => (double)d).ToArray();

            // 计算直方图数据
            //double min = values.Min();
            //double max = values.Max();

            float binSize = (max - min) / binCount;
            if(binSize<=0) 
                return plt.GetImage(800, 600);

            float[] counts = new float[binCount+2]; //first and last bin for outliers

            foreach (var v in data)
            {
                if(float.IsNaN(v) || float.IsInfinity(v))
                    continue;

                int bin = (int)((v - min) / binSize);
                if (bin < 0) bin = -1;
                if (bin > binCount) bin = binCount;
                counts[bin+1]++;
            }
            List<Bar> bars = new List<Bar>(binCount + 2);
            for (int i = 0; i < counts.Length; i++)
            {
                if(counts[i] <= 0)
                    continue;

                var bar = new Bar
                {
                    Position = min + binSize * (i - 1 + 0.5),
                    Value = counts[i],
                    Size = binSize,
                    LineWidth = 0.3f,
                    FillColor = (i>0 && i<= binCount) ? Colors.Blue : Colors.Red // outliers in red
                };
                bars.Add(bar);
            }

            // 绘制直方图
            var barPlt = plt.Add.Bars(bars);


            if (loLimit.HasValue)
                plt.Add.VerticalLine(loLimit.Value, width: 2, color: Colors.Red, LinePattern.Dashed).Text = $"{loLimit:F3}";
            else
                plt.Add.VerticalLine(min, width: 2, color: Colors.Green, LinePattern.Dashed).Text = $"{min:F4}";


            if (hiLimit.HasValue)
                plt.Add.VerticalLine(hiLimit.Value, width: 2, color: Colors.Red, LinePattern.Dashed).Text = $"{hiLimit:F3}";
            else
                plt.Add.VerticalLine(max, width: 2, color: Colors.Green, LinePattern.Dashed).Text = $"{max:F4}";

            //Box box = new Box
            //{
            //    Position = counts.Max() / 2,
            //    WhiskerMin = boxpara.WhiskerMin,//线的最低位置
            //    BoxMin = boxpara.BoxMin,//箱体的最低位置
            //    BoxMiddle = boxpara.BoxMiddle,//箱体的中间位置
            //    BoxMax = boxpara.BoxMax,//箱体的最高位置
            //    WhiskerMax = boxpara.WhiskerMax,//线的最高位置
            //    Width = counts.Max() * 0.7,
            //    Orientation = Orientation.Horizontal //not supported yet
            //};
            //var boxPlot = plt.Add.Box(box);

            var rectangle = plt.Add.Rectangle(new CoordinateRect(boxpara.BoxMin, boxpara.BoxMax, counts.Max() * 0.25, counts.Max() * 0.75));
            rectangle.FillColor = Colors.Orange.WithOpacity(0.5);

            var lineMin = plt.Add.Line(boxpara.WhiskerMin, counts.Max() * 0.4, boxpara.WhiskerMin, counts.Max() * 0.6);
            lineMin.Color = Colors.Black.WithOpacity(0.5);
            lineMin.LineWidth = 1;

            var lineMax = plt.Add.Line(boxpara.WhiskerMax, counts.Max() * 0.4, boxpara.WhiskerMax, counts.Max() * 0.6);
            lineMax.Color = Colors.Black.WithOpacity(0.5);
            lineMax.LineWidth = 1;

            var lineMiddle = plt.Add.Line(boxpara.BoxMiddle, counts.Max() * 0.25, boxpara.BoxMiddle, counts.Max() * 0.75);
            lineMiddle.Color = Colors.Black.WithOpacity(0.5);
            lineMiddle.LineWidth = 1;

            var lineHorizontal1 = plt.Add.Line(boxpara.WhiskerMin, counts.Max()*0.5, boxpara.WhiskerMax, counts.Max()*0.5);
            lineHorizontal1.Color = Colors.Black.WithOpacity(0.5);
            lineHorizontal1.LineWidth = 1;

            plt.Legend.IsVisible = true;
            plt.Font.Automatic();

            plt.Axes.SetLimitsX(min - (max - min)*0.1, max + (max - min)*0.1);
            //plt.SaveFig($"C:\\Users\\harlin\\Documents\\SillyMonkey\\stdfData\\M3\\output\\{title}_Histogram.png");

            return plt.GetImage(800, 600);
        }


        /// <summary>
        /// 生成对比趋势图（原始 vs 清洗）
        /// 标记被移除的数据点（红色 X）
        /// </summary>
        public static Image GenerateComparisonTrendChart(IEnumerable<float> data_raw, IEnumerable<int> dataxs_raw, IEnumerable<float> data_pass, IEnumerable<int> dataxs_pass, BoxPlotPara boxpara, string title, float min, float max)
        {
            var plt = new Plot();

            // 一次性物化，尽可能复用已有数组引用
            var dataRawArray = data_raw as float[] ?? data_raw.ToArray();
            var dataXsRawArray = dataxs_raw as int[] ?? dataxs_raw.ToArray();
            var dataPassArray = data_pass as float[] ?? data_pass.ToArray();
            var dataXsPassArray = dataxs_pass as int[] ?? dataxs_pass.ToArray();

            if (dataRawArray.Length == 0 || dataPassArray.Length == 0)
            {
                plt.Title(title);
                return plt.GetImage(800, 600);
            }

            plt.Title(title, size: 18);
            plt.XLabel("Part Index");
            plt.YLabel("Measurement Value");
            plt.Legend.IsVisible = true;
            plt.Legend.Alignment = Alignment.UpperRight;
            plt.Grid.IsVisible = true;

            // 构建原始数据的 xs/ys（单次遍历，处理 NaN/Infinity，防止越界）
            int nRaw = Math.Min(dataRawArray.Length, dataXsRawArray.Length);
            var xs1List = new List<double>(nRaw);
            var ys1List = new List<double>(nRaw);
            for (int i = 0; i < nRaw; i++)
            {
                float v = dataRawArray[i];
                if (!float.IsNaN(v) && !float.IsInfinity(v))
                {
                    xs1List.Add((double)dataXsRawArray[i]);
                    ys1List.Add((double)v);
                }
            }
            var xs1 = xs1List.ToArray();
            var ys1 = ys1List.ToArray();

            var signalxy1 = plt.Add.SignalXY(xs1, ys1, Colors.Blue.WithOpacity(0.5));
            signalxy1.LineWidth = 1;

            // 构建通过（清洗后）数据的 xs/ys（单次遍历，处理 NaN/Infinity，防止越界）
            int nPass = Math.Min(dataPassArray.Length, dataXsPassArray.Length);
            var xs2List = new List<double>(nPass);
            var ys2List = new List<double>(nPass);
            for (int i = 0; i < nPass; i++)
            {
                float v = dataPassArray[i];
                if (!float.IsNaN(v) && !float.IsInfinity(v))
                {
                    xs2List.Add((double)dataXsPassArray[i]);
                    ys2List.Add((double)v);
                }
            }
            var xs2 = xs2List.ToArray();
            var ys2 = ys2List.ToArray();

            var signalxy2 = plt.Add.SignalXY(xs2, ys2, Colors.Orange.WithOpacity(0.5));
            signalxy2.LineWidth = 1;

            Box box = new Box
            {
                Position = xs1.Length / 2.0,
                WhiskerMin = boxpara.WhiskerMin,//线的最低位置
                BoxMin = boxpara.BoxMin,//箱体的最低位置
                BoxMiddle = boxpara.BoxMiddle,//箱体的中间位置
                BoxMax = boxpara.BoxMax,//箱体的最高位置
                WhiskerMax = boxpara.WhiskerMax,//线的最高位置
                Width = xs1.Length * 0.7,
                FillColor = Colors.Orange.WithOpacity(0.2),
                LineColor = Colors.Black.WithOpacity(0.5),
            };
            var boxPlot = plt.Add.Box(box);

            plt.Axes.SetLimitsY(min - (max - min) * 0.1, max + (max - min) * 0.1);
            //plt.SaveFig($"C:\\Users\\harlin\\Documents\\SillyMonkey\\stdfData\\M3\\output\\{title}_Trend.png");  

            return plt.GetImage(800, 600);
        }

        public static Image GenerateBySiteBoxPlot(List<BoxPlotPara> boxparas, float? loLimit, float? hiLimit, string title, float min, float max)
        {
            var plt = new Plot();

            plt.Title(title, size: 18);
            plt.XLabel("Site");
            plt.YLabel("Measurement Value");
            plt.Grid.IsVisible = true;

            if (boxparas == null || boxparas.Count == 0)
                return plt.GetImage(800, 600);

            if (loLimit.HasValue)
                plt.Add.HorizontalLine(loLimit.Value, width: 2, color: Colors.Red, LinePattern.Dashed).Text = $"{loLimit:F3}";
            else
                plt.Add.HorizontalLine(min, width: 2, color: Colors.Green, LinePattern.Dashed).Text = $"{min:F4}";


            if (hiLimit.HasValue)
                plt.Add.HorizontalLine(hiLimit.Value, width: 2, color: Colors.Red, LinePattern.Dashed).Text = $"{hiLimit:F3}";
            else
                plt.Add.HorizontalLine(max, width: 2, color: Colors.Green, LinePattern.Dashed).Text = $"{max:F4}";

            int i = 0;
            foreach(var boxpara in boxparas)
            {
                Box box = new Box
                {
                    Position = 1 + i++,
                    WhiskerMin = boxpara.WhiskerMin,//线的最低位置
                    BoxMin = boxpara.BoxMin,//箱体的最低位置
                    BoxMiddle = boxpara.BoxMiddle,//箱体的中间位置
                    BoxMax = boxpara.BoxMax,//箱体的最高位置
                    WhiskerMax = boxpara.WhiskerMax,//线的最高位置
                    Width = boxpara.Width,
                };
                var boxPlot = plt.Add.Box(box);
            }


            plt.Legend.IsVisible = true;
            plt.Font.Automatic();
            //plt.SaveFig($"C:\\Users\\harlin\\Documents\\SillyMonkey\\stdfData\\M3\\output\\{title}_Histogram.png");

            return plt.GetImage(800, 600);
        }

    }
}