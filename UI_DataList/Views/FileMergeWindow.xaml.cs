using Prism.Commands;
using SillyMonkey.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UI_DataList.Views {
    /// <summary>
    /// Interaction logic for FileMergeWindow.xaml
    /// </summary>
    public partial class FileMergeWindow : Window {
        public FileMergeWindow(IEnumerable<string> fileList) {
            InitializeComponent();
            DataContext = this;
            FileList = new ObservableCollection<string>(fileList);
            EnableFiles = new ObservableCollection<string>();
        }

        public SubWindowReturnHandler ReturnHandler { get; set; }
        public ObservableCollection<string> FileList {get; set;}
        public ObservableCollection<string> EnableFiles { get; set; }

        private DelegateCommand<ListBox> _addFile;
        public DelegateCommand<ListBox> AddFile =>
            _addFile ?? (_addFile = new DelegateCommand<ListBox>(ExecuteAddFile));

        void ExecuteAddFile(ListBox parameter) {
            foreach (var v in parameter.SelectedItems) {
                if (!EnableFiles.Contains((string)v)) {
                    EnableFiles.Add(v as string);
                }
            }
        }

        private DelegateCommand<ListBox> _removeFile;
        public DelegateCommand<ListBox> RemoveFile =>
            _removeFile ?? (_removeFile = new DelegateCommand<ListBox>(ExecuteRemoveFile));

        void ExecuteRemoveFile(ListBox parameter) {
            var af = new List<object>();
            foreach (var v in parameter.SelectedItems) {
                af.Add(v);
            }
            foreach (var v in af) {
                EnableFiles.Remove(v as string);
            }

        }

        private DelegateCommand _applyMerge;
        public DelegateCommand ApplyMerge =>
            _applyMerge ?? (_applyMerge = new DelegateCommand(ExecuteApplyMerge));

        void ExecuteApplyMerge() {
            if (EnableFiles.Count > 1) {
                ReturnHandler?.Invoke((from r in EnableFiles
                                       select r).ToList());
                this.Close();
            } else {
                System.Windows.Forms.MessageBox.Show("At Least two data!");
            }
        }

    }
}
