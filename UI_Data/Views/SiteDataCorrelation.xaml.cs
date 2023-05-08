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
using System.Windows.Controls;
using UI_Data.ViewModels;

namespace UI_Data.Views {
    /// <summary>
    /// Interaction logic for SiteDataCorrelation
    /// </summary>
    public partial class SiteDataCorrelation : UserControl, INavigationAware, IDataView {
        public SiteDataCorrelation(IRegionManager regionManager, IEventAggregator ea) {
            InitializeComponent();
            _regionManager = regionManager;
            _ea = ea;

            timer_Item.Interval = 300;
            timer_Item.Elapsed += Timer_Item_Elapsed;
        }

        public TabType CurrentTabType { get { return TabType.SiteDataCorTab; } }

        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;
        public SubData? CurrentData { get { return _subData; } }
        List<SubData> _subDataList = null;
        public List<SubData> SubDataList { get { return _subDataList; } }

        private string _regionName;

        private SiteDataCorr_FastDataGridModel _rawDataModel;

        private Timer timer_Item = new Timer();

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {

        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            if (_subData.FilterId == 0) {
                _subData = (SubData)navigationContext.Parameters["subData"];
                _subDataList = new List<SubData>();
                _subDataList.Add(_subData);

                _rawDataModel = new SiteDataCorr_FastDataGridModel(_subData);
                rawGrid.Model = _rawDataModel;

                this.Tag = $"SiteCorr_|{_subData.FilterId:X8}";

                _regionName = $"Region_Corr_{_subData.FilterId:X8}";
                RegionManager.SetRegionName(contentCtr, _regionName);


                var parameters = new NavigationParameters();
                parameters.Add("subData", _subData);
                _regionManager.RequestNavigate(_regionName, "SiteCorrChart", parameters);

                _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateView);

            }
        }

        private void UpdateView(SubData data) {
            if (_subData.Equals(data)) {
                _rawDataModel.UpdateView();
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
                saveFileDialog.FileName = "SiteCorrelation_";
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
                _ea.GetEvent<Event_SiteCorrItemSelected>().Publish(new Tuple<string, SubData>(_selectedItem, _subData));

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
    }
}
