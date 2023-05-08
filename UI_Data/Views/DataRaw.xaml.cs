using DataContainer;
using FastWpfGrid;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using UI_Data.ViewModels;

namespace UI_Data.Views {
    /// <summary>
    /// Interaction logic for DataRaw
    /// </summary>
    public partial class DataRaw : UserControl, INavigationAware, IDataView {
        public DataRaw(IRegionManager regionManager, IEventAggregator ea) {
            InitializeComponent();
            _regionManager = regionManager;
            _ea = ea;

            timer_Item.Interval = 300;
            timer_Item.Elapsed += Timer_Item_Elapsed;
        }

        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;
        public SubData? CurrentData {
            get {
                if (_subData.StdFilePath is null)
                    return null;
                else
                    return _subData;
            }
        }
        List<SubData> _subDataList = null;
        public List<SubData> SubDataList { get { return _subDataList; } }

        public TabType CurrentTabType { get { return TabType.RawDataTab; } }

        int _fileIdx = -1;

        private string _regionName;

        private void UpdateView(SubData data) {
            if (data.Equals(_subData)) {
                _rawDataModel.UpdateView();
            }
        }

        private DataRaw_FastDataGridModel _rawDataModel;

        private Timer timer_Item = new Timer();


        private DelegateCommand<object> _closeCmd;
        public DelegateCommand<object> CloseCommand =>
            _closeCmd ?? (_closeCmd = new DelegateCommand<object>(ExecuteCloseCommand));

        void ExecuteCloseCommand(object x) {
            //_regionManager.Regions["Region_DataView"].Remove(x);
            _ea.GetEvent<Event_CloseData>().Publish(_subData);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];

            return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            ;
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            if (_fileIdx == -1) {
                _subData = (SubData)navigationContext.Parameters["subData"];
                _fileIdx = (int)navigationContext.Parameters["fileIdx"];

                _subDataList = new List<SubData>();
                _subDataList.Add(_subData);


                _rawDataModel = new DataRaw_FastDataGridModel(_subData);
                rawGrid.Model = _rawDataModel;

                this.Tag = $"File_{_fileIdx}|{_subData.FilterId:x8}";

                _regionName = $"Region_{_subData.FilterId:x8}";
                RegionManager.SetRegionName(contentCtr, _regionName);

                _ea.GetEvent<Event_SubDataTabSelected>().Publish(_subData);
                //ShowSummary();

                _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateView);

            }
        }

        private List<string> _selectedItemList = new List<string>();

        private void ShowSummary() {
            var parameters = new NavigationParameters();
            parameters.Add("subData", _subData);

            _regionManager.RequestNavigate(_regionName, "Summary", parameters);
        }

        private void ShowTrend() {

            var parameters = new NavigationParameters();
            parameters.Add("itemList", _selectedItemList);
            parameters.Add("subData", _subData);


            _regionManager.RequestNavigate(_regionName, "Trend", parameters);
        }

        private void ShowRaw() {
            var parameters = new NavigationParameters();
            parameters.Add("subData", _subData);
            _regionManager.RequestNavigate(_regionName, "Raw", parameters);
        }
        private void ShowBox() {

        }
        private void ShowWaferMap() {

            var parameters = new NavigationParameters();
            parameters.Add("subData", _subData);

            _regionManager.RequestNavigate(_regionName, "WaferMap", parameters);

        }

        private void ShowCorr() {

            var parameters = new NavigationParameters();
            parameters.Add("itemList", _selectedItemList);
            parameters.Add("subData", _subData);

            _regionManager.RequestNavigate(_regionName, "ItemCorr", parameters);
        }
        
        private void ShowSiteCorr() {
            var parameters = new NavigationParameters();
            parameters.Add("subData", _subData);
            _regionManager.RequestNavigate("Region_DataView", "SiteDataCorrelation", parameters);
        }

        private async void ExportToExcelAsync() {
            string path;
            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()) {
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = "Excel Files | *.csv";
                saveFileDialog.DefaultExt = "csv";
                saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(_subData.StdFilePath) + "_statistic";
                saveFileDialog.ValidateNames = true;
                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
                    return;
                }
                path = saveFileDialog.FileName;
            };

            _ea.GetEvent<Event_Log>().Publish("Writing......");
            await System.Threading.Tasks.Task.Run(() => {
                try {
                    using (var sw = new StreamWriter(path)) {
                        StringBuilder sb = new StringBuilder();

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


        private void rawGrid_SelectedCellsChanged(object sender, FastWpfGrid.SelectionChangedEventArgs e) {
            _selectedItemList.Clear();

            foreach (var v in rawGrid.GetSelectedModelRows()) {

                //Console.WriteLine($"row:{v}");
                var id = _rawDataModel.GetTestId(v);
                if (!string.IsNullOrEmpty(id)) _selectedItemList.Add(id);
            }

            if (_selectedItemList.Count > 0) {
                _ea.GetEvent<Event_ItemsSelected>().Publish(new Tuple<SubData, List<string>>(_subData, _selectedItemList));
            }

        }

        private void rawGrid_ColumnHeaderDoubleClick(object arg1, FastWpfGrid.ColumnClickEventArgs arg2) {
            _rawDataModel.SortColumn(arg2.Column);
        }

        private void EnableChartView() {
            if (splitter.IsEnabled == false) {
                splitter.IsEnabled = true;
                ThicknessAnimation marginAnimation = new ThicknessAnimation();
                marginAnimation.From = new System.Windows.Thickness(0, 0, -contentCtr.ActualWidth, 0);
                marginAnimation.To = new System.Windows.Thickness(0, 0, 0, 0);
                marginAnimation.Duration = TimeSpan.FromMilliseconds(300);

                contentCtr.BeginAnimation(FrameworkElement.MarginProperty, marginAnimation);
            }
        }

        private void DisableChartView() {
            if (splitter.IsEnabled == true) {
                splitter.IsEnabled = false;

                ThicknessAnimation marginAnimation = new ThicknessAnimation();
                marginAnimation.From = new System.Windows.Thickness(0, 0, 0, 0);
                marginAnimation.To = new System.Windows.Thickness(0, 0, -contentCtr.ActualWidth, 0);
                marginAnimation.Duration = TimeSpan.FromMilliseconds(300);

                contentCtr.BeginAnimation(FrameworkElement.MarginProperty, marginAnimation);
            }
        }

        private void OpenSummary_Click(object sender, System.Windows.RoutedEventArgs e) {
            if(btOpenSummary.IsChecked == false) {
                DisableChartView();
                _regionManager.Regions[_regionName].RemoveAll();
            } else {
                EnableChartView();
                //btOpenSummary.IsChecked = false;
                btShowTrend.IsChecked = false;
                btShowRaw.IsChecked = false;
                btShowCorr.IsChecked = false;
                btShowWaferMap.IsChecked = false;
                ShowSummary();
            }
        }

        private void ShowTrend_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (btShowTrend.IsChecked == false) {
                DisableChartView();
                _regionManager.Regions[_regionName].RemoveAll();
            } else {
                EnableChartView();
                btOpenSummary.IsChecked = false;
                //btShowTrend.IsChecked = false;
                btShowRaw.IsChecked = false;
                btShowCorr.IsChecked = false;
                btShowWaferMap.IsChecked = false;
                ShowTrend();
            }
        }

        private void ShowRaw_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (btShowRaw.IsChecked == false) {
                DisableChartView();
                _regionManager.Regions[_regionName].RemoveAll();
            } else {
                EnableChartView();
                btOpenSummary.IsChecked = false;
                btShowTrend.IsChecked = false;
                //btShowRaw.IsChecked = false;
                btShowCorr.IsChecked = false;
                btShowWaferMap.IsChecked = false;
                ShowRaw();
            }
        }

        private void ShowCorr_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (btShowCorr.IsChecked == false) {
                DisableChartView();
                _regionManager.Regions[_regionName].RemoveAll();
            } else {
                EnableChartView();
                btOpenSummary.IsChecked = false;
                btShowTrend.IsChecked = false;
                btShowRaw.IsChecked = false;
                //btShowCorr.IsChecked = false;
                btShowWaferMap.IsChecked = false;
                ShowCorr();
            }
        }

        private void ShowWaferMap_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (btShowWaferMap.IsChecked == false) {
                DisableChartView();
                _regionManager.Regions[_regionName].RemoveAll();
            } else {
                EnableChartView();
                btOpenSummary.IsChecked = false;
                btShowTrend.IsChecked = false;
                btShowRaw.IsChecked = false;
                btShowCorr.IsChecked = false;
                //btShowWaferMap.IsChecked = false;
                ShowWaferMap();
            }
        }

        private void CorrelationBySite_Click(object sender, System.Windows.RoutedEventArgs e) {
            ShowSiteCorr();
        }

        private void ExportToExcel_Click(object sender, System.Windows.RoutedEventArgs e) {
            ExportToExcelAsync();
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

    }
}
