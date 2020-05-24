using System;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;
using InteractiveDataDisplay.WPF;

namespace SillyMonkeyD.ViewModels {
    public class ItemChartTabViewModel : ViewModelBase, ITab {
        public ItemChartTabViewModel(IDataAcquire dataAcquire, int filterId) {
            Init(dataAcquire, filterId);
            InitUI();
        }

        public int FilterId { get; private set; }
        public IDataAcquire DataAcquire { get; private set; }

        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public int WindowFlag { get; private set; }
        public DataCollection Data { get { return GetProperty(() => Data); } private set { SetProperty(() => Data, value); } }

        private void Init(IDataAcquire dataAcquire, int filterId) {
            DataAcquire = dataAcquire;
            FilterId = filterId;
            WindowFlag = 2;

            var i = dataAcquire.GetFilterIndex(filterId);

            if (DataAcquire.FileName.Length > 15)
                TabTitle = DataAcquire.FileName.Substring(0, 15) + "..." + $"-F{i}-RAW";
            else
                TabTitle = DataAcquire.FileName + $"-F{i}-RAW";
            FilePath = DataAcquire.FilePath;

            //Data = new WaferMapGridModel(new WaferMapTable(DataAcquire, FilterId));
        }


        public void UpdateFilter() {
            
            RaisePropertyChanged("Data");
        }

        #region UI
        public ICommand ExportToExcel { get; private set; }

        private void InitUI() {

        }
        #endregion
    }
}