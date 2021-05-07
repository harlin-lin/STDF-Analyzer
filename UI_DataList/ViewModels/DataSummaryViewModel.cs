using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using DataInterface;
using SillyMonkey.Core;

namespace UI_DataList.ViewModels {
    public class DataSummaryViewModel : BindableBase {
        IEventAggregator _ea;

        string _summary;
        public string Summary {
            get { return _summary; }
            private set { SetProperty(ref _summary, value); } }

        public DataSummaryViewModel(IEventAggregator ea) {
            _ea = ea;
            Summary = "";

            _ea.GetEvent<Event_DataSelected>().Subscribe(UpdateSummary);
            _ea.GetEvent<Event_SubDataSelected>().Subscribe(UpdateSubSummary);
        }

        private void UpdateSubSummary(SubData dataSelected) {
            Summary = dataSelected.DataAcquire.GetFilteredSummary(dataSelected.FilterId);
        }

        private void UpdateSummary(IDataAcquire dataSelected) {
            if(dataSelected is null) {
                Summary = "";
            } else {
                Summary = dataSelected.GetSummary();
            }
        }

    }
}
