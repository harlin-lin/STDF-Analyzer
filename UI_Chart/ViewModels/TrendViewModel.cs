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

        private float _sigmaLow,_sigmaHigh, _min, _max, _lowLimit, _highLimit;

        private XyDataSeries<double, double> _trendData= new XyDataSeries<double, double>();
        public XyDataSeries<double, double> TrendData {
            get { return _trendData; }
            set { SetProperty(ref _trendData, value); }
        }
        public ObservableCollection<IRenderableSeriesViewModel> _trendSeries= new ObservableCollection<IRenderableSeriesViewModel>();
        public ObservableCollection<IRenderableSeriesViewModel> TrendSeries { 
            get { return _trendSeries; }
            set { SetProperty(ref _trendSeries, value); }
        }

        private IAxisViewModel _xAxis;
        public IAxisViewModel XAxis {
            get { return _xAxis; }
            set { SetProperty(ref _xAxis, value); }
        }
        private IAxisViewModel _yAxis;
        public IAxisViewModel YAxis {
            get { return _yAxis; }
            set { SetProperty(ref _yAxis, value); }
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


            //var data = (from r in da.GetFilteredItemData(_selectedIds[0], _subData.FilterId)
            //            select (double)r);
            //var xs = Enumerable.Range(0, data.Count()).Select(x=> (double)x);

            //var statistic = da.GetFilteredStatistic(_subData.FilterId, _selectedIds[0]);
            //_min = statistic.MinValue ?? 0;
            //_max = statistic.MaxValue ?? 1;

            //_sigmaLow = statistic.MeanValue??0 - statistic.Sigma??1 * 6;
            //_sigmaHigh = statistic.MeanValue??0 + statistic.Sigma??1 * 6;

            //var idInfo = da.GetTestInfo(_selectedIds[0]);
            //_lowLimit = idInfo.GetUnScaledLowLimit() ?? _min;
            //_highLimit = idInfo.GetUnScaledHighLimit() ?? _max;


            //TrendData.Clear();
            //TrendData.Append(xs, data);
            //RaisePropertyChanged("TrendData");

            var xs = Enumerable.Range(1, da.GetFilteredChipsCount(_subData.FilterId));
            TrendSeries.Clear();
            for(int i=0; i< (_selectedIds.Count>16 ? 16: _selectedIds.Count); i++) {
                var data = from r in da.GetFilteredItemData(_selectedIds[i], _subData.FilterId)
                            select r;

                var statistic = da.GetFilteredStatistic(_subData.FilterId, _selectedIds[i]);
                if (i == 0) {
                    _min = statistic.MinValue ?? 0;
                    _max = statistic.MaxValue ?? 1;

                    _sigmaLow = (statistic.MeanValue ?? 0) - (statistic.Sigma?? 1) * 6;
                    _sigmaHigh = (statistic.MeanValue ?? 0) + (statistic.Sigma ?? 1) * 6;

                    var idInfo = da.GetTestInfo(_selectedIds[0]);
                    _lowLimit = idInfo.GetUnScaledLowLimit() ?? _min;
                    _highLimit = idInfo.GetUnScaledHighLimit() ?? _max;
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

                TrendSeries.Add(new LineRenderableSeriesViewModel {
                    DataSeries = series,
                    Stroke = GetColor(i)
                });
            }
            RaisePropertyChanged("TrendSeries");

            //set the y axix
            if (IfTrendLimitBySigma) {
                ExecuteCmdSelectAxisSigma();
            }else if (IfTrendLimitByMinMax) {
                ExecuteCmdSelectAxisMinMax();
            }else if (IfTrendLimitByLimit) {
                ExecuteCmdSelectAxisLimit();
            }else {
                ExecuteCmdSelectAxisUser();
            }

            XAxis.VisibleRange.SetMinMax(1, xs.Count());
            RaisePropertyChanged("XAxis");
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


        void InitUi() {
            XAxis = new NumericAxisViewModel {
                //AxisTitle = "XAxis",
                DrawMinorGridLines = false,
                DrawMajorBands = false,
                DrawMajorGridLines = true,
                TextFormatting = "#",
                FontSize = 8,
                VisibleRange = new DoubleRange(1, 1),
            };
            YAxis = new NumericAxisViewModel {
                AxisAlignment= AxisAlignment.Right,
                //AxisTitle = "YAxis",
                DrawMinorGridLines = false,
                DrawMajorBands = false,
                DrawMajorGridLines = true,
                TextFormatting = "e2",
                FontSize=8,
                VisibleRange = new DoubleRange(0, 1),
            };

            IfTrendLimitBySigma = true;
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
            string dftName = _selectedIds[0];
            if (_selectedIds.Count > 1) dftName += "_cmp";
            if (GetAndCheckPath("PNG | *.png", dftName, out filePath)) {
                (e as SciChartSurface).ExportToFile(filePath, SciChart.Core.ExportType.Png, false);
            }
            
        }

        private DelegateCommand _CmdSelectAxisSigma;
        public DelegateCommand CmdSelectAxisSigma =>
            _CmdSelectAxisSigma ?? (_CmdSelectAxisSigma = new DelegateCommand(ExecuteCmdSelectAxisSigma));

        void ExecuteCmdSelectAxisSigma() {
            YAxis.VisibleRange.SetMinMax(_sigmaLow, _sigmaHigh);
            RaisePropertyChanged("YAxis");
            UserTrendLowRange = _sigmaLow.ToString();
            UserTrendHighRange = _sigmaHigh.ToString();
        }

        private DelegateCommand _CmdSelectAxisMinMax;
        public DelegateCommand CmdSelectAxisMinMax =>
            _CmdSelectAxisMinMax ?? (_CmdSelectAxisMinMax = new DelegateCommand(ExecuteCmdSelectAxisMinMax));

        void ExecuteCmdSelectAxisMinMax() {
            YAxis.VisibleRange.SetMinMax(_min, _max);
            RaisePropertyChanged("YAxis");
            UserTrendLowRange = _min.ToString();
            UserTrendHighRange = _max.ToString();
        }

        private DelegateCommand _CmdSelectAxisLimit;
        public DelegateCommand CmdSelectAxisLimit =>
            _CmdSelectAxisLimit ?? (_CmdSelectAxisLimit = new DelegateCommand(ExecuteCmdSelectAxisLimit));

        void ExecuteCmdSelectAxisLimit() {
            YAxis.VisibleRange.SetMinMax(_lowLimit, _highLimit);
            RaisePropertyChanged("YAxis");
            UserTrendLowRange = _lowLimit.ToString();
            UserTrendHighRange = _highLimit.ToString();
        }

        private DelegateCommand _CmdSelectAxisUser;
        public DelegateCommand CmdSelectAxisUser =>
            _CmdSelectAxisUser ?? (_CmdSelectAxisUser = new DelegateCommand(ExecuteCmdSelectAxisUser));

        void ExecuteCmdSelectAxisUser() {
            float l, h;
            try {
                float.TryParse(UserTrendLowRange, out l);
                float.TryParse(UserTrendHighRange, out h);
                YAxis.VisibleRange.SetMinMax(l, h);
                RaisePropertyChanged("YAxis");
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
            ExecuteCmdSelectAxisUser();
        }

    }
}
