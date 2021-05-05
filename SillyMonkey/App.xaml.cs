using SillyMonkey.Views;
using Prism.Ioc;
using Prism.Unity;
using Prism.Modularity;
using System.Windows;

namespace SillyMonkey
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) {
            moduleCatalog.AddModule<UI_DataList.UI_DataListModule>();
            moduleCatalog.AddModule<UI_Data.UI_DataModule>();
        }
    }
}
