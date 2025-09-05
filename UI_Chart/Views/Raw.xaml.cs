using DataContainer;
using FastWpfGrid;
using Prism.Events;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Controls;
using UI_Chart.ViewModels;

namespace UI_Chart.Views {
    /// <summary>
    /// Interaction logic for Raw
    /// </summary>
    public partial class Raw : UserControl, INavigationAware {
        public Raw(IRegionManager regionManager, IEventAggregator ea) {
            InitializeComponent();
            _regionManager = regionManager;
            _ea = ea;

            timer_Item.Interval = 300;
            timer_Item.Elapsed += Timer_Item_Elapsed;
        }
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;

        private Timer timer_Item = new Timer();

        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
                _subData = data;
                _rawDataModel = new FastDataGridModel(_subData);
                rawgrid.Model = _rawDataModel;

                _ea.GetEvent<Event_FilterUpdated>().Subscribe(x => {
                    if (_subData.Equals(x)) {
                        _rawDataModel.NotifyRefresh();
                    }
                });

            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return false;
            //var data = (SubData)navigationContext.Parameters["subData"];

            //return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            _ea.GetEvent<Event_FilterUpdated>().Unsubscribe(x => {
                if (_subData.Equals(x)) {
                    _rawDataModel.NotifyRefresh();
                }
            });

        }


        private FastDataGridModel _rawDataModel;

        //private async void ExportToExcelAsync() {
        //    string path;
        //    using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()) {
        //        saveFileDialog.AddExtension = true;
        //        saveFileDialog.Filter = "Excel Files | *.csv";
        //        saveFileDialog.DefaultExt = "csv";
        //        saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(_subData.StdFilePath);
        //        saveFileDialog.ValidateNames = true;
        //        if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
        //            return;
        //        }
        //        path = saveFileDialog.FileName;
        //    };

        //    await System.Threading.Tasks.Task.Run(() => {
        //        //get file path

        //        //write data
        //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //        using (var p = new ExcelPackage()) {
        //            var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);

        //            string phase = "Loading start";
        //            int percent = 0;
        //            System.Threading.Timer myTimer = new System.Threading.Timer((x) => {
        //                SetProgress(phase, percent);
        //            }, null, 100, 100);


        //            //write basic info
        //            //var ws1 = p.Workbook.Worksheets.Add("BasicInfo");

        //            //write raw data
        //            var ws2 = p.Workbook.Worksheets.Add("Raw");
        //            var testItems = dataAcquire.GetFilteredItemStatistic(_subData.FilterId);
        //            var chips = dataAcquire.GetFilteredPartIndex(_subData.FilterId);


        //            //header
        //            ws2.Cells[1, 1].Value = "Test NO";
        //            ws2.Cells[1, 2].Value = "Cord";
        //            ws2.Cells[1, 3].Value = "HardBin";
        //            ws2.Cells[1, 4].Value = "SoftBin";
        //            ws2.Cells[1, 5].Value = "Site";

        //            ws2.Cells[2, 1].Value = "Test Name";
        //            ws2.Cells[3, 1].Value = "Low Limit";
        //            ws2.Cells[4, 1].Value = "High Limit";
        //            ws2.Cells[5, 1].Value = "Unit";

        //            phase = "Export chip idx";
        //            percent = 1;

        //            int i = 6;
        //            foreach (var v in chips) {
        //                ws2.Cells[i, 1].Value = v;
        //                ws2.Cells[i, 2].Value = dataAcquire.GetWaferCord(v);
        //                ws2.Cells[i, 3].Value = dataAcquire.GetHardBin(v);
        //                ws2.Cells[i, 4].Value = dataAcquire.GetSoftBin(v);
        //                ws2.Cells[i, 5].Value = dataAcquire.GetSite(v);
        //                i++;
        //            }

        //            phase = "Export chip items";
        //            percent = 5;


        //            int col = 6;
        //            double totalItemCnt = testItems.Count();
        //            foreach (var v in testItems) {
        //                int row = 6;
        //                ws2.Cells[1, col].Value = v.TestNumber;
        //                ws2.Cells[2, col].Value = v.TestText;
        //                ws2.Cells[3, col].Value = v.LoLimit;
        //                ws2.Cells[4, col].Value = v.HiLimit;
        //                ws2.Cells[5, col].Value = v.Unit;

        //                foreach (var r in dataAcquire.GetFilteredItemData(v.TestNumber, _subData.FilterId)) {
        //                    ws2.Cells[row, col].Value = r;
        //                    row++;
        //                }
        //                col++;

        //                percent = (int)((col / totalItemCnt) * 100);
        //            }
        //            myTimer.Dispose();

        //            _ea.GetEvent<Event_Log>().Publish("Excel Writing......");
        //            ws2.Cells[1, 1, chips.Count() + 6, testItems.Count() + 6].SaveToText(new System.IO.FileInfo(path), new ExcelOutputTextFormat());
        //            //p.SaveAs(new System.IO.FileInfo(path));
        //            //File.WriteAllBytes(path, p.GetAsByteArray());  // send the file

        //        }
        //    });
        //    _ea.GetEvent<Event_Log>().Publish("Excel exported at:" + path);
        //}

        private async void ExportToExcelAsync() {
            string path;
            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()) {
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = "Excel Files | *.csv";
                saveFileDialog.DefaultExt = "csv";
                saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(_subData.StdFilePath) + "_raw";
                saveFileDialog.ValidateNames = true;
                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
                    return;
                }
                path = saveFileDialog.FileName;
            };

            string phase = "Loading start";
            int percent = 0;
            System.Threading.Timer myTimer = new System.Threading.Timer((x) => {
                SetProgress(phase, percent);
            }, null, 100, 100);

            await System.Threading.Tasks.Task.Run(() => {
                try {

                    using(System.IO.StreamWriter sw = new System.IO.StreamWriter(path)) {
                        var _da = StdDB.GetDataAcquire(_subData.StdFilePath);
                        StringBuilder sb = new StringBuilder();

                        var testItems = _da.GetFilteredItemStatistic(_subData.FilterId);
                        //header
                        sb.Append("FileName,Index,Cord,Time,HBin,SBin,Site");
                        foreach(var item in testItems) {
                            sb.Append($",{item.TestNumber}");
                        }
                        sw.WriteLine(sb.ToString());
                        sb.Clear();

                        sb.Append(",TestText,,,,,");
                        foreach (var item in testItems) {
                            sb.Append($",{item.TestText}");
                        }
                        sw.WriteLine(sb.ToString());
                        sb.Clear();

                        sb.Append(",HiLimit,,,,,");
                        foreach (var item in testItems) {
                            sb.Append($",{item.HiLimit}");
                        }
                        sw.WriteLine(sb.ToString());
                        sb.Clear();

                        sb.Append(",LoLimit,,,,,");
                        foreach (var item in testItems) {
                            sb.Append($",{item.LoLimit}");
                        }
                        sw.WriteLine(sb.ToString());
                        sb.Clear();

                        sb.Append(",Unit,,,,,");
                        foreach (var item in testItems) {
                            sb.Append($",{item.Unit}");
                        }
                        sw.WriteLine(sb.ToString());
                        sb.Clear();

                        phase = "Writing......";
                        for (int c = 2; c < _rawDataModel.ColumnCount; c++) {
                            var idx = _da.GetFilteredPartIndex(_subData.FilterId).ElementAt(c-2);
                            sb.Append($"{Path.GetFileNameWithoutExtension(_da.GetMergedSubFileByIdx(idx))},{idx.ToString()},{_da.GetWaferCord(idx)},{_da.GetTestTime(idx).ToString()},{_da.GetHardBin(idx).ToString()},{_da.GetSoftBin(idx).ToString()},{_da.GetSite(idx).ToString()}");

                            for (int r = 0; r < _rawDataModel.RowCount; r++) {
                                sb.Append($",{_rawDataModel.GetCellText(r, c)}");
                            }
                            sw.WriteLine(sb.ToString());
                            sb.Clear();
                            percent = (int)(c * 100.0 / _rawDataModel.ColumnCount);
                        }
                        sw.Close();
                    }
                }
                catch {
                    _ea.GetEvent<Event_Log>().Publish("Write failed");
                }
            });
            myTimer.Dispose();

            _ea.GetEvent<Event_Log>().Publish("Exported at:" + path);
        }



        private void SetProgress(string log, int percent) {
            _ea.GetEvent<Event_Progress>().Publish(new Tuple<string, int>(log, percent));
        }

        private void ExportToExcel_Click(object sender, System.Windows.RoutedEventArgs e) {
            ExportToExcelAsync();
        }

        private void tbTestNameFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            lock (this)
            {
                timer_Item.Stop();
                timer_Item.Start();
            }
        }

        private void Timer_Item_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                timer_Item.Stop();
                this.Dispatcher.Invoke(() => {
                    _rawDataModel.FilterColumn(1, tbTestNameFilter.Text);
                });
            }
        }

    }

}
