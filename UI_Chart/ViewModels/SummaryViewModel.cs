using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SciChart.Core.Messaging;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UI_Chart.ViewModels {
    public class SummaryViewModel : BindableBase {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;

        public SummaryViewModel() {

        }
    }
}
