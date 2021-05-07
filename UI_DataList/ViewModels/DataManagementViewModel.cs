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

    public class FileNode {
        public string NodeName { get; private set; }
        public string NodeTip { get; private set; }
        public bool ExtractedDone { get; private set; }
        public ObservableCollection<ISubNode> SubDataList { get; private set; }

        public IDataAcquire DataAcquire { get; }

        public FileNode(IDataAcquire dataAcquire) {
            DataAcquire = dataAcquire;
            SubDataList = new ObservableCollection<ISubNode>();
            Update();
        }

        public void Update() {
            NodeName = DataAcquire.FileName;
            NodeTip = DataAcquire.FilePath;
            ExtractedDone = DataAcquire.ParseDone;
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
            get { return _files; }
            private set { SetProperty(ref _files, value); } 
        }

        public DataManagementViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_OpenData>().Subscribe(CreateNewRawTab);

            Files = new ObservableCollection<FileNode>();

            SelectData = new DelegateCommand<object>((x)=> {
                if(x.GetType().Name == "FileNode") {
                    _ea.GetEvent<Event_DataSelected>().Publish((x as FileNode).DataAcquire);
                }else if(x.GetType().Name == "FilterNode") {
                    _ea.GetEvent<Event_SubDataSelected>().Publish(new SubData((x as FilterNode).ParentNode.DataAcquire, (x as FilterNode).FilterId));
                } else {
                    _ea.GetEvent<Event_DataSelected>().Publish(null);
                }

            });

            OpenData = new DelegateCommand<object>((x)=> {
                //if (x.GetType().Name == "FileNode") {
                //    var f = (x as FileNode).DataAcquire;
                //    int id=f.CreateFilter();
                //    _ea.GetEvent<Event_OpenData>().Publish(new SubData(f,id));
                //} else 
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

            });

            ///////////////////////////////////////////////////
            //test
            StdFileHelper stdFileHelper = new StdFileHelper();
            //var v =stdFileHelper.AddFile(@"E:\Data\12345678.stdf");
            var v = stdFileHelper.AddFile(@"C:\Users\Harlin\Documents\SillyMonkey\stdfData\12345678.stdf");
            v.ExtractDone += V_ExtractDone;
            v.ExtractStdf();
            //////////////////////////////////////////////////


        }

        private void V_ExtractDone(IDataAcquire data) {
            data.CreateFilter();
            Files.Add(new FileNode(data));
        }

        private void CreateNewRawTab(SubData data) {
            var parameters = new NavigationParameters();
            parameters.Add("subData", data);

            _regionManager.RequestNavigate("Region_DataView", "DataRaw", parameters);
        }


        public DelegateCommand<object> SelectData { get; private set; }
        public DelegateCommand<object> OpenData { get; private set; }

    }
}
