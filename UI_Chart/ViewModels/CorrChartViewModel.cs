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
    public class CorrChartViewModel : BindableBase {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        string _selectedId;
        List<SubData> _subDataList;


        private float _sigmaLow,_sigmaHigh, _min, _max;

        #region Binding_prop
        public ObservableCollection<IRenderableSeriesViewModel> _histoSeries = new ObservableCollection<IRenderableSeriesViewModel>();
        public ObservableCollection<IRenderableSeriesViewModel> HistoSeries {
            get { return _histoSeries; }
            set { SetProperty(ref _histoSeries, value); }
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


        #endregion

        public CorrChartViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_CorrItemSelected>().Subscribe(UpdateItems);

            InitUi();
        }
        void UpdateItems(Tuple<string, IEnumerable<SubData>> para) {
            _subDataList = new List<SubData>(para.Item2);
            _selectedId = para.Item1;

            bool axisFlg = false;

            for (int i = 0; i < (_subDataList.Count > 16 ? 16 : _subDataList.Count); i++) {
                var da = StdDB.GetDataAcquire(_subDataList[i].StdFilePath);
                if (!da.IfFilterContainsTestId(_subDataList[i].FilterId, _selectedId)) continue;

                var data = da.GetFilteredItemData(_selectedId, _subDataList[i].FilterId);

                var statistic = da.GetFilteredStatistic(_subDataList[i].FilterId, _selectedId);
                if (axisFlg == false) {
                    _min = statistic.MinValue ?? 0;
                    _max = statistic.MaxValue ?? 1;

                    _sigmaLow = (statistic.MeanValue ?? 0) - (statistic.Sigma ?? 1) * 6;
                    _sigmaHigh = (statistic.MeanValue ?? 0) + (statistic.Sigma ?? 1) * 6;

                    var idInfo = da.GetTestInfo(_selectedId);
                    LowLimit = idInfo.LoLimit ?? _min;
                    HighLimit = idInfo.HiLimit ?? _max;
                    axisFlg = true;
                } else {
                    if (statistic.MinValue.HasValue) {
                        _min = statistic.MinValue.Value < _min ? statistic.MinValue.Value : _min;
                    }
                    if (statistic.MaxValue.HasValue) {
                        _max = statistic.MaxValue.Value > _max ? statistic.MaxValue.Value : _max;
                    }

                    var sigmaLow = (statistic.MeanValue ?? 0) - (statistic.Sigma ?? 1) * 6;
                    var sigmaHigh = (statistic.MeanValue ?? 0) + (statistic.Sigma ?? 1) * 6;

                    if (sigmaLow < _sigmaLow) _sigmaLow = sigmaLow;
                    if (sigmaHigh > _sigmaHigh) _sigmaHigh = sigmaHigh;
                }

            }


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
            if (_selectedId == null || _subDataList.Count == 0) return;
            var maxCnt=0;
            HistoSeries.Clear();
            for (int i = 0; i < (_subDataList.Count > 16 ? 16 : _subDataList.Count); i++) {

                var da = StdDB.GetDataAcquire(_subDataList[i].StdFilePath);
                if (!da.IfFilterContainsTestId(_subDataList[i].FilterId, _selectedId)) continue;

                var data = da.GetFilteredItemData(_selectedId, _subDataList[i].FilterId);

                var histo = GetHistogramData(start, stop, data);
                var series = new XyDataSeries<float, int>();
                series.Append(histo.Item1, histo.Item2);
                series.SeriesName = $"F_{i}:{_subDataList[i].FilterId:X8}";

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

        private DelegateCommand<object> _CmdSaveHisto;
        public DelegateCommand<object> CmdSaveHisto =>
            _CmdSaveHisto ?? (_CmdSaveHisto = new DelegateCommand<object>(ExecuteCmdSaveHisto));

        void ExecuteCmdSaveHisto(object e) {
            string filePath;
            if (_selectedId == null || _subDataList.Count == 0) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            string dftName = _selectedId + "_CorrHisto";
            if (_subDataList.Count > 1) dftName += "_cmp";
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
