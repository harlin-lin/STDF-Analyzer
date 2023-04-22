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
using DataContainer;
using FileReader;

namespace fastGridTest {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        SubData _subData;
        FastDataGridModel _rawDataModel;
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            OpenStdFile(@"C:\Users\Harlin\Documents\SillyMonkey\stdfData\12345678.stdf");
            //var _rawDataModel = new FastDataGridModel(_subData);
            _rawDataModel = new FastDataGridModel(_subData);
            rawgrid.Model = _rawDataModel;
            rawgrid.ColumnHeaderDoubleClick += Rawgrid_ColumnHeaderDoubleClick;

        }

        private void Rawgrid_ColumnHeaderDoubleClick(object arg1, FastWpfGrid.ColumnClickEventArgs arg2) {
            Console.WriteLine(arg2.Column.ToString());
            _rawDataModel.SortColumn(arg2.Column);
        }

        private void OpenStdFile(string path) {
            try {
                var info = new System.IO.FileInfo(path);
            }
            catch {
                return;
            }

            if (StdDB.IfExsistFile(path)) {
                return;
            }
            try {
                var dataAcquire = StdDB.CreateSubContainer(path);
                using (var data = new StdReader(path, StdFileType.STD)) {
                    data.ExtractStdf();
                }
                var id = dataAcquire.CreateFilter();
                _subData = new SubData(path, id);

            }
            catch (Exception e) {
                return;
            }

        }
    }
}
