using DataInterface;
using SillyMonkeyD.ViewModels;
using System.Windows.Controls;

namespace SillyMonkeyD.Views {
    /// <summary>
    /// Interaction logic for GridTab.xaml
    /// </summary>
    public partial class RawGridTab : TabItem {
        RawGridTabViewModel rawgridTab;
        public RawGridTab(IDataAcquire dataAcquire, int filterId) {
            InitializeComponent();

            rawgridTab = new RawGridTabViewModel(dataAcquire, filterId, this);
            DataContext = rawgridTab;

        }

    }
}
