using DataInterface;
using SillyMonkeyD.ViewModels;
using System.Windows.Controls;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using SciChart.Data.Model;
using System.Collections.Generic;

namespace SillyMonkeyD.Views {
    /// <summary>
    /// Interaction logic for ItemChartTab.xaml
    /// </summary>
    public partial class ItemChartTab : TabItem {
        ItemChartTabViewModel itemChartTab;
        public ItemChartTab(IDataAcquire dataAcquire, int filterId, List<TestID> testIDs) {
            InitializeComponent();
            itemChartTab = new ItemChartTabViewModel(dataAcquire, filterId, testIDs, this);
            DataContext = itemChartTab;
        }
    }
}
