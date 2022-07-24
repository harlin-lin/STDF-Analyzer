using DataContainer;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using UI_Chart.Views;
using OfficeOpenXml;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace UI_Data.ViewModels {
    public class DataRawViewModel : BindableBase, INavigationAware, IDataView {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;
        public SubData? CurrentData { get { return _subData; } }
        public TabType CurrentTabType { get { return TabType.RawDataTab; } }

        int _fileIdx=-1;
        int _filterIdx=-1;

        private ObservableCollection<Item> _testItems;
        public ObservableCollection<Item> TestItems {
            get { return _testItems; }
            set { SetProperty(ref _testItems, value); }
        }

        private string _header;
        public string Header {
            get { return _header; }
            set { SetProperty(ref _header, value); }
        }

        private string _regionName;
        public string RegionName {
            get { return _regionName; }
            set { SetProperty(ref _regionName, value); }
        }

        public DataRawViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateView);

            InitUi();
        }

        private void UpdateView(SubData data) {
            if(data.Equals(_subData)) {
                var dataAcquire = StdDB.GetDataAcquire(data.StdFilePath);
                TestItems = new ObservableCollection<Item>(dataAcquire.GetFilteredItemStatistic(data.FilterId));
                RaisePropertyChanged("TestItems");
            }
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
                _filterIdx = (int)navigationContext.Parameters["filterIdx"];


                var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);

                TestItems = new  ObservableCollection<Item>(dataAcquire.GetFilteredItemStatistic(_subData.FilterId));

                Header = $"File_{_fileIdx}|Filter_{_filterIdx}";

                RegionName = $"Region_{_subData.FilterId:x8}";

                //ShowTrend();                
                ExecuteOpenSummaryCommand();
            }
        }

        private List<string> _selectedItemList= new List<string>();

        private void ShowTrend() {

            var parameters = new NavigationParameters();
            parameters.Add("itemList", _selectedItemList);
            parameters.Add("subData", _subData);


            _regionManager.RequestNavigate(RegionName, "Trend", parameters);
        }
        private void ShowBox() {

        }
        private void ShowWaferMap() {

            var parameters = new NavigationParameters();
            parameters.Add("subData", _subData);

            _regionManager.RequestNavigate(RegionName, "WaferMap", parameters);

        }

        private void ShowCorr() {

            var parameters = new NavigationParameters();
            parameters.Add("itemList", _selectedItemList);
            parameters.Add("subData", _subData);

            _regionManager.RequestNavigate(RegionName, "ItemCorr", parameters);


        }

        private async void ExportToExcelAsync() {
            string path;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog()) {
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = "Excel Files | *.csv";
                saveFileDialog.DefaultExt = "csv";
                saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(_subData.StdFilePath);
                saveFileDialog.ValidateNames = true;
                if (saveFileDialog.ShowDialog() != DialogResult.OK) {
                    return;
                }
                path = saveFileDialog.FileName;
            };

            #region getRcm
            //List<ItemStatistic> rcmSt = new List<ItemStatistic>();
            //List<int> wfCnt = new List<int>();
            //var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);
            //var waferId = dataAcquire.GetFilteredItemData("201008_0", _subData.FilterId).ToList();
            //var rcmValue = dataAcquire.GetFilteredItemData("620001_0", _subData.FilterId).ToList();
            //for (int i=1; i<=25; i++) {
            //    var rcmByWf = (from r in dataAcquire.GetAllIndex()
            //                where waferId[r] == i
            //                select rcmValue[r]).ToList();
            //    wfCnt.Add(rcmByWf.Count);
            //    var st = new ItemStatistic(rcmByWf, 5, 62);
            //    rcmSt.Add(st);
            //}

            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //using (var p = new ExcelPackage()) {
            //    //write raw data
            //    var ws2 = p.Workbook.Worksheets.Add("Raw");


            //    ws2.Cells[1, 1].Value = "Wafer NO";
            //    ws2.Cells[1, 2].Value = "Lo Limit";
            //    ws2.Cells[1, 3].Value = "Hi Limit";
            //    ws2.Cells[1, 4].Value = "Mean";
            //    ws2.Cells[1, 5].Value = "Min";
            //    ws2.Cells[1, 6].Value = "Max";
            //    ws2.Cells[1, 7].Value = "Cp";
            //    ws2.Cells[1, 8].Value = "Cpk";
            //    ws2.Cells[1, 9].Value = "Sigma";
            //    ws2.Cells[1, 10].Value = "Total Cnt";
            //    ws2.Cells[1, 11].Value = "Pass Cnt";
            //    ws2.Cells[1, 12].Value = "Fail Cnt";
            //    ws2.Cells[1, 13].Value = "Fail Rate";

            //    for (int i = 1; i <= 25; i++) {
            //        ws2.Cells[i+1, 1].Value = i;
            //        ws2.Cells[i + 1, 2].Value = 5;
            //        ws2.Cells[i + 1, 3].Value = 62;
            //        ws2.Cells[i + 1, 4].Value = rcmSt[i - 1].MeanValue;
            //        ws2.Cells[i + 1, 5].Value = rcmSt[i - 1].MinValue;
            //        ws2.Cells[i + 1, 6].Value = rcmSt[i - 1].MaxValue;
            //        ws2.Cells[i + 1, 7].Value = rcmSt[i - 1].Cp;
            //        ws2.Cells[i + 1, 8].Value = rcmSt[i - 1].Cpk;
            //        ws2.Cells[i + 1, 9].Value = rcmSt[i - 1].Sigma;
            //        ws2.Cells[i + 1, 10].Value = wfCnt[i - 1];
            //        ws2.Cells[i + 1, 11].Value = rcmSt[i - 1].PassCount;
            //        ws2.Cells[i + 1, 12].Value = rcmSt[i - 1].FailCount;
            //        ws2.Cells[i + 1, 13].Value = (rcmSt[i - 1].FailCount*100.0)/ wfCnt[i - 1];
            //    }

            //    ws2.Cells[1, 1, 26, 13].SaveToText(new System.IO.FileInfo(path), new ExcelOutputTextFormat());

            //}

            #endregion


            await System.Threading.Tasks.Task.Run(() => {
                //get file path

                //write data
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var p = new ExcelPackage()) {
                    var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);

                    string phase = "Loading start";
                    int percent = 0;
                    System.Threading.Timer myTimer = new System.Threading.Timer((x) => {
                        SetProgress(phase, percent);
                    }, null, 100, 100);


                    //write basic info
                    //var ws1 = p.Workbook.Worksheets.Add("BasicInfo");

                    //write raw data
                    var ws2 = p.Workbook.Worksheets.Add("Raw");
                    var testItems = dataAcquire.GetFilteredItemStatistic(_subData.FilterId);
                    var chips = dataAcquire.GetFilteredPartIndex(_subData.FilterId);


                    //header
                    ws2.Cells[1, 1].Value = "Test NO";
                    ws2.Cells[1, 2].Value = "Cord";
                    ws2.Cells[1, 3].Value = "HardBin";
                    ws2.Cells[1, 4].Value = "SoftBin";
                    ws2.Cells[1, 5].Value = "Site";

                    ws2.Cells[2, 1].Value = "Test Name";
                    ws2.Cells[3, 1].Value = "Low Limit";
                    ws2.Cells[4, 1].Value = "High Limit";
                    ws2.Cells[5, 1].Value = "Unit";

                    phase = "Export chip idx";
                    percent = 1;

                    int i = 6;
                    foreach (var v in chips) {
                        ws2.Cells[i, 1].Value = dataAcquire.GetPartId(v);
                        ws2.Cells[i, 2].Value = dataAcquire.GetWaferCord(v);
                        ws2.Cells[i, 3].Value = dataAcquire.GetHardBin(v);
                        ws2.Cells[i, 4].Value = dataAcquire.GetSoftBin(v);
                        ws2.Cells[i, 5].Value = dataAcquire.GetSite(v);
                        i++;
                    }

                    phase = "Export chip items";
                    percent = 5;


                    int col = 6;
                    double totalItemCnt = testItems.Count();
                    foreach (var v in testItems) {
                        int row = 6;
                        ws2.Cells[1, col].Value = v.TestNumber;
                        ws2.Cells[2, col].Value = v.TestText;
                        ws2.Cells[3, col].Value = v.LoLimit;
                        ws2.Cells[4, col].Value = v.HiLimit;
                        ws2.Cells[5, col].Value = v.Unit;

                        foreach (var r in dataAcquire.GetFilteredItemData(v.TestNumber, _subData.FilterId)) {
                            ws2.Cells[row, col].Value = r;
                            row++;
                        }
                        col++;

                        percent = (int)((col / totalItemCnt) * 100);
                    }
                    ws2.Cells[1, 1, chips.Count() + 6, testItems.Count() + 6].SaveToText(new System.IO.FileInfo(path), new ExcelOutputTextFormat());
                    //p.SaveAs(new System.IO.FileInfo(path));
                    //File.WriteAllBytes(path, p.GetAsByteArray());  // send the file

                    myTimer.Dispose();
                }
            });
            _ea.GetEvent<Event_Log>().Publish("Excel exported at:" + path);
        }

        private void SetProgress(string log, int percent) {
            _ea.GetEvent<Event_Progress>().Publish(new Tuple<string, int>(log, percent));
        }

        private DelegateCommand<object> _closeCmd;
        public DelegateCommand<object> CloseCommand =>
            _closeCmd ?? (_closeCmd = new DelegateCommand<object>(ExecuteCloseCommand));

        void ExecuteCloseCommand(object x) {
            //_regionManager.Regions["Region_DataView"].Remove(x);
            _ea.GetEvent<Event_CloseData>().Publish(_subData);
        }

        public DelegateCommand ShowTrendCommand { get; private set; }
        public DelegateCommand ShowBoxCommand { get; private set; }
        public DelegateCommand ShowWaferMapCommand { get; private set; }
        public DelegateCommand ShowCorrCommand { get; private set; }
        public DelegateCommand ExportToExcelCommand { get; private set; }
        private DelegateCommand openSummary;
        public DelegateCommand OpenSummaryCommand =>
            openSummary ?? (openSummary = new DelegateCommand(ExecuteOpenSummaryCommand));

        void ExecuteOpenSummaryCommand() {
            var parameters = new NavigationParameters();
            parameters.Add("subData", _subData);

            _regionManager.RequestNavigate(RegionName, "Summary", parameters);

        }

        public DelegateCommand<object> OnSelectColumn { get; private set; }
        public DelegateCommand<object> OnSelectRow { get; private set; }
        public DelegateCommand<object> OnSelectionChanged { get; private set; }

        public void InitUi() {

            ShowTrendCommand = new DelegateCommand(ShowTrend);
            ShowBoxCommand = new DelegateCommand(ShowBox);
            ShowWaferMapCommand = new DelegateCommand(ShowWaferMap);
            ShowCorrCommand = new DelegateCommand(ShowCorr);
            ExportToExcelCommand = new DelegateCommand(ExportToExcelAsync);

            OnSelectionChanged = new DelegateCommand<object>((x) => {
                _selectedItemList.Clear();
                var grid = x as System.Windows.Controls.DataGrid;
                foreach(var v in grid.SelectedItems) {
                    _selectedItemList.Add((v as Item).TestNumber);
                }

                if (_selectedItemList.Count > 0) {
                    _ea.GetEvent<Event_ItemsSelected>().Publish(new Tuple<SubData, List<string>>(_subData, _selectedItemList));
                }
            });

        }

        private DelegateCommand _showRawCommand;
        public DelegateCommand ShowRawCommand =>
            _showRawCommand ?? (_showRawCommand = new DelegateCommand(ExecuteShowRawCommand));

        void ExecuteShowRawCommand() {
            var parameters = new NavigationParameters();
            parameters.Add("subData", _subData);
            _regionManager.RequestNavigate(RegionName, "Raw", parameters);
        }



    }
}
