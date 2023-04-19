using DataContainer;
using MapBase;
using Microsoft.Win32;
using Prism.Events;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using UI_Chart.ViewModels;

namespace UI_Chart.Views {
    /// <summary>
    /// Interaction logic for WaferMap
    /// </summary>
    public partial class WaferMap : UserControl, INavigationAware {
        public WaferMap(IRegionManager regionManager, IEventAggregator ea) {
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

        //private WaferDataModel _waferData;

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return false;
            //var data = (SubData)navigationContext.Parameters["subData"];

            //return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            _ea.GetEvent<Event_FilterUpdated>().Unsubscribe(UpdateChart);
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
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


        void UpdateChart(SubData subData) {
            if (subData.Equals(_subData)) {
                if (cbUserCord.IsChecked.Value) {
                    waferMap.WaferDataSource = new WaferDataModel(_subData, (Item)cbItemX.SelectedItem, (Item)cbItemY.SelectedItem, (Item)cbItemWaferNO.SelectedItem);
                } else {
                    waferMap.WaferDataSource = new WaferDataModel(_subData);
                }
            }
        }


        void ExecuteCmdApply() {
            waferMap.WaferDataSource = new WaferDataModel(_subData, (Item)cbItemX.SelectedItem, (Item)cbItemY.SelectedItem, (Item)cbItemWaferNO.SelectedItem);
        }


        void ExecuteCmdChangeUserCord() {
            if (!cbUserCord.IsChecked.Value) {
                waferMap.WaferDataSource = new WaferDataModel(_subData);
            } else {
                if (cbItemWaferNO.SelectedItem != null && cbItemX.SelectedItem != null && cbItemY.SelectedItem != null)
                    ExecuteCmdApply();
            }
        }


        ///<summary>
        /// Check if file is Good for writing
        ///</summary>
        ///<param name="filePath">File path</param>
        ///<returns></returns>
        public static bool IsFileGoodForWriting(string filePath) {
            FileStream stream = null;
            FileInfo file = new FileInfo(filePath);

            try {
                stream = file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            } catch (Exception) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            } finally {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return true;
        }

        public SaveFileDialog CreateFileDialog(string filter) {
            var saveFileDialog = new SaveFileDialog {
                Filter = filter,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            return saveFileDialog;
        }

        private bool GetAndCheckPath(string filter, string dftName, out string path) {
            var ret = false;
            var isGoodPath = false;
            var saveFileDialog = CreateFileDialog(filter);
            saveFileDialog.FileName = dftName;
            path = null;

            while (!isGoodPath) {
                if (saveFileDialog.ShowDialog() == true) {
                    if (IsFileGoodForWriting(saveFileDialog.FileName)) {
                        path = saveFileDialog.FileName;
                        isGoodPath = true;
                        ret = true;
                    } else {
                        System.Windows.MessageBox.Show(
                            "File is inaccesible for writing or you can not create file in this location, please choose another one.");
                    }
                } else {
                    isGoodPath = true;
                }
            }

            return ret;
        }

        private void buttonSave_Click(object sender, System.Windows.RoutedEventArgs e) {
            string filePath;

            string dftName = Path.GetFileNameWithoutExtension(_subData.StdFilePath);
            if (GetAndCheckPath("PNG | *.png", dftName, out filePath)) {
                var image = waferMap.GetWaferMap();
                if (image is null) {
                    System.Windows.MessageBox.Show("Select single map first");
                } else {
                    using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                        System.Windows.Media.Imaging.BitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
                        encoder.Save(fileStream);
                    }
                }
            }

        }

        private void buttonCopy_Click(object sender, System.Windows.RoutedEventArgs e) {
            var image = waferMap.GetWaferMap();
            if (image is null) {
                System.Windows.MessageBox.Show("Select single map first");
            } else {
                System.Windows.Clipboard.SetImage(image);
                _ea.GetEvent<Event_Log>().Publish("Copied to clipboard");
            }

        }

        private void cbUserCord_Click(object sender, System.Windows.RoutedEventArgs e) {
            ExecuteCmdChangeUserCord();
        }

        private void buttonApplyUserCord_Click(object sender, System.Windows.RoutedEventArgs e) {
            ExecuteCmdApply();
        }


    }


    public class WaferDataModel : IWaferData {
        private List<DieInfo> _dieInfoList = new List<DieInfo>();
        public IEnumerable<DieInfo> DieInfoList { get { return _dieInfoList; } }

        public Dictionary<ushort, Tuple<string, string>> HBinInfo { get; private set; }

        public Dictionary<ushort, Tuple<string, string>> SBinInfo { get; private set; }

        public short XUbound { get; private set; }

        public short YUbound { get; private set; }

        public short XLbound { get; private set; }

        public short YLbound { get; private set; }

        SubData _subData;

        public WaferDataModel(SubData subData) {
            _subData = subData;

            var da = StdDB.GetDataAcquire(subData.StdFilePath);
            HBinInfo = da.GetHBinInfo();
            SBinInfo = da.GetSBinInfo();

            _dieInfoList.Clear();

            foreach (var v in da.GetFilteredPartIndex(_subData.FilterId)) {
                var cord = da.GetWaferCordTuple(v);
                _dieInfoList.Add(new DieInfo(v, cord.Item1, cord.Item2, da.GetHardBin(v), da.GetSoftBin(v), da.GetSite(v), da.GetPassFail(v), 1));
            }

            var xs = from r in _dieInfoList
                     select r.X;
            var ys = from r in _dieInfoList
                     select r.Y;

            if (xs.Count() > 0 && ys.Count() > 0) {
                XUbound = xs.Max();
                XLbound = xs.Min();
                YUbound = ys.Max();
                YLbound = ys.Min();
            }
        }

        public WaferDataModel(SubData subData, Item x, Item y, Item w) {
            _subData = subData;

            var da = StdDB.GetDataAcquire(subData.StdFilePath);
            HBinInfo = da.GetHBinInfo();
            SBinInfo = da.GetSBinInfo();

            _dieInfoList.Clear();

            try {
                foreach (var v in da.GetFilteredPartIndex(_subData.FilterId)) {
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

                if (xs.Count() > 0 && ys.Count() > 0) {
                    XUbound = xs.Max();
                    XLbound = xs.Min();
                    YUbound = ys.Max();
                    YLbound = ys.Min();
                }
            } catch {

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
