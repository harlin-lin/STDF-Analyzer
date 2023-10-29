using DataContainer;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.IO;
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

            _ea.GetEvent<Event_DataSelected>().Subscribe((x)=>{
                if (string.IsNullOrEmpty(x)) IfFileSelectionValid = false;
                IfFileSelectionValid = true;
            });
            _ea.GetEvent<Event_SubDataSelected>().Subscribe((x) => {
                IfFileSelectionValid = false;
            });
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
                    System.Windows.Forms.MessageBox.Show("Invalid File");
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
            var mergerWindow = new FileMergeWindow(StdDB.GetAllFiles());
            mergerWindow.ReturnHandler += new SubWindowReturnHandler((x) => {
                var ll = x as List<string>;

                _ea.GetEvent<Event_MergeFiles>().Publish(ll);
            });
            mergerWindow.ShowDialog();
        }

        private DelegateCommand openSubDataMergeDiag;
        public DelegateCommand OpenSubDataMergeDiag =>
            openSubDataMergeDiag ?? (openSubDataMergeDiag = new DelegateCommand(ExecuteOpenSubDataMergeDiag));

        void ExecuteOpenSubDataMergeDiag() {

            var ss = from r in StdDB.GetAllSubData()
                     select $"{r.FilterId:x8}|{r.StdFilePath}";

            var mergerWindow = new FileMergeWindow(ss.ToList());
            mergerWindow.ReturnHandler += new SubWindowReturnHandler((x) => {
                var ll = from r in (x as IEnumerable<string>)
                         let s = r.Split('|')
                         select new SubData(s[1], int.Parse(s[0], System.Globalization.NumberStyles.HexNumber));

                _ea.GetEvent<Event_MergeSubData>().Publish(ll);
            });
            mergerWindow.ShowDialog();
        }

        private bool _ifFileSelectionValid=false;
        public bool IfFileSelectionValid {
            get { return _ifFileSelectionValid; }
            set { SetProperty(ref _ifFileSelectionValid, value); }
        }

        private DelegateCommand openRtFileMergeDiag;
        public DelegateCommand OpenRtFileMergeDiag =>
            openRtFileMergeDiag ?? (openRtFileMergeDiag = new DelegateCommand(ExecuteOpenRtFileMergeDiag));

        void ExecuteOpenRtFileMergeDiag() {
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Title = "选择数据源文件";
            //openFileDialog.Filter = "All|*.*|STDF|*.stdf|STD|*.std";
            //openFileDialog.FileName = string.Empty;
            //openFileDialog.FilterIndex = 1;
            //openFileDialog.Multiselect = false;
            //openFileDialog.RestoreDirectory = false;
            //openFileDialog.DefaultExt = "stdf";
            //if (openFileDialog.ShowDialog() == false) {
            //    return;
            //}

            //var paths = openFileDialog.FileNames;
            //foreach (string path in paths) {
            //    var ext = System.IO.Path.GetExtension(path).ToLower();
            //    if (ext == ".stdf" || ext == ".std") {
            //        _ea.GetEvent<Event_AddRtFile>().Publish(path);
            //    } else {
            //        System.Windows.Forms.MessageBox.Show("Invalid File");
            //    }
            //    break;
            //}
            System.Windows.Forms.MessageBox.Show("TBD: use subdata merge instead");
        }

        private DelegateCommand _openCorrelationDiag;
        public DelegateCommand OpenCorrelationDiag =>
             _openCorrelationDiag ?? ( _openCorrelationDiag = new DelegateCommand(ExecuteOpenCorrelationDiag));

        void ExecuteOpenCorrelationDiag() {
            var corrWindow = new CorrDataSelectWindow(from r in StdDB.GetAllSubData() select r);
            corrWindow.ReturnHandler += new SubWindowReturnHandler((x) => {
                var ll = x as IEnumerable<SubData>;

                _ea.GetEvent<Event_CorrData>().Publish(ll);
            });
            corrWindow.ShowDialog();
        }

        private DelegateCommand _openSetupDiag;
        public DelegateCommand OpenSetupDiag =>
            _openSetupDiag ?? (_openSetupDiag = new DelegateCommand(ExecuteOpenSetupDiag));

        void ExecuteOpenSetupDiag() {
            var setupWindow = new SetupWindow();
            setupWindow.ShowDialog();
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
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }


        private DelegateCommand _cmdSetDftProgram;
        public DelegateCommand CmdSetDftProgram =>
            _cmdSetDftProgram ?? (_cmdSetDftProgram = new DelegateCommand(ExecuteCmdSetDftProgram));

        void ExecuteCmdSetDftProgram() {
            string str = System.Environment.CurrentDirectory;
            string pgmPath = str + "\\SillyMonkey.exe";
            string icoPath = str + "\\SA_48.ico";
            try {
                SA.SetFileOpenApp(".std", pgmPath, icoPath);
                SA.SetFileOpenApp(".stdf", pgmPath, icoPath);
            }
            catch {
                System.Windows.Forms.MessageBox.Show("Failed to set the default pgm");
                return;
            }
            System.Windows.Forms.MessageBox.Show("Done");
        }
    }
}
