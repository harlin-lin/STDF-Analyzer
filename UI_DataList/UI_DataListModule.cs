using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UI_DataList.Views;

namespace UI_DataList {
    public class UI_DataListModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("Region_DataList", typeof(DataManagement));
            regionManager.RegisterViewWithRegion("Region_Summary", typeof(DataSummary));
            regionManager.RegisterViewWithRegion("Region_Filter", typeof(DataFilter));
            regionManager.RegisterViewWithRegion("Region_Menu", typeof(TopMenu));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            
        }
    }
}