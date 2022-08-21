using DataContainer;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System.Data;
using System.Linq;
using System.Windows;
using FastWpfGrid;
using System.Collections.Generic;
using System;

namespace UI_Chart.ViewModels {
    public class RawViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;

        //const int CNT_PER_PAGE = 10;
        //private int totalPartCnt = 0;
        //private int totalPages = 0;
        //private int currPage = 0;

        public RawViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(x => {
                if (_subData.Equals(x)) {
                    _rawDataModel.NotifyRefresh();
                }
            });

        }

        //private void InitUI() {
        //    dt.Columns.Add("Idx");
        //    dt.Columns[0].DataType = typeof(int);
        //    dt.Columns.Add("TestNumber");
        //    for (int i = 0; i < CNT_PER_PAGE; i++) {
        //        dt.Columns.Add($"{i}");
        //    }
        //}

        //private void UpdateTable() {
        //    dt.Clear();

        //    var da = StdDB.GetDataAcquire(_subData.StdFilePath);

        //    totalPartCnt = da.GetFilteredChipsCount(_subData.FilterId);
        //    totalPages = (totalPartCnt / CNT_PER_PAGE) + ((totalPartCnt % CNT_PER_PAGE) > 0 ? 1 : 0);
        //    PageCnt = $"{currPage} / {totalPages - 1}";
        //    JumpPage = $"{currPage}";

        //    //add column
        //    var offset = currPage * CNT_PER_PAGE;
        //    var viewCnt = totalPartCnt > (offset + CNT_PER_PAGE) ? CNT_PER_PAGE : totalPartCnt - offset;
        //    DataRow r, r_cord, r_time, r_hbin, r_sbin, r_site;

        //    r = dt.NewRow();
        //    r_cord = dt.NewRow();
        //    r_time = dt.NewRow();
        //    r_hbin = dt.NewRow();
        //    r_sbin = dt.NewRow();
        //    r_site = dt.NewRow();

        //    r[0] = 0;
        //    r_cord[0] = 0;
        //    r_time[0] = 0;
        //    r_hbin[0] = 0;
        //    r_sbin[0] = 0;
        //    r_site[0] = 0;

        //    r[1] = "PartIdx";
        //    r_cord[1] = "Cord";
        //    r_time[1] = "Time";
        //    r_hbin[1] = "HBin";
        //    r_sbin[1] = "SBin";
        //    r_site[1] = "Site";

        //    int i = 2;
        //    foreach (var c in da.GetFilteredPartIndex(_subData.FilterId).Skip(offset).Take(viewCnt)) {
        //        r[i] = c;
        //        r_cord[i] = da.GetWaferCord(c);
        //        r_time[i] = da.GetTestTime(c);
        //        r_hbin[i] = da.GetHardBin(c);
        //        r_sbin[i] = da.GetSoftBin(c);
        //        r_site[i] = da.GetSite(c);
        //        i++;
        //    }
        //    dt.Rows.Add(r);
        //    dt.Rows.Add(r_cord);
        //    dt.Rows.Add(r_time);
        //    dt.Rows.Add(r_hbin);
        //    dt.Rows.Add(r_sbin);
        //    dt.Rows.Add(r_site);

        //    int idx = 1;
        //    foreach (var uid in da.GetTestIDs()) {
        //        r = dt.NewRow();
        //        r[0] = idx++;
        //        r[1] = uid;
        //        i = 2;
        //        foreach (var v in da.GetFilteredItemData(uid, _subData.FilterId).Skip(offset).Take(viewCnt)) {
        //            r[i++] = v;
        //        }
        //        dt.Rows.Add(r);
        //    }
        //    RaisePropertyChanged("TestItems");
        //}

        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
                _subData = data;
                RawDataModel = new FastDataGridModel(_subData);
                //InitUI();
                //UpdateTable();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];

            return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {

        }


        //private DataTable dt = new DataTable();
        //public DataTable TestItems {
        //    get { return dt; }
        //    set { SetProperty(ref dt, value); }
        //}


        //private string _pageCount;
        //public string PageCnt {
        //    get { return _pageCount; }
        //    set { SetProperty(ref _pageCount, value); }
        //}

        //private string _jumpPage;
        //public string JumpPage {
        //    get { return _jumpPage; }
        //    set { SetProperty(ref _jumpPage, value); }
        //}

        //private DelegateCommand jumpFirstPage;
        //public DelegateCommand JumpFirstPage =>
        //    jumpFirstPage ?? (jumpFirstPage = new DelegateCommand(ExecuteJumpFirstPage));

        //void ExecuteJumpFirstPage() {
        //    currPage = 0;
        //    UpdateTable();
        //}

        //private DelegateCommand jumpPreviousPage;
        //public DelegateCommand JumpPreviousPage =>
        //    jumpPreviousPage ?? (jumpPreviousPage = new DelegateCommand(ExecuteJumpPreviousPage));

        //void ExecuteJumpPreviousPage() {
        //    if (currPage > 0) {
        //        currPage--;
        //        UpdateTable();
        //    }
        //}

        //private DelegateCommand jumpNextPage;
        //public DelegateCommand JumpNextPage =>
        //    jumpNextPage ?? (jumpNextPage = new DelegateCommand(ExecuteJumpNextPage));

        //void ExecuteJumpNextPage() {
        //    if (currPage < (totalPages - 1)) {
        //        currPage++;
        //        UpdateTable();
        //    }
        //}

        //private DelegateCommand jumpLastPage;
        //public DelegateCommand JumpLastPage =>
        //    jumpLastPage ?? (jumpLastPage = new DelegateCommand(ExecuteJumpLastPage));

        //void ExecuteJumpLastPage() {
        //    currPage = totalPages - 1;
        //    UpdateTable();
        //}

        //private DelegateCommand _jumpSelectedPage;
        //public DelegateCommand JumpSelectedPage =>
        //    _jumpSelectedPage ?? (_jumpSelectedPage = new DelegateCommand(ExecuteJumpSelectedPage));

        //void ExecuteJumpSelectedPage() {
        //    int targetPage = 0;
        //    if (int.TryParse(JumpPage, out targetPage)) {
        //        if (targetPage <= (totalPages - 1) && targetPage >= 0) {
        //            currPage = targetPage;
        //            UpdateTable();
        //        }
        //    } else {
        //        MessageBox.Show("Wrong target box");
        //    }
        //}


        private FastGridModelBase _rawDataModel;
        public FastGridModelBase RawDataModel {
            get { return _rawDataModel; }
            set { SetProperty(ref _rawDataModel, value); }
        }


    }
}
