using DataContainer;
using MapBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using UI_Chart.ViewModels;

namespace UI_Chart.Views {
    /// <summary>
    /// Interaction logic for WaferMap
    /// </summary>
    public partial class WaferMap : UserControl {
        public WaferMap() {
            InitializeComponent();
        }

        private void chart_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            var vm = (INotifyPropertyChanged)(chart.DataContext);
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            //System.Diagnostics.Debug.WriteLine(sender.ToString());
            var vm = (WaferMapViewModel)sender;
            //switch (e.PropertyName) {
            //        break;
            //}

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
            }
            catch {

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
