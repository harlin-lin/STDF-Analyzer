using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UI_Data.ViewModels {
    public class DataCorrelationViewModel : BindableBase, INavigationAware {
        public DataCorrelationViewModel() {

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
