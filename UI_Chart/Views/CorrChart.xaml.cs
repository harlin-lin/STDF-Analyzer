using DataContainer;
using Microsoft.Win32;
using Prism.Events;
using Prism.Regions;
using ScottPlot.Plottable;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Utils;

namespace UI_Chart.Views {
    /// <summary>
    /// Interaction logic for Trend
    /// </summary>
    public partial class CorrChart : UserControl, INavigationAware {
        public CorrChart(IRegionManager regionManager, IEventAggregator ea) {
            InitializeComponent();

            _regionManager = regionManager;
            _ea = ea;

            histoChart.RightClicked -= histoChart.DefaultRightClickEvent;
            histoChart.Configuration.DoubleClickBenchmark = false;
            histoChart.Configuration.LockHorizontalAxis = true;
        }

        IRegionManager _regionManager;
        IEventAggregator _ea;

        string _selectedId;
        List<SubData> _subDataList = new List<SubData>();
        public List<SubData> SubDataList {
            get { return _subDataList; }
        }

        int _corrDataIdx = -1;

        private float _sigmaLow, _sigmaHigh, _min, _max;

        #region Binding_prop
        private float _lowLimit;
        private float _highLimit;
        private string _itemTitle;

        bool _dataValid;

        int SigmaByIdx(int idx) {
            return 6 - idx;
        }

        bool isInvalid(float f) {
            return float.IsNaN(f) || float.IsInfinity(f);
        }
        #endregion

        void ClearChart() {
            //clear chart
            histoChart.Plot.Clear();
            histoChart.Refresh();

            tbSummary.Text = string.Empty;
        }

        private void UpdateFilter(SubData data) {
            if (_subDataList.Contains(data)) {
                UpdateData();
            }
        }

        private void UpdateData() {
            if (_selectedId == null || _subDataList.Count == 0) {
                _dataValid = false;
                //ClearChart();
                return;
            } else {
                _dataValid = true;
            }
            for (int i = 0; i < (_subDataList.Count > 16 ? 16 : _subDataList.Count); i++) {
                var da = StdDB.GetDataAcquire(_subDataList[i].StdFilePath);
                if (da.GetFilteredChipsCount(_subDataList[i].FilterId) == 0) {
                    _dataValid = false;
                    ClearChart();
                    return;
                }
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder sb_subData = new StringBuilder();
            StringBuilder sb_mean = new StringBuilder();
            StringBuilder sb_median = new StringBuilder();
            StringBuilder sb_min = new StringBuilder();
            StringBuilder sb_max = new StringBuilder();
            StringBuilder sb_cp = new StringBuilder();
            StringBuilder sb_cpk = new StringBuilder();
            StringBuilder sb_sigma = new StringBuilder();

            sb_subData.Append($"{"SubData:",-13}");
            sb_mean.Append($"{"Mean:",-13}");
            sb_median.Append($"{"Median:",-13}");
            sb_min.Append($"{"Min:",-13}");
            sb_max.Append($"{"Max:",-13}");
            sb_cp.Append($"{"CP:",-13}");
            sb_cpk.Append($"{"CPK:",-13}");
            sb_sigma.Append($"{"Sigma:",-13}");

            for (int i = 0; i < (_subDataList.Count > 16 ? 16 : _subDataList.Count); i++) {
                var da = StdDB.GetDataAcquire(_subDataList[i].StdFilePath);
                if (!da.IfContainsTestId(_selectedId)) continue;

                var statistic_raw = da.GetFilteredStatistic(_subDataList[i].FilterId, _selectedId);
                ItemStatistic statistic;

                if (toggleOutlier.IsChecked.Value) {
                    statistic = da.GetFilteredStatisticIgnoreOutlier(_subDataList[i].FilterId, _selectedId, SigmaByIdx(cbOutlierSigma.SelectedIndex));
                } else {
                    statistic = statistic_raw;
                }

                //var statistic = da.GetFilteredStatistic(_subDataList[i].FilterId, _selectedId);

                sb_subData.Append($"{_subDataList[i].FilterId.ToString("X8"),-13}");
                sb_mean.Append($"{statistic.MeanValue,-13}");
                sb_median.Append($"{statistic.MedianValue,-13}");
                sb_min.Append($"{statistic.MinValue,-13}");
                sb_max.Append($"{statistic.MaxValue,-13}");
                sb_cp.Append($"{statistic.Cp,-13}");
                sb_cpk.Append($"{statistic.Cpk,-13}");
                sb_sigma.Append($"{statistic.Sigma,-13}");

                if (i == 0) {
                    var info = da.GetTestInfo(_selectedId);
                    _itemTitle = $"{_selectedId}:{info.TestText}\n";


                    _min = statistic.MinValue;
                    _max = statistic.MaxValue;

                    _sigmaLow = statistic.GetSigmaRangeLow(6);
                    _sigmaHigh = statistic.GetSigmaRangeHigh(6);

                    var idInfo = da.GetTestInfo(_selectedId);
                    _lowLimit = idInfo.LoLimit ?? _min;
                    _highLimit = idInfo.HiLimit ?? _max;

                    var item = da.GetTestInfo(_selectedId);
                    sb.Append($"{_selectedId,-13}");
                    sb.Append($"{item.TestText}\r\n");
                    sb.Append($"{"Lo Limit:",-13}{item.LoLimit,-13}{item.Unit,-13}\r\n");
                    sb.Append($"{"Hi Limit:",-13}{item.HiLimit,-13}{item.Unit,-13}\r\n");
                } else {
                    _min = statistic.MinValue < _min ? statistic.MinValue : _min;
                    _max = statistic.MaxValue > _max ? statistic.MaxValue : _max;

                    var sigmaLow = statistic.GetSigmaRangeLow(6);
                    var sigmaHigh = statistic.GetSigmaRangeHigh(6);

                    if (sigmaLow < _sigmaLow) _sigmaLow = sigmaLow;
                    if (sigmaHigh > _sigmaHigh) _sigmaHigh = sigmaHigh;
                }


            }

            sb.Append(sb_subData);
            sb.AppendLine();
            sb.Append(sb_mean);
            sb.AppendLine();
            sb.Append(sb_median);
            sb.AppendLine();
            sb.Append(sb_min);
            sb.AppendLine();
            sb.Append(sb_max);
            sb.AppendLine();
            sb.Append(sb_cp);
            sb.AppendLine();
            sb.Append(sb_cpk);
            sb.AppendLine();
            sb.Append(sb_sigma);

            tbSummary.Text = sb.ToString();

            _itemTitle += sb_mean;


            #region histogramChart
            //set the y axix
            if (rbSigma.IsChecked.Value) {
                ExecuteCmdSelectAxisSigmaHisto();
            } else if (rbMinMax.IsChecked.Value) {
                ExecuteCmdSelectAxisMinMaxHisto();
            } else if (rbLimit.IsChecked.Value) {
                ExecuteCmdSelectAxisLimitHisto();
            } else {
                ExecuteCmdSelectAxisUserHisto();
            }

            #endregion
        }



        void UpdateItems(Tuple<string, IEnumerable<SubData>> para) {
            _subDataList.Clear();
            _subDataList.AddRange(para.Item2);
            _selectedId = para.Item1;

            UpdateData();
        }

        //default 100 bins, and enable outliers count, total 112bins
        (double[], double[], double) GetHistogramData(double start, double stop, IEnumerable<float> data) {
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

            return (rangeCnt, range, step);
        }

        void UpdateHistoSeries(float fstart, float fstop) {
            if (!_dataValid) return;
            if (isInvalid(fstart) || isInvalid(fstop)) return;

            histoChart.Plot.Clear();
            histoChart.Plot.Title(_itemTitle, true, null, 12);
            histoChart.Plot.Legend(true, ScottPlot.Alignment.UpperRight);

            double maxCnt = 0;

            double start = fstart;
            double stop = fstop;
            if (start == stop) {
                start -= 1;
                stop += 1;
            }

            for (int i = 0; i < (_subDataList.Count > 16 ? 16 : _subDataList.Count); i++) {

                var da = StdDB.GetDataAcquire(_subDataList[i].StdFilePath);
                if (!da.IfContainsTestId(_selectedId)) continue;

                var data = da.GetFilteredItemData(_selectedId, _subDataList[i].FilterId);

                var histo = GetHistogramData(start, stop, data);

                var color = SA.GetColor(i);

                var bar = histoChart.Plot.AddBar(histo.Item1, histo.Item2, Color.FromArgb(color.A, color.R, color.G, color.B));
                bar.BarWidth = histo.Item3 > 0 ? histo.Item3 : 1;
                bar.Label = SubDataList[i].FilterId.ToString("X8");

                if (i == 0) {
                    maxCnt = histo.Item2.Max();
                } else {
                    if (maxCnt < histo.Item2.Max()) maxCnt = histo.Item2.Max();
                }
            }

            var ov = 5 * (stop - start) / 100;
            if (ov == 0) ov = 1;
            var actStart = start - ov;
            var actStop = stop + ov;
            histoChart.Plot.SetAxisLimitsX(actStart, actStop);
            //histoChart.Plot.SetAxisLimitsY(0, maxCnt*1.1);

            histoChart.Plot.AxisAutoY();
            histoChart.Refresh();

        }


        public void OnNavigatedTo(NavigationContext navigationContext) {
            if (_corrDataIdx == -1) {
                _subDataList = new List<SubData>((IEnumerable<SubData>)navigationContext.Parameters["subDataList"]);
                _corrDataIdx = (int)navigationContext.Parameters["corrDataIdx"];

                _ea.GetEvent<Event_CorrItemSelected>().Subscribe(UpdateItems);
                _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateFilter);

                UpdateData();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            _ea.GetEvent<Event_CorrItemSelected>().Unsubscribe(UpdateItems);
            _ea.GetEvent<Event_FilterUpdated>().Unsubscribe(UpdateFilter);
        }


        ///<summary>
        /// Check if file is Good for writing
        ///</summary>
        ///<param name="filePath">File path</param>
        ///<returns></returns>
        public static bool IsFileGoodForWriting(string filePath) {
            FileStream stream = null;
            FileInfo file = new FileInfo(filePath);

            try {
                stream = file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            }
            catch (Exception) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            } finally {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return true;
        }

        public SaveFileDialog CreateFileDialog(string filter) {
            var saveFileDialog = new SaveFileDialog {
                Filter = filter,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            return saveFileDialog;
        }

        private bool GetAndCheckPath(string filter, string dftName, out string path) {
            var ret = false;
            var isGoodPath = false;
            var saveFileDialog = CreateFileDialog(filter);
            saveFileDialog.FileName = dftName;
            path = null;

            while (!isGoodPath) {
                if (saveFileDialog.ShowDialog() == true) {
                    if (IsFileGoodForWriting(saveFileDialog.FileName)) {
                        path = saveFileDialog.FileName;
                        isGoodPath = true;
                        ret = true;
                    } else {
                        System.Windows.MessageBox.Show(
                            "File is inaccesible for writing or you can not create file in this location, please choose another one.");
                    }
                } else {
                    isGoodPath = true;
                }
            }

            return ret;
        }

        void ExecuteCmdSelectAxisSigmaHisto() {
            if (!_dataValid) return;
            UpdateHistoSeries(_sigmaLow, _sigmaHigh);

            tbLowRange.Text = _sigmaLow.ToString();
            tbHighRange.Text = _sigmaHigh.ToString();
        }

        void ExecuteCmdSelectAxisMinMaxHisto() {
            if (!_dataValid) return;
            UpdateHistoSeries(_min, _max);

            tbLowRange.Text = _min.ToString();
            tbHighRange.Text = _max.ToString();
        }

        void ExecuteCmdSelectAxisLimitHisto() {
            if (!_dataValid) return;
            UpdateHistoSeries(_lowLimit, _highLimit);

            tbLowRange.Text = _lowLimit.ToString();
            tbHighRange.Text = _highLimit.ToString();
        }

        void ExecuteCmdSelectAxisUserHisto() {
            if (!_dataValid) return;
            float l, h;
            try {
                float.TryParse(tbLowRange.Text, out l);
                float.TryParse(tbHighRange.Text, out h);
                UpdateHistoSeries(l, h);
            }
            catch {
                System.Windows.MessageBox.Show("Wrong Limit");
            }
        }


        //void UpdateHistogram(CorrChartViewModel vm) {
        //    var series = vm.HistoSeries;

        //    histoChart.Plot.Clear();

        //    var cnt = series.Count;

        //    for (int j = 0; j < cnt; j++) {
        //        var color = SA.GetColor(j);

        //        var bar = histoChart.Plot.AddBar(series[j].Item1, series[j].Item2, Color.FromArgb(color.A, color.R, color.G, color.B));
        //        bar.BarWidth = vm.HistoViewRange.Item3 > 0 ? vm.HistoViewRange.Item3 : 1;
        //        bar.Label = vm.SubDataList[j].FilterId.ToString("X8");
        //    }
        //    histoChart.Plot.Legend(true, ScottPlot.Alignment.UpperRight);

        //    histoChart.Plot.AddVerticalLine(vm._lowLimit, Color.Red);
        //    histoChart.Plot.AddVerticalLine(vm._highLimit, Color.Red);

        //    histoChart.Plot.SetAxisLimitsX(vm.HistoViewRange.Item1, vm.HistoViewRange.Item2);

        //    //histoChart.Plot.XLabel("Test Index");
        //    //histoChart.Plot.YLabel("Value");
        //    histoChart.Plot.Title(vm.ItemTitle);

        //    histoChart.Refresh();
        //}

        private void histoChart_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            histoChart.Plot.AxisAutoY();
            histoChart.Refresh();
        }

        private void buttonSave_Click(object sender, System.Windows.RoutedEventArgs e) {
            string filePath;
            if (_selectedId == null || _subDataList.Count == 0) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            string dftName = _selectedId + "_CorrHisto";
            if (_subDataList.Count > 1) dftName += "_cmp";
            if (GetAndCheckPath("PNG | *.png", dftName, out filePath)) {
                histoChart.Plot.SaveFig(filePath);
            }

        }

        private void buttonCopy_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (_selectedId == null || _subDataList.Count == 0) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            var image = histoChart.Plot.GetBitmap();
            System.Windows.Clipboard.SetImage(System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            image.GetHbitmap(),
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()));

            _ea.GetEvent<Event_Log>().Publish("Copied to clipboard");

        }

        private void rbSigma_Checked(object sender, System.Windows.RoutedEventArgs e) {
            ExecuteCmdSelectAxisSigmaHisto();
        }

        private void rbMinMax_Checked(object sender, System.Windows.RoutedEventArgs e) {
            ExecuteCmdSelectAxisMinMaxHisto();
        }

        private void rbLimit_Checked(object sender, System.Windows.RoutedEventArgs e) {
            ExecuteCmdSelectAxisLimitHisto();
        }

        private void rbUserRange_Checked(object sender, System.Windows.RoutedEventArgs e) {
            ExecuteCmdSelectAxisUserHisto();
        }

        private void btApplyUserRange_Click(object sender, System.Windows.RoutedEventArgs e) {
            ExecuteCmdSelectAxisUserHisto();
        }

        private void toggleOutlier_Click(object sender, System.Windows.RoutedEventArgs e) {
            UpdateData();
        }

        private void cbOutlierSigma_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            UpdateData();
        }
    }
}
