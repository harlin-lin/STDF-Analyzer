using DataContainer;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Utils;


namespace UI_Chart.ViewModels {
    public class SiteCorrChartViewModel : BindableBase {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        string _selectedId;
        SubData _subData;


        private float _sigmaLow, _sigmaHigh, _min, _max;

        #region Binding_prop
        private List<(double[], double[])> _histoSeries = new List<(double[], double[])>();
        public List<(double[], double[])> HistoSeries {
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

        private (double,double) _xRangeHisto = (0, 1);
        public (double, double) XRangeHisto {
            get { return _xRangeHisto; }
            set { SetProperty(ref _xRangeHisto, value); }
        }

        private (double, double) _yRangeHisto = (0, 1);
        public (double, double) YRangeHisto {
            get { return _yRangeHisto; }
            set { SetProperty(ref _yRangeHisto, value); }
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

        private string _correlationSummary = "";
        public string CorrelationSummary {
            get { return _correlationSummary; }
            set { SetProperty(ref _correlationSummary, value); }
        }

        private string _itemTitle;
        public string ItemTitle {
            get { return _itemTitle; }
            set { SetProperty(ref _itemTitle, value); }
        }

        private bool _ignoreOutlierHisto = true;
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

        int SigmaByIdx(int idx) {
            return 6 - idx;
        }
        #endregion

        public SiteCorrChartViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_SiteCorrItemSelected>().Subscribe(UpdateItems);
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateView);
        }

        private void UpdateView(SubData data) {
            if (_subData.Equals(data)) {
                UpdateView();
            }
        }

        private void UpdateView() {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_subData = new StringBuilder();
            StringBuilder sb_mean = new StringBuilder();
            StringBuilder sb_median = new StringBuilder();
            StringBuilder sb_min = new StringBuilder();
            StringBuilder sb_max = new StringBuilder();
            StringBuilder sb_cp = new StringBuilder();
            StringBuilder sb_cpk = new StringBuilder();
            StringBuilder sb_sigma = new StringBuilder();

            sb_subData.Append($"{"Site:",-13}");
            sb_mean.Append($"{"Mean:",-13}");
            sb_median.Append($"{"Median:",-13}");
            sb_min.Append($"{"Min:",-13}");
            sb_max.Append($"{"Max:",-13}");
            sb_cp.Append($"{"CP:",-13}");
            sb_cpk.Append($"{"CPK:",-13}");
            sb_sigma.Append($"{"Sigma:",-13}");

            var da = StdDB.GetDataAcquire(_subData.StdFilePath);
            var sites = da.GetSites();
            for (int i = 0; i < (sites.Length > 16 ? 16 : sites.Length); i++) {
                if (!da.IfContainsTestId(_selectedId)) continue;

                var statistic_raw = da.GetFilteredStatisticBySite(_subData.FilterId, _selectedId, sites[i]);
                ItemStatistic statistic;

                if (_ignoreOutlierHisto) {
                    statistic = da.GetFilteredStatisticIgnoreOutlierBySite(_subData.FilterId, _selectedId, SigmaByIdx(OutlierRangeIdxHisto), sites[i]);
                } else {
                    statistic = statistic_raw;
                }

                //var statistic = da.GetFilteredStatistic(_subDataList[i].FilterId, _selectedId);

                sb_subData.Append($"{sites[i].ToString(),-13}");
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
                    LowLimit = idInfo.LoLimit ?? _min;
                    HighLimit = idInfo.HiLimit ?? _max;

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

            CorrelationSummary = sb.ToString();

            _itemTitle += sb_mean;
            RaisePropertyChanged("ItemTitle");

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



        void UpdateItems(Tuple<string, SubData> para) {
            _subData = para.Item2;
            _selectedId = para.Item1;

            UpdateView();
        }

        //default 100 bins, and enable outliers count, total 112bins
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
                if (float.IsNaN(f) || float.IsInfinity(f)) continue;
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

        void UpdateHistoSeries(float start, float stop) {
            if (_selectedId == null || _subData.FilterId == 0) return;
            double maxCnt = 0;
            _histoSeries.Clear();

            if (float.IsNaN(start) || float.IsInfinity(start) || float.IsNaN(stop) || float.IsInfinity(stop)) return;

            var da = StdDB.GetDataAcquire(_subData.StdFilePath);
            var sites = da.GetSites();
            for (int i = 0; i < (sites.Length > 16 ? 16 : sites.Length); i++) {

                if (!da.IfContainsTestId(_selectedId)) continue;

                var data = da.GetFilteredItemDataBySite(_selectedId, _subData.FilterId, sites[i]);

                var histo = GetHistogramData(start, stop, data);
                _histoSeries.Add(histo);

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
            _xRangeHisto = (actStart, actStop);
            RaisePropertyChanged("XRangeHisto");

            _yRangeHisto = (0, maxCnt);
            RaisePropertyChanged("YRangeHisto");

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
            } catch (Exception) {
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
            if (_selectedId == null || _subData.FilterId == 0) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            string dftName = _selectedId + "_SiteCorrHisto";
            if (GetAndCheckPath("PNG | *.png", dftName, out filePath)) {
                (e as ScottPlot.WpfPlot).Plot.SaveFig(filePath);
            }

        }

        private DelegateCommand<object> _CmdCopy;
        public DelegateCommand<object> CmdCopy =>
            _CmdCopy ?? (_CmdCopy = new DelegateCommand<object>(ExecuteCmdCopy));

        void ExecuteCmdCopy(object e) {
            if (_selectedId == null || _subData.FilterId == 0) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            var image = (e as ScottPlot.WpfPlot).Plot.GetBitmap();
            System.Windows.Clipboard.SetImage(System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            image.GetHbitmap(),
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()));

            _ea.GetEvent<Event_Log>().Publish("Copied to clipboard");
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
            } catch {
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

        private DelegateCommand _cmdChangedSigmaOutlierIdxHisto;
        public DelegateCommand CmdChangedSigmaOutlierIdxHisto =>
            _cmdChangedSigmaOutlierIdxHisto ?? (_cmdChangedSigmaOutlierIdxHisto = new DelegateCommand(ExecuteCmdChangedSigmaOutlierIdxHisto));

        void ExecuteCmdChangedSigmaOutlierIdxHisto() {
            UpdateView();
        }

        private DelegateCommand _cmdToggleOutlierHisto;
        public DelegateCommand CmdToggleOutlierHisto =>
            _cmdToggleOutlierHisto ?? (_cmdToggleOutlierHisto = new DelegateCommand(ExecuteCmdToggleOutlierHisto));

        void ExecuteCmdToggleOutlierHisto() {
            UpdateView();
        }
    }
}
