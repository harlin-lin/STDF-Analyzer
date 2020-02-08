using DataParse;
using SillyMonkeyD.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SillyMonkeyD.Views {
    /// <summary>
    /// Interaction logic for FileManagement.xaml
    /// </summary>
    public partial class FileManagement : UserControl {
        public FileManagement() {
            InitializeComponent();
        }
        StdfParse dataParse;
        FilterEditor filter;

        private void Button_Click(object sender, RoutedEventArgs e) {

            dataParse = new StdfParse(@"D:\ASRProj\STDF\Data\CP3-CP-FRB098.1-PTD211I-63KAL138.1-FRB098-01F6-20191015003425.stdf");
            dataParse.ExtractStdf();

            filter = new FilterEditor(dataParse, dataParse.GetAllFilter().Keys.ToList()[0]);
            filter.Show();

        }
    }
}
