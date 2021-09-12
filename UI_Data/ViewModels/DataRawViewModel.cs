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

                SplitterWidth = 0;
                ChartViewWidth = 0;
            }
        }

        private void OpenChartView() {
            SplitterWidth = 3;
            ChartViewWidth = 100;
        }

        private HashSet<int> _selectedRowIdxList = new HashSet<int>();
        private HashSet<int> _selectedColIdxList = new HashSet<int>();
        private HashSet<FastGridCellAddress> _selectedCellList = new HashSet<FastGridCellAddress>();

        private void ShowTrend() {
            OpenChartView();

            if (_selectedRowIdxList.Count > 0) {
                _ea.GetEvent<Event_ShowTrend>().Publish(_selectedRowIdxList);
            }

        }
        private void ShowHistogram() {

        }
        private void ShowBox() {

        }
        private void ShowWaferMap() {

        }
        private void ExportToExcel() {

        }

        public DelegateCommand<object> CloseCommand { get; private set; }
        public DelegateCommand ShowTrendCommand { get; private set; }
        public DelegateCommand ShowHistogramCommand { get; private set; }
        public DelegateCommand ShowBoxCommand { get; private set; }
        public DelegateCommand ShowWaferMapCommand { get; private set; }
        public DelegateCommand ExportToExcelCommand { get; private set; }


        public DelegateCommand<object> OnSelectColumn { get; private set; }
        public DelegateCommand<object> OnSelectRow { get; private set; }
        public DelegateCommand<object> OnSelectionChanged { get; private set; }


        enum SelectType {
            row,col,cell
        }
        private SelectType _selectType= SelectType.cell;

        public void InitUi() {
            CloseCommand = new DelegateCommand<object>((x)=> {
                if (_regionManager.Regions["Region_DataView"].Views.Contains(x)) {
                    _regionManager.Regions["Region_DataView"].Remove(x);
                }

            });

            ShowTrendCommand = new DelegateCommand(ShowTrend);
            ShowHistogramCommand = new DelegateCommand(ShowHistogram);
            ShowBoxCommand = new DelegateCommand(ShowBox);
            ShowWaferMapCommand = new DelegateCommand(ShowWaferMap);
            ExportToExcelCommand = new DelegateCommand(ExportToExcel);


            OnSelectColumn = new DelegateCommand<object>((x)=> {
                _selectType = SelectType.col;
            });

            OnSelectRow = new DelegateCommand<object>((x) => {
                _selectType = SelectType.row;
            });

            OnSelectionChanged = new DelegateCommand<object>((x) => {
                _selectedRowIdxList.Clear();
                _selectedColIdxList.Clear();
                _selectedCellList.Clear();

                switch (_selectType) {
                    case SelectType.row:
                        _selectedRowIdxList = (x as FastWpfGrid.FastGridControl).GetSelectedRows();
                        break;
                    default:break;
                }
                foreach (var v in _selectedRowIdxList) {
                    System.Diagnostics.Debug.WriteLine(v);
                }

                _selectType = SelectType.cell;
            });

        }



    }
}
