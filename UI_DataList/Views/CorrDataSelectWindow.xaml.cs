using DataContainer;
using Prism.Commands;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UI_DataList.Views {
    /// <summary>
    /// CorrDataSelectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CorrDataSelectWindow : Window {
        public CorrDataSelectWindow(IEnumerable<SubData> dataList) {
            InitializeComponent();
            DataContext = this;
            DataList = new ObservableCollection<SubData>(dataList);
            EnableDataList = new ObservableCollection<SubData>();
        }

        public SubWindowReturnHandler ReturnHandler { get; set; }
        public ObservableCollection<SubData> DataList { set;  get; }
        public ObservableCollection<SubData> EnableDataList { set; get; }

        private DelegateCommand _addAllData;
        public DelegateCommand AddAllData =>
            _addAllData ?? (_addAllData = new DelegateCommand(ExecuteAddAllData));

        void ExecuteAddAllData() {
            EnableDataList.Clear();
            foreach (var v in DataList)
                EnableDataList.Add(v);

        }

        private DelegateCommand<ListBox> _addData;
        public DelegateCommand<ListBox> AddData =>
            _addData ?? (_addData = new DelegateCommand<ListBox>(ExecuteAddData));

        void ExecuteAddData(ListBox parameter) {
            if (parameter.SelectedItems.Count >= 0) {
                foreach (var v in parameter.SelectedItems)
                    if (!EnableDataList.Contains((SubData)v))
                        EnableDataList.Add((SubData)v);
            }

            //EnableDataList.OrderBy(x => x);
        }

        private DelegateCommand<ListBox> _removeData;
        public DelegateCommand<ListBox> RemoveData =>
            _removeData ?? (_removeData = new DelegateCommand<ListBox>(ExecuteRemoveData));

        void ExecuteRemoveData(ListBox parameter) {
            if (parameter.SelectedItems.Count >= 0)
                foreach (var v in parameter.SelectedItems)
                    EnableDataList.Remove((SubData)v);
        }

        private DelegateCommand _removeAllData;
        public DelegateCommand RemoveAllData =>
            _removeAllData ?? (_removeAllData = new DelegateCommand(ExecuteRemoveAllData));

        void ExecuteRemoveAllData() {
            EnableDataList.Clear();
        }

        private DelegateCommand _apply;
        public DelegateCommand Apply =>
            _apply ?? (_apply = new DelegateCommand(ExecuteApply));

        void ExecuteApply() {
            if (EnableDataList.Count > 1) {
                ReturnHandler?.Invoke((from r in EnableDataList
                                       select r).ToList());
                this.Close();
            } else {
                System.Windows.Forms.MessageBox.Show("At Least two data!");
            }
        }
    }
}
