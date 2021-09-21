using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using System.Windows.Media;
using SillyMonkey.Core;
using Prism.Regions;
using Prism.Events;
using DataContainer;

namespace UI_Chart.ViewModels {
    public class TrendViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;
        List<string> _selectedIds;

        private IList<DataPoint> _trendData;
        public IList<DataPoint> TrendData {
            get { return _trendData; }
            set { SetProperty(ref _trendData, value); }
        }

        private Color _lineColor= Color.FromRgb(0, 0, 255);
        public Color LineColor {
            get { return _lineColor; }
            set { SetProperty(ref _lineColor, value); }
        }

        public TrendViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateChart);
            _ea.GetEvent<Event_ItemsSelected>().Subscribe(UpdateItems);

            
            InitUi();
        }
        void UpdateItems(Tuple<SubData, List<string>> para) {
            if (!para.Item1.Equals(_subData)) return;
            if (para.Item2 == null || para.Item2.Count == 0) return;

            _selectedIds.Clear();
            _selectedIds.AddRange(para.Item2);

            UpdateData();
        }
        void UpdateData() {
            if (_selectedIds == null || _selectedIds.Count == 0) return;
            int i = 0;
            TrendData = (from r in StdDB.GetDataAcquire(_subData.StdFilePath).GetFilteredItemData(_selectedIds[0], _subData.FilterId)
                         let p = new DataPoint(i++, r.HasValue ? r.Value : double.NaN)
                         select p).ToList();
        }

        void UpdateChart(SubData subData) {
            if (subData.Equals(_subData)) {

                UpdateData();
            }
        }

        void InitUi() {
        
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
                _subData = data;

                _selectedIds = new List<string>((List<string>)navigationContext.Parameters["itemList"]);
                
                UpdateData();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];

            return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            
        }
    }
}
