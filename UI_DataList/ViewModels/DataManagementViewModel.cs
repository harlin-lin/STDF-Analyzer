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
using System.Threading.Tasks;
using FileReader;

namespace UI_DataList.ViewModels {

    public interface ISubNode {
        string NodeName { get; }
        string NodeTip { get; }
        FileNode ParentNode { get; }
    }

    public class SiteNode : ISubNode {
        public string NodeName { get; private set; }
        public string NodeTip { get; private set; }

        public byte Site { get; private set; }

        public FileNode ParentNode { get; private set; }

        public SiteNode(FileNode file, byte site, int cnt) {
            ParentNode = file;
            Site = site;
            NodeName = $"Site {site}: {cnt}";
            NodeTip = ParentNode.NodeTip;
        }
    }

    public class SiteCollectionNode : ISubNode {
        public string NodeName { get; private set; }
        public string NodeTip { get; private set; }

        public IEnumerable<SiteNode> SiteList { get; private set; }

        public FileNode ParentNode { get; private set; }

        public SiteCollectionNode(FileNode file) {
            ParentNode = file;
            if (!file.DataAcquire.ParseDone) 
                return;
            SiteList = (from r in file.DataAcquire.GetSitesChipCount()
                        let l = new SiteNode(file, r.Key, r.Value)
                        select l);
            NodeName = "Site List";
            NodeTip = file.NodeTip;
        }
    }

    public class FilterNode : ISubNode {
        public string NodeName { get; private set; }

        public string NodeTip { get; private set; }

        public FileNode ParentNode { get; private set; }

        public int FilterId { get; }

        public FilterNode(FileNode file, int filterId, int filterIdx) {
            ParentNode = file;
            FilterId = filterId;
            NodeName = $"Filter_{filterIdx}:{filterId.ToString("x8")}";
            NodeTip = "";
        }
    }

    public class FileNode :BindableBase{
        public string NodeName { get; private set; }
        public string NodeTip { get; private set; }
        public bool ExtractedDone { get; private set; }
        public ObservableCollection<ISubNode> SubDataList { get; private set; }

        public IDataAcquire DataAcquire { get; }

        public FileNode(IDataAcquire dataAcquire) {
            DataAcquire = dataAcquire;
            SubDataList = new ObservableCollection<ISubNode>();
            NodeName = DataAcquire.FileName;
            NodeTip = DataAcquire.FilePath;
            ExtractedDone = DataAcquire.ParseDone;
            //Update();
        }

        public void Update() {
            ExtractedDone = DataAcquire.ParseDone;
            RaisePropertyChanged("ExtractedDone");
            SubDataList.Clear();
            SubDataList.Add(new SiteCollectionNode(this));
            int i = 0;
            foreach(var f in DataAcquire.GetAllFilter().Keys) {
                SubDataList.Add(new FilterNode(this, f, i++));
            }
        }
    }

    public class DataManagementViewModel : BindableBase {
        IEventAggregator _ea;
        IRegionManager _regionManager;

        private ObservableCollection<FileNode> _files;
        public ObservableCollection<FileNode> Files {
            get { return _files ?? (_files = new ObservableCollection<FileNode>()); }
            private set { SetProperty(ref _files, value); } 
        }

        public DataManagementViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_OpenData>().Subscribe(CreateNewRawTab);
            _ea.GetEvent<Event_OpenFile>().Subscribe(OpenStdFile);
            _ea.GetEvent<Event_ParseFileDone>().Subscribe(ParseDone, ThreadOption.UIThread);


        }

        bool IfContainFile(string path) {
            foreach(var v in Files) {
                if(v.DataAcquire.FilePath == path) {
                    return true;
                }
            }
            return false;
        }

        private async void OpenStdFile(string path) {
            if (IfContainFile(path)) return;

            var dataAcquire = new StdReader(path, StdFileType.STD);
            var file = new FileNode(dataAcquire);
            Files.Add(file);

            dataAcquire.ExtractDone += ((x) => {
                _ea.GetEvent<Event_ParseFileDone>().Publish(x);
            });

            await System.Threading.Tasks.Task.Run(() => { 
                dataAcquire.ExtractStdf(); 
            });
        }

        private void ParseDone(IDataAcquire dataAcquire) {
            var id = dataAcquire.CreateFilter();
            Files.First((f) => {
                if (f.DataAcquire.FilePath == dataAcquire.FilePath) return true;
                return false;
            }).Update();
            RaisePropertyChanged("Files");
            _ea.GetEvent<Event_OpenData>().Publish(new SubData(dataAcquire, id));
        }

        private void CreateNewRawTab(SubData data) {
            foreach(var f in Files) {
                f.Update();
            }
            RaisePropertyChanged("Files");

            var parameters = new NavigationParameters();
            parameters.Add("subData", data);

            _regionManager.RequestNavigate("Region_DataView", "DataRaw", parameters);
        }



        private DelegateCommand<object> _selectData;
        public DelegateCommand<object> SelectData =>
            _selectData ?? (_selectData = new DelegateCommand<object>(ExecuteSelectData));

        void ExecuteSelectData(object x) {
            if (x.GetType().Name == "FileNode") {
                if((x as FileNode).DataAcquire.ParseDone)
                    _ea.GetEvent<Event_DataSelected>().Publish((x as FileNode).DataAcquire);
            } else if (x.GetType().Name == "FilterNode") {
                _ea.GetEvent<Event_SubDataSelected>().Publish(new SubData((x as FilterNode).ParentNode.DataAcquire, (x as FilterNode).FilterId));
            } else {
                _ea.GetEvent<Event_DataSelected>().Publish(null);
            }
        }

        private DelegateCommand<object> _openData;
        public DelegateCommand<object> OpenData =>
            _openData ?? (_openData = new DelegateCommand<object>(ExecuteOpenData));

        void ExecuteOpenData(object x) {
            if (x.GetType().Name == "FilterNode") {
                _ea.GetEvent<Event_OpenData>().Publish(new SubData((x as FilterNode).ParentNode.DataAcquire, (x as FilterNode).FilterId));
            } else if (x.GetType().Name == "SiteNode") {
                var s = (x as SiteNode);
                var f = s.ParentNode.DataAcquire;
                int id;
                FilterSetup filter = new FilterSetup("Site:" + s.Site);
                filter.EnableSingleSite(f.GetSites(), s.Site);
                id = f.CreateFilter(filter);

                s.ParentNode.Update();
                RaisePropertyChanged("Files");

                _ea.GetEvent<Event_OpenData>().Publish(new SubData(f, id));
            }
        }

        private DelegateCommand<object> _cmdCloseFile;
        public DelegateCommand<object> CmdCloseFile =>
            _cmdCloseFile ?? (_cmdCloseFile = new DelegateCommand<object>(ExecuteCmdCloseFile, CanExecuteCmdCloseFile));

        void ExecuteCmdCloseFile(object parameter) {
            var t = parameter.GetType();
        }

        bool CanExecuteCmdCloseFile(object parameter) {
            return true;
        }

    }
}
