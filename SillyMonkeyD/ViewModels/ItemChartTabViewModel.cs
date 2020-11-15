using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Core.Extensions;
using System.Linq;

namespace SillyMonkeyD.ViewModels {
    public class ItemChartTabViewModel : ViewModelBase, ITab {
        public ItemChartTabViewModel(IDataAcquire dataAcquire, int filterId, List<TestID> testIDs, TabItem tab) {
            Init(dataAcquire, filterId, testIDs);
            CorrespondingTab = tab;
            InitUI();
        }

        public int FilterId { get; private set; }
        public IDataAcquire DataAcquire { get; private set; }

        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public int WindowFlag { get { return 2; } }
        public TabType TabType { get { return TabType.ChartTab; } }
        public bool IsMainTab { get { return false; } }
        public Thickness LocationInTablist { get { return IsMainTab ? new Thickness(0, 0, 3, 0) : new Thickness(25, 0, 3, 0); } }
        public TabItem CorrespondingTab { get; }

        public ObservableCollection<IRenderableSeriesViewModel> TrendSeries { get { return GetProperty(() => TrendSeries); } private set { SetProperty(() => TrendSeries, value); } }
        public string TrendChartTitle { get { return GetProperty(() => TrendChartTitle); } private set { SetProperty(() => TrendChartTitle, value); } }


        public ObservableCollection<IRenderableSeriesViewModel> HistogramSeries { get { return GetProperty(() => HistogramSeries); } private set { SetProperty(() => HistogramSeries, value); } }



        private List<float?[]> _itemsData;
        private List<TestID> _testIDs;

        private void Init(IDataAcquire dataAcquire, int filterId, List<TestID> iDs) {
            DataAcquire = dataAcquire;
            FilterId = filterId;

            if(iDs is null) {
                _testIDs = new List<TestID>();
                _testIDs.Add(dataAcquire.GetFilteredTestIDs(filterId)[5]);
                _testIDs.Add(dataAcquire.GetFilteredTestIDs(filterId)[6]);
                //_testIDs.Add(dataAcquire.GetFilteredTestIDs(filterId)[234]);
                //_testIDs.Add(dataAcquire.GetFilteredTestIDs(filterId)[235]);
            } else {
                _testIDs = new List<TestID>(iDs);
            }
            
            var i = dataAcquire.GetFilterIndex(filterId);

            if (DataAcquire.FileName.Length > 15)
                TabTitle = DataAcquire.FileName.Substring(0, 15) + "..." + $"-F{i}-TREND";
            else
                TabTitle = DataAcquire.FileName + $"-F{i}-TREND";
            FilePath = DataAcquire.FilePath;

            _itemsData = new List<float?[]>();

            InitUI();

            UpdateChart();
        }

        public void UpdateChart() {
            UpdateTitle();
            _itemsData.Clear();

            foreach (var id in _testIDs)
                _itemsData.Add(DataAcquire.GetFilteredItemData(id, FilterId));
            TrendSeries = TrendChartModel.GetChartData(_itemsData, _testIDs, DataAcquire.GetFilteredChipsInfo(FilterId));
            RaisePropertyChanged("TrendSeries");
        }

        private void UpdateTitle() {
            if (_testIDs.Count == 1) {
                var t = DataAcquire.GetTestInfo(_testIDs[0]).TestText;
                TrendChartTitle = $"Trend:{_testIDs[0].MainNumber}.{_testIDs[0].SubNumber} {t}";
            } else {
                string s = "Trend:";
                foreach(var t in _testIDs) {
                    s += $"{t.MainNumber}.{t.SubNumber}_";
                }
                s = s.Remove(s.Length - 1);
                TrendChartTitle = s;
            }

        }

        public void UpdateTestIds(List<TestID> iDs) {
            _testIDs = new List<TestID>(iDs);
            UpdateChart();
        }

        public void UpdateFilter() {

            _testIDs=DataAcquire.GetFilteredTestIDs(FilterId).Intersect(_testIDs).ToList();
            UpdateChart();
        }

        #region UI
        public ICommand ExportToExcel { get; private set; }

        private void InitUI() {


        }
        #endregion
    }
}