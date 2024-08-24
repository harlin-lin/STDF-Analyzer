using DataContainer;
using MapBase;
using Microsoft.Win32;
using Prism.Events;
using Prism.Regions;
using ScottPlot.Palettes;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using UI_Chart.ViewModels;
using OfficeOpenXml;

namespace UI_Chart.Views
{
    /// <summary>
    /// Interaction logic for WaferMap
    /// </summary>
    public partial class WaferMap : UserControl, INavigationAware
    {
        public WaferMap(IRegionManager regionManager, IEventAggregator ea)
        {
            InitializeComponent();

            _regionManager = regionManager;
            _ea = ea;

            cbBinMode.ItemsSource = Enum.GetValues(typeof(MapBinMode)).OfType<MapBinMode>();
            cbViewMode.ItemsSource = Enum.GetValues(typeof(MapViewMode)).OfType<MapViewMode>();
            cbRtDataMode.ItemsSource = Enum.GetValues(typeof(MapRtDataMode)).OfType<MapRtDataMode>();

            cbBinMode.SelectedItem = MapBinMode.SBin;
            cbViewMode.SelectedItem = MapViewMode.Single;
            cbRtDataMode.SelectedItem = MapRtDataMode.OverWrite;
        }

        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;

        private WaferDataModel _waferData;

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
            //var data = (SubData)navigationContext.Parameters["subData"];

            //return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _ea.GetEvent<Event_FilterUpdated>().Unsubscribe(UpdateChart);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data))
            {
                _subData = data;

                var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);
                var items = dataAcquire.GetFilteredItemStatistic(_subData.FilterId);

                _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateChart);

                cbItemX.ItemsSource = items;
                cbItemY.ItemsSource = items;
                cbItemWaferNO.ItemsSource = items;

                waferMap.WaferDataSource = new WaferDataModel(_subData);
            }
        }


        void UpdateChart(SubData subData)
        {
            if (subData.Equals(_subData))
            {
                if (cbUserCord.IsChecked.Value)
                {
                    waferMap.WaferDataSource = new WaferDataModel(_subData, (Item)cbItemX.SelectedItem, (Item)cbItemY.SelectedItem, (Item)cbItemWaferNO.SelectedItem);
                }
                else
                {
                    waferMap.WaferDataSource = new WaferDataModel(_subData);
                }
            }
        }


        void ExecuteCmdApply()
        {
            waferMap.WaferDataSource = new WaferDataModel(_subData, (Item)cbItemX.SelectedItem, (Item)cbItemY.SelectedItem, (Item)cbItemWaferNO.SelectedItem);
        }


        void ExecuteCmdChangeUserCord()
        {
            if (!cbUserCord.IsChecked.Value)
            {
                waferMap.WaferDataSource = new WaferDataModel(_subData);
            }
            else
            {
                if (cbItemWaferNO.SelectedItem != null && cbItemX.SelectedItem != null && cbItemY.SelectedItem != null)
                    ExecuteCmdApply();
            }
        }


        ///<summary>
        /// Check if file is Good for writing
        ///</summary>
        ///<param name="filePath">File path</param>
        ///<returns></returns>
        public static bool IsFileGoodForWriting(string filePath)
        {
            FileStream stream = null;
            FileInfo file = new FileInfo(filePath);

            try
            {
                stream = file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            }
            catch (Exception)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return true;
        }

        public SaveFileDialog CreateFileDialog(string filter)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            return saveFileDialog;
        }

        private bool GetAndCheckPath(string filter, string dftName, out string path)
        {
            var ret = false;
            var isGoodPath = false;
            var saveFileDialog = CreateFileDialog(filter);
            saveFileDialog.FileName = dftName;
            path = null;

            while (!isGoodPath)
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    if (IsFileGoodForWriting(saveFileDialog.FileName))
                    {
                        path = saveFileDialog.FileName;
                        isGoodPath = true;
                        ret = true;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            "File is inaccesible for writing or you can not create file in this location, please choose another one.");
                    }
                }
                else
                {
                    isGoodPath = true;
                }
            }

            return ret;
        }

        private void buttonSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string filePath;

            string dftName = Path.GetFileNameWithoutExtension(_subData.StdFilePath);
            if (GetAndCheckPath("PNG | *.png", dftName, out filePath))
            {
                var image = waferMap.GetWaferMap();
                if (image is null)
                {
                    System.Windows.MessageBox.Show("Select single map first");
                }
                else
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        System.Windows.Media.Imaging.BitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
                        encoder.Save(fileStream);
                    }
                }
            }

        }
        private void buttonxlsxSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string path;
            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = "Excel Files | *.xlsx";
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.FileName = "SiteCorrelation_";
                saveFileDialog.ValidateNames = true;
                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                path = saveFileDialog.FileName;
            };

            _ea.GetEvent<Event_Log>().Publish("Writing......");

            try
            {
                FileInfo fileInfo = new FileInfo(path);
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    
                    var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                    _waferData = new WaferDataModel(_subData);

                    ushort[,] _freshHBinMaps = new ushort[_waferData.XUbound + 1, _waferData.YUbound + 1];
                    ushort[,] _freshSBinMaps = new ushort[_waferData.XUbound + 1, _waferData.YUbound + 1];
                    ushort[,] _hBinMaps = new ushort[_waferData.XUbound + 1, _waferData.YUbound + 1];
                    ushort[,] _sBinMaps = new ushort[_waferData.XUbound + 1, _waferData.YUbound + 1];


                    foreach (var die in _waferData.DieInfoList)
                    {

                        if ((_hBinMaps[die.X, die.Y] == 0) && (_sBinMaps[die.X, die.Y] == 0))
                        {
                            _freshHBinMaps[die.X, die.Y] = die.HBin;
                            _freshSBinMaps[die.X, die.Y] = die.SBin;
                            _hBinMaps[die.X, die.Y] = die.HBin;
                            _sBinMaps[die.X, die.Y] = die.SBin;
                        }

                        else
                        {
                            _hBinMaps[die.X, die.Y] = die.HBin;
                            _sBinMaps[die.X, die.Y] = die.SBin;
                        }
                    }

                    // Write data into the SheetData
                    //IRow row0 = sheet.CreateRow(0);
                    //ICell cellx = row0.CreateCell(3);
                    //cellx.SetCellValue("X");

                    //// Write data into the SheetData
                    //IRow row2 = sheet.CreateRow(2);
                    //ICell celly = row2.CreateCell(0);
                    //celly.SetCellValue("Y");

                    //IRow row1 = sheet.CreateRow(1);
                    //for (int col = 0; col <= _waferData.XUbound; col++)
                    //{
                    //    ICell cellXheader = row1.CreateCell(col+2);
                    //    cellXheader.SetCellValue(col);
                    //    cellXheader.CellStyle = style;
                    //}

                    //IRow rowlast = sheet.CreateRow(_waferData.YUbound + 2);
                    //for (int col = 0; col <= _waferData.XUbound; col++)
                    //{
                    //    ICell cellxtail = rowlast.CreateCell(col + 2);
                    //    cellxtail.SetCellValue(col);
                    //    cellxtail.CellStyle = style;
                    //}


                    //for (int row = _waferData.YUbound; row >= _waferData.YLbound; row--)
                    //{
                    //    IRow rowy = sheet.CreateRow(row + 2);
                    //    ICell cellyheader = rowy.CreateCell(1);
                    //    ICell cellytail = rowy.CreateCell(_waferData.XUbound + 2);
                    //    cellyheader.SetCellValue(row);
                    //    cellyheader.SetCellValue(row);
                    //    cellytail.CellStyle = style;
                    //    cellytail.CellStyle = style;
                    //}
                    worksheet.Cells[1, 3].Value = "X";
                    worksheet.Cells[1, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[3, 1].Value = "Y";
                    worksheet.Cells[3, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[_waferData.YUbound + 5, 3].Value = "X";
                    worksheet.Cells[_waferData.YUbound + 5, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[_waferData.YUbound + 3, 1].Value = "Y";
                    worksheet.Cells[_waferData.YUbound + 3, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    int rawrowidx = 1;
                    
                    for (int _diey = _waferData.YUbound; _diey >= _waferData.YLbound; _diey--)
                    {
                        int rawcolidx = 1;
                        worksheet.Cells[rawrowidx + 2, 2].Value = _diey;
                        worksheet.Cells[rawrowidx + 2, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rawrowidx + 2, _waferData.XUbound + 4].Value = _diey;
                        worksheet.Cells[rawrowidx + 2, _waferData.XUbound + 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        for (int _diex = _waferData.XLbound; _diex <= _waferData.XUbound; _diex++)
                        {
                            worksheet.Cells[2, rawcolidx + 2].Value = _diex;
                            worksheet.Cells[2, rawcolidx + 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Cells[_waferData.YUbound + 4, rawcolidx + 2].Value = _diex;
                            worksheet.Cells[_waferData.YUbound + 4, rawcolidx + 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                            if (cbRtDataMode.SelectedItem is MapRtDataMode.FirstOnly)
                            {
                                if (_freshSBinMaps[_diex, _diey] != 0)
                                {
                                    worksheet.Cells[rawrowidx + 2, rawcolidx + 2].Value = _freshSBinMaps[_diex, _diey];
                                    worksheet.Cells[rawrowidx + 2, rawcolidx + 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                }
                            }
                            else
                            {
                                if (_sBinMaps[_diex, _diey] != 0)
                                {
                                    worksheet.Cells[rawrowidx + 2, rawcolidx + 2].Value = _sBinMaps[_diex, _diey];
                                    worksheet.Cells[rawrowidx + 2, rawcolidx + 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                }
                            }
                            rawcolidx++;
                        }

                        rawrowidx++;
                        
                    }
                   
                    package.Save();
                }

            }
            catch
            {
                _ea.GetEvent<Event_Log>().Publish("Write failed");
            }
            _ea.GetEvent<Event_Log>().Publish("Exported at:" + path);
        }

        private void SetProgress(string log, int percent)
        {
            _ea.GetEvent<Event_Progress>().Publish(new Tuple<string, int>(log, percent));
        }


        private void buttonCopy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var image = waferMap.GetWaferMap();
            if (image is null)
            {
                System.Windows.MessageBox.Show("Select single map first");
            }
            else
            {
                System.Windows.Clipboard.SetImage(image);
                _ea.GetEvent<Event_Log>().Publish("Copied to clipboard");
            }

        }

        private void cbUserCord_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ExecuteCmdChangeUserCord();
        }

        private void buttonApplyUserCord_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ExecuteCmdApply();
        }


    }


    public class WaferDataModel : IWaferData
    {
        private List<DieInfo> _dieInfoList = new List<DieInfo>();
        public IEnumerable<DieInfo> DieInfoList { get { return _dieInfoList; } }

        public Dictionary<ushort, Tuple<string, string>> HBinInfo { get; private set; }

        public Dictionary<ushort, Tuple<string, string>> SBinInfo { get; private set; }

        public short XUbound { get; private set; }

        public short YUbound { get; private set; }

        public short XLbound { get; private set; }

        public short YLbound { get; private set; }

        SubData _subData;

        public WaferDataModel(SubData subData)
        {
            _subData = subData;

            var da = StdDB.GetDataAcquire(subData.StdFilePath);
            HBinInfo = da.GetHBinInfo();
            SBinInfo = da.GetSBinInfo();

            _dieInfoList.Clear();

            foreach (var v in da.GetFilteredPartIndex(_subData.FilterId))
            {
                var cord = da.GetWaferCordTuple(v);
                _dieInfoList.Add(new DieInfo(v, cord.Item1, cord.Item2, da.GetHardBin(v), da.GetSoftBin(v), da.GetSite(v), da.GetPassFail(v), 1));
            }

            var xs = from r in _dieInfoList
                     select r.X;
            var ys = from r in _dieInfoList
                     select r.Y;

            if (xs.Count() > 0 && ys.Count() > 0)
            {
                XUbound = xs.Max();
                XLbound = xs.Min();
                YUbound = ys.Max();
                YLbound = ys.Min();
            }
        }

        public WaferDataModel(SubData subData, Item x, Item y, Item w)
        {
            _subData = subData;

            var da = StdDB.GetDataAcquire(subData.StdFilePath);
            HBinInfo = da.GetHBinInfo();
            SBinInfo = da.GetSBinInfo();

            _dieInfoList.Clear();

            try
            {
                foreach (var v in da.GetFilteredPartIndex(_subData.FilterId))
                {
                    var cordX = da.GetItemData(x.TestNumber, v);
                    if (float.IsNaN(cordX) || float.IsInfinity(cordX)) continue;

                    var cordY = da.GetItemData(y.TestNumber, v);
                    if (float.IsNaN(cordY) || float.IsInfinity(cordY)) continue;

                    var waferNO = da.GetItemData(w.TestNumber, v);
                    if (float.IsNaN(waferNO) || float.IsInfinity(waferNO)) continue;

                    _dieInfoList.Add(new DieInfo(v, (short)cordX, (short)cordY, da.GetHardBin(v), da.GetSoftBin(v), da.GetSite(v), da.GetPassFail(v), (short)waferNO));
                }

                var xs = from r in _dieInfoList
                         select r.X;
                var ys = from r in _dieInfoList
                         select r.Y;

                if (xs.Count() > 0 && ys.Count() > 0)
                {
                    XUbound = xs.Max();
                    XLbound = xs.Min();
                    YUbound = ys.Max();
                    YLbound = ys.Min();
                }
            }
            catch
            {

            }

        }

        //public void UpdateData() {
        //    _dieInfoList.Clear();

        //    var da = StdDB.GetDataAcquire(_subData.StdFilePath);
        //    foreach (var v in da.GetFilteredPartIndex(_subData.FilterId)) {
        //        var cord = da.GetWaferCordTuple(v);
        //        _dieInfoList.Add(new DieInfo(v, cord.Item1, cord.Item2, da.GetHardBin(v), da.GetSoftBin(v), da.GetSite(v), da.GetPassFail(v), 1));
        //    }

        //    var xs = from r in _dieInfoList
        //             select r.X;
        //    var ys = from r in _dieInfoList
        //             select r.Y;

        //    XUbound = xs.Max();
        //    XLbound = xs.Min();
        //    YUbound = ys.Max();
        //    YLbound = ys.Min();

        //}

        //public void EnableUserCord(Item x, Item y, Item w) {
        //    _dieInfoList.Clear();

        //    var da = StdDB.GetDataAcquire(_subData.StdFilePath);
        //    foreach (var v in da.GetFilteredPartIndex(_subData.FilterId)) {
        //        var cordX = da.GetItemData(x.TestNumber, v);
        //        if (float.IsNaN(cordX) || float.IsInfinity(cordX)) continue;

        //        var cordY = da.GetItemData(y.TestNumber, v);
        //        if (float.IsNaN(cordY) || float.IsInfinity(cordY)) continue;

        //        var waferNO = da.GetItemData(w.TestNumber, v);
        //        if (float.IsNaN(waferNO) || float.IsInfinity(waferNO)) continue;

        //        _dieInfoList.Add(new DieInfo(v, (short)cordX, (short)cordY, da.GetHardBin(v), da.GetSoftBin(v), da.GetSite(v), da.GetPassFail(v), (short)waferNO));
        //    }

        //    var xs = from r in _dieInfoList
        //             select r.X;
        //    var ys = from r in _dieInfoList
        //             select r.Y;

        //    XUbound = xs.Max();
        //    XLbound = xs.Min();
        //    YUbound = ys.Max();
        //    YLbound = ys.Min();

        //}

        //public void DisableUserCord() {
        //    UpdateData();
        //}
    }

}
