using DataContainer;
using MapBase;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
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

            foreach (var v in da.GetFilteredPartIndex(subData.FilterId)) {
                var cord = da.GetWaferCordTuple(v);
                _dieInfoList.Add(new DieInfo(v, cord.Item1.Value, cord.Item2.Value, da.GetHardBin(v), da.GetSoftBin(v), da.GetSite(v), da.GetPassFail(v), 1));
            }

            var xs = from r in _dieInfoList
                    select r.X;
            var ys = from r in _dieInfoList
                     select r.Y;

            XUbound = xs.Max();
            XLbound = xs.Min();
            YUbound = xs.Max();
            YLbound = xs.Min();


        }

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

                WaferData = new WaferDataModel(_subData);
            }
        }


        void UpdateChart(SubData subData) {
            if (subData.Equals(_subData)) {
                WaferData = new WaferDataModel(_subData);
            }
        }
    }
}
