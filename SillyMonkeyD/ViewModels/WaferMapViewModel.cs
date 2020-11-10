using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;

namespace SillyMonkeyD.ViewModels {
    public class WaferMapViewModel : ViewModelBase, ITab {
        public WaferMapViewModel(IDataAcquire dataAcquire, int filterId, TabItem tab) {
            Init(dataAcquire, filterId);
            CorrespondingTab = tab;
            InitUI();
        }

        public int FilterId { get; private set; }
        public IDataAcquire DataAcquire { get; private set; }

        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public int WindowFlag { get; private set; }
        public TabType TabType { get { return TabType.WaferMapTab; } }
        public bool IsMainTab { get { return false; } }
        public Thickness LocationInTablist { get { return IsMainTab ? new Thickness(0, 0, 3, 0) : new Thickness(25, 0, 3, 0); } }
        public TabItem CorrespondingTab { get; }

        public WaferMapGridModel Data { get { return GetProperty(() => Data); } private set { SetProperty(() => Data, value); } }

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

            Data = new WaferMapGridModel(DataAcquire, FilterId);
        }


        public void UpdateFilter() {
            Data.Update();
            RaisePropertyChanged("Data");
        }

        #region UI
        public ICommand ExportToExcel { get; private set; }

        private void InitUI() {

        }
        #endregion
    }
}