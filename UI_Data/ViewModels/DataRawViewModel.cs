using DataInterface;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using FastWpfGrid;

namespace UI_Data.ViewModels {
    public class DataRawViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        private int _splitterWidth;
        public int SplitterWidth {
            get { return _splitterWidth; }
            set { SetProperty(ref _splitterWidth, value); }
        }

        private int _chartViewWidth;
        public int ChartViewWidth {
            get { return _chartViewWidth; }
            set {
                SetProperty(ref _chartViewWidth, value);
            }
        }

        int _filterId;
        IDataAcquire _dataAcquire;

        private StdLogGridModel _dataTable;
        public StdLogGridModel DataTable {
            get { return _dataTable; }
            set { SetProperty(ref _dataTable, value); }
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

        public DataRawViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;

            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateView);

            SplitterWidth = 0;
            ChartViewWidth = 0;

            InitUi();
        }

        private void UpdateView(SubData data) {
            if(data.DataAcquire.FilePath == _dataAcquire.FilePath && data.FilterId == _filterId) {
                DataTable.Update();
                RaisePropertyChanged("DataTable");
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];

            return data.DataAcquire.FilePath==_dataAcquire.FilePath && data.FilterId==_filterId;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            ;
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            if (DataTable is null) {
                var data = (SubData)navigationContext.Parameters["subData"];
                _filterId = data.FilterId;
                _dataAcquire = data.DataAcquire;
                DataTable = new StdLogGridModel(_dataAcquire, _filterId);

                Header = $"F:{_filterId.ToString("x8")}";

                RegionName = $"Region_{_filterId.ToString("x8")}";
            }
        }

        public void InitUi() {
            CloseCommand = new DelegateCommand<object>((x)=> {
                //var v = _regionManager.Regions["Region_DataView"].Views.First().Equals(x);
                if (_regionManager.Regions["Region_DataView"].Views.Contains(x)) {
                    _regionManager.Regions["Region_DataView"].Remove(x);
                }

                //_ea.GetEvent<Event_CloseData>().Publish(x);
            });
        }

        public DelegateCommand<object> CloseCommand { get; private set; }

    }
}
