using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using SillyMonkey.Core;
using Prism.Regions;
using Prism.Events;
using DataContainer;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Model.ChartSeries;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using SciChart.Charting.Visuals;

namespace UI_Chart.ViewModels {
    public class TrendViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;
        List<string> _selectedIds;

        private float _sigmaLow,_sigmaHigh, _min, _max;

        #region Binding_prop
        public ObservableCollection<IRenderableSeriesViewModel> _trendSeries= new ObservableCollection<IRenderableSeriesViewModel>();
        public ObservableCollection<IRenderableSeriesViewModel> TrendSeries { 
            get { return _trendSeries; }
            set { SetProperty(ref _trendSeries, value); }
        }

        public ObservableCollection<IRenderableSeriesViewModel> _histoSeries = new ObservableCollection<IRenderableSeriesViewModel>();
        public ObservableCollection<IRenderableSeriesViewModel> HistoSeries {
            get { return _histoSeries; }
            set { SetProperty(ref _histoSeries, value); }
        }

        private IAxisViewModel _xAxisTrend;
        public IAxisViewModel XAxisTrend {
            get { return _xAxisTrend; }
            set { SetProperty(ref _xAxisTrend, value); }
        }
        private IAxisViewModel _yAxisTrend;
        public IAxisViewModel YAxisTrend {
            get { return _yAxisTrend; }
            set { SetProperty(ref _yAxisTrend, value); }
        }

        private float _lowLimit;
        public float LowLimit {
            get { return _lowLimit; }
            set { SetProperty(ref _lowLimit, value); }
        }

        private float _highLimit;
        public float HighLimit {
            get { return _highLimit; }
            set { SetProperty(ref _highLimit, value); }
        }

        private string _userTrendLowRange;
        public string UserTrendLowRange {
            get { return _userTrendLowRange; }
            set { SetProperty(ref _userTrendLowRange, value); }
        }

        private string _userTrendHighRange;
        public string UserTrendHighRange {
            get { return _userTrendHighRange; }
            set { SetProperty(ref _userTrendHighRange, value); }
        }

        private bool _ifTrendLimitBySigma;
        public bool IfTrendLimitBySigma {
            get { return _ifTrendLimitBySigma; }
            set { SetProperty(ref _ifTrendLimitBySigma, value); }
        }

        private bool _ifTrendLimitByMinMax;
        public bool IfTrendLimitByMinMax {
            get { return _ifTrendLimitByMinMax; }
            set { SetProperty(ref _ifTrendLimitByMinMax, value); }
        }

        private bool _ifTrendLimitByLimit;
        public bool IfTrendLimitByLimit {
            get { return _ifTrendLimitByLimit; }
            set { SetProperty(ref _ifTrendLimitByLimit, value); }
        }

        private bool _ifTrendLimitByUser;
        public bool IfTrendLimitByUser {
            get { return _ifTrendLimitByUser; }
            set { SetProperty(ref _ifTrendLimitByUser, value); }
        }

        private IAxisViewModel _xAxisHisto;
        public IAxisViewModel XAxisHisto {
            get { return _xAxisHisto; }
            set { SetProperty(ref _xAxisHisto, value); }
        }
        private IAxisViewModel _yAxisHisto;
        public IAxisViewModel YAxisHisto {
            get { return _yAxisHisto; }
            set { SetProperty(ref _yAxisHisto, value); }
        }

        private string _userHistoLowRange;
        public string UserHistoLowRange {
            get { return _userHistoLowRange; }
            set { SetProperty(ref _userHistoLowRange, value); }
        }

        private string _userHistoHighRange;
        public string UserHistoHighRange {
            get { return _userHistoHighRange; }
            set { SetProperty(ref _userHistoHighRange, value); }
        }

        private bool _ifHistoLimitBySigma;
        public bool IfHistoLimitBySigma {
            get { return _ifHistoLimitBySigma; }
            set { SetProperty(ref _ifHistoLimitBySigma, value); }
        }

        private bool _ifHistoLimitByMinMax;
        public bool IfHistoLimitByMinMax {
            get { return _ifHistoLimitByMinMax; }
            set { SetProperty(ref _ifHistoLimitByMinMax, value); }
        }

        private bool _ifHistoLimitByLimit;
        public bool IfHistoLimitByLimit {
            get { return _ifHistoLimitByLimit; }
            set { SetProperty(ref _ifHistoLimitByLimit, value); }
        }

        private bool _ifHistoLimitByUser;
        public bool IfHistoLimitByUser {
            get { return _ifHistoLimitByUser; }
            set { SetProperty(ref _ifHistoLimitByUser, value); }
        }

        private bool _ifShowLegendCheckBox = false;
        public bool IfShowLegendCheckBox {
            get { return _ifShowLegendCheckBox; }
            set { SetProperty(ref _ifShowLegendCheckBox, value); }
        }
        #endregion

        public TrendViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateChart);
            _ea.GetEvent<Event_ItemsSelected>().Subscribe(UpdateItems);

            
            InitUi();
        }
        void UpdateItems(Tuple<SubData, List<string>> para) {
            if (!para.Item1.Equals(_subData)) return;
            if (para.Item2 == null || para.Item2.Count == 0) return;

            _selectedIds.Clear();
            _selectedIds.AddRange(para.Item2);

            UpdateData();
        }
        void UpdateData() {
            if (_selectedIds == null || _selectedIds.Count == 0) return;
            var da = StdDB.GetDataAcquire(_subData.StdFilePath);

            if (_selectedIds.Count > 1) {
                IfShowLegendCheckBox = true;
            } else {
                IfShowLegendCheckBox = false;
            }

            #region trendChart
            var xs = da.GetFilteredPartIndex(_subData.FilterId);
            TrendSeries.Clear();
            for(int i=0; i< (_selectedIds.Count>16 ? 16: _selectedIds.Count); i++) {
                var data = da.GetFilteredItemData(_selectedIds[i], _subData.FilterId);

                var statistic = da.GetFilteredStatistic(_subData.FilterId, _selectedIds[i]);
                if (i == 0) {
                    _min = statistic.MinValue ?? 0;
                    _max = statistic.MaxValue ?? 1;

                    _sigmaLow = (statistic.MeanValue ?? 0) - (statistic.Sigma?? 1) * 6;
                    _sigmaHigh = (statistic.MeanValue ?? 0) + (statistic.Sigma ?? 1) * 6;

                    var idInfo = da.GetTestInfo(_selectedIds[0]);
                    LowLimit = idInfo.LoLimit ?? _min;
                    HighLimit = idInfo.HiLimit ?? _max;
                } else {
                    if (statistic.MinValue.HasValue) { 
                        _min = statistic.MinValue.Value<_min ? statistic.MinValue.Value : _min;
                    }
                    if (statistic.MaxValue.HasValue) {
                        _max = statistic.MaxValue.Value>_max ? statistic.MaxValue.Value :_max;
                    }

                    var sigmaLow = (statistic.MeanValue ?? 0) - (statistic.Sigma ?? 1) * 6;
                    var sigmaHigh = (statistic.MeanValue ?? 0) + (statistic.Sigma ?? 1) * 6;

                    if (sigmaLow < _sigmaLow) _sigmaLow = sigmaLow;
                    if (sigmaHigh > _sigmaHigh) _sigmaHigh = sigmaHigh;
                }

                var series = new XyDataSeries<int, float>();
                series.Append(xs, data);
                series.SeriesName = _selectedIds[i];

                TrendSeries.Add(new LineRenderableSeriesViewModel {
                    DataSeries = series,
                    Stroke = GetColor(i)
                });

            }
            RaisePropertyChanged("TrendSeries");

            //set the y axix
            if (IfTrendLimitBySigma) {
                ExecuteCmdSelectAxisSigmaTrend();
            }else if (IfTrendLimitByMinMax) {
                ExecuteCmdSelectAxisMinMaxTrend();
            }else if (IfTrendLimitByLimit) {
                ExecuteCmdSelectAxisLimitTrend();
            }else {
                ExecuteCmdSelectAxisUserTrend();
            }

            XAxisTrend.VisibleRange.SetMinMax(1, xs.Last());
            RaisePropertyChanged("XAxisTrend");
            #endregion

            #region histogramChart
            //set the y axix
            if (IfHistoLimitBySigma) {
                ExecuteCmdSelectAxisSigmaHisto();
            } else if (IfHistoLimitByMinMax) {
                ExecuteCmdSelectAxisMinMaxHisto();
            } else if (IfHistoLimitByLimit) {
                ExecuteCmdSelectAxisLimitHisto();
            } else {
                ExecuteCmdSelectAxisUserHisto();
            }

            #endregion

        }

        void UpdateChart(SubData subData) {
            if (subData.Equals(_subData)) {

                UpdateData();
            }
        }

        Color GetColor(int idx) {
            switch (idx) {
                case 0: return Color.FromRgb(0, 0, 255);
                case 1: return Color.FromRgb(255, 182, 193);
                case 2: return Color.FromRgb(220, 20, 60);
                case 3: return Color.FromRgb(255, 20, 147);
                case 4: return Color.FromRgb(255, 0, 255);
                case 5: return Color.FromRgb(148, 0, 211);
                case 6: return Color.FromRgb(72, 61, 139);
                case 7: return Color.FromRgb(100, 149, 237);
                case 8: return Color.FromRgb(70, 130, 180);
                case 9: return Color.FromRgb(95, 158, 160);
                case 10: return Color.FromRgb(0, 255, 255);
                case 11: return Color.FromRgb(47, 79, 79);
                case 12: return Color.FromRgb(46, 139, 87);
                case 13: return Color.FromRgb(85, 107, 47);
                case 14: return Color.FromRgb(255, 255, 0);
                case 15: return Color.FromRgb(255, 165, 0);
                default: return Color.FromRgb(255, 69, 0);
            }
        }

        //default 100 bins, and enable outliers count, total 112bins
        (float[], int[]) GetHistogramData(float start, float stop, IEnumerable<float> data) {
            var step = (stop - start) / 100;
            float[] range = new float[113];
            var actStart = start - step * 5;
            var actStop = stop + step * 5;

            for (int i = 0; i < 113; i++) {
                range[i] = start + (i-6) * step;
            }
            int[] rangeCnt = new int[113];

            foreach(var f in data) {
                if (float.IsNaN(f) || float.IsInfinity(f)) continue;
                if (f < actStart) {
                    rangeCnt[0]++;
                } else if (f >= actStop) {
                    rangeCnt[112]++;
                } else {
                    var idx = (int)Math.Round((f-actStart) / step) + 1;
                    rangeCnt[idx]++;
                }
            }

            return (range, rangeCnt);
        }

        void UpdateHistoSeries(float start, float stop) {
            if (_selectedIds == null || _selectedIds.Count == 0) return;
            var da = StdDB.GetDataAcquire(_subData.StdFilePath);

            var maxCnt=0;
            HistoSeries.Clear();
            for (int i = 0; i < (_selectedIds.Count > 16 ? 16 : _selectedIds.Count); i++) {
                var data = da.GetFilteredItemData(_selectedIds[i], _subData.FilterId);

                var histo = GetHistogramData(start, stop, data);
                var series = new XyDataSeries<float, int>();
                series.Append(histo.Item1, histo.Item2);
                series.SeriesName = _selectedIds[i];

                HistoSeries.Add(new ColumnRenderableSeriesViewModel {
                    DataSeries = series,
                    Stroke = Colors.DarkBlue,
                    Fill = new SolidColorBrush(GetColor(i)),
                    DataPointWidth = 1
                });

                if (i == 0) {
                    maxCnt = histo.Item2.Max();
                } else {
                    if (maxCnt < histo.Item2.Max()) maxCnt = histo.Item2.Max();
                }
            }
            RaisePropertyChanged("HistoSeries");

            var step = (stop - start) / 100;
            var actStart = start - step * 5;
            var actStop = stop + step * 5;
            XAxisHisto.VisibleRange.SetMinMax(actStart, actStop);
            RaisePropertyChanged("XAxisHisto");

            YAxisHisto.VisibleRange.SetMinMax(0, maxCnt);
            RaisePropertyChanged("YAxisHisto");

        }

        void InitUi() {
            XAxisTrend = new NumericAxisViewModel {
                //AxisTitle = "XAxis",
                DrawMinorGridLines = false,
                DrawMajorBands = false,
                DrawMajorGridLines = true,
                TextFormatting = "#",
                FontSize = 10,
                TickTextBrush = Brushes.Black,
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(400),
                VisibleRange = new DoubleRange(1, 1),
            };
            YAxisTrend = new NumericAxisViewModel {
                AxisAlignment= AxisAlignment.Right,
                //AxisTitle = "YAxis",
                DrawMinorGridLines = false,
                DrawMajorBands = false,
                DrawMajorGridLines = true,
                TextFormatting = "f3",
                FontSize = 10,
                TickTextBrush = Brushes.Black,
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(400),
                VisibleRange = new DoubleRange(0, 1),
            };

            IfTrendLimitBySigma = true;

            XAxisHisto = new NumericAxisViewModel {
                //AxisTitle = "XAxis",
                DrawMinorGridLines = false,
                DrawMajorBands = false,
                DrawMajorGridLines = true,
                TextFormatting = "f3",
                FontSize = 10,
                TickTextBrush = Brushes.Black,
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(400),
                VisibleRange = new DoubleRange(1, 1),
            };
            YAxisHisto = new NumericAxisViewModel {
                AxisAlignment = AxisAlignment.Right,
                //AxisTitle = "YAxis",
                DrawMinorGridLines = false,
                DrawMajorBands = false,
                DrawMajorGridLines = true,
                TextFormatting = "#",
                FontSize = 10,
                TickTextBrush = Brushes.Black,
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(400),
                VisibleRange = new DoubleRange(0, 1),
            };

            IfHistoLimitBySigma = true;

        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
                _subData = data;

                _selectedIds = new List<string>((List<string>)navigationContext.Parameters["itemList"]);
                
                UpdateData();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];

            return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            
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

        private DelegateCommand<object> _CmdSaveTrend;
        public DelegateCommand<object> CmdSaveTrend =>
            _CmdSaveTrend ?? (_CmdSaveTrend = new DelegateCommand<object>(ExecuteCmdSaveTrend));

        void ExecuteCmdSaveTrend(object e) {
            string filePath;
            if (_selectedIds == null || _selectedIds.Count == 0) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            var txt = StdDB.GetDataAcquire(_subData.StdFilePath).GetTestInfo(_selectedIds[0]).TestText;

            string dftName = $"{_selectedIds[0]}_{txt}_Trend";
            if (_selectedIds.Count > 1) dftName += "_cmp";
            if (GetAndCheckPath("PNG | *.png", dftName, out filePath)) {
                (e as SciChartSurface).ExportToFile(filePath, SciChart.Core.ExportType.Png, false);
            }
            
        }

        private DelegateCommand _CmdSelectAxisSigmaTrend;
        public DelegateCommand CmdSelectAxisSigmaTrend =>
            _CmdSelectAxisSigmaTrend ?? (_CmdSelectAxisSigmaTrend = new DelegateCommand(ExecuteCmdSelectAxisSigmaTrend));

        void ExecuteCmdSelectAxisSigmaTrend() {
            YAxisTrend.VisibleRange.SetMinMax(_sigmaLow, _sigmaHigh);
            RaisePropertyChanged("YAxisTrend");
            UserTrendLowRange = _sigmaLow.ToString();
            UserTrendHighRange = _sigmaHigh.ToString();
        }

        private DelegateCommand _CmdSelectAxisMinMaxTrend;
        public DelegateCommand CmdSelectAxisMinMaxTrend =>
            _CmdSelectAxisMinMaxTrend ?? (_CmdSelectAxisMinMaxTrend = new DelegateCommand(ExecuteCmdSelectAxisMinMaxTrend));

        void ExecuteCmdSelectAxisMinMaxTrend() {
            var ov = 0.05 * (_max - _min);
            YAxisTrend.VisibleRange.SetMinMax(_min-ov, _max+ov);
            RaisePropertyChanged("YAxisTrend");
            UserTrendLowRange = _min.ToString();
            UserTrendHighRange = _max.ToString();
        }

        private DelegateCommand _CmdSelectAxisLimitTrend;
        public DelegateCommand CmdSelectAxisLimitTrend =>
            _CmdSelectAxisLimitTrend ?? (_CmdSelectAxisLimitTrend = new DelegateCommand(ExecuteCmdSelectAxisLimitTrend));

        void ExecuteCmdSelectAxisLimitTrend() {
            var ov = 0.1 * (HighLimit - LowLimit);
            YAxisTrend.VisibleRange.SetMinMax(LowLimit-ov, HighLimit+ov);
            RaisePropertyChanged("YAxisTrend");
            UserTrendLowRange = LowLimit.ToString();
            UserTrendHighRange = HighLimit.ToString();
        }

        private DelegateCommand _CmdSelectAxisUserTrend;
        public DelegateCommand CmdSelectAxisUserTrend =>
            _CmdSelectAxisUserTrend ?? (_CmdSelectAxisUserTrend = new DelegateCommand(ExecuteCmdSelectAxisUserTrend));

        void ExecuteCmdSelectAxisUserTrend() {
            float l, h;
            try {
                float.TryParse(UserTrendLowRange, out l);
                float.TryParse(UserTrendHighRange, out h);
                YAxisTrend.VisibleRange.SetMinMax(l, h);
                RaisePropertyChanged("YAxisTrend");
            }
            catch {
                System.Windows.MessageBox.Show("Wrong Limit");
            }
        }

        private DelegateCommand _CmdApplyTrendRange;
        public DelegateCommand CmdApplyTrendRange =>
            _CmdApplyTrendRange ?? (_CmdApplyTrendRange = new DelegateCommand(ExecuteCmdApplyTrendRange));

        void ExecuteCmdApplyTrendRange() {
            IfTrendLimitByUser = true;
            ExecuteCmdSelectAxisUserTrend();
        }




        private DelegateCommand<object> _CmdSaveHisto;
        public DelegateCommand<object> CmdSaveHisto =>
            _CmdSaveHisto ?? (_CmdSaveHisto = new DelegateCommand<object>(ExecuteCmdSaveHisto));

        void ExecuteCmdSaveHisto(object e) {
            string filePath;
            if (_selectedIds == null || _selectedIds.Count == 0) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            var txt = StdDB.GetDataAcquire(_subData.StdFilePath).GetTestInfo(_selectedIds[0]).TestText;

            string dftName = $"{_selectedIds[0]}_{txt}_Histo";
            if (_selectedIds.Count > 1) dftName += "_cmp";
            if (GetAndCheckPath("PNG | *.png", dftName, out filePath)) {
                (e as SciChartSurface).ExportToFile(filePath, SciChart.Core.ExportType.Png, false);
            }

        }

        private DelegateCommand _CmdSelectAxisSigmaHisto;
        public DelegateCommand CmdSelectAxisSigmaHisto =>
            _CmdSelectAxisSigmaHisto ?? (_CmdSelectAxisSigmaHisto = new DelegateCommand(ExecuteCmdSelectAxisSigmaHisto));

        void ExecuteCmdSelectAxisSigmaHisto() {
            UpdateHistoSeries(_sigmaLow, _sigmaHigh);

            UserHistoLowRange = _sigmaLow.ToString();
            UserHistoHighRange = _sigmaHigh.ToString();
        }

        private DelegateCommand _CmdSelectAxisMinMaxHisto;
        public DelegateCommand CmdSelectAxisMinMaxHisto =>
            _CmdSelectAxisMinMaxHisto ?? (_CmdSelectAxisMinMaxHisto = new DelegateCommand(ExecuteCmdSelectAxisMinMaxHisto));

        void ExecuteCmdSelectAxisMinMaxHisto() {
            UpdateHistoSeries(_min, _max);

            UserHistoLowRange = _min.ToString();
            UserHistoHighRange = _max.ToString();
        }

        private DelegateCommand _CmdSelectAxisLimitHisto;
        public DelegateCommand CmdSelectAxisLimitHisto =>
            _CmdSelectAxisLimitHisto ?? (_CmdSelectAxisLimitHisto = new DelegateCommand(ExecuteCmdSelectAxisLimitHisto));

        void ExecuteCmdSelectAxisLimitHisto() {
            UpdateHistoSeries(LowLimit, HighLimit);

            UserHistoLowRange = LowLimit.ToString();
            UserHistoHighRange = HighLimit.ToString();
        }

        private DelegateCommand _CmdSelectAxisUserHisto;
        public DelegateCommand CmdSelectAxisUserHisto =>
            _CmdSelectAxisUserHisto ?? (_CmdSelectAxisUserHisto = new DelegateCommand(ExecuteCmdSelectAxisUserHisto));

        void ExecuteCmdSelectAxisUserHisto() {
            float l, h;
            try {
                float.TryParse(UserHistoLowRange, out l);
                float.TryParse(UserHistoHighRange, out h);
                UpdateHistoSeries(l, h);
            }
            catch {
                System.Windows.MessageBox.Show("Wrong Limit");
            }
        }

        private DelegateCommand _CmdApplyHistoRange;
        public DelegateCommand CmdApplyHistoRange =>
            _CmdApplyHistoRange ?? (_CmdApplyHistoRange = new DelegateCommand(ExecuteCmdApplyHistoRange));

        void ExecuteCmdApplyHistoRange() {
            IfHistoLimitByUser = true;
            ExecuteCmdSelectAxisUserHisto();
        }


    }
}
