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
using System.Windows.Media;

namespace SillyMonkeyD.ViewModels {
    public static class TrendChartModel {
        public static ObservableCollection<IRenderableSeriesViewModel> GetChartData(List<float?[]> itemsData, List<TestID> iDs,  List<IChipInfo> chips) {
            ObservableCollection<IRenderableSeriesViewModel> renderableSeries = new ObservableCollection<IRenderableSeriesViewModel>();

            for(int j=0; j<itemsData.Count; j++) {
                var xyData = new XyDataSeries<double, double>() { SeriesName = $"{iDs[j].MainNumber}.{iDs[j].SubNumber}" };
                for (int i = 0; i < itemsData[j].Length; i++) {
                    if (itemsData[j][i].HasValue)
                        xyData.Append(chips[i].InternalId, itemsData[j][i].Value);
                }

                //var x = (from r in chips
                //         let d = (double)r.InternalId
                //         select d);
                //xyData.Append(x, itemsData[j].Select(a=>(double)a));
                renderableSeries.Add(new LineRenderableSeriesViewModel() {
                    DataSeries = xyData,
                    DrawNaNAs = SciChart.Charting.Visuals.RenderableSeries.LineDrawMode.Gaps,
                    StrokeThickness = 1,
                    Stroke = GetColor(j),
                    // Set the StyleKey equal to the x:Key in XAML
                    StyleKey = "LineSeriesStyle0"
                }); ; 
            }

            return renderableSeries;
        }

        static Color GetColor(int i) {
            switch(i%16){
                case 0: return Color.FromRgb(0, 0, 255);
                case 1: return Color.FromRgb(60, 179, 113);
                case 2: return Color.FromRgb(0, 100, 0);
                case 3: return Color.FromRgb(106, 90, 205);
                case 4: return Color.FromRgb(176, 196, 222);
                case 5: return Color.FromRgb(95, 158, 160);
                case 6: return Color.FromRgb(0, 139, 139);
                case 7: return Color.FromRgb(218, 165, 32);
                case 8: return Color.FromRgb(255, 165, 0);
                case 9: return Color.FromRgb(139, 69, 19);
                case 10: return Color.FromRgb(255, 69, 0);
                case 11: return Color.FromRgb(240, 128, 128);
                case 12: return Color.FromRgb(128, 0, 0);
                case 13: return Color.FromRgb(0, 128, 128);
                case 14: return Color.FromRgb(138, 43, 226);
                case 15: return Color.FromRgb(139, 0, 139);
                default: return Color.FromRgb(75, 0, 130);
            }
        }
    }
}
