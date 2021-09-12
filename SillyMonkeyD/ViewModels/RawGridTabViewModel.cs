using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;
using OfficeOpenXml;

namespace SillyMonkeyD.ViewModels {
    public delegate void SelectedTestItemsEventHandler(List<TestID> iDs);
    public class RawGridTabViewModel : ViewModelBase, ITab {
        public RawGridTabViewModel(IDataAcquire dataAcquire, int filterId, TabItem tab) {
            Init(dataAcquire, filterId);
            CorrespondingTab = tab;
            InitUI();
        }

        public int FilterId { get; private set; }
        public IDataAcquire DataAcquire { get; private set; }

        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public int WindowFlag { get; private set; }
        public TabType TabType => TabType.RawDataTab;
        public bool IsMainTab { get { return true; } }
        public Thickness LocationInTablist { get { return IsMainTab ? new Thickness(0, 0, 3, 0) : new Thickness(25, 0, 3, 0); } }
        public TabItem CorrespondingTab { get; }

        public StdLogGridModel Data { get { return GetProperty(() => Data); } private set { SetProperty(() => Data, value); } }

        public SelectedTestItemsEventHandler SelectedTestItemsEvent;

        private void Init(IDataAcquire dataAcquire, int filterId) {
            DataAcquire = dataAcquire;
            FilterId = filterId;
            WindowFlag = 1;

            var i = dataAcquire.GetFilterIndex(filterId);

            if (DataAcquire.FileName.Length > 15)
                TabTitle = DataAcquire.FileName.Substring(0, 15) + "..." + $"-F{i}-RAW";
            else
                TabTitle = DataAcquire.FileName + $"-F{i}-RAW";
            FilePath = DataAcquire.FilePath;

            Data = new StdLogGridModel(DataAcquire, FilterId);
        }


        public void UpdateFilter() {
            Data.Update();
            RaisePropertyChanged("Data");
        }

        #region UI
        public ICommand ExportToExcel { get; private set; }
        public ICommand CreateHistogram { get; private set; }

        private void InitUI() {

            CreateHistogram = new DelegateCommand(() => {
                ;
            });

            ExportToExcel = new DelegateCommand(() => {
            bool rotate = true;
            ////check the data length is legal
            ////max col length is 16384, row length 1048577
            //var cnt = DataAcquire.GetFilteredChipsCount(FilterId);
            //if ( cnt > 16380 && cnt < 1048570) { 
            //    if(System.Windows.MessageBox.Show("Chip count larger than 16380, will rotate the sheet!", "Warning", MessageBoxButton.OK) == MessageBoxResult.OK) {
            //        rotate = true;
            //    }
            //} else if(cnt >= 1048570) {
            //    if (System.Windows.MessageBox.Show("Chip count larger than 1048570, please filter the chip count!", "Warning", MessageBoxButton.OK) == MessageBoxResult.OK) {
            //        return;
            //    }
            //}

            //get file path
            using (SaveFileDialog saveFileDialog = new SaveFileDialog()) {
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.ValidateNames = true;
                if (saveFileDialog.ShowDialog() != DialogResult.OK) {
                    return;
                }
                var path = saveFileDialog.FileName;

                //write data
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var p = new ExcelPackage()) {
                    //write basic info
                    //var ws1 = p.Workbook.Worksheets.Add("BasicInfo");

                    //write raw data
                    var ws2 = p.Workbook.Worksheets.Add("Raw");
                    var testIds = DataAcquire.GetFilteredTestIDs(FilterId);
                    var testInfos = DataAcquire.GetFilteredTestIDs_Info(FilterId);
                    var statistic = DataAcquire.GetFilteredStatistic(FilterId);
                    var chips = DataAcquire.GetFilteredChipsInfo(FilterId);

                    //header
                    if (rotate) {
                        ws2.Cells[1, 1].Value = "Test NO";
                        ws2.Cells[2, 1].Value = "Test Name";
                        ws2.Cells[3, 1].Value = "Low Limit";
                        ws2.Cells[4, 1].Value = "High Limit";
                        ws2.Cells[5, 1].Value = "Unit";
                        //ws2.Cells[6, 1].Value = "Min";
                        //ws2.Cells[7, 1].Value = "Max";
                        //ws2.Cells[8, 1].Value = "Mean";
                        //ws2.Cells[9, 1].Value = "Sigma";
                        //ws2.Cells[10, 1].Value = "Cpk";
                        //ws2.Cells[11, 1].Value = "Pass Cnt";

                        for (int i = 0; i < chips.Count; i++) {
                                //ws2.Cells[12 + i, 1].Value = chips[i].PartId;
                                //ws2.Cells[12 + i, 2].Value = chips[i].WaferCord.ToString();
                                //ws2.Cells[12 + i, 3].Value = chips[i].HardBin;
                                //ws2.Cells[12 + i, 4].Value = chips[i].SoftBin;
                                //ws2.Cells[12 + i, 5].Value = chips[i].Result;
                                //ws2.Cells[12 + i, 6].Value = chips[i].Site;
                                //ws2.Cells[12 + i, 7].Value = chips[i].TestTime;
                                ws2.Cells[6 + i, 1].Value = chips[i].PartId;
                                ws2.Cells[6 + i, 2].Value = chips[i].WaferCord.ToString();
                                ws2.Cells[6 + i, 3].Value = chips[i].HardBin;
                                ws2.Cells[6 + i, 4].Value = chips[i].SoftBin;
                                ws2.Cells[6 + i, 5].Value = chips[i].Result;
                                ws2.Cells[6 + i, 6].Value = chips[i].Site;
                            }

                            ws2.Cells[2, 2].Value = "Cord";
                        ws2.Cells[2, 3].Value = "HardBin";
                        ws2.Cells[2, 4].Value = "SoftBin";
                        ws2.Cells[2, 5].Value = "Result";
                        ws2.Cells[2, 6].Value = "Site";
                        //ws2.Cells[2, 7].Value = "Test Time";

                    } else {
                        ws2.Cells[1, 1].Value = "Test NO";
                        ws2.Cells[1, 2].Value = "Test Name";
                        ws2.Cells[1, 3].Value = "Low Limit";
                        ws2.Cells[1, 4].Value = "High Limit";
                        ws2.Cells[1, 5].Value = "Unit";
                        ws2.Cells[1, 6].Value = "Min";
                        ws2.Cells[1, 7].Value = "Max";
                        ws2.Cells[1, 8].Value = "Mean";
                        ws2.Cells[1, 9].Value = "Sigma";
                        ws2.Cells[1, 10].Value = "Cpk";
                        ws2.Cells[1, 11].Value = "Pass Cnt";

                        for (int i = 0; i < chips.Count; i++) {
                            ws2.Cells[1, 12 + i].Value = chips[i].PartId;
                            ws2.Cells[2, 12 + i].Value = chips[i].WaferCord.ToString();
                            ws2.Cells[3, 12 + i].Value = chips[i].HardBin;
                            ws2.Cells[4, 12 + i].Value = chips[i].SoftBin;
                            ws2.Cells[5, 12 + i].Value = chips[i].Result;
                            ws2.Cells[6, 12 + i].Value = chips[i].Site;
                            ws2.Cells[7, 12 + i].Value = chips[i].TestTime;
                        }

                        ws2.Cells[2, 2].Value = "Cord";
                        ws2.Cells[3, 2].Value = "HardBin";
                        ws2.Cells[4, 2].Value = "SoftBin";
                        ws2.Cells[5, 2].Value = "Result";
                        ws2.Cells[6, 2].Value = "Site";
                        ws2.Cells[7, 2].Value = "Test Time";
                    }

                    for (int t = 0; t < testIds.Count; t++) {
                        var rst = DataAcquire.GetFilteredItemData(testIds[t], FilterId);
                        var info = testInfos[testIds[t]];
                        var stat = statistic[testIds[t]];

                        if (rotate) {
                                //ws2.Cells[1, t + 8].Value = testIds[t].ToString();
                                //ws2.Cells[2, t + 8].Value = info.TestText;
                                //ws2.Cells[3, t + 8].Value = info.LoLimit;
                                //ws2.Cells[4, t + 8].Value = info.HiLimit;
                                //ws2.Cells[5, t + 8].Value = info.Unit;
                                //ws2.Cells[6, t + 8].Value = stat.MinValue;
                                //ws2.Cells[7, t + 8].Value = stat.MaxValue;
                                //ws2.Cells[8, t + 8].Value = stat.MeanValue;
                                //ws2.Cells[9, t + 8].Value = stat.Sigma;
                                //ws2.Cells[10, t + 8].Value = stat.Cpk;
                                //ws2.Cells[11, t + 8].Value = stat.PassCount;

                                //for (int l = 0; l < rst.Length; l++) {
                                //    ws2.Cells[12 + l, t + 8].Value = rst[l];
                                //}
                                ws2.Cells[1, t + 7].Value = testIds[t].ToString();
                                ws2.Cells[2, t + 7].Value = info.TestText;
                                ws2.Cells[3, t + 7].Value = info.LoLimit;
                                ws2.Cells[4, t + 7].Value = info.HiLimit;
                                ws2.Cells[5, t + 7].Value = info.Unit;

                                for (int l = 0; l < rst.Length; l++) {
                                    ws2.Cells[6 + l, t + 7].Value = rst[l];
                                }
                            } else {
                            ws2.Cells[t + 8, 1].Value = testIds[t].ToString();
                            ws2.Cells[t + 8, 2].Value = info.TestText;
                            ws2.Cells[t + 8, 3].Value = info.LoLimit;
                            ws2.Cells[t + 8, 4].Value = info.HiLimit;
                            ws2.Cells[t + 8, 5].Value = info.Unit;
                            ws2.Cells[t + 8, 6].Value = stat.MinValue;
                            ws2.Cells[t + 8, 7].Value = stat.MaxValue;
                            ws2.Cells[t + 8, 8].Value = stat.MeanValue;
                            ws2.Cells[t + 8, 9].Value = stat.Sigma;
                            ws2.Cells[t + 8, 10].Value = stat.Cpk;
                            ws2.Cells[t + 8, 11].Value = stat.PassCount;

                            for (int l = 0; l < rst.Length; l++) {
                                ws2.Cells[t + 8, 12 + l].Value = rst[l];
                            }
                        }

                    }

                    p.SaveAs(new System.IO.FileInfo(path));
                    System.Windows.MessageBox.Show("EXCEL DONE", "DONE", MessageBoxButton.OK);
                        //File.WriteAllBytes(path, p.GetAsByteArray());  // send the file

                    }
                }



            });
        }

        #endregion

    }
}