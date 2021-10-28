using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace UI_DataList.ViewModels {
    public class FileMergeWindowViewModel : BindableBase {
        IEventAggregator _ea;
        IRegionManager _regionManager;


        private List<string> fileList;
        public List<string> FileList{
            get { return fileList; }
            set { SetProperty(ref fileList, value); }
        }

        private ObservableCollection<string> enableFiles= new ObservableCollection<string>();
        public ObservableCollection<string> EnableFiles {
            get { return enableFiles; }
            set { SetProperty(ref enableFiles, value); }
        }

        public FileMergeWindowViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;

        }

        private DelegateCommand<ListBox> _addFile;
        public DelegateCommand<ListBox> AddFile =>
            _addFile ?? (_addFile = new DelegateCommand<ListBox>(ExecuteAddFile));

        void ExecuteAddFile(ListBox parameter) {
            foreach(var v in parameter.SelectedItems) {
                if (!EnableFiles.Contains(v)) {
                    EnableFiles.Add(v as string);
                }
            }
        }

        private DelegateCommand<ListBox> _removeFile;
        public DelegateCommand<ListBox> RemoveFile=>
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
            if (EnableFiles.Count <= 1) {
                System.Windows.MessageBox.Show("At least select 2 files");
            } else {
                _ea.GetEvent<Event_MergeFiles>().Publish(enableFiles.ToList());
            }
        }
    }
}
