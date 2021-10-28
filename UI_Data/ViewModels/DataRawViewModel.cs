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
        public SubData CurrentData { get { return _subData; } }

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
                TestItems = new ObservableCollection<Item>(dataAcquire.GetFilteredItems(data.FilterId));
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

                TestItems = new  ObservableCollection<Item>(dataAcquire.GetFilteredItems(_subData.FilterId));

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

            await System.Threading.Tasks.Task.Run(() => {
                //get file path

                //write data
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var p = new ExcelPackage()) {
                    var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);

                    string phase="Loading start";
                    int percent = 0;
                    System.Threading.Timer myTimer = new System.Threading.Timer((x) => {
                        SetProgress(phase, percent);
                    }, null, 100, 100);


                    //write basic info
                    //var ws1 = p.Workbook.Worksheets.Add("BasicInfo");

                    //write raw data
                    var ws2 = p.Workbook.Worksheets.Add("Raw");
                    var testItems = dataAcquire.GetFilteredItems(_subData.FilterId);
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

                        percent = (int)((col/totalItemCnt)*100);
                    }
                    ws2.Cells[1,1, chips.Count()+6 , testItems.Count() + 6].SaveToText(new System.IO.FileInfo(path), new ExcelOutputTextFormat());
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
            _closeCmd ?? (_closeCmd = new DelegateCommand<object>(ExecuteCommandName));

        void ExecuteCommandName(object x) {
            //_regionManager.Regions["Region_DataView"].Remove(x);
            _ea.GetEvent<Event_CloseData>().Publish(_subData);
        }

        public DelegateCommand ShowTrendCommand { get; private set; }
        public DelegateCommand ShowBoxCommand { get; private set; }
        public DelegateCommand ShowWaferMapCommand { get; private set; }
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



    }
}
