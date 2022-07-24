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

        private float _allsigmaLowTrend, _allsigmaHighTrend, _allminTrend, _allmaxTrend;
        private float _allsigmaLowHisto, _allsigmaHighHisto, _allminHisto, _allmaxHisto;

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

        private IRange _xRangeTrend = new DoubleRange(0, 1);
        public IRange XRangeTrend {
            get { return _xRangeTrend; }
            set { SetProperty(ref _xRangeTrend, value); }
        }

        private IRange _yRangeTrend = new DoubleRange(0, 1);
        public IRange YRangeTrend {
            get { return _yRangeTrend; }
            set { SetProperty(ref _yRangeTrend, value); }
        }

        private IRange _xRangeHisto = new DoubleRange(0, 1);
        public IRange XRangeHisto {
            get { return _xRangeHisto; }
            set { SetProperty(ref _xRangeHisto, value); }
        }

        private IRange _yRangeHisto = new DoubleRange(0, 1);
        public IRange YRangeHisto {
            get { return _yRangeHisto; }
            set { SetProperty(ref _yRangeHisto, value); }
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

        private bool _ifTrendLimitBySigma = true;
        public bool IfTrendLimitBySigma {
            get { return _ifTrendLimitBySigma; }
            set { SetProperty(ref _ifTrendLimitBySigma, value); }
        }

        private bool _ifTrendLimitByMinMax = false;
        public bool IfTrendLimitByMinMax {
            get { return _ifTrendLimitByMinMax; }
            set { SetProperty(ref _ifTrendLimitByMinMax, value); }
        }

        private bool _ifTrendLimitByLimit = false;
        public bool IfTrendLimitByLimit {
            get { return _ifTrendLimitByLimit; }
            set { SetProperty(ref _ifTrendLimitByLimit, value); }
        }

        private bool _ifTrendLimitByUser = false;
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

        private bool _ifHistoLimitBySigma = true;
        public bool IfHistoLimitBySigma {
            get { return _ifHistoLimitBySigma; }
            set { SetProperty(ref _ifHistoLimitBySigma, value); }
        }

        private bool _ifHistoLimitByMinMax = false;
        public bool IfHistoLimitByMinMax {
            get { return _ifHistoLimitByMinMax; }
            set { SetProperty(ref _ifHistoLimitByMinMax, value); }
        }

        private bool _ifHistoLimitByLimit = false;
        public bool IfHistoLimitByLimit {
            get { return _ifHistoLimitByLimit; }
            set { SetProperty(ref _ifHistoLimitByLimit, value); }
        }

        private bool _ifHistoLimitByUser = false;
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

        private bool _enAxisLimitTrend=true;
        public bool EnAxisLimitTrend {
            get { return _enAxisLimitTrend; }
            set { SetProperty(ref _enAxisLimitTrend, value); }
        }

        private bool _enAxisMinMaxTrend=false;
        public bool EnAxisMinMaxTrend {
            get { return _enAxisMinMaxTrend; }
            set { SetProperty(ref _enAxisMinMaxTrend, value); }
        }

        private bool _enAxisMeanTrend = false;
        public bool EnAxisMeanTrend {
            get { return _enAxisMeanTrend; }
            set { SetProperty(ref _enAxisMeanTrend, value); }
        }

        private bool _enAxisMedianTrend = false;
        public bool EnAxisMedianTrend {
            get { return _enAxisMedianTrend; }
            set { SetProperty(ref _enAxisMedianTrend, value); }
        }

        private bool _enAxisSigma6Trend = true;
        public bool EnAxisSigma6Trend {
            get { return _enAxisSigma6Trend; }
            set { SetProperty(ref _enAxisSigma6Trend, value); }
        }

        private bool _enAxisSigma3Trend = false;
        public bool EnAxisSigma3Trend {
            get { return _enAxisSigma3Trend; }
            set { SetProperty(ref _enAxisSigma3Trend, value); }
        }

        private float _minTrend;
        public float MinTrend {
            get { return _minTrend; }
            set { SetProperty(ref _minTrend, value); }
        }

        private float _maxTrend;
        public float MaxTrend {
            get { return _maxTrend; }
            set { SetProperty(ref _maxTrend, value); }
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

        private float _sigma6LTrend;
        public float Sigma6LTrend {
            get { return _sigma6LTrend; }
            set { SetProperty(ref _sigma6LTrend, value); }
        }
        private float _sigma6HTrend;
        public float Sigma6HTrend {
            get { return _sigma6HTrend; }
            set { SetProperty(ref _sigma6HTrend, value); }
        }
        private float _sigma3LTrend;
        public float Sigma3LTrend {
            get { return _sigma3LTrend; }
            set { SetProperty(ref _sigma3LTrend, value); }
        }
        private float _sigma3HTrend;
        public float Sigma3HTrend {
            get { return _sigma3HTrend; }
            set { SetProperty(ref _sigma3HTrend, value); }
        }


        private bool _enAxisLimitHisto = true;
        public bool EnAxisLimitHisto {
            get { return _enAxisLimitHisto; }
            set { SetProperty(ref _enAxisLimitHisto, value); }
        }

        private bool _enAxisMinMaxHisto = false;
        public bool EnAxisMinMaxHisto {
            get { return _enAxisMinMaxHisto; }
            set { SetProperty(ref _enAxisMinMaxHisto, value); }
        }

        private bool _enAxisMeanHisto = false;
        public bool EnAxisMeanHisto {
            get { return _enAxisMeanHisto; }
            set { SetProperty(ref _enAxisMeanHisto, value); }
        }

        private bool _enAxisMedianHisto = false;
        public bool EnAxisMedianHisto {
            get { return _enAxisMedianHisto; }
            set { SetProperty(ref _enAxisMedianHisto, value); }
        }

        private bool _enAxisSigma6Histo = true;
        public bool EnAxisSigma6Histo {
            get { return _enAxisSigma6Histo; }
            set { SetProperty(ref _enAxisSigma6Histo, value); }
        }

        private bool _enAxisSigma3Histo = false;
        public bool EnAxisSigma3Histo {
            get { return _enAxisSigma3Histo; }
            set { SetProperty(ref _enAxisSigma3Histo, value); }
        }

        private float _minHisto;
        public float MinHisto {
            get { return _minHisto; }
            set { SetProperty(ref _minHisto, value); }
        }

        private float _maxHisto;
        public float MaxHisto {
            get { return _maxHisto; }
            set { SetProperty(ref _maxHisto, value); }
        }

        private float _meanHisto;
        public float MeanHisto {
            get { return _meanHisto; }
            set { SetProperty(ref _meanHisto, value); }
        }

        private float _meadianHisto;
        public float MedianHisto {
            get { return _meadianHisto; }
            set { SetProperty(ref _meadianHisto, value); }
        }

        private float _sigma6LHisto;
        public float Sigma6LHisto {
            get { return _sigma6LHisto; }
            set { SetProperty(ref _sigma6LHisto, value); }
        }
        private float _sigma6HHisto;
        public float Sigma6HHisto {
            get { return _sigma6HHisto; }
            set { SetProperty(ref _sigma6HHisto, value); }
        }
        private float _sigma3LHisto;
        public float Sigma3LHisto {
            get { return _sigma3LHisto; }
            set { SetProperty(ref _sigma3LHisto, value); }
        }
        private float _sigma3HHisto;
        public float Sigma3HHisto {
            get { return _sigma3HHisto; }
            set { SetProperty(ref _sigma3HHisto, value); }
        }

        #endregion

        public TrendViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateFilter);
            _ea.GetEvent<Event_ItemsSelected>().Subscribe(UpdateItems);
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

                _xRangeTrend.SetMinMax(1, _deviceCount);
                RaisePropertyChanged("XRangeTrend");

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
                    _allminTrend = statistic.MinValue;
                    _allmaxTrend = statistic.MaxValue;

                    _allsigmaLowTrend = statistic.GetSigmaRangeLow(SigmaByIdx(TrendSigmaSelectionIdx));
                    _allsigmaHighTrend = statistic.GetSigmaRangeHigh(SigmaByIdx(TrendSigmaSelectionIdx));

                    if (_selectedIds.Count == 1) {
                        var info = da.GetTestInfo(_selectedIds[0]);
                        var failRate = (statistic.FailCount * 100.0 / statistic.ValidCount).ToString("f3") + "%";
                        _itemTitleTrend = $"{_selectedIds[0]}:{info.TestText}\nmean|{statistic.MeanValue:f3}  median|{statistic.MedianValue:f3}  cpk|{statistic.Cpk:f3}  σ|{statistic.Sigma:f3}  fail|{statistic.FailCount}/{statistic.ValidCount}={failRate}";
                    } else {
                        _itemTitleTrend = _selectedIds[0];
                    }

                    MinTrend = statistic.MinValue;
                    MaxTrend = statistic.MaxValue;
                    MeanTrend = statistic.MeanValue;
                    MedianTrend = statistic.MedianValue;
                    Sigma3LTrend = statistic.GetSigmaRangeLow(3);
                    Sigma3HTrend = statistic.GetSigmaRangeHigh(3);
                    Sigma6LTrend = statistic.GetSigmaRangeLow(6);
                    Sigma6HTrend = statistic.GetSigmaRangeHigh(6);

                } else {
                    _allminTrend = statistic.MinValue < _allminTrend ? statistic.MinValue : _allminTrend;
                    _allmaxTrend = statistic.MaxValue > _allmaxTrend ? statistic.MaxValue : _allmaxTrend;

                    var sigmaLow = statistic.GetSigmaRangeLow(SigmaByIdx(TrendSigmaSelectionIdx));
                    var sigmaHigh = statistic.GetSigmaRangeHigh(SigmaByIdx(TrendSigmaSelectionIdx));

                    if (sigmaLow < _allsigmaLowTrend) _allsigmaLowTrend = sigmaLow;
                    if (sigmaHigh > _allsigmaHighTrend) _allsigmaHighTrend = sigmaHigh;

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
                    _allminHisto = statistic.MinValue;
                    _allmaxHisto = statistic.MaxValue;

                    _allsigmaLowHisto = statistic.GetSigmaRangeLow(SigmaByIdx(HistoSigmaSelectionIdx));
                    _allsigmaHighHisto = statistic.GetSigmaRangeHigh(SigmaByIdx(HistoSigmaSelectionIdx));

                    if (_selectedIds.Count == 1) {
                        var info = da.GetTestInfo(_selectedIds[0]);
                        var failRate = (statistic.FailCount * 100.0 / statistic.ValidCount).ToString("f3") + "%";
                        _itemTitleHisto = $"{_selectedIds[0]}:{info.TestText}\nmean|{statistic.MeanValue:f3}  median|{statistic.MedianValue:f3}  cpk|{statistic.Cpk:f3}  σ|{statistic.Sigma:f3}  fail|{statistic.FailCount}/{statistic.ValidCount}={failRate}";
                    } else {
                        _itemTitleHisto = _selectedIds[0];
                    }

                    MinHisto = statistic.MinValue;
                    MaxHisto = statistic.MaxValue;
                    MeanHisto = statistic.MeanValue;
                    MedianHisto = statistic.MedianValue;
                    Sigma3LHisto = statistic.GetSigmaRangeLow(3);
                    Sigma3HHisto = statistic.GetSigmaRangeHigh(3);
                    Sigma6LHisto = statistic.GetSigmaRangeLow(6);
                    Sigma6HHisto = statistic.GetSigmaRangeHigh(6);


                } else {
                    _allminHisto = statistic.MinValue < _allminHisto ? statistic.MinValue : _allminHisto;
                    _allmaxHisto = statistic.MaxValue > _allmaxHisto ? statistic.MaxValue : _allmaxHisto;

                    var sigmaLow = statistic.GetSigmaRangeLow(SigmaByIdx(HistoSigmaSelectionIdx));
                    var sigmaHigh = statistic.GetSigmaRangeHigh(SigmaByIdx(HistoSigmaSelectionIdx));

                    if (sigmaLow < _allsigmaLowHisto) _allsigmaLowHisto = sigmaLow;
                    if (sigmaHigh > _allsigmaHighHisto) _allsigmaHighHisto = sigmaHigh;

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
            _xRangeHisto.SetMinMax(actStart, actStop);
            RaisePropertyChanged("XRangeHisto");

            _yRangeHisto.SetMinMax(0, maxCnt);
            RaisePropertyChanged("YRangeHisto");

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
            var ov = 0.05 * (_allsigmaHighTrend - _allsigmaLowTrend);
            if (ov == 0) ov = 1;
            _yRangeTrend.SetMinMax(_allsigmaLowTrend-ov, _allsigmaHighTrend+ov);
            RaisePropertyChanged("YRangeTrend");
            _xRangeTrend.SetMinMax(1, _deviceCount);
            RaisePropertyChanged("XRangeTrend");

            UserTrendLowRange = _allsigmaLowTrend.ToString("f3");
            UserTrendHighRange = _allsigmaHighTrend.ToString("f3");
        }

        private DelegateCommand _CmdSelectAxisMinMaxTrend;
        public DelegateCommand CmdSelectAxisMinMaxTrend =>
            _CmdSelectAxisMinMaxTrend ?? (_CmdSelectAxisMinMaxTrend = new DelegateCommand(ExecuteCmdSelectAxisMinMaxTrend));

        void ExecuteCmdSelectAxisMinMaxTrend() {
            var ov = 0.05 * (_allmaxTrend - _allminTrend);
            if (ov == 0) ov = 1;
            _yRangeTrend.SetMinMax(_allminTrend - ov, _allmaxTrend + ov);
            RaisePropertyChanged("YRangeTrend");
            _xRangeTrend.SetMinMax(1, _deviceCount);
            RaisePropertyChanged("XRangeTrend");

            UserTrendLowRange = _allminTrend.ToString("f3");
            UserTrendHighRange = _allmaxTrend.ToString("f3");
        }

        private DelegateCommand _CmdSelectAxisLimitTrend;
        public DelegateCommand CmdSelectAxisLimitTrend =>
            _CmdSelectAxisLimitTrend ?? (_CmdSelectAxisLimitTrend = new DelegateCommand(ExecuteCmdSelectAxisLimitTrend));

        void ExecuteCmdSelectAxisLimitTrend() {
            float l = float.IsNaN(LowLimit) ? _allminTrend : LowLimit;
            float h = float.IsNaN(HighLimit) ? _allmaxTrend : HighLimit;

            var ov = 0.1 * (h - l);
            if (ov == 0) ov = 1;
            _yRangeTrend.SetMinMax(l - ov, h + ov);
            RaisePropertyChanged("YRangeTrend");
            _xRangeTrend.SetMinMax(1, _deviceCount);
            RaisePropertyChanged("XRangeTrend");

            UserTrendLowRange = l.ToString("f3");
            UserTrendHighRange = h.ToString("f3");
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
                _yRangeTrend.SetMinMax(l - ov, h + ov);
                RaisePropertyChanged("YRangeTrend");
            }
            catch {
                System.Windows.MessageBox.Show("Wrong Limit");
            }
            _xRangeTrend.SetMinMax(1, _deviceCount);
            RaisePropertyChanged("XRangeTrend");
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
            UpdateHistoSeries(_allsigmaLowHisto, _allsigmaHighHisto);

            UserHistoLowRange = _allsigmaLowHisto.ToString("f3");
            UserHistoHighRange = _allsigmaHighHisto.ToString("f3");
        }

        private DelegateCommand _CmdSelectAxisMinMaxHisto;
        public DelegateCommand CmdSelectAxisMinMaxHisto =>
            _CmdSelectAxisMinMaxHisto ?? (_CmdSelectAxisMinMaxHisto = new DelegateCommand(ExecuteCmdSelectAxisMinMaxHisto));

        void ExecuteCmdSelectAxisMinMaxHisto() {
            UpdateHistoSeries(_allminHisto, _allmaxHisto);

            UserHistoLowRange = _allminHisto.ToString("f3");
            UserHistoHighRange = _allmaxHisto.ToString("f3");
        }

        private DelegateCommand _CmdSelectAxisLimitHisto;
        public DelegateCommand CmdSelectAxisLimitHisto =>
            _CmdSelectAxisLimitHisto ?? (_CmdSelectAxisLimitHisto = new DelegateCommand(ExecuteCmdSelectAxisLimitHisto));

        void ExecuteCmdSelectAxisLimitHisto() {
            float l = float.IsNaN(LowLimit) ? _allminHisto : LowLimit;
            float h = float.IsNaN(HighLimit) ? _allmaxHisto : HighLimit;

            UpdateHistoSeries(l, h);

            UserHistoLowRange = l.ToString();
            UserHistoHighRange = h.ToString();
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
