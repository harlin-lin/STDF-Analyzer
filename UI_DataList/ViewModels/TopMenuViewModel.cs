using DataContainer;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using UI_DataList.Views;

namespace UI_DataList.ViewModels {
    public class TopMenuViewModel : BindableBase {
        IEventAggregator _ea;
        IRegionManager _regionManager;


        public TopMenuViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;

        }

        private DelegateCommand _openFileDiag;
        public DelegateCommand OpenFileDiag =>
            _openFileDiag ?? (_openFileDiag = new DelegateCommand(ExecuteOpenFileDiag));

        void ExecuteOpenFileDiag() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择数据源文件";
            openFileDialog.Filter = "All|*.*|STDF|*.stdf|STD|*.std";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = false;
            openFileDialog.DefaultExt = "stdf";
            if (openFileDialog.ShowDialog() == false) {
                return;
            }

            var paths = openFileDialog.FileNames;
            foreach (string path in paths) {
                var ext = System.IO.Path.GetExtension(path).ToLower();
                if (ext == ".stdf" || ext == ".std") {
                    _ea.GetEvent<Event_OpenFile>().Publish(path);
                } else {
                    //log message not supported
                }
            }
        }

        private DelegateCommand _closeAllFiles;
        public DelegateCommand CloseAllFiles =>
            _closeAllFiles ?? (_closeAllFiles = new DelegateCommand(ExecuteCloseAllFiles));

        void ExecuteCloseAllFiles() {
            _ea.GetEvent<Event_CloseAllFiles>().Publish();
        }

        private DelegateCommand openFileMergeDiag;
        public DelegateCommand OpenFileMergeDiag =>
            openFileMergeDiag ?? (openFileMergeDiag = new DelegateCommand(ExecuteOpenFileMergeDiag));

        void ExecuteOpenFileMergeDiag() {
            var mergerWindow = new FileMergeWindow();
            var vm = (mergerWindow.DataContext as FileMergeWindowViewModel);
            vm.FileList = StdDB.GetAllFiles();
            mergerWindow.ShowDialog();
        }


        private DelegateCommand _cmdExit;
        public DelegateCommand CmdExit =>
            _cmdExit ?? (_cmdExit = new DelegateCommand(ExecuteCmdExit));

        void ExecuteCmdExit() {
            _ea.GetEvent<Event_CloseSillyMonkey>().Publish();
        }

        private DelegateCommand _cmdAbout;
        public DelegateCommand CmdAbout =>
            _cmdAbout ?? (_cmdAbout = new DelegateCommand(ExecuteCmdAbout));

        void ExecuteCmdAbout() {
            MessageBox.Show("SillyMonkey V2.0\nAuthor: Harlin Zhang\nMail:harlin_zhang@outlook.com");
        }

        private DelegateCommand _cmdHelp;
        public DelegateCommand CmdHelp =>
            _cmdHelp ?? (_cmdHelp = new DelegateCommand(ExecuteCmdHelp));

        void ExecuteCmdHelp() {
            ;
        }
    }
}
