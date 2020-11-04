using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Core.Extensions;

namespace SillyMonkeyD.ViewModels {
    public class ItemChartTabViewModel : ViewModelBase, ITab {
        public ItemChartTabViewModel(IDataAcquire dataAcquire, int filterId, List<TestID> testIDs) {
            Init(dataAcquire, filterId, testIDs);
            InitUI();
        }

        public int FilterId { get; private set; }
        public IDataAcquire DataAcquire { get; private set; }

        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public int WindowFlag { get; private set; }

        public ObservableCollection<IRenderableSeriesViewModel> RenderableSeries { get { return GetProperty(() => RenderableSeries); } private set { SetProperty(() => RenderableSeries, value); } }

        private List<float?[]> _itemsData;
        private List<TestID> _testIDs;

        private void Init(IDataAcquire dataAcquire, int filterId, List<TestID> testIDs) {
            DataAcquire = dataAcquire;
            FilterId = filterId;
            _testIDs = testIDs;
            
            WindowFlag = 2;

            var i = dataAcquire.GetFilterIndex(filterId);

            if (DataAcquire.FileName.Length > 15)
                TabTitle = DataAcquire.FileName.Substring(0, 15) + "..." + $"-F{i}-TREND";
            else
                TabTitle = DataAcquire.FileName + $"-F{i}-TREND";
            FilePath = DataAcquire.FilePath;

            _itemsData = new List<float?[]>();

            UpdateFilter();
        }


        public void UpdateFilter() {
            _itemsData.Clear();

            //debug code
            _testIDs = new List<TestID>();
            _testIDs.Add(DataAcquire.GetFilteredTestIDs(FilterId)[19]);

            if (!_testIDs.IsNullOrEmpty()) {
                foreach(var id in _testIDs)
                    _itemsData.Add(DataAcquire.GetFilteredItemData(id, FilterId));
            }
            RenderableSeries = TrendChartModel.GetChartData(_itemsData);
            RaisePropertyChanged("RenderableSeries");
        }

        #region UI
        public ICommand ExportToExcel { get; private set; }

        private void InitUI() {

        }
        #endregion
    }
}