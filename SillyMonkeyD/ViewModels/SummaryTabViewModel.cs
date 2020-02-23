using System;
using System.Windows.Documents;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;
using FileHelper;

namespace SillyMonkeyD.ViewModels {
    public class SummaryTabViewModel : ViewModelBase, ITab {
        public delegate void SummaryUpdateHandler();

        public SummaryTabViewModel(IDataAcquire dataAcquire, int filterId) {
            Init(dataAcquire, filterId);

            InitUI();

        }

        private SummaryHelper _summaryHelper;

        public int FilterId { get; private set; }
        public IDataAcquire DataAcquire { get; private set; }

        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public FlowDocument Summary { get; private set; }

        public SummaryUpdateHandler SummaryUpdateEvent;

        private void Init(IDataAcquire dataAcquire, int filterId) {
            DataAcquire = dataAcquire;
            FilterId = filterId;

            var i = dataAcquire.GetFilterIndex(filterId);

            if (DataAcquire.FileName.Length > 15)
                TabTitle = DataAcquire.FileName.Substring(0, 15) + "..." + $"-F{i}-SUM";
            else
                TabTitle = DataAcquire.FileName + $"-F{i}-SUM";
            FilePath = DataAcquire.FilePath;

            _summaryHelper = new SummaryHelper(dataAcquire, filterId);

            Summary = _summaryHelper.GetSummary();
        }

        public void UpdateFilter() {
            Summary = _summaryHelper.GetSummary();

            SummaryUpdateEvent?.Invoke();
        }

        #region UI
        public ICommand ExportToExcel { get; private set; }

        private void InitUI() {

            ExportToExcel = new DelegateCommand(() => {
                ;
            });

        }

        #endregion

    }
}