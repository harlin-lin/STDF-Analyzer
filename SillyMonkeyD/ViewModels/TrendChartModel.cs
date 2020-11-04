using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DataInterface;
using SciChart.Charting.Model.DataSeries;

namespace SillyMonkeyD.ViewModels {
    public static class TrendChartModel {
        public static ObservableCollection<IRenderableSeriesViewModel> GetChartData(List<float?[]> itemsData) {
            ObservableCollection<IRenderableSeriesViewModel> renderableSeries = new ObservableCollection<IRenderableSeriesViewModel>();

            foreach(var itemData in itemsData) {
                var xyData = new XyDataSeries<double, double>();
                for (int i = 0; i < itemData.Length; i++) {
                    if(itemData[i].HasValue)
                        xyData.Append(i, itemData[i].Value);
                }

                renderableSeries.Add(new LineRenderableSeriesViewModel() {
                    DataSeries = xyData,
                    // Set the StyleKey equal to the x:Key in XAML
                    StyleKey = "LineSeriesStyle0"
                });
            }

            return renderableSeries;
        }
    }
}
