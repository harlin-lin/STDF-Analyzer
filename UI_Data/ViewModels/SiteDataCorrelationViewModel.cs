using DataContainer;
using OfficeOpenXml;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace UI_Data.ViewModels {
    public class SiteDataCorrelationViewModel : BindableBase, INavigationAware, IDataView {
        public TabType CurrentTabType { get { return TabType.SiteDataCorTab; } }

        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;
        public SubData? CurrentData { get { return _subData; } }
        List<SubData> _subDataList = null;
        public List<SubData> SubDataList { get { return _subDataList; } }

        private DataTable dt;
        public DataTable TestItems {
            get { return dt; }
            set { SetProperty(ref dt, value); }
        }

        private string _header;
        public string Header {
            get { return _header; }
            set { SetProperty(ref _header, value); }
        }

        //private string _regionName;
        //public string RegionName {
        //    get { return _regionName; }
        //    set { SetProperty(ref _regionName, value); }
        //}

        public SiteDataCorrelationViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateView);

        }

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

                Header = $"SiteCorr_|{_subData.FilterId:X8}";

                //RegionName = $"Region_Corr_{_corrDataIdx}";
                InitView();
                UpdateView();
            }
        }

        private void InitView() {
            dt = new DataTable();
            dt.Columns.Add("TestNumber");
            dt.Columns.Add("TestText");
            dt.Columns.Add("LoLimit");
            dt.Columns.Add("HiLimit");
            dt.Columns.Add("Unit");

            var da = StdDB.GetDataAcquire(_subData.StdFilePath);
            var sites = da.GetSites();

            for (int i = 0; i < sites.Length; i++) {
                dt.Columns.Add("Mean S:" + sites[i]);
            }
            for (int i = 0; i < sites.Length; i++) {
                dt.Columns.Add("Min S:" + sites[i]);
            }
            for (int i = 0; i < sites.Length; i++) {
                dt.Columns.Add("Max S:" + sites[i]);
            }
            for (int i = 0; i < sites.Length; i++) {
                dt.Columns.Add("Cp S:" + sites[i]);
            }
            for (int i = 0; i < sites.Length; i++) {
                dt.Columns.Add("Cpk S:" + sites[i]);
            }
            for (int i = 0; i < sites.Length; i++) {
                dt.Columns.Add("Sigma S:" + sites[i]);
            }
        }

        private void UpdateView() {
            var da = StdDB.GetDataAcquire(_subData.StdFilePath);
            var sites = da.GetSites();
            int cnt = sites.Length;

            var baseItem = da.GetFilteredItemStatistic(_subData.FilterId);

            dt.Rows.Clear();

            foreach (var v in baseItem) {
                DataRow r = dt.NewRow();
                r[0] = v.TestNumber;
                r[1] = v.TestText;
                r[2] = v.LoLimit;
                r[3] = v.HiLimit;
                r[4] = v.Unit;
                for (int i = 0; i < cnt; i++) {
                    var s = da.GetFilteredStatisticBySite(_subData.FilterId, v.TestNumber, sites[i]);
                    r[5 + i] = s.MeanValue;
                    r[5 + 1 * cnt + i] = s.MinValue;
                    r[5 + 2 * cnt + i] = s.MaxValue;
                    r[5 + 3 * cnt + i] = s.Cp;
                    r[5 + 4 * cnt + i] = s.Cpk;
                    r[5 + 5 * cnt + i] = s.Sigma;
                }
                dt.Rows.Add(r);
            }


            RaisePropertyChanged("TestItems");
        }


        private void UpdateView(SubData data) {
            if (_subData.Equals(data)) {
                UpdateView();
            }
        }

        //private string corrErrorLimit;
        //public string CorrErrorLimit {
        //    get { return corrErrorLimit; }
        //    set { SetProperty(ref corrErrorLimit, value); }
        //}

        //private string corrWarnLimit;
        //public string CorrWarnLimit {
        //    get { return corrWarnLimit; }
        //    set { SetProperty(ref corrWarnLimit, value); }
        //}

        //private DelegateCommand _applyLimt;
        //public DelegateCommand ApplyLimitCommand =>
        //    _applyLimt ?? (_applyLimt = new DelegateCommand(ExecuteApplyLimitCommand));

        //void ExecuteApplyLimitCommand() {

        //}

        private DelegateCommand _exportToExcel;
        public DelegateCommand ExportToExcelCommand =>
            _exportToExcel ?? (_exportToExcel = new DelegateCommand(ExecuteExportToExcelCommand));

        void ExecuteExportToExcelCommand() {
            ExportToExcelAsync();
        }

        private async void ExportToExcelAsync() {
            string path;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog()) {
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = "Excel Files | *.xlsx";
                saveFileDialog.DefaultExt = "csv";
                saveFileDialog.FileName = "Correlation_xxx";
                saveFileDialog.ValidateNames = true;
                if (saveFileDialog.ShowDialog() != DialogResult.OK) {
                    return;
                }
                path = saveFileDialog.FileName;
            };

            var da = StdDB.GetDataAcquire(_subData.StdFilePath);
            var sites = da.GetSites();


            await System.Threading.Tasks.Task.Run(() => {
                //get file path

                //write data
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var p = new ExcelPackage()) {
                    var ws1 = p.Workbook.Worksheets.Add("FileList");
                    for (int i = 0; i < sites.Length; i++) {
                        ws1.Cells[i + 1, 1].Value = _subData.StdFilePath;
                    }

                    //write raw data
                    var ws2 = p.Workbook.Worksheets.Add("Correlation");
                    ws2.Cells["A1"].LoadFromDataTable(TestItems, true);

                    p.SaveAs(new System.IO.FileInfo(path));
                    File.WriteAllBytes(path, p.GetAsByteArray());  // send the file

                }
            });
            _ea.GetEvent<Event_Log>().Publish("Excel exported at:" + path);
        }

        private void SetProgress(string log, int percent) {
            _ea.GetEvent<Event_Progress>().Publish(new Tuple<string, int>(log, percent));
        }

        private string _selectedItem;

        private DelegateCommand<object> _onselection;
        public DelegateCommand<object> OnSelectionChanged =>
            _onselection ?? (_onselection = new DelegateCommand<object>(ExecuteOnSelectionChanged));

        void ExecuteOnSelectionChanged(object parameter) {
            var grid = parameter as System.Windows.Controls.DataGrid;
            if (grid.SelectedItem is null) return;
            _selectedItem = (grid.SelectedItem as DataRowView).Row[0].ToString();
            _ea.GetEvent<Event_SiteCorrItemSelected>().Publish(new Tuple<string, SubData>(_selectedItem, _subData));
        }


        private DelegateCommand<object> _closeCmd;
        public DelegateCommand<object> CloseCommand =>
            _closeCmd ?? (_closeCmd = new DelegateCommand<object>(ExecuteCloseCommand));

        void ExecuteCloseCommand(object x) {
            _regionManager.Regions["Region_DataView"].Remove(x);
        }

    }
}
