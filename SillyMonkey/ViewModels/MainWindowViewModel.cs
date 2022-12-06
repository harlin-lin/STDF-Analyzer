﻿using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Linq;
using System.Windows;
using UI_Data.ViewModels;
using Utils;

namespace SillyMonkey.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        IEventAggregator _ea;
        IRegionManager _regionManager;
        
        private string _title = "StdfAnalyzer";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator ea)
        {
            _regionManager = regionManager;
            _ea = ea;

        }

        private DelegateCommand<object> selectSubdata;
        public DelegateCommand<object> SelectTab =>
            selectSubdata ?? (selectSubdata = new DelegateCommand<object>(ExecuteSelectSubData));

        void ExecuteSelectSubData(object parameter) {
            if (parameter is null || !parameter.GetType().Name.Equals("DataRaw")) return;
            var view = (parameter as UI_Data.Views.DataRaw).DataContext as DataRawViewModel;
            if (view.CurrentData.HasValue) {
                _ea.GetEvent<Event_SubDataTabSelected>().Publish(view.CurrentData.Value);
            }
        }

        private DelegateCommand<DragEventArgs> mainWindowDropped;
        public DelegateCommand<DragEventArgs> MainWindowDropped =>
            mainWindowDropped ?? (mainWindowDropped = new DelegateCommand<DragEventArgs>(ExecuteMainWindowDropped));

        void ExecuteMainWindowDropped(DragEventArgs parameter) {
            var paths = ((System.Array)parameter.Data.GetData(DataFormats.FileDrop));
            if (paths is null) return;
            foreach (string path in paths) {
                var ext = System.IO.Path.GetExtension(path).ToLower();
                if (ext == ".stdf" || ext == ".std") {
                    _ea.GetEvent<Event_OpenFile>().Publish(path);
                } else {
                    MessageBox.Show("Only support stdf or std file");
                }
            }

        }


        private DelegateCommand mainWindowLoaded;

        public DelegateCommand MainWindowLoaded => 
            mainWindowLoaded ?? (mainWindowLoaded = new DelegateCommand(MainWindow_LoadExecute));

        private void MainWindow_LoadExecute() {
            SA.Init();

            string[] commandLineArgs = Environment.GetCommandLineArgs(); // [a-zA-Z]:[\\\/](?:[a-zA-Z0-9]+[\\\/])*([a-zA-Z0-9]+.*)

            //string s="";
            //foreach(var v in commandLineArgs) {
            //    s += v;
            //    s += "\n";
            //}

            //System.IO.File.WriteAllText(@"C:\Users\Harlin\Documents\temp\123.txt", s);

            commandLineArgs = commandLineArgs.Skip(1).ToArray(); 
            if (commandLineArgs.Length < 1) {

            } else {
                var path = commandLineArgs[0];
                var ext = System.IO.Path.GetExtension(path).ToLower();
                if (ext == ".stdf" || ext == ".std") {
                    _ea.GetEvent<Event_OpenFile>().Publish(path);
                } else {
                    //System.Windows.Forms.MessageBox.Show("Invalid File");
                }

            }
        }


    }
}
