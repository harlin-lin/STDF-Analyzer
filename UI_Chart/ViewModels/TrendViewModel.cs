﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using SillyMonkey.Core;
using Prism.Regions;
using Prism.Events;
using DataContainer;
using System.Drawing;
using SciChart.Charting.Model.DataSeries;

namespace UI_Chart.ViewModels {
    public class TrendViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;
        List<string> _selectedIds;

        //private IList<DataPoint> _trendData;
        //public IList<DataPoint> TrendData {
        //    get { return _trendData; }
        //    set { SetProperty(ref _trendData, value); }
        //}

        private XyDataSeries<double, double> _trendData= new XyDataSeries<double, double>();
        public XyDataSeries<double, double> TrendData {
            get { return _trendData; }
            set { SetProperty(ref _trendData, value); }
        }

        //private Color _lineColor= Color.Blue;
        //public Color LineColor {
        //    get { return _lineColor; }
        //    set { SetProperty(ref _lineColor, value); }
        //}

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
            //TrendData = (from r in StdDB.GetDataAcquire(_subData.StdFilePath).GetFilteredItemData(_selectedIds[0], _subData.FilterId)
            //             let p = new DataPoint(i++, r)
            //             select p).ToList();
            //TrendData = null;
            //var dataSeries = new XyDataSeries<double, double>();
            var data = (from r in StdDB.GetDataAcquire(_subData.StdFilePath).GetFilteredItemData(_selectedIds[0], _subData.FilterId)
                        select (double)r);
            var xs = Enumerable.Range(0, data.Count()).Select(x=> (double)x);
            //dataSeries.Append( xs, data);
            //TrendData = dataSeries;
            TrendData.Clear();
            TrendData.Append(xs, data);
            RaisePropertyChanged("TrendData");
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
