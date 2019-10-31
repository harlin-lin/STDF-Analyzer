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

        private void Grid_ItemsSourceChanged(object sender, EventArgs e) {
            //grid.AutoSizeFixedColumns(0, grid.Columns.Count - 1, 10);
            foreach (var v in grid.Columns) {
                v.Width = new GridLength(40);
            }
            grid.Columns[1].Width = new GridLength(80);
        }
    }
}
