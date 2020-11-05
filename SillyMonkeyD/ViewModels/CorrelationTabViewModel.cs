using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;

namespace SillyMonkeyD.ViewModels {
    public class CorrelationTabViewModel : ViewModelBase, ITab {

        public CorrelationTabViewModel(List<Tuple<IDataAcquire, int>> dataFilterTuple, TabItem tab) {
            DataAcquire = null;
            FilterId = 0;
            WindowFlag = 1;

            TabTitle = $"QTY:{dataFilterTuple.Count}-CORR";
            FilePath = "";
            foreach (var v in dataFilterTuple) {
                FilePath += $"{v.Item1.FileName}:{v.Item2}-";
            }

            _dataFilterTuple = dataFilterTuple;

            Data = new CorrGridModel(new CorrelationTable(dataFilterTuple));

            CorrespondingTab = tab;

            InitUI();
        }

        List<Tuple<IDataAcquire, int>> _dataFilterTuple;

        public int FilterId { get; private set; }
        public IDataAcquire DataAcquire { get; private set; }
        public string TabTitle { get { return GetProperty(() => TabTitle); } private set { SetProperty(() => TabTitle, value); } }
        public string FilePath { get { return GetProperty(() => FilePath); } private set { SetProperty(() => FilePath, value); } }
        public int WindowFlag { get; private set; }
        public TabType TabType { get { return TabType.RawDataCorTab; } }
        public bool IsMainTab { get { return false; } }
        public Thickness LocationInTablist { get { return IsMainTab ? new Thickness(0, 0, 3, 0) : new Thickness(25, 0, 3, 0); } }
        public TabItem CorrespondingTab { get; }

        public CorrGridModel Data { get { return GetProperty(() => Data); } private set { SetProperty(() => Data, value); } }

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