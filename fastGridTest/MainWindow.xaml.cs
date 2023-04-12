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
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            OpenStdFile(@"E:\temp\VTEST_CP_EOS_ASR5821A0-WAFER_P09N71.00_P09N71-02B5_TTJ-25_POS-86_ASR5821-PC-02_CP1_RP0_1969_20220911_005525.stdf");
            //var _rawDataModel = new FastDataGridModel(_subData);
            var _rawDataModel = new DataRaw_FastDataGridModel(_subData);
            rawgrid.Model = _rawDataModel;

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
