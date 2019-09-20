using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

namespace SillyMonkey {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            //Setup();
            SetupDatatable();
        }

        void Setup() {
            Random r = new Random();

            int maxCol = 10;
            List<object> row = new List<object>(maxCol);
            for(int i=0; i< maxCol; i++) {
                row.Add(r.NextDouble().ToString());
            }

            grid.ItemsSource = row;

        }
        void SetupDatatable() {
            Stopwatch sp = new Stopwatch();


            Random r = new Random();

            int maxCol = 100;
            int maxRow = 1000;
            DataTable dataTable = new DataTable();
            List<object> row = new List<object>(maxCol);

            for (int i = 0; i < maxCol; i++) {
                dataTable.Columns.Add(i.ToString());
            }

            for (int i=0; i<maxRow; i++) {
                for (int j = 0; j < maxCol; j++) {
                    row.Add(r.NextDouble());
                }
                var v = row.ToArray();
                dataTable.Rows.Add(v);
                row.Clear();
            }
            sp.Start();
            grid.ItemsSource= dataTable.DefaultView;
            
            //grid.UpdateLayout();
            Console.WriteLine(sp.ElapsedMilliseconds);
            sp.Stop();
            sp.Reset();

        }

    }
}
