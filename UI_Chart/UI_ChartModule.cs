using UI_Chart.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace UI_Chart
{
    public class UI_ChartModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Trend>();
            containerRegistry.RegisterForNavigation<Summary>();

        }
    }
}