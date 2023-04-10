using System.ComponentModel;
using System.Drawing;
using System.Windows.Controls;
using UI_Chart.ViewModels;
using Utils;

namespace UI_Chart.Views {
    /// <summary>
    /// Interaction logic for Trend
    /// </summary>
    public partial class CorrChart : UserControl {
        public CorrChart() {
            InitializeComponent();

            histoChart.RightClicked -= histoChart.DefaultRightClickEvent;
            histoChart.Configuration.DoubleClickBenchmark = false;
            histoChart.Configuration.LockHorizontalAxis = true;
        }


        private void chart_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            var vm = (INotifyPropertyChanged)(chart.DataContext);
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            //System.Diagnostics.Debug.WriteLine(sender.ToString());
            var vm = (CorrChartViewModel)sender;
            switch (e.PropertyName) {
                case "HistoSeries":
                    UpdateHistogram(vm);
                    break;
            }

        }

        void UpdateHistogram(CorrChartViewModel vm) {
            var series = vm.HistoSeries;

            histoChart.Plot.Clear();

            var cnt = series.Count;

            for (int j = 0; j < cnt; j++) {
                var color = SA.GetColor(j);

                var bar = histoChart.Plot.AddBar(series[j].Item1, series[j].Item2, Color.FromArgb(color.A, color.R, color.G, color.B));
                bar.BarWidth = vm.HistoViewRange.Item3 > 0 ? vm.HistoViewRange.Item3 : 1;
                bar.Label = vm.SubDataList[j].FilterId.ToString("X8");
            }
            histoChart.Plot.Legend(true, ScottPlot.Alignment.UpperRight);

            histoChart.Plot.AddVerticalLine(vm.LowLimit, Color.Red);
            histoChart.Plot.AddVerticalLine(vm.HighLimit, Color.Red);

            histoChart.Plot.SetAxisLimitsX(vm.HistoViewRange.Item1, vm.HistoViewRange.Item2);

            //histoChart.Plot.XLabel("Test Index");
            //histoChart.Plot.YLabel("Value");
            histoChart.Plot.Title(vm.ItemTitle);

            histoChart.Refresh();
        }

        private void histoChart_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            histoChart.Plot.AxisAutoY();
            histoChart.Refresh();
        }
    }
}
