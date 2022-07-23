using DataContainer;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Utils;

namespace UI_Chart.ViewModels {
    public class TrendViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;
        List<string> _selectedIds;

        private int _deviceCount;

        private float _sigmaLowTrend, _sigmaHighTrend, _minTrend, _maxTrend;
        private float _sigmaLowHisto, _sigmaHighHisto, _minHisto, _maxHisto;

        ChartAxis _trendAxisMode = ChartAxis.Sigma;
        ChartAxis _histoAxisMode = ChartAxis.Sigma;

        #region Binding_prop
        public ObservableCollection<IRenderableSeriesViewModel> _trendSeries = new ObservableCollection<IRenderableSeriesViewModel>();
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

        private float _lowLimit = float.NaN;
        public float LowLimit {
            get { return _lowLimit; }
            set { SetProperty(ref _lowLimit, value); }
        }

        private float _highLimit = float.NaN;
        public float HighLimit {
            get { return _highLimit; }
            set { SetProperty(ref _highLimit, value); }
        }

        private float _meanTrend;
        public float MeanTrend {
            get { return _meanTrend; }
            set { SetProperty(ref _meanTrend, value); }
        }

        private float _meadianTrend;
        public float MedianTrend {
            get { return _meadianTrend; }
            set { SetProperty(ref _meadianTrend, value); }
        }

        private float _lowSigmaTrend;
        public float LowSigmaTrend {
            get { return _lowSigmaTrend; }
            set { SetProperty(ref _lowSigmaTrend, value); }
        }

        private float _highSigmaTrend;
        public float HighSigmaTrend {
            get { return _highSigmaTrend; }
            set { SetProperty(ref _highSigmaTrend, value); }
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

        private int _trendSigmaSelectionIdx=0;
        public int TrendSigmaSelectionIdx {
            get { return _trendSigmaSelectionIdx; }
            set { SetProperty(ref _trendSigmaSelectionIdx, value); }
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

        private bool _ignoreOutlierTrend=true;
        public bool IgnoreOutlierTrend {
            get { return _ignoreOutlierTrend; }
            set { SetProperty(ref _ignoreOutlierTrend, value); }
        }

        private int _outlierRangeIdxTrend = 0;
        public int OutlierRangeIdxTrend {
            get { return _outlierRangeIdxTrend; }
            set { SetProperty(ref _outlierRangeIdxTrend, value); }
        }

        private bool _ignoreOutlierHisto=true;
        public bool IgnoreOutlierHisto {
            get { return _ignoreOutlierHisto; }
            set { SetProperty(ref _ignoreOutlierHisto, value); }
        }

        private int _outlierRangeIdxHisto = 0;
        public int OutlierRangeIdxHisto {
            get { return _outlierRangeIdxHisto; }
            set { SetProperty(ref _outlierRangeIdxHisto, value); }
        }

        private int _histoSigmaSelectionIdx = 0;
        public int HistoSigmaSelectionIdx {
            get { return _histoSigmaSelectionIdx; }
            set { SetProperty(ref _histoSigmaSelectionIdx, value); }
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

        private string _itemTitleTrend;
        public string ItemTitleTrend {
            get { return _itemTitleTrend; }
            set { SetProperty(ref _itemTitleTrend, value); }
        }

        private string _itemTitleHisto;
        public string ItemTitleHisto {
            get { return _itemTitleHisto; }
            set { SetProperty(ref _itemTitleHisto, value); }
        }

        int SigmaByIdx(int idx) {
            return 6 - idx;
        }
        #endregion

        public TrendViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateFilter);
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

        //get raw data
        void UpdateData() {
            if (_selectedIds == null || _selectedIds.Count == 0) return;
            var da = StdDB.GetDataAcquire(_subData.StdFilePath);

            if (_selectedIds.Count == 1) {
                var idInfo = da.GetTestInfo(_selectedIds[0]);
                LowLimit = idInfo.LoLimit ?? float.NaN;
                HighLimit = idInfo.HiLimit ?? float.NaN;
                IfShowLegendCheckBox = false;
            } else {
                LowLimit = float.NaN;
                HighLimit = float.NaN;
                IfShowLegendCheckBox = true;
            }
            _deviceCount = da.GetFilteredChipsCount(_subData.FilterId);
            if (_deviceCount > 0) {
                var xs = da.GetFilteredPartIndex(_subData.FilterId);
                TrendSeries.Clear();
                for (int i = 0; i < (_selectedIds.Count > 16 ? 16 : _selectedIds.Count); i++) {
                    var data = da.GetFilteredItemData(_selectedIds[i], _subData.FilterId);

                    var series = new XyDataSeries<int, float>();
                    series.Append(xs, data);
                    series.SeriesName = _selectedIds[i];

                    TrendSeries.Add(new LineRenderableSeriesViewModel {
                        DataSeries = series,
                        Stroke = SillyMonkeySetup.GetColor(i)
                    });

                }
                RaisePropertyChanged("TrendSeries");

                XAxisTrend.VisibleRange.SetMinMax(1, _deviceCount);
                RaisePropertyChanged("XAxisTrend");

            } else {
                TrendSeries.Clear();
                RaisePropertyChanged("TrendSeries");
            }
            UpdateTrendViewRange();

            UpdateHistoViewRange();
        }

        void UpdateFilter(SubData subData) {
            if (subData.Equals(_subData)) {

                UpdateData();
            }
        }

        void UpdateTrendViewRange() {
            var da = StdDB.GetDataAcquire(_subData.StdFilePath);

            for (int i = 0; i < (_selectedIds.Count > 16 ? 16 : _selectedIds.Count); i++) {
                var statistic_raw = da.GetFilteredStatistic(_subData.FilterId, _selectedIds[i]);
                ItemStatistic statistic;

                if (_ignoreOutlierTrend) {
                    statistic = da.GetFilteredStatisticIgnoreOutlier(_subData.FilterId, _selectedIds[i], SigmaByIdx(OutlierRangeIdxTrend));
                } else {
                    statistic = statistic_raw;
                }
                
                if (i == 0) {
                    _minTrend = statistic.MinValue;
                    _maxTrend = statistic.MaxValue;

                    _sigmaLowTrend = statistic.GetSigmaRangeLow(SigmaByIdx(TrendSigmaSelectionIdx));
                    _sigmaHighTrend = statistic.GetSigmaRangeHigh(SigmaByIdx(TrendSigmaSelectionIdx));

                    if (_selectedIds.Count == 1) {
                        var info = da.GetTestInfo(_selectedIds[0]);
                        var failRate = (statistic.FailCount * 100.0 / statistic.ValidCount).ToString("f3") + "%";
                        _itemTitleTrend = $"{_selectedIds[0]}:{info.TestText}\nmean|{statistic.MeanValue:f3}  median|{statistic.MedianValue:f3}  fail|{statistic.FailCount}/{statistic.ValidCount}={failRate}  cpk|{statistic.Cpk:f3}";
                    } else {
                        _itemTitleTrend = _selectedIds[0];
                    }


                } else {
                    _minTrend = statistic.MinValue < _minTrend ? statistic.MinValue : _minTrend;
                    _maxTrend = statistic.MaxValue > _maxTrend ? statistic.MaxValue : _maxTrend;

                    var sigmaLow = statistic.GetSigmaRangeLow(SigmaByIdx(TrendSigmaSelectionIdx));
                    var sigmaHigh = statistic.GetSigmaRangeHigh(SigmaByIdx(TrendSigmaSelectionIdx));

                    if (sigmaLow < _sigmaLowTrend) _sigmaLowTrend = sigmaLow;
                    if (sigmaHigh > _sigmaHighTrend) _sigmaHighTrend = sigmaHigh;

                    _itemTitleTrend += " & " + _selectedIds[i];
                }
            }
            RaisePropertyChanged("ItemTitleTrend");

            //set the y axix
            if (IfTrendLimitBySigma) {
                ExecuteCmdSelectAxisSigmaTrend();
            } else if (IfTrendLimitByMinMax) {
                ExecuteCmdSelectAxisMinMaxTrend();
            } else if (IfTrendLimitByLimit) {
                ExecuteCmdSelectAxisLimitTrend();
            } else {
                ExecuteCmdSelectAxisUserTrend();
            }
        }

        void UpdateHistoViewRange() {
            var da = StdDB.GetDataAcquire(_subData.StdFilePath);

            for (int i = 0; i < (_selectedIds.Count > 16 ? 16 : _selectedIds.Count); i++) {
                var statistic_raw = da.GetFilteredStatistic(_subData.FilterId, _selectedIds[i]);
                ItemStatistic statistic;

                if (_ignoreOutlierHisto) {
                    statistic = da.GetFilteredStatisticIgnoreOutlier(_subData.FilterId, _selectedIds[i], SigmaByIdx(OutlierRangeIdxHisto));
                } else {
                    statistic = statistic_raw;
                }

                if (i == 0) {
                    _minHisto = statistic.MinValue;
                    _maxHisto = statistic.MaxValue;

                    _sigmaLowHisto = statistic.GetSigmaRangeLow(SigmaByIdx(HistoSigmaSelectionIdx));
                    _sigmaHighHisto = statistic.GetSigmaRangeHigh(SigmaByIdx(HistoSigmaSelectionIdx));

                    if (_selectedIds.Count == 1) {
                        var info = da.GetTestInfo(_selectedIds[0]);
                        var failRate = (statistic.FailCount * 100.0 / statistic.ValidCount).ToString("f3") + "%";
                        _itemTitleHisto = $"{_selectedIds[0]}:{info.TestText}\nmean|{statistic.MeanValue:f3}  median|{statistic.MedianValue:f3}  fail|{statistic.FailCount}/{statistic.ValidCount}={failRate}  cpk|{statistic.Cpk:f3}";
                    } else {
                        _itemTitleHisto = _selectedIds[0];
                    }


                } else {
                    _minHisto = statistic.MinValue < _minHisto ? statistic.MinValue : _minHisto;
                    _maxHisto = statistic.MaxValue > _maxHisto ? statistic.MaxValue : _maxHisto;

                    var sigmaLow = statistic.GetSigmaRangeLow(SigmaByIdx(HistoSigmaSelectionIdx));
                    var sigmaHigh = statistic.GetSigmaRangeHigh(SigmaByIdx(HistoSigmaSelectionIdx));

                    if (sigmaLow < _sigmaLowHisto) _sigmaLowHisto = sigmaLow;
                    if (sigmaHigh > _sigmaHighHisto) _sigmaHighHisto = sigmaHigh;

                    _itemTitleHisto += " & " + _selectedIds[i];
                }
            }
            RaisePropertyChanged("ItemTitleHisto");

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
        }

        //default 100 bins, and enable outliers count, total 112bins
        (float[], int[]) GetHistogramData(float start, float stop, IEnumerable<float> data) {
            var step = (stop - start) / 100;
            float[] range = new float[113];
            var actStart = start - step * 5;
            var actStop = stop + step * 5;

            for (int i = 0; i < 113; i++) {
                range[i] = start + (i - 6) * step;
            }
            int[] rangeCnt = new int[113];

            foreach (var f in data) {
                if (float.IsNaN(f) || float.IsInfinity(f)) continue;
                if (f < actStart) {
                    rangeCnt[0]++;
                } else if (f >= actStop) {
                    rangeCnt[112]++;
                } else {
                    var idx = (int)Math.Round((f - actStart) / step) + 1;
                    rangeCnt[idx]++;
                }
            }

            return (range, rangeCnt);
        }

        private 

        void UpdateHistoSeries(float start, float stop) {
            if (_selectedIds == null || _selectedIds.Count == 0) return;
            var da = StdDB.GetDataAcquire(_subData.StdFilePath);

            var maxCnt = 0;
            HistoSeries.Clear();
            if (_deviceCount == 0) return;
            for (int i = 0; i < (_selectedIds.Count > 16 ? 16 : _selectedIds.Count); i++) {
                var data = da.GetFilteredItemData(_selectedIds[i], _subData.FilterId);

                var histo = GetHistogramData(start, stop, data);
                var series = new XyDataSeries<float, int>();
                series.Append(histo.Item1, histo.Item2);
                series.SeriesName = _selectedIds[i];

                HistoSeries.Add(new ColumnRenderableSeriesViewModel {
                    DataSeries = series,
                    Stroke = Colors.DarkBlue,
                    Fill = new SolidColorBrush(SillyMonkeySetup.GetColor(i)),
                    DataPointWidth = 1
                });

                if (i == 0) {
                    maxCnt = histo.Item2.Max();
                } else {
                    if (maxCnt < histo.Item2.Max()) maxCnt = histo.Item2.Max();
                }
            }
            RaisePropertyChanged("HistoSeries");

            var ov = 5* (stop - start) / 100;
            if (ov == 0) ov = 1;
            var actStart = start - ov;
            var actStop = stop + ov;
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
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(200),
                VisibleRange = new DoubleRange(1, 1),
                StyleKey= "GridLineStyle",
            };
            YAxisTrend = new NumericAxisViewModel {
                AxisAlignment = AxisAlignment.Right,
                //AxisTitle = "YAxis",
                DrawMinorGridLines = false,
                DrawMajorBands = false,
                DrawMajorGridLines = true,
                TextFormatting = "f3",
                FontSize = 10,
                TickTextBrush = Brushes.Black,
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(200),
                VisibleRange = new DoubleRange(0, 1),
                StyleKey = "GridLineStyle",
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
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(200),
                VisibleRange = new DoubleRange(1, 1),
                StyleKey = "GridLineStyle",
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
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(200),
                VisibleRange = new DoubleRange(0, 1),
                StyleKey = "GridLineStyle",
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
            if (_selectedIds == null || _selectedIds.Count == 0) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            var txt = StdDB.GetDataAcquire(_subData.StdFilePath).GetTestInfo(_selectedIds[0]).TestText;

            string dftName = $"{_selectedIds[0]}_{txt}_Trend";
            if (_selectedIds.Count > 1) dftName += "_cmp";
            if (GetAndCheckPath("PNG | *.png", dftName, out string filePath)) {
                (e as SciChartSurface).ExportToFile(filePath, SciChart.Core.ExportType.Png, false);
            }

        }

        private DelegateCommand _CmdSelectAxisSigmaTrend;
        public DelegateCommand CmdSelectAxisSigmaTrend =>
            _CmdSelectAxisSigmaTrend ?? (_CmdSelectAxisSigmaTrend = new DelegateCommand(ExecuteCmdSelectAxisSigmaTrend));

        void ExecuteCmdSelectAxisSigmaTrend() {
            var ov = 0.05 * (_sigmaHighTrend - _sigmaLowTrend);
            if (ov == 0) ov = 1;
            YAxisTrend.VisibleRange.SetMinMax(_sigmaLowTrend-ov, _sigmaHighTrend+ov);
            RaisePropertyChanged("YAxisTrend");
            XAxisTrend.VisibleRange.SetMinMax(1, _deviceCount);
            RaisePropertyChanged("XAxisTrend");

            UserTrendLowRange = _sigmaLowTrend.ToString("f3");
            UserTrendHighRange = _sigmaHighTrend.ToString("f3");
            _trendAxisMode = ChartAxis.Sigma;
        }

        private DelegateCommand _CmdSelectAxisMinMaxTrend;
        public DelegateCommand CmdSelectAxisMinMaxTrend =>
            _CmdSelectAxisMinMaxTrend ?? (_CmdSelectAxisMinMaxTrend = new DelegateCommand(ExecuteCmdSelectAxisMinMaxTrend));

        void ExecuteCmdSelectAxisMinMaxTrend() {
            var ov = 0.05 * (_maxTrend - _minTrend);
            if (ov == 0) ov = 1;
            YAxisTrend.VisibleRange.SetMinMax(_minTrend - ov, _maxTrend + ov);
            RaisePropertyChanged("YAxisTrend");
            XAxisTrend.VisibleRange.SetMinMax(1, _deviceCount);
            RaisePropertyChanged("XAxisTrend");

            UserTrendLowRange = _minTrend.ToString("f3");
            UserTrendHighRange = _maxTrend.ToString("f3");
            _trendAxisMode = ChartAxis.MinMax;
        }

        private DelegateCommand _CmdSelectAxisLimitTrend;
        public DelegateCommand CmdSelectAxisLimitTrend =>
            _CmdSelectAxisLimitTrend ?? (_CmdSelectAxisLimitTrend = new DelegateCommand(ExecuteCmdSelectAxisLimitTrend));

        void ExecuteCmdSelectAxisLimitTrend() {
            float l = float.IsNaN(LowLimit) ? _minTrend : LowLimit;
            float h = float.IsNaN(HighLimit) ? _maxTrend : HighLimit;

            var ov = 0.1 * (h - l);
            if (ov == 0) ov = 1;
            YAxisTrend.VisibleRange.SetMinMax(l - ov, h + ov);
            RaisePropertyChanged("YAxisTrend");
            XAxisTrend.VisibleRange.SetMinMax(1, _deviceCount);
            RaisePropertyChanged("XAxisTrend");

            UserTrendLowRange = l.ToString("f3");
            UserTrendHighRange = h.ToString("f3");
            _trendAxisMode = ChartAxis.Limit;
        }

        private DelegateCommand _CmdSelectAxisUserTrend;
        public DelegateCommand CmdSelectAxisUserTrend =>
            _CmdSelectAxisUserTrend ?? (_CmdSelectAxisUserTrend = new DelegateCommand(ExecuteCmdSelectAxisUserTrend));

        void ExecuteCmdSelectAxisUserTrend() {
            try {
                float.TryParse(UserTrendLowRange, out float l);
                float.TryParse(UserTrendHighRange, out float h);
                var ov = 0.1 * (h - l);
                if (ov == 0) ov = 1;
                YAxisTrend.VisibleRange.SetMinMax(l - ov, h + ov);
                RaisePropertyChanged("YAxisTrend");
            }
            catch {
                System.Windows.MessageBox.Show("Wrong Limit");
            }
            XAxisTrend.VisibleRange.SetMinMax(1, _deviceCount);
            RaisePropertyChanged("XAxisTrend");
            _trendAxisMode = ChartAxis.User;
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

        private DelegateCommand<object> _CmdCopy;
        public DelegateCommand<object> CmdCopy =>
            _CmdCopy ?? (_CmdCopy = new DelegateCommand<object>(ExecuteCmdCopy));

        void ExecuteCmdCopy(object e) {
            if (_selectedIds == null || _selectedIds.Count == 0) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            var image = (e as SciChartSurface).ExportToBitmapSource();
            System.Windows.Clipboard.SetImage(image);
            _ea.GetEvent<Event_Log>().Publish("Copied to clipboard");
        }


        private DelegateCommand _CmdSelectAxisSigmaHisto;
        public DelegateCommand CmdSelectAxisSigmaHisto =>
            _CmdSelectAxisSigmaHisto ?? (_CmdSelectAxisSigmaHisto = new DelegateCommand(ExecuteCmdSelectAxisSigmaHisto));

        void ExecuteCmdSelectAxisSigmaHisto() {
            UpdateHistoSeries(_sigmaLowHisto, _sigmaHighHisto);

            UserHistoLowRange = _sigmaLowHisto.ToString("f3");
            UserHistoHighRange = _sigmaHighHisto.ToString("f3");
            _histoAxisMode = ChartAxis.Sigma;
        }

        private DelegateCommand _CmdSelectAxisMinMaxHisto;
        public DelegateCommand CmdSelectAxisMinMaxHisto =>
            _CmdSelectAxisMinMaxHisto ?? (_CmdSelectAxisMinMaxHisto = new DelegateCommand(ExecuteCmdSelectAxisMinMaxHisto));

        void ExecuteCmdSelectAxisMinMaxHisto() {
            UpdateHistoSeries(_minHisto, _maxHisto);

            UserHistoLowRange = _minHisto.ToString("f3");
            UserHistoHighRange = _maxHisto.ToString("f3");
            _histoAxisMode = ChartAxis.MinMax;
        }

        private DelegateCommand _CmdSelectAxisLimitHisto;
        public DelegateCommand CmdSelectAxisLimitHisto =>
            _CmdSelectAxisLimitHisto ?? (_CmdSelectAxisLimitHisto = new DelegateCommand(ExecuteCmdSelectAxisLimitHisto));

        void ExecuteCmdSelectAxisLimitHisto() {
            float l = float.IsNaN(LowLimit) ? _minHisto : LowLimit;
            float h = float.IsNaN(HighLimit) ? _maxHisto : HighLimit;

            UpdateHistoSeries(l, h);

            UserHistoLowRange = l.ToString();
            UserHistoHighRange = h.ToString();
            _histoAxisMode = ChartAxis.Limit;
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
            _histoAxisMode = ChartAxis.User;
        }

        private DelegateCommand _CmdApplyHistoRange;
        public DelegateCommand CmdApplyHistoRange =>
            _CmdApplyHistoRange ?? (_CmdApplyHistoRange = new DelegateCommand(ExecuteCmdApplyHistoRange));

        void ExecuteCmdApplyHistoRange() {
            IfHistoLimitByUser = true;
            ExecuteCmdSelectAxisUserHisto();
        }

        private DelegateCommand _cmdChangedSigmaRangeIdxTrend;
        public DelegateCommand CmdChangedSigmaRangeIdxTrend =>
            _cmdChangedSigmaRangeIdxTrend ?? (_cmdChangedSigmaRangeIdxTrend = new DelegateCommand(ExecuteCmdChangedSigmaRangeIdxTrend));

        void ExecuteCmdChangedSigmaRangeIdxTrend() {
            UpdateTrendViewRange();
        }

        private DelegateCommand _cmdChangedSigmaOutlierIdxTrend;
        public DelegateCommand CmdChangedSigmaOutlierIdxTrend =>
            _cmdChangedSigmaOutlierIdxTrend ?? (_cmdChangedSigmaOutlierIdxTrend = new DelegateCommand(ExecuteCmdChangedSigmaOutlierIdxTrend));

        void ExecuteCmdChangedSigmaOutlierIdxTrend() {
            UpdateTrendViewRange();
        }

        private DelegateCommand _cmdToggleOutlierTrend;
        public DelegateCommand CmdToggleOutlierTrend =>
            _cmdToggleOutlierTrend ?? (_cmdToggleOutlierTrend = new DelegateCommand(ExecuteCmdToggleOutlierTrend));

        void ExecuteCmdToggleOutlierTrend() {
            UpdateTrendViewRange();
        }


        private DelegateCommand _cmdChangedSigmaRangeIdxHisto;
        public DelegateCommand CmdChangedSigmaRangeIdxHisto =>
            _cmdChangedSigmaRangeIdxHisto ?? (_cmdChangedSigmaRangeIdxHisto = new DelegateCommand(ExecuteCmdChangedSigmaRangeIdxHisto));

        void ExecuteCmdChangedSigmaRangeIdxHisto() {
            UpdateHistoViewRange();
        }

        private DelegateCommand _cmdChangedSigmaOutlierIdxHisto;
        public DelegateCommand CmdChangedSigmaOutlierIdxHisto =>
            _cmdChangedSigmaOutlierIdxHisto ?? (_cmdChangedSigmaOutlierIdxHisto = new DelegateCommand(ExecuteCmdChangedSigmaOutlierIdxHisto));

        void ExecuteCmdChangedSigmaOutlierIdxHisto() {
            UpdateHistoViewRange();
        }

        private DelegateCommand _cmdToggleOutlierHisto;
        public DelegateCommand CmdToggleOutlierHisto =>
            _cmdToggleOutlierHisto ?? (_cmdToggleOutlierHisto = new DelegateCommand(ExecuteCmdToggleOutlierHisto));

        void ExecuteCmdToggleOutlierHisto() {
            UpdateHistoViewRange();
        }

        private DelegateCommand _cmdZoomOutTrend;
        public DelegateCommand CmdZoomOutTrend =>
            _cmdZoomOutTrend ?? (_cmdZoomOutTrend = new DelegateCommand(ExecuteCmdZoomOutTrend));

        void ExecuteCmdZoomOutTrend() {
            //set the y axix
            if (IfTrendLimitBySigma) {
                ExecuteCmdSelectAxisSigmaTrend();
            } else if (IfTrendLimitByMinMax) {
                ExecuteCmdSelectAxisMinMaxTrend();
            } else if (IfTrendLimitByLimit) {
                ExecuteCmdSelectAxisLimitTrend();
            } else {
                ExecuteCmdSelectAxisUserTrend();
            }
        }
    }
}
