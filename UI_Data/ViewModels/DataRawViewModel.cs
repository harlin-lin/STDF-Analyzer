using DataContainer;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FastWpfGrid;
using System.IO;
using System.Text;

namespace UI_Data.ViewModels {
    public class DataRawViewModel : BindableBase, INavigationAware, IDataView {
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

        int _fileIdx=-1;

        //private ObservableCollection<Item> _testItems;
        //public ObservableCollection<Item> TestItems {
        //    get { return _testItems; }
        //    set { SetProperty(ref _testItems, value); }
        //}

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
                //var dataAcquire = StdDB.GetDataAcquire(data.StdFilePath);
                //TestItems = new ObservableCollection<Item>(dataAcquire.GetFilteredItemStatistic(data.FilterId));
                //RaisePropertyChanged("TestItems");
                (_rawDataModel as DataRaw_FastDataGridModel).UpdateView();
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
                
                _subDataList = new List<SubData>();
                _subDataList.Add(_subData);

                //var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);
                //TestItems = new  ObservableCollection<Item>(dataAcquire.GetFilteredItemStatistic(_subData.FilterId));
                RawDataModel = new DataRaw_FastDataGridModel(_subData);

                Header = $"File_{_fileIdx}|{_subData.FilterId:x8}";

                RegionName = $"Region_{_subData.FilterId:x8}";

                _ea.GetEvent<Event_SubDataTabSelected>().Publish(_subData);
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
                saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(_subData.StdFilePath)+"_statistic";
                saveFileDialog.ValidateNames = true;
                if (saveFileDialog.ShowDialog() != DialogResult.OK) {
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
                }
                catch {
                    _ea.GetEvent<Event_Log>().Publish("Write failed");
                }
            });



            //await System.Threading.Tasks.Task.Run(() => {
            //    //get file path

            //    //write data
            //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //    using (var p = new ExcelPackage()) {
            //        var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);

            //        string phase = "Loading start";
            //        int percent = 0;
            //        System.Threading.Timer myTimer = new System.Threading.Timer((x) => {
            //            SetProgress(phase, percent);
            //        }, null, 100, 100);


            //        //write raw data
            //        var ws2 = p.Workbook.Worksheets.Add("Statistic");
            //        var testItems = dataAcquire.GetFilteredItemStatistic(_subData.FilterId);

            //        //header
            //        ws2.Cells[1, 1].Value = "Idx";
            //        ws2.Cells[1, 2].Value = "TestNumber";
            //        ws2.Cells[1, 3].Value = "TestText";
            //        ws2.Cells[1, 4].Value = "LoLimit";
            //        ws2.Cells[1, 5].Value = "HiLimit";
            //        ws2.Cells[1, 6].Value = "Unit";
            //        ws2.Cells[1, 7].Value = "PassCnt";
            //        ws2.Cells[1, 8].Value = "FailCnt";
            //        ws2.Cells[1, 9].Value = "FailPer";
            //        ws2.Cells[1, 10].Value = "MeanValue";
            //        ws2.Cells[1, 11].Value = "MedianValue";
            //        ws2.Cells[1, 12].Value = "MinValue";
            //        ws2.Cells[1, 13].Value = "MaxValue";
            //        ws2.Cells[1, 14].Value = "Cp";
            //        ws2.Cells[1, 15].Value = "Cpk";
            //        ws2.Cells[1, 16].Value = "Sigma";

            //        phase = "Exporting";
            //        percent = 1;

            //        int i = 2;
            //        foreach (var v in testItems) {
            //            ws2.Cells[i, 1].Value = v.Idx;
            //            ws2.Cells[i, 2].Value = v.TestNumber;
            //            ws2.Cells[i, 3].Value = v.TestText;
            //            ws2.Cells[i, 4].Value = v.LoLimit;
            //            ws2.Cells[i, 5].Value = v.HiLimit;
            //            ws2.Cells[i, 6].Value = v.Unit;
            //            ws2.Cells[i, 7].Value = v.PassCnt;
            //            ws2.Cells[i, 8].Value = v.FailCnt;
            //            ws2.Cells[i, 9].Value = v.FailPer;
            //            ws2.Cells[i, 10].Value = v.MeanValue;
            //            ws2.Cells[i, 11].Value = v.MedianValue;
            //            ws2.Cells[i, 12].Value = v.MinValue;
            //            ws2.Cells[i, 13].Value = v.MaxValue;
            //            ws2.Cells[i, 14].Value = v.Cp;
            //            ws2.Cells[i, 15].Value = v.Cpk;
            //            ws2.Cells[i, 16].Value = v.Sigma;

            //            i++;
            //        }

            //        percent = 100;
            //        myTimer.Dispose();

            //        _ea.GetEvent<Event_Log>().Publish("Excel Writing......");
            //        ws2.Cells[1, 1, i, 16].SaveToText(new System.IO.FileInfo(path), new ExcelOutputTextFormat());
            //        //p.SaveAs(new System.IO.FileInfo(path));
            //        //File.WriteAllBytes(path, p.GetAsByteArray());  // send the file

            //    }
            //});
            _ea.GetEvent<Event_Log>().Publish("Exported at:" + path);
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

            //OnSelectionChanged = new DelegateCommand<object>((x) => {
            //    _selectedItemList.Clear();
            //    var grid = x as System.Windows.Controls.DataGrid;
            //    foreach(var v in grid.SelectedItems) {
            //        _selectedItemList.Add((v as Item).TestNumber);
            //    }

            //    if (_selectedItemList.Count > 0) {
            //        _ea.GetEvent<Event_ItemsSelected>().Publish(new Tuple<SubData, List<string>>(_subData, _selectedItemList));
            //    }
            //});
            OnSelectionChanged = new DelegateCommand<object>((x) => {
                _selectedItemList.Clear();
                var grid = x as FastGridControl;
                
                foreach (var v in grid.GetSelectedModelRows()) {

                    //Console.WriteLine($"row:{v}");
                    var id = (_rawDataModel as DataRaw_FastDataGridModel).GetTestId(v);
                    if(!string.IsNullOrEmpty(id)) _selectedItemList.Add(id);
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

        private DelegateCommand _correlationBySiteCommand;
        public DelegateCommand CorrelationBySiteCommand =>
            _correlationBySiteCommand ?? (_correlationBySiteCommand = new DelegateCommand(ExecuteCorrelationBySiteCommand));

        void ExecuteCorrelationBySiteCommand() {
            var parameters = new NavigationParameters();
            parameters.Add("subData", _subData);
            _regionManager.RequestNavigate("Region_DataView", "SiteDataCorrelation", parameters);


        }

        private FastGridModelBase _rawDataModel;
        public FastGridModelBase RawDataModel {
            get { return _rawDataModel; }
            set { SetProperty(ref _rawDataModel, value); }
        }

    }
}
