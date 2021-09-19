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
using DataContainer;
using FileReader;

namespace UI_DataList.ViewModels {

    public interface ISubNode {
        string NodeName { get; }
        string FilePath { get; }
        FileNode ParentNode { get; }
    }

    public class SiteNode : ISubNode {
        public string NodeName { get; private set; }
        public string FilePath { get; private set; }

        public byte Site { get; private set; }

        public FileNode ParentNode { get; private set; }

        public SiteNode(FileNode file, byte site, int cnt) {
            ParentNode = file;
            Site = site;
            NodeName = $"Site {site}: {cnt}";
            FilePath = ParentNode.FilePath;
        }
    }

    public class SiteCollectionNode : ISubNode {
        public string NodeName { get; private set; }
        public string FilePath { get; private set; }

        public IEnumerable<SiteNode> SiteList { get; private set; }

        public FileNode ParentNode { get; private set; }

        public SiteCollectionNode(FileNode file) {
            ParentNode = file;
            if (!file.ExtractedDone) 
                return;
            var dataAcquire = StdDB.GetDataAcquire(FilePath);
            SiteList = (from r in dataAcquire.GetSitesChipCount()
                        let l = new SiteNode(file, r.Key, r.Value)
                        select l);
            NodeName = "Site List";
            FilePath = file.FilePath;
        }
    }

    public class FilterNode : ISubNode {
        public string NodeName { get; private set; }

        public string FilePath { get; private set; }

        public FileNode ParentNode { get; private set; }

        public int FilterId { get; }

        public FilterNode(FileNode file, int filterId, int filterIdx) {
            ParentNode = file;
            FilterId = filterId;
            NodeName = $"Filter_{filterIdx}:{filterId.ToString("x8")}";
            FilePath = "";
        }
    }

    public class FileNode : BindableBase {
        public string NodeName { get; private set; }
        public string FilePath { get; private set; }
        public bool ExtractedDone { get; private set; }
        public ObservableCollection<ISubNode> SubDataList { get; private set; }

        public FileNode(string path) {
            SubDataList = new ObservableCollection<ISubNode>();
            FilePath = path;
            NodeName = System.IO.Path.GetFileName(path);
            ExtractedDone = false;
        }

        public void Update() {
            var dataAcquire = StdDB.GetDataAcquire(FilePath);
            ExtractedDone = dataAcquire.LoadingDone;
            RaisePropertyChanged("ExtractedDone");
            SubDataList.Clear();
            SubDataList.Add(new SiteCollectionNode(this));
            int i = 0;
            foreach(var f in dataAcquire.GetAllFilterId()) {
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
        }

        private async void OpenStdFile(string path) {
            if (StdDB.IfExsistFile(path)) return;
            var f = new FileNode(path);
            Files.Add(f);

            using (var data = new StdReader(path, StdFileType.STD)) {
                try {
                    await Task.Run(() => { data.ExtractStdf();});
                }catch {
                    Files.Remove(f);
                    return;
                }
                LoadingDone(path);
            }

        }

        private void LoadingDone(string path) {
            var dataAcquire = StdDB.GetDataAcquire(path);
            var id = dataAcquire.CreateFilter();
            Files.First((f) => {
                if (f.FilePath == dataAcquire.FilePath) return true;
                return false;
            }).Update();
            RaisePropertyChanged("Files");
            _ea.GetEvent<Event_OpenData>().Publish(new SubData(path, id));
        }

        private void CreateNewRawTab(SubData data) {
            var parameters = new NavigationParameters();
            parameters.Add("subData", data);

            _regionManager.RequestNavigate("Region_DataView", "DataRaw", parameters);
        }



        private DelegateCommand<object> _selectData;
        public DelegateCommand<object> SelectData =>
            _selectData ?? (_selectData = new DelegateCommand<object>(ExecuteSelectData));

        void ExecuteSelectData(object x) {
            if (x.GetType().Name == "FileNode") {
                if((x as FileNode).ExtractedDone)
                    _ea.GetEvent<Event_DataSelected>().Publish((x as FileNode).FilePath);
            } else if (x.GetType().Name == "FilterNode") {
                _ea.GetEvent<Event_SubDataSelected>().Publish(new SubData((x as FilterNode).ParentNode.FilePath, (x as FilterNode).FilterId));
            } else {
                _ea.GetEvent<Event_DataSelected>().Publish(null);
            }
        }

        private DelegateCommand<object> _openData;
        public DelegateCommand<object> OpenData =>
            _openData ?? (_openData = new DelegateCommand<object>(ExecuteOpenData));

        void ExecuteOpenData(object x) {
            if (x.GetType().Name == "FilterNode") {
                _ea.GetEvent<Event_OpenData>().Publish(new SubData((x as FilterNode).ParentNode.FilePath, (x as FilterNode).FilterId));
            } else if (x.GetType().Name == "SiteNode") {
                var s = (x as SiteNode);
                var f = StdDB.GetDataAcquire(s.FilePath);
                var id = f.CreateFilter(s.Site);

                s.ParentNode.Update();
                RaisePropertyChanged("Files");

                _ea.GetEvent<Event_OpenData>().Publish(new SubData(s.FilePath, id));
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
