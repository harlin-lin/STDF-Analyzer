using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MapBase {
    partial class MapBaseControl{

        public Color[,] MapDataSource {
            get { return (Color[,])GetValue(MapDataSourceProperty); }
            set { SetValue(MapDataSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MapDataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapDataSourceProperty =
            DependencyProperty.Register("MapDataSource", typeof(Color[,]), typeof(MapBaseControl), new PropertyMetadata(null, OnMapDataSourcePropertyChanged));

        private static void OnMapDataSourcePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
            ((MapBaseControl)dependencyObject).OnWaferDataSourcePropertyChanged();
        }

        private void OnWaferDataSourcePropertyChanged() {
            if (MapDataSource is null) return;
            _waferColor = MapDataSource;
            CreateRawBuffer();
        }


        public bool EnableZoom {
            get { return (bool)GetValue(EnableZoomProperty); }
            set { SetValue(EnableZoomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableZoom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableZoomProperty =
            DependencyProperty.Register("EnableZoom", typeof(bool), typeof(MapBaseControl), new PropertyMetadata(true));


        public string WaferNo;

    }
}
