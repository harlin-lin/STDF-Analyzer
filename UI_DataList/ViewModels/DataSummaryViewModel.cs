﻿using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using DataContainer;
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
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateSubSummary);

        }

        private void UpdateSubSummary(SubData dataSelected) {
            Summary = GenerateBriefSummary(dataSelected);
        }

        private void UpdateSummary(string filePath) {
            if(filePath=="") {
                Summary = "";
            } else {
                Summary = GenerateBriefSummary(filePath);
            }
        }


        private string GenerateBriefSummary(SubData subData) {
            return "Summary";
        }

        private string GenerateBriefSummary(string filePath) {
            return "Summary";
        }

    }
}
