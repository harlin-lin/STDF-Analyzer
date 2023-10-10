using DataContainer;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using UI_Data.ViewModels;

namespace UI_Data.Views {
    /// <summary>
    /// Interaction logic for DataCorrelation
    /// </summary>
    public partial class DataCorrelation : UserControl, INavigationAware, IDataView {
        public DataCorrelation(IRegionManager regionManager, IEventAggregator ea) {
            InitializeComponent();
            _regionManager = regionManager;
            _ea = ea;

            timer_Item.Interval = 300;
            timer_Item.Elapsed += Timer_Item_Elapsed;
        }

        private double chartViewWidth = 0;

        public SubData? CurrentData { get { return null; } }
        public TabType CurrentTabType { get { return TabType.RawDataCorTab; } }

        IRegionManager _regionManager;
        IEventAggregator _ea;

        List<SubData> _subDataList = null;
        public List<SubData> SubDataList { get { return _subDataList; } }

        int _corrDataIdx = -1;

        private string _regionName;

        private DataCorr_FastDataGridModel _rawDataModel;

        private Timer timer_Item = new Timer();

        int SigmaByIdx(int idx) {
            return 6 - idx;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {

        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            if (_corrDataIdx == -1) {
                _subDataList = new List<SubData>((IEnumerable<SubData>)navigationContext.Parameters["subDataList"]);
                _corrDataIdx = (int)navigationContext.Parameters["corrDataIdx"];

                _rawDataModel = new DataCorr_FastDataGridModel(_subDataList, (CorrItemType)(cbCorrItems.SelectedIndex), toggleOutlier.IsChecked.Value, SigmaByIdx(cbOutlierSigma.SelectedIndex));
                rawGrid.Model = _rawDataModel;

                this.Tag = $"Corr_{_corrDataIdx}";

                _regionName = $"Region_Corr_{_corrDataIdx}";
                RegionManager.SetRegionName(contentCtr, _regionName);

                _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateView);

            }
        }

        private void UpdateView(SubData data) {
            if (_subDataList.Contains(data)) {
                _rawDataModel.UpdateView((CorrItemType)(cbCorrItems.SelectedIndex), toggleOutlier.IsChecked.Value, SigmaByIdx(cbOutlierSigma.SelectedIndex));
            }
        }

        private void ExportToExcel_Click(object sender, System.Windows.RoutedEventArgs e) {
            ExportToExcelAsync();
        }



        private async void ExportToExcelAsync() {
            string path;
            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()) {
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = "Excel Files | *.csv";
                saveFileDialog.DefaultExt = "csv";
                saveFileDialog.FileName = "DataCorrelation_";
                saveFileDialog.ValidateNames = true;
                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
                    return;
                }
                path = saveFileDialog.FileName;
            };

            _ea.GetEvent<Event_Log>().Publish("Writing......");
            await System.Threading.Tasks.Task.Run(() => {
                try {

                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path)) {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < _subDataList.Count; i++) {
                            sw.WriteLine($"{_subDataList[i].FilterId:X8},{_subDataList[i].StdFilePath}");
                        }

                        for (int c = 0; c < _rawDataModel.ColumnCount; c++) {
                            if (c > 0) sb.Append(',');
                            sb.Append(_rawDataModel.GetColumnHeaderText(c));
                        }
                        sw.WriteLine(sb.ToString());
                        sb.Clear();

                        for (int r = 0; r < _rawDataModel.RowCount; r++) {
                            for (int c = 0; c < _rawDataModel.ColumnCount; c++) {
                                if (c > 0) sb.Append(',');
                                sb.Append(_rawDataModel.GetCellText(r, c));
                            }
                            sw.WriteLine(sb.ToString());
                            sb.Clear();
                        }
                        sw.Close();
                    }
                } catch {
                    _ea.GetEvent<Event_Log>().Publish("Write failed");
                }
            });

            _ea.GetEvent<Event_Log>().Publish("Exported at:" + path);
        }

        private void SetProgress(string log, int percent) {
            _ea.GetEvent<Event_Progress>().Publish(new Tuple<string, int>(log, percent));
        }

        private string _selectedItem;

        private void rawGrid_SelectedCellsChanged(object sender, FastWpfGrid.SelectionChangedEventArgs e) {
            var rr = rawGrid.GetSelectedModelRows();
            if (rr is null) return;
            if (rr.Count == 0) return;
            _selectedItem = _rawDataModel.GetTestId(rr.ElementAt(0));
            if (!string.IsNullOrEmpty(_selectedItem))
                _ea.GetEvent<Event_CorrItemSelected>().Publish(new Tuple<string, IEnumerable<SubData>>(_selectedItem, _subDataList));

        }

        private DelegateCommand<object> _closeCmd;
        public DelegateCommand<object> CloseCommand =>
            _closeCmd ?? (_closeCmd = new DelegateCommand<object>(ExecuteCloseCommand));

        void ExecuteCloseCommand(object x) {
            _regionManager.Regions["Region_DataView"].Remove(x);
        }

        private void tbTestNameFilter_TextChanged(object sender, TextChangedEventArgs e) {
            lock (this) {
                timer_Item.Stop();
                timer_Item.Start();
            }
        }

        private void Timer_Item_Elapsed(object sender, ElapsedEventArgs e) {
            lock (this) {
                timer_Item.Stop();
                this.Dispatcher.Invoke(() => {
                    _rawDataModel.FilterColumn(2, tbTestNameFilter.Text);
                });
            }
        }

        private void splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            double minWidth = 300;
            if (mainGrid.ColumnDefinitions[3].ActualWidth > minWidth) {
                chartViewWidth = mainGrid.ColumnDefinitions[3].ActualWidth;
            } else {
                chartViewWidth = 0;
            }
        }

        private void EnableChartView() {
            if (splitter.IsEnabled == false) {
                splitter.IsEnabled = true;

                if (chartViewWidth == 0) chartViewWidth = mainGrid.ActualWidth * 0.6;

                mainGrid.ColumnDefinitions[3].Width = new GridLength(chartViewWidth);
            }
        }

        private void DisableChartView() {
            if (splitter.IsEnabled == true) {
                splitter.IsEnabled = false;

                mainGrid.ColumnDefinitions[3].Width = new GridLength(0);
            }
        }

        private void ShowTrend_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (btShowTrend.IsChecked == false) {
                DisableChartView();
                _regionManager.Regions[_regionName].RemoveAll();
            } else {
                EnableChartView();
                //btShowTrend.IsChecked = false;
                ShowTrend();
            }
        }

        private void ShowTrend() {
            var parameters = new NavigationParameters();
            parameters.Add("subDataList", _subDataList);
            parameters.Add("corrDataIdx", _corrDataIdx);
            _regionManager.RequestNavigate(_regionName, "CorrChart", parameters);
        }

        private void cbCorrItems_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_corrDataIdx == -1) return;
            _rawDataModel.UpdateView((CorrItemType)(cbCorrItems.SelectedIndex), toggleOutlier.IsChecked.Value, SigmaByIdx(cbOutlierSigma.SelectedIndex));

        }

        private void toggleOutlier_Click(object sender, RoutedEventArgs e) {
            if (_corrDataIdx == -1) return;
            _rawDataModel.UpdateView((CorrItemType)(cbCorrItems.SelectedIndex), toggleOutlier.IsChecked.Value, SigmaByIdx(cbOutlierSigma.SelectedIndex));
        }

        private void cbOutlierSigma_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_corrDataIdx == -1) return;
            _rawDataModel.UpdateView((CorrItemType)(cbCorrItems.SelectedIndex), toggleOutlier.IsChecked.Value, SigmaByIdx(cbOutlierSigma.SelectedIndex));
        }

    }
}
