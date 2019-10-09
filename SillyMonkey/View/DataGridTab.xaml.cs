using C1.WPF.FlexGrid;
using System;
using System.Collections.Generic;
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
            grid.ItemsSource = Customer.GetCustomerList(100);
            grid.MinColumnWidth = 85;
            grid.AllowDragging = C1.WPF.FlexGrid.AllowDragging.Both;
            grid.AllowResizing = C1.WPF.FlexGrid.AllowResizing.Both;
            grid.CellFactory = new MyCellFactory();

            foreach (Column c in grid.Columns) {
                if (c.ColumnName == "Country" || c.ColumnName == "Name" || c.ColumnName == "OrderAverage") {
                    //Hide these columns.
                    c.Visible = false;
                } else if (c.ColumnName == "Id") {
                    c.IsReadOnly = true;
                } else if (c.ColumnName == "CountryId") {
                    //Sets the DataMap to the country column so a picker is used to select the country.
                    Dictionary<int, string> dct = new Dictionary<int, string>();
                    foreach (var country in Customer.GetCountries()) {
                        dct[dct.Count] = country.Value;
                    }
                    c.ValueConverter = new ColumnValueConverter(dct);
                    c.Header = "Country";
                    c.HorizontalAlignment = HorizontalAlignment.Left;
                    c.Width = new GridLength(120);
                } else if (c.ColumnName == "OrderTotal" || c.ColumnName == "OrderAverage") {
                    //Sets currency format the these columns
                    c.Format = "C";
                } else if (c.ColumnName == "Address") {
                    c.HeaderTextWrapping = true;
                }
            }

        }
    }
}
