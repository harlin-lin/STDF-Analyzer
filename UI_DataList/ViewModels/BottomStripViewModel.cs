using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UI_DataList.ViewModels {
    public class BottomStripViewModel : BindableBase {
        IEventAggregator _ea;

        private string _info;
        public string Info {
            get { return _info; }
            set { SetProperty(ref _info, value); }
        }

        private int _progress;
        public int Progress {
            get { return _progress; }
            set { 
                SetProperty(ref _progress, value);
                if (value >= 100) {
                    SetProperty(ref _progress, 0);
                    Info = "";
                }
            }
        }

        private string _currentFileInfo;
        public string CurrentFileInfo {
            get { return _currentFileInfo; }
            set { SetProperty(ref _currentFileInfo, value); }
        }

        public BottomStripViewModel(IEventAggregator ea) {
            _ea = ea;

            Info = "";
            Progress = 0;
            _ea.GetEvent<Event_Progress>().Subscribe(x=> {
                Info = x.Item1;
                Progress = x.Item2;
            });

            _ea.GetEvent<Event_Log>().Subscribe(x => {
                Info = x;
                Progress = 0;
            });

            _ea.GetEvent<Event_FileInfo>().Subscribe(x => {
                CurrentFileInfo = x;
            });

        }
    }
}
