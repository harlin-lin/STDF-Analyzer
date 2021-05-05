using DataInterface;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SillyMonkey.Core;
using FileHelper;

namespace UI_DataList.ViewModels {

    public interface ISubNode {
        string NodeName { get; }
        string NodeTip { get; }
    }
    public class SiteNode : ISubNode {
        public string NodeName { get; private set; }
        public string NodeTip { get; private set; }

        public Dictionary<byte, int> SiteList { get; private set; }

        public SiteNode(IDataAcquire dataAcquire) {
            SiteList = new Dictionary<byte, int>(dataAcquire.GetSitesChipCount());
            NodeName = "Site List";
            NodeTip = $"Total {SiteList.Count} sites";
        }
    }

    public class FilterNode : ISubNode {
        public string NodeName { get; private set; }

        public string NodeTip { get; private set; }

        public FilterNode(int filterId, int filterIdx) {
            NodeName = $"Filter_{filterIdx}:{filterId.ToString("x8")}";
            NodeTip = "";
        }
    }

    public class FileNode {
        public string NodeName { get; private set; }
        public string NodeTip { get; private set; }
        public bool ExtractedDone { get; private set; }
        public ObservableCollection<ISubNode> SubDataList { get; private set; }

        private IDataAcquire _dataAcquire;

        public FileNode(IDataAcquire dataAcquire) {
            _dataAcquire = dataAcquire;
            SubDataList = new ObservableCollection<ISubNode>();
            Update();
        }

        public void Update() {
            NodeName = _dataAcquire.FileName;
            NodeTip = _dataAcquire.FilePath;
            ExtractedDone = _dataAcquire.ParseDone;
            SubDataList.Clear();
            SubDataList.Add(new SiteNode(_dataAcquire));
            int i = 1;
            foreach(var f in _dataAcquire.GetAllFilter().Keys) {
                SubDataList.Add(new FilterNode(f, i++));
            }
        }
    }

    public class DataManagementViewModel : BindableBase {
        IEventAggregator _ea;
        IRegionManager _regionManager;

        private ObservableCollection<FileNode> _files;
        public ObservableCollection<FileNode> Files {
            get { return _files; }
            private set { SetProperty(ref _files, value); } 
        }

        public DataManagementViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_NewFilter>().Subscribe(CreateNewRawTab);

            Files = new ObservableCollection<FileNode>();

            SelectData = new DelegateCommand<object>((x)=> {
                var t = x.GetType();

            });

            //test
            StdFileHelper stdFileHelper = new StdFileHelper();
            var v =stdFileHelper.AddFile(@"C:\Users\Harlin\Documents\SillyMonkey\stdfData\12345678.stdf");
            v.ExtractDone += V_ExtractDone;
            v.ExtractStdf();



        }

        private void V_ExtractDone(IDataAcquire data) {
            data.CreateFilter();
            Files.Add(new FileNode(data));
        }

        private void CreateNewRawTab(SubData data) {
            var parameters = new NavigationParameters();
            parameters.Add("tab", data);

            _regionManager.RequestNavigate("Region_DataExplorer", "DataRaw", parameters);
        }


        public DelegateCommand<object> SelectData { get; private set; }


    }
}
