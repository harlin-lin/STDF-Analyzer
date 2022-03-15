using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MapBase {
    partial class WaferMapControl {

        public IWaferData WaferDataSource {
            get { return (IWaferData)GetValue(WaferDataProperty); }
            set { 
                SetValue(WaferDataProperty, value); 
            }
        }

        //public static DependencyObject GetTarget(IWaferData waferData) {
        //    if (waferData == null)
        //        throw new ArgumentNullException("waferData");

        //    //waferData.GetValue(WaferDataProperty)
        //    return null as DependencyObject;
        //}

        //public static void SetTarget(IWaferData waferData, DependencyObject value) {
        //    if (waferData == null)
        //        throw new ArgumentNullException("waferData");

        //    //waferData.SetValue(WaferDataProperty, value);
        //}


        // Using a DependencyProperty as the backing store for WaferData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WaferDataProperty =
            DependencyProperty.Register(nameof(WaferDataSource), typeof(IWaferData), typeof(WaferMapControl), new PropertyMetadata(null, OnDataSourceChanged)); //FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, 

        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((WaferMapControl)d).OnDataSourceChanged((IWaferData)e.NewValue);
        }

        public MapBinMode BinMode {
            get { return (MapBinMode)GetValue(BinModeProperty); }
            set { SetValue(BinModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BinMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BinModeProperty =
            DependencyProperty.Register("BinMode", typeof(MapBinMode), typeof(WaferMapControl), new PropertyMetadata(MapBinMode.SBin, OnMapBinModeChanged));

        private static void OnMapBinModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((WaferMapControl)d).UpdateBinMode();
        }



        public MapViewMode ViewMode {
            get { return (MapViewMode)GetValue(ViewModeProperty); }
            set { SetValue(ViewModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModeProperty =
            DependencyProperty.Register("ViewMode", typeof(MapViewMode), typeof(WaferMapControl), new PropertyMetadata(MapViewMode.Split, OnMapViewModeChanged));

        private static void OnMapViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((WaferMapControl)d).UpdateViewMode();
        }

        public MapRtDataMode RtDataMode {
            get { return (MapRtDataMode)GetValue(RtDataModeProperty); }
            set { SetValue(RtDataModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RtDataMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RtDataModeProperty =
            DependencyProperty.Register("RtDataMode", typeof(MapRtDataMode), typeof(WaferMapControl), new PropertyMetadata(MapRtDataMode.OverWrite, OnMapRtDataModeChanged));

        private static void OnMapRtDataModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((WaferMapControl)d).UpdateRtDataMode();
        }
    }
}
