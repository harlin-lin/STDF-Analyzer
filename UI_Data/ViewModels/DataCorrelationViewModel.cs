using DataContainer;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace UI_Data.ViewModels {
    public class CorrItem {
        public string TestNumber { get; private set; }

        public string TestText { get; private set; }
        public float? LoLimit { get; private set; }
        public float? HiLimit { get; private set; }
        public string Unit { get; private set; }

        public float?[] MeanValue { get; private set; }
        public float?[] MinValue { get; private set; }
        public float?[] MaxValue { get; private set; }
        public float?[] Cp { get; private set; }
        public float?[] Cpk { get; private set; }
        public float?[] Sigma { get; private set; }

        //public CorrItem()
    }

    public class DataCorrelationViewModel : BindableBase, INavigationAware, IDataView {
        public SubData? CurrentData { get { return null; } }
        public TabType CurrentTabType { get { return TabType.RawDataCorTab; } }

        IRegionManager _regionManager;
        IEventAggregator _ea;

        List<SubData> _subDataList;
        int _corrDataIdx = -1;

        private DataTable dt;
        public DataTable TestItems {
            get { return dt; }
            set { SetProperty(ref dt, value); }
        }

        private string _header;
        public string Header {
            get { return _header; }
            set { SetProperty(ref _header, value); }
        }

        //private string _regionName;
        //public string RegionName {
        //    get { return _regionName; }
        //    set { SetProperty(ref _regionName, value); }
        //}

        public DataCorrelationViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateView);

        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {

        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            if(_corrDataIdx == -1) {
                _subDataList = new List<SubData>((IEnumerable<SubData>)navigationContext.Parameters["subDataList"]);
                _corrDataIdx = (int)navigationContext.Parameters["corrDataIdx"];

                Header = $"Corr_{_corrDataIdx}";

                //RegionName = $"Region_Corr_{_corrDataIdx}";
                InitView();
                UpdateView();
            }
        }

        private void InitView() {
            dt = new DataTable();
            dt.Columns.Add("TestNumber");
            dt.Columns.Add("TestText");
            dt.Columns.Add("LoLimit");
            dt.Columns.Add("HiLimit");
            dt.Columns.Add("Unit");

            for(int i=0; i<_subDataList.Count; i++) {
                dt.Columns.Add("MeanValue_" + i);
            }
            for (int i = 0; i < _subDataList.Count; i++) {
                dt.Columns.Add("MinValue_" + i);
            }
            for (int i = 0; i < _subDataList.Count; i++) {
                dt.Columns.Add("MaxValue_" + i);
            }
            for (int i = 0; i < _subDataList.Count; i++) {
                dt.Columns.Add("Cp_" + i);
            }
            for (int i = 0; i < _subDataList.Count; i++) {
                dt.Columns.Add("Cpk_" + i);
            }
            for (int i = 0; i < _subDataList.Count; i++) {
                dt.Columns.Add("Sigma_" + i);
            }
        }

        private void UpdateView() {
            List<IDataAcquire> allDa = new List<IDataAcquire>();

            int cnt = _subDataList.Count;

            allDa.Add(StdDB.GetDataAcquire(_subDataList[0].StdFilePath));
            List<string> allId = new List<string>(allDa[0].GetTestIDs());
            var baseItem = allDa[0].GetFilteredItemStatistic(_subDataList[0].FilterId);

            for (int i = 1; i < cnt; i++) {
                allDa.Add(StdDB.GetDataAcquire(_subDataList[i].StdFilePath));
            }

            dt.Rows.Clear();

            foreach (var v in baseItem) {
                DataRow r = dt.NewRow();
                r[0] = v.TestNumber;
                r[1] = v.TestText;
                r[2] = v.LoLimit;
                r[3] = v.HiLimit;
                r[4] = v.Unit;
                for (int i = 0; i < cnt; i++) {
                    if (!allDa[i].IfContainsTestId(v.TestNumber)) continue;
                    var s = allDa[i].GetFilteredStatistic(_subDataList[i].FilterId, v.TestNumber);
                    r[5 + i] = s.MeanValue;
                    r[5 + 1 * cnt + i] = s.MinValue;
                    r[5 + 2 * cnt + i] = s.MaxValue;
                    r[5 + 3 * cnt + i] = s.Cp;
                    r[5 + 4 * cnt + i] = s.Cpk;
                    r[5 + 5 * cnt + i] = s.Sigma;
                }
                dt.Rows.Add(r);
            }

            for (int i = 1; i < cnt; i++) {
                var appendId = allDa[i].GetTestIDs().Except(allId);
                foreach(var uid in appendId) {
                    DataRow r = dt.NewRow();
                    var  s = allDa[i].GetFilteredStatistic(_subDataList[i].FilterId, uid);
                    var v = allDa[i].GetTestInfo(uid);
                    r[0] = uid;
                    r[1] = v.TestText;
                    r[2] = v.LoLimit;
                    r[3] = v.HiLimit;
                    r[4] = v.Unit;

                    r[5 + i] = s.MeanValue;
                    r[5 + 1 * cnt + i] = s.MinValue;
                    r[5 + 2 * cnt + i] = s.MaxValue;
                    r[5 + 3 * cnt + i] = s.Cp;
                    r[5 + 4 * cnt + i] = s.Cpk;
                    r[5 + 5 * cnt + i] = s.Sigma;
                    dt.Rows.Add(r);
                }
            }

            RaisePropertyChanged("TestItems");
        }


        private void UpdateView(SubData data) {
            if (_subDataList.Contains(data)) {
                UpdateView();
            }
        }

        private string corrErrorLimit;
        public string CorrErrorLimit {
            get { return corrErrorLimit; }
            set { SetProperty(ref corrErrorLimit, value); }
        }

        private string corrWarnLimit;
        public string CorrWarnLimit {
            get { return corrWarnLimit; }
            set { SetProperty(ref corrWarnLimit, value); }
        }

        private DelegateCommand _exportToExcel;
        public DelegateCommand ExportToExcelCommand =>
            _exportToExcel ?? (_exportToExcel = new DelegateCommand(ExecuteExportToExcelCommand));

        void ExecuteExportToExcelCommand() {

        }

        private DelegateCommand _applyLimt;
        public DelegateCommand ApplyLimitCommand =>
            _applyLimt ?? (_applyLimt = new DelegateCommand(ExecuteApplyLimitCommand));

        void ExecuteApplyLimitCommand() {

        }


        private string _selectedItem;

        private DelegateCommand<object> _onselection;
        public DelegateCommand<object> OnSelectionChanged =>
            _onselection ?? (_onselection = new DelegateCommand<object>(ExecuteOnSelectionChanged));

        void ExecuteOnSelectionChanged(object parameter) {
            var grid = parameter as System.Windows.Controls.DataGrid;
            _selectedItem = (grid.SelectedItem as DataRowView).Row[0].ToString();
            _ea.GetEvent<Event_CorrItemSelected>().Publish(new Tuple<string, IEnumerable<SubData>>(_selectedItem, _subDataList));
        }


        private DelegateCommand<object> _closeCmd;
        public DelegateCommand<object> CloseCommand =>
            _closeCmd ?? (_closeCmd = new DelegateCommand<object>(ExecuteCloseCommand));

        void ExecuteCloseCommand(object x) {
            _regionManager.Regions["Region_DataView"].Remove(x);
        }

    }
}
