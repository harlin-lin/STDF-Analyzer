using DataInterface;
using SillyMonkeyD.ViewModels;
using System.Windows.Controls;

namespace SillyMonkeyD.Views {
    /// <summary>
    /// Interaction logic for WaferMapTab.xaml
    /// </summary>
    public partial class WaferMapTab : TabItem {
        WaferMapViewModel mapgridTab;

        public WaferMapTab(IDataAcquire dataAcquire, int filterId) {
            InitializeComponent();

            mapgridTab = new WaferMapViewModel(dataAcquire, filterId);
            DataContext = mapgridTab;

        }
    }
}
