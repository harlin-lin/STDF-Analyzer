using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UI_Chart.Views;

namespace UI_Chart {
    public class UI_ChartModule : IModule {
        public void OnInitialized(IContainerProvider containerProvider) {
            var regionManager = containerProvider.Resolve<IRegionManager>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterForNavigation<Trend>();
            containerRegistry.RegisterForNavigation<Summary>();
            containerRegistry.RegisterForNavigation<Raw>();
            containerRegistry.RegisterForNavigation<ItemCorr>();
            containerRegistry.RegisterForNavigation<WaferMap>();

            containerRegistry.RegisterForNavigation<CorrChart>();
            containerRegistry.RegisterForNavigation<SiteCorrChart>();

        }
    }
}