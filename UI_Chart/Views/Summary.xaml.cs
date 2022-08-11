using SciChart.Charting.Model.DataSeries;
using System.Linq;
using System.Windows.Controls;

namespace UI_Chart.Views {
    /// <summary>
    /// Interaction logic for Summary
    /// </summary>
    public partial class Summary : UserControl {
        public Summary() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            // Create a data series of type X=double, Y=double
            var dataSeries = new XyDataSeries<int, int>();

            // Append data to series. SciChart automatically redraws
            dataSeries.Append(Enumerable.Range(0, 2).ToArray(), Enumerable.Range(0, 2).ToArray());

            scatterRenderSeries.DataSeries = dataSeries;

            sciChart.ZoomExtents();
        }
    }
}
