using C1.WPF.FlexGrid;
using SillyMonkey.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SillyMonkey.View {
    /// <summary>
    /// DataGridTab.xaml 的交互逻辑
    /// </summary>
    public partial class DataGridTab : UserControl {
        public DataGridTab() {
            InitializeComponent();
        }

        //public DataGridTabModel DataSource {
        //    get => (DataGridTabModel)GetValue(DataSourceProperty);
        //    set => SetValue(DataSourceProperty, value);
        //}

        //public static readonly DependencyProperty DataSourceProperty =
        //    DependencyProperty.Register("DataSource", typeof(DataGridTabModel), typeof(DataGridTab), new PropertyMetadata(null, (s, e) => {
        //        if (s is DataGridTab uc) {
        //            if (e.OldValue is INotifyPropertyChanged oldV) {
        //                oldV.PropertyChanged -= uc.DataSource_Changed;
        //            }

        //            if (e.NewValue is INotifyPropertyChanged newV) {
        //                newV.PropertyChanged += uc.DataSource_Changed;
        //            }
        //        }
        //    }));

        //private void DataSource_Changed(object sender, PropertyChangedEventArgs e) {
        //    // Logic Here
        //    if (e.PropertyName == "Data") {
        //        for (int i = 0; i < DataSource.Data.Columns.Count; i++) {
        //            grid.Columns.Add(new Column());
        //            grid.Columns[i].Header = DataSource.Data.Columns[i].ColumnName;
        //        }
        //        for (int r = 0; r < DataSource.Data.Rows.Count; r++) {
        //            for (int c = 0; c < DataSource.Data.Columns.Count; c++) {
        //                grid[r, c] = DataSource.Data.Rows[r][c];
        //            }
        //        }
        //    }
        //}

        //// Do Not Forget To Remove Event On UserControl Unloaded
        //private void DataGridTab_Unloaded(object sender, RoutedEventArgs e) {
        //    if (DataSource is INotifyPropertyChanged incc) {
        //        incc.PropertyChanged -= DataSource_Changed;
        //    }
        //}

        //private void DataGridTab_Loaded(object sender, RoutedEventArgs e) {
        //    if (DataSource != null) {
        //        this.DataContext = DataSource;
        //        for (int i = 0; i < DataSource.Data.Columns.Count; i++) {
        //            grid.Columns.Add(new Column());
        //            grid.Columns[i].Header = DataSource.Data.Columns[i].ColumnName;
        //        }
        //        for (int r = 0; r < DataSource.Data.Rows.Count; r++) {
        //            grid.Rows.Add(new Row());
        //            for (int c = 0; c < DataSource.Data.Columns.Count; c++) {
        //                var v = DataSource.Data.Rows[r][c];
        //                grid[r, c] = v;
        //            }
        //        }
        //    }

        //}
    }
}
