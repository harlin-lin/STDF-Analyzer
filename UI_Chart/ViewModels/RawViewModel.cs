using DataContainer;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System.Data;
using System.Linq;
using System.Windows;
using FastWpfGrid;
using System.Collections.Generic;
using System;
using OfficeOpenXml;
using System.Windows.Forms;

namespace UI_Chart.ViewModels {
    public class RawViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;

        //const int CNT_PER_PAGE = 10;
        //private int totalPartCnt = 0;
        //private int totalPages = 0;
        //private int currPage = 0;

        public RawViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(x => {
                if (_subData.Equals(x)) {
                    _rawDataModel.NotifyRefresh();
                }
            });

        }

        //private void InitUI() {
        //    dt.Columns.Add("Idx");
        //    dt.Columns[0].DataType = typeof(int);
        //    dt.Columns.Add("TestNumber");
        //    for (int i = 0; i < CNT_PER_PAGE; i++) {
        //        dt.Columns.Add($"{i}");
        //    }
        //}

        //private void UpdateTable() {
        //    dt.Clear();

        //    var da = StdDB.GetDataAcquire(_subData.StdFilePath);

        //    totalPartCnt = da.GetFilteredChipsCount(_subData.FilterId);
        //    totalPages = (totalPartCnt / CNT_PER_PAGE) + ((totalPartCnt % CNT_PER_PAGE) > 0 ? 1 : 0);
        //    PageCnt = $"{currPage} / {totalPages - 1}";
        //    JumpPage = $"{currPage}";

        //    //add column
        //    var offset = currPage * CNT_PER_PAGE;
        //    var viewCnt = totalPartCnt > (offset + CNT_PER_PAGE) ? CNT_PER_PAGE : totalPartCnt - offset;
        //    DataRow r, r_cord, r_time, r_hbin, r_sbin, r_site;

        //    r = dt.NewRow();
        //    r_cord = dt.NewRow();
        //    r_time = dt.NewRow();
        //    r_hbin = dt.NewRow();
        //    r_sbin = dt.NewRow();
        //    r_site = dt.NewRow();

        //    r[0] = 0;
        //    r_cord[0] = 0;
        //    r_time[0] = 0;
        //    r_hbin[0] = 0;
        //    r_sbin[0] = 0;
        //    r_site[0] = 0;

        //    r[1] = "PartIdx";
        //    r_cord[1] = "Cord";
        //    r_time[1] = "Time";
        //    r_hbin[1] = "HBin";
        //    r_sbin[1] = "SBin";
        //    r_site[1] = "Site";

        //    int i = 2;
        //    foreach (var c in da.GetFilteredPartIndex(_subData.FilterId).Skip(offset).Take(viewCnt)) {
        //        r[i] = c;
        //        r_cord[i] = da.GetWaferCord(c);
        //        r_time[i] = da.GetTestTime(c);
        //        r_hbin[i] = da.GetHardBin(c);
        //        r_sbin[i] = da.GetSoftBin(c);
        //        r_site[i] = da.GetSite(c);
        //        i++;
        //    }
        //    dt.Rows.Add(r);
        //    dt.Rows.Add(r_cord);
        //    dt.Rows.Add(r_time);
        //    dt.Rows.Add(r_hbin);
        //    dt.Rows.Add(r_sbin);
        //    dt.Rows.Add(r_site);

        //    int idx = 1;
        //    foreach (var uid in da.GetTestIDs()) {
        //        r = dt.NewRow();
        //        r[0] = idx++;
        //        r[1] = uid;
        //        i = 2;
        //        foreach (var v in da.GetFilteredItemData(uid, _subData.FilterId).Skip(offset).Take(viewCnt)) {
        //            r[i++] = v;
        //        }
        //        dt.Rows.Add(r);
        //    }
        //    RaisePropertyChanged("TestItems");
        //}

        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
                _subData = data;
                RawDataModel = new FastDataGridModel(_subData);
                //InitUI();
                //UpdateTable();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];

            return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {

        }


        private FastGridModelBase _rawDataModel;
        public FastGridModelBase RawDataModel {
            get { return _rawDataModel; }
            set { SetProperty(ref _rawDataModel, value); }
        }

        private DelegateCommand _exportToExcelCommand;
        public DelegateCommand ExportToExcelCommand =>
            _exportToExcelCommand ?? (_exportToExcelCommand = new DelegateCommand(ExportToExcelAsync));

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
                        ws2.Cells[i, 1].Value = v;
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
    }
}
