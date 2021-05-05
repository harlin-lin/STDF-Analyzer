using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public DataRawViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;

            SplitterWidth = 0;
            ChartViewWidth = 0;

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
    }
}
