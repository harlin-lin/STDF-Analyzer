using SciChart.Charting.Model.DataSeries;
using SciChart.Examples.ExternalDependencies.Data;
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
            var dataSeries = new XyDataSeries<double, double>();

            var data = DataManager.Instance.GetDampedSinewave(1.0, 0.02, 200);

            // Append data to series. SciChart automatically redraws
            dataSeries.Append(data.XData, data.YData);

            scatterRenderSeries.DataSeries = dataSeries;

            sciChart.ZoomExtents();
        }
    }
}
