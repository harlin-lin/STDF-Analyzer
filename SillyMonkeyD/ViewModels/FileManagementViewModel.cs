using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DataInterface;
using DataParse;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using FileHelper;
using Microsoft.Win32;
using SillyMonkeyD.Views;

namespace SillyMonkeyD.ViewModels {

    public class FileManagementViewModel : ViewModelBase {

        public FileManagementViewModel() {
            Files = new ObservableCollection<IDataAcquire>();
            Sites = null;
            SelectedFile = null;
            //SelectedSite = new KeyValuePair<byte, int>();

            TabList = new ObservableCollection<DXTabItem>();

            InitUiCtr();
        }

        private FilterEditor _filterWindow;
        private Data _dataWindow;


        public ObservableCollection<IDataAcquire> Files { get; private set; }
        public ObservableCollection<DXTabItem> TabList { get; private set; }
        public Dictionary<byte, int> Sites { get { return GetProperty(() => Sites); } private set { SetProperty(() => Sites, value); } }
        public IDataAcquire SelectedFile { get { return GetProperty(() => SelectedFile); } set { SetProperty(() => SelectedFile, value); } }
        public KeyValuePair<byte, int> SelectedSite { get { return GetProperty(() => SelectedSite); } set { SetProperty(() => SelectedSite, value); } }
        public DXTabItem SelectedTab { get { return GetProperty(() => SelectedTab); } set { SetProperty(() => SelectedTab, value); } }
        public string FileInfo { get { return GetProperty(() => FileInfo); } private set { SetProperty(() => FileInfo, value); } }



        public async void AddFile(string path) {
            IDataAcquire data = new StdfParse(path);
            Files.Add(data);
            //extract the files
            await System.Threading.Tasks.Task.Run(new Action(() => data.ExtractStdf()));
        }
        private void RemoveFile(IDataAcquire data) {
            Files.Remove(data);
            GC.Collect();
        }

        private void AddTab(IDataAcquire dataAcquire, int filterId) {
            RawGridTab rawGridTab = new RawGridTab(dataAcquire, filterId);
            ((DataViewModel)_dataWindow.DataContext).AddTab(rawGridTab);
            TabList.Add(rawGridTab); 
        }

        private void RemoveTab(DXTabItem tabItem) {
            ((DataViewModel)_dataWindow.DataContext).RemoveTab(tabItem);
            TabList.Remove(tabItem);
        }

        private void LoadWindow() {
            if (_dataWindow is null) {
                //await System.Threading.Tasks.Task.Run(new Action(() => _dataWindow = new Data()));
                _dataWindow = new Data();
                ((DataViewModel)_dataWindow.DataContext).SelectedTabEvent += ((e) => SelectedTab = e);
                //_dataWindow.Show();
            }
            if (_filterWindow is null) {
                //await System.Threading.Tasks.Task.Run(new Action(() => new FilterEditor()));
                _filterWindow = new FilterEditor();
                ((FilterEditorViewModel)_filterWindow.DataContext).FilterUpdatedEvent += ((x, y)=> {
                    UpdateFilter(x, y);
                });
                //_filterWindow.Visibility = Visibility.Hidden;
                //_filterWindow.Show();
            }

        }

        private void ShowFilterWindow(IDataAcquire dataAcquire, int filterId) {
            ((FilterEditorViewModel)_filterWindow.DataContext).UpdateFilter(dataAcquire, filterId);
            _filterWindow.Show();
        }

        private void ShowDataWindow(DXTabItem tabItem) {
            _dataWindow.Show();
            ((DataViewModel)_dataWindow.DataContext).FocusTab(tabItem);
        }
        private void ShowDataWindow() {
            _dataWindow.Show();
        }

        private void UpdateFilter(IDataAcquire dataAcquire, int filterId) {
            foreach(var t in TabList) {
                if(((ITab)t.DataContext).DataAcquire == dataAcquire && ((ITab)t.DataContext).FilterId == filterId) {
                    ((ITab)t.DataContext).UpdateFilter();
                }
            }
        }

        private DXTabItem FindOpendTab(IDataAcquire dataAcquire, int filterId, string tabType) {
            foreach (var t in TabList) {
                if (((ITab)t.DataContext).DataAcquire == dataAcquire && ((ITab)t.DataContext).FilterId == filterId) {
                    if (t.GetType().Name == tabType)
                        return t;
                }
            }
            return null;
        }

        #region UI
        public ICommand<DragEventArgs> DropCommand { get; private set; }
        public ICommand<IDataAcquire> FileCloseCommand { get; private set; }
        public ICommand<DXTabItem> TabCloseCommand { get; private set; }
        public ICommand FocusTab { get; private set; }
        public ICommand OpenFileRawData { get; private set; }
        public ICommand OpenSiteRawData { get; private set; }
        public ICommand UpdateInfo { get; private set; }
        public ICommand OpenFileDiag { get; private set; }
        public ICommand LoadedCommand { get; private set; }
        public ICommand<DXTabItem> SetFilter { get; private set; }

        private void InitUiCtr() {
            DropCommand = new DelegateCommand<DragEventArgs>((e) => {
                var paths = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
                foreach (string path in paths) {
                    var ext = System.IO.Path.GetExtension(path).ToLower();
                    if (ext == ".stdf" || ext == ".std") {
                        AddFile(path);
                    } else {
                        //log message not supported
                    }
                }
            });

            OpenFileDiag = new DelegateCommand(()=> {
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
                        AddFile(path);
                    } else {
                        //log message not supported
                    }
                }

            });

            FileCloseCommand = new DelegateCommand<IDataAcquire>((e) => {
                RemoveFile(e);
            });
            TabCloseCommand = new DelegateCommand<DXTabItem>((e) => {
                RemoveTab(e);
            });

            FocusTab = new DelegateCommand(() => {
                ShowDataWindow(SelectedTab);

            });

            OpenFileRawData = new DelegateCommand(() => {
                var v = FindOpendTab(SelectedFile, SelectedFile.GetFilterID(null), "RawGridTab");
                if(v is null) {
                    AddTab(SelectedFile, SelectedFile.GetFilterID(null));
                    ShowDataWindow();
                } else {
                    ShowDataWindow(v);
                }

            });
            OpenSiteRawData = new DelegateCommand(() => {
                var v = FindOpendTab(SelectedFile, SelectedFile.GetFilterID(SelectedSite.Key), "RawGridTab");
                if (v is null) {
                    AddTab(SelectedFile, SelectedFile.GetFilterID(SelectedSite.Key));
                    ShowDataWindow();
                } else {
                    ShowDataWindow(v);
                }
            });

            UpdateInfo = new DelegateCommand(() => {
                if (SelectedFile is null)
                    Sites = null;
                else if (SelectedFile.ParseDone) {
                    Sites = SelectedFile.GetSitesChipCount();
                    FileInfo = StdFileHelper.GetBriefSummary(SelectedFile);
                }
                else
                    Sites = null;
            });
            LoadedCommand = new DelegateCommand(() => {
                LoadWindow();
            });

            SetFilter = new DelegateCommand<DXTabItem>((e) => {
                ShowFilterWindow(((ITab)e.DataContext).DataAcquire, ((ITab)e.DataContext).FilterId);
            });
        }


        #endregion

    }
}