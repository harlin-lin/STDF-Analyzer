using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace scottplotTrial {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            trendChart.RightClicked -= trendChart.DefaultRightClickEvent;
            trendChart.Configuration.DoubleClickBenchmark = false;
            trendChart.Configuration.LockVerticalAxis = true;

            histoChart.RightClicked -= histoChart.DefaultRightClickEvent;
            histoChart.Configuration.DoubleClickBenchmark = false;
            histoChart.Configuration.LockHorizontalAxis = true;
        }

        double GetValidData(float f) {
            if (float.IsNaN(f) || float.IsInfinity(f))
                return 0;
            return f;
        }

        bool IfValidData(float f) {
            if (float.IsNaN(f) || float.IsInfinity(f))
                return false;
            return true;
        }


        void UpdateTrend() {
            trendChart.Plot.Clear();

            var s = GetSeries(data);

            var color = Color.Blue;

            if (s.Item1.Count() > 0 && s.Item2.Count() > 0) {
                trendChart.Plot.AddSignalXY(s.Item1, s.Item2.ToArray(), Color.FromArgb(color.A, color.R, color.G, color.B), "123");
            }

            trendChart.Plot.Legend(true, ScottPlot.Alignment.UpperRight);

            trendChart.Plot.SetAxisLimitsY(ll, hl);

            trendChart.Plot.AddHorizontalLine(ll, Color.Red);
            trendChart.Plot.AddHorizontalLine(hl, Color.Red);

            //trendChart.Plot.XLabel("Test Index");
            //trendChart.Plot.YLabel("Value");
            trendChart.Plot.Title(testname, false);

            trendChart.Refresh();
        }

        void UpdateHistogram() {
            var start = ll;
            var stop = hl;

            var histo = GetHistogramData(start, stop, data);
            
            double step = (stop - start) / 100;
            var ov = 5 * (stop - start) / 100;
            if (ov == 0) ov = 1;
            var actStart = start - ov;
            var actStop = stop + ov;
            var RangeHisto = (actStart, actStop, step);

            histoChart.Plot.Clear();

            var color = Color.Blue;

            var bar = histoChart.Plot.AddBar(histo.Item1, histo.Item2, Color.FromArgb(color.A, color.R, color.G, color.B));
            bar.BarWidth = RangeHisto.Item3 > 0 ? RangeHisto.Item3 : 1;
            bar.Label = testnumber;

            histoChart.Plot.Legend(true, ScottPlot.Alignment.UpperRight);

            histoChart.Plot.AddVerticalLine(ll, Color.Red);
            histoChart.Plot.AddVerticalLine(hl, Color.Red);

            histoChart.Plot.SetAxisLimitsX(RangeHisto.Item1, RangeHisto.Item2);

            //histoChart.Plot.XLabel("Test Index");
            //histoChart.Plot.YLabel("Value");
            histoChart.Plot.Title(testname);

            histoChart.Refresh();
        }


        private void trendChart_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            trendChart.Plot.AxisAutoX();
            trendChart.Refresh();
        }

        private void histoChart_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            histoChart.Plot.AxisAutoY();
            histoChart.Refresh();
        }

        List<float> data = new List<float>();
        string testname;
        string testnumber;
        float ll, hl;
        ItemStatistic statistic;

        private void Button_Click(object sender, RoutedEventArgs e) {
            UpdateTrend();
            UpdateHistogram();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var ss = File.ReadAllLines(@"E:\temp\tempdata\302002.csv");
            testnumber = ss[0];
            testname = ss[1];
            ll = float.Parse(ss[2]);
            hl = float.Parse(ss[3]);
            for (int i = 5; i<ss.Length; i++){
                data.Add(float.Parse(ss[i]));
            }

            statistic = new ItemStatistic(data,ll, hl);

            UpdateTrend();
            UpdateHistogram();

        }


        (double[], double[]) GetHistogramData(float start, float stop, IEnumerable<float> data) {
            if (start == stop) {
                start -= 1;
                stop += 1;
            }
            var step = (stop - start) / 100;
            double[] range = new double[103];
            var actStart = start;// - step * 5;
            var actStop = stop;// + step * 5;

            for (int i = 0; i < 103; i++) {
                range[i] = start + (i - 1) * step;
            }
            double[] rangeCnt = new double[103];

            foreach (var f in data) {
                if (isInvalid(f)) continue;
                if (f < actStart) {
                    rangeCnt[0]++;
                } else if (f > actStop) {
                    rangeCnt[102]++;
                } else {
                    var idx = (int)Math.Round((f - actStart) / step) + 1;
                    rangeCnt[idx]++;
                }
            }

            return (rangeCnt, range);
        }
        bool isInvalid(float f) {
            return float.IsNaN(f) || float.IsInfinity(f);
        }

        (double[], double[]) GetSeries(IEnumerable<float> data) {
            int i = 0;
            List<double> xs = new List<double>(data.Count());
            List<double> ys = new List<double>(data.Count());
            foreach (var r in data) {
                i++;
                if (IfValidData(r)) {
                    xs.Add(i);
                    ys.Add(r);
                }
            }
            return (xs.ToArray(), ys.ToArray());
        }
    }


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
            return MeanValue - Sigma * times;
        }

        public float GetSigmaRangeHigh(int times) {
            return MeanValue + Sigma * times;
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

            if (!ll.HasValue && !hl.HasValue) {
                PassCount = ValidCount;
                FailCount = ValidCount - PassCount;
            } else {
                foreach (var v in listUnNullItems) {
                    if (ll.HasValue && !hl.HasValue) {
                        if (v >= ll)
                            PassCount++;
                    } else if (!ll.HasValue && hl.HasValue) {
                        if (v <= hl)
                            PassCount++;
                    } else {
                        if (v >= ll && v <= hl)
                            PassCount++;
                    }
                }
                FailCount = ValidCount - PassCount;
            }


        }
    }


}
