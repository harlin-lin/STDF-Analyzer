using DataContainer;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UI_Data.ViewModels {
    public class CorrItem {
        public string TestNumber { get; private set; }

        public string TestText { get; private set; }
        public float? LoLimit { get; private set; }
        public float? HiLimit { get; private set; }
        public string Unit { get; private set; }

        public float?[] MeanValue { get; private set; }
        public float?[] MinValue { get; private set; }
        public float?[] MaxValue { get; private set; }
        public float?[] Cp { get; private set; }
        public float?[] Cpk { get; private set; }
        public float?[] Sigma { get; private set; }

        //public CorrItem()
    }

    public class DataCorrelationViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        int _fileIdx = -1;
        int _filterIdx = -1;

        List<SubData> _subDataList = new List<SubData>();

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

        public DataCorrelationViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateView);

        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            throw new NotImplementedException();
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            throw new NotImplementedException();
        }

        private void UpdateView(SubData data) {
            if (_subDataList.Contains(data)) {
                var dataAcquire = StdDB.GetDataAcquire(data.StdFilePath);
                //TestItems = new ObservableCollection<Item>(dataAcquire.GetFilteredItems(data.FilterId));
                RaisePropertyChanged("TestItems");
            }
        }


    }
}
