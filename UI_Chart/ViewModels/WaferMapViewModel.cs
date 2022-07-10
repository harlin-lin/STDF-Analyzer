using DataContainer;
using MapBase;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace UI_Chart.ViewModels {
    public class WaferDataModel : IWaferData {
        private List<DieInfo> _dieInfoList = new List<DieInfo>();
        public IEnumerable<DieInfo> DieInfoList { get { return _dieInfoList; }}

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
                _dieInfoList.Add(new DieInfo(v, cord.Item1.Value, cord.Item2.Value, da.GetHardBin(v), da.GetSoftBin(v), da.GetSite(v), da.GetPassFail(v), 1));
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

                if(xs.Count()>0 && ys.Count() > 0) {
                    XUbound = xs.Max();
                    XLbound = xs.Min();
                    YUbound = ys.Max();
                    YLbound = ys.Min();
                }
            }
            catch {
                
            }

        }

        //public void UpdateData() {
        //    _dieInfoList.Clear();

        //    var da = StdDB.GetDataAcquire(_subData.StdFilePath);
        //    foreach (var v in da.GetFilteredPartIndex(_subData.FilterId)) {
        //        var cord = da.GetWaferCordTuple(v);
        //        _dieInfoList.Add(new DieInfo(v, cord.Item1.Value, cord.Item2.Value, da.GetHardBin(v), da.GetSoftBin(v), da.GetSite(v), da.GetPassFail(v), 1));
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

    public class WaferMapViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;

        private WaferDataModel _waferData;
        public WaferDataModel WaferData {
            get { return _waferData; }
            set { SetProperty(ref _waferData, value); }
        }

        private IEnumerable<MapBinMode> _mapBinModeList = Enum.GetValues(typeof(MapBinMode)).OfType<MapBinMode>();
        public IEnumerable<MapBinMode> MapBinModeList {
            get { return _mapBinModeList; }
            set { SetProperty(ref _mapBinModeList, value); }
        }

        private MapBinMode _selectedMapBinMode = MapBinMode.SBin;
        public MapBinMode SelectedMapBinMode {
            get { return _selectedMapBinMode; }
            set { SetProperty(ref _selectedMapBinMode, value); }
        }

        private IEnumerable<MapRtDataMode> _mapRtDataModeList = Enum.GetValues(typeof(MapRtDataMode)).OfType<MapRtDataMode>();
        public IEnumerable<MapRtDataMode> MapRtDataModeList {
            get { return _mapRtDataModeList; }
            set { SetProperty(ref _mapRtDataModeList, value); }
        }

        private MapRtDataMode _selectedMapRtDataMode = MapRtDataMode.OverWrite;
        public MapRtDataMode SelectedMapRtDataMode {
            get { return _selectedMapRtDataMode; }
            set { SetProperty(ref _selectedMapRtDataMode, value); }
        }

        private IEnumerable<MapViewMode> _mapViewModeList = Enum.GetValues(typeof(MapViewMode)).OfType<MapViewMode>();
        public IEnumerable<MapViewMode> MapViewModeList {
            get { return _mapViewModeList; }
            set { SetProperty(ref _mapViewModeList, value); }
        }

        private MapViewMode _selectedMapViewMode = MapViewMode.Single;
        public MapViewMode SelectedMapViewMode {
            get { return _selectedMapViewMode; }
            set { SetProperty(ref _selectedMapViewMode, value); }
        }

        private IEnumerable<Item> _items;
        public IEnumerable<Item> AllItems {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        private Item _selectedCordX;
        public Item SelectedCordX {
            get { return _selectedCordX; }
            set { SetProperty(ref _selectedCordX, value); }
        }

        private Item _selectedCordY;
        public Item SelectedCordY {
            get { return _selectedCordY; }
            set { SetProperty(ref _selectedCordY, value); }
        }

        private Item _selectedWaferNO;
        public Item SelectedWaferNO {
            get { return _selectedWaferNO; }
            set { SetProperty(ref _selectedWaferNO, value); }
        }

        public WaferMapViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateChart);

        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];

            return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {

        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
                _subData = data;

                var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);
                AllItems = dataAcquire.GetFilteredItemStatistic(_subData.FilterId);

                WaferData = new WaferDataModel(_subData);
            }
        }


        void UpdateChart(SubData subData) {
            if (subData.Equals(_subData)) {
                WaferData = new WaferDataModel(_subData);
            }
        }

        private DelegateCommand cmdApply;
        public DelegateCommand CmdApply =>
            cmdApply ?? (cmdApply = new DelegateCommand(ExecuteCmdApply));

        void ExecuteCmdApply() {
            WaferData = new WaferDataModel(_subData, SelectedCordX, SelectedCordY, SelectedWaferNO);
        }

        private DelegateCommand<object> cmdDisableUserCord;
        public DelegateCommand<object> CmdChangeUserCord =>
            cmdDisableUserCord ?? (cmdDisableUserCord = new DelegateCommand<object>(ExecuteCmdChangeUserCord));

        void ExecuteCmdChangeUserCord(object ifChecked) {
            if (!(bool)ifChecked) {
                WaferData = new WaferDataModel(_subData);
            } else {
                if (_selectedWaferNO != null && _selectedCordX != null && _selectedCordY != null)
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
            }
            catch (Exception) {
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

        private DelegateCommand<object> _CmdSave;
        public DelegateCommand<object> CmdSave =>
            _CmdSave ?? (_CmdSave = new DelegateCommand<object>(ExecuteCmdSave));

        void ExecuteCmdSave(object e) {
            string filePath;

            string dftName = Path.GetFileNameWithoutExtension(_subData.StdFilePath);
            if (GetAndCheckPath("PNG | *.png", dftName, out filePath)) {
                var image = (e as WaferMapControl).GetWaferMap();
                if(image is null) {
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

        private DelegateCommand<object> _CmdCopy;
        public DelegateCommand<object> CmdCopy =>
            _CmdCopy ?? (_CmdCopy = new DelegateCommand<object>(ExecuteCmdCopy));

        void ExecuteCmdCopy(object e) {
            var image = (e as WaferMapControl).GetWaferMap();
            if (image is null) {
                System.Windows.MessageBox.Show("Select single map first");
            } else {
                System.Windows.Clipboard.SetImage(image);
                _ea.GetEvent<Event_Log>().Publish("Copied to clipboard");
            }
        }

    }
}
