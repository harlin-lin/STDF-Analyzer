using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;

namespace SillyMonkeyD.ViewModels {
    public class DataViewModel : ViewModelBase {

        public ObservableCollection<DXTabItem> DataTabItems { get; private set; }
        public DXTabItem SelectedTab { get { return GetProperty(() => SelectedTab); } set { SetProperty(() => SelectedTab, value); } }

        public DataViewModel() {
            DataTabItems = new ObservableCollection<DXTabItem>();
            SelectedTab = null;

            InitUiCtr();
        }

        public void AddTab(DXTabItem tabItem) {
            DataTabItems.Add(tabItem);
            tabItem.Focus();
        }


        #region UI
        public ICommand TabSelectionChanged { get; private set; }

        private void InitUiCtr() {
            TabSelectionChanged = new DelegateCommand(() => {

            });



        }

        #endregion

    }
}