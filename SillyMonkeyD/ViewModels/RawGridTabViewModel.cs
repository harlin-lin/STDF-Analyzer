using System;
using System.Data;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;

namespace SillyMonkeyD.ViewModels {
    public class RawGridTabViewModel : ViewModelBase {

        public RawGridTabViewModel(IDataAcquire dataAcquire, int filterId) {
            Init(dataAcquire, filterId);
            InitUI();
        }

        const int MinPerPageCount = 30;
        const int DefaultPerPageCount = 100;

        private int _filterId { get; set; }
        private IDataAcquire _dataAcquire;

        private int _countPerPage;

        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public DataTable Data { get { return GetProperty(() => Data); } private set { SetProperty(() => Data, value); } }
        public int CountPerPage {
            get { return _countPerPage; }
            set {
                if (value > MinPerPageCount)
                    _countPerPage = value;
                else
                    _countPerPage = MinPerPageCount;
            }
        }
        public int TotalCount { get { return GetProperty(() => TotalCount); } private set { SetProperty(() => TotalCount, value); } }
        public int TotalPages { get { return GetProperty(() => TotalPages); } private set { SetProperty(() => TotalPages, value); } }
        public int CurrentPageIndex { get { return GetProperty(() => CurrentPageIndex); } private set { SetProperty(() => CurrentPageIndex, value); } }




        private void Init(IDataAcquire dataAcquire, int filterId) {
            _dataAcquire = dataAcquire;
            _filterId = filterId;

            if (_dataAcquire.FileName.Length > 15)
                TabTitle = _dataAcquire.FileName.Substring(0, 15) + "...";
            else
                TabTitle = _dataAcquire.FileName;
            FilePath = _dataAcquire.FilePath;

            CountPerPage = DefaultPerPageCount;
            TotalCount = _dataAcquire.GetFilteredChipSummary(_filterId).TotalCount;
            TotalPages = TotalCount / CountPerPage + 1;
            CurrentPageIndex = 1;
            UpdateDataToStartPage();


        }

        private void UpdateDataToStartPage() {
            CurrentPageIndex = 1;
            if (TotalPages > 1)
                Data = _dataAcquire.GetFilteredItemData(0, CountPerPage, _filterId, true);
            else
                Data = _dataAcquire.GetFilteredItemData(0, TotalCount, _filterId, true);

            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }
        private void UpdateDataToLastPage() {
            if (CurrentPageIndex > 1) {
                CurrentPageIndex--;
                Data = _dataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, CountPerPage, _filterId, true);
            }
            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }
        private void UpdateDataToNextPage() {
            if (CurrentPageIndex < TotalPages) {
                CurrentPageIndex++;
                int leftCnt = TotalCount - (CurrentPageIndex - 1) * CountPerPage;
                if (leftCnt > CountPerPage)
                    Data = _dataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, CountPerPage, _filterId, true);
                else
                    Data = _dataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, leftCnt, _filterId, true);
            }
            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }
        private void UpdateDataToEndPage() {
            CurrentPageIndex = TotalPages;
            int leftCnt = TotalCount - (CurrentPageIndex - 1) * CountPerPage;
            Data = _dataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, leftCnt, _filterId, true);

            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }


        #region UI
        public ICommand JumpStartPage { get; private set; }
        public ICommand JumpLastPage { get; private set; }
        public ICommand JumpNextPage { get; private set; }
        public ICommand JumpEndPage { get; private set; }
        public ICommand ExportToExcel { get; private set; }
        public ICommand CreateHistogram { get; private set; }

        private void InitUI() {
            JumpStartPage = new DelegateCommand(() => { UpdateDataToStartPage(); });
            JumpLastPage = new DelegateCommand(() => { UpdateDataToLastPage(); });
            JumpNextPage = new DelegateCommand(() => { UpdateDataToNextPage(); });
            JumpEndPage = new DelegateCommand(() => { UpdateDataToEndPage(); });

            CreateHistogram = new DelegateCommand(() => {
                ;
            });

        }

        #endregion

    }
}