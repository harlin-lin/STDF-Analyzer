using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;

namespace SillyMonkeyD.ViewModels {
    public class MiscViewModel : ViewModelBase {
        public ObservableCollection<DXTabItem> DataTabItems { get; private set; }
        public DXTabItem SelectedTab { get { return GetProperty(() => SelectedTab); } set { SetProperty(() => SelectedTab, value); } }

        public SelectedTabHandler SelectedTabEvent;

        public MiscViewModel() {
            DataTabItems = new ObservableCollection<DXTabItem>();
            SelectedTab = null;

            InitUiCtr();
        }

        public void AddTab(DXTabItem tabItem) {
            DataTabItems.Add(tabItem);
            FocusTab(tabItem);
        }

        public void RemoveTab(DXTabItem tabItem) {
            DataTabItems.Remove(tabItem);
        }

        public void FocusTab(DXTabItem tabItem) {
            if (tabItem is null) return;
            tabItem.IsSelected = true;
        }

        #region UI
        public ICommand TabSelectionChanged { get; private set; }

        private void InitUiCtr() {
            TabSelectionChanged = new DelegateCommand(() => {
                SelectedTabEvent?.Invoke(SelectedTab);
            });



        }

        #endregion
    }
}