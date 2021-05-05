using UI_Data.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace UI_Data
{
    public class UI_DataModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider){
        
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DataRaw>();
            containerRegistry.RegisterForNavigation<DataCorrelation>();
        }
    }
}