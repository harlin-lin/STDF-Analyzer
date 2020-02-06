using DataParse;
using FileHelper;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestWPF {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }
        StdfParse dataParse;

        private void generateReport() {

            SummaryHelper sh = new SummaryHelper(dataParse, dataParse.GetAllFilter().Keys.ToList()[0]);

            rtb.Document = sh.GetSummary();
        }

        private void saveReport() {

            //rtb.SaveFile(@"C: \Users\Harlin\Documents\Projects\STDF\Data\5502A_2K.rtf", RichTextBoxStreamType.RichText);
            System.IO.File.WriteAllText(@"C: \Users\Harlin\Documents\Projects\STDF\Data\5502A_2K.rtf", SummaryHelper.RTF(rtb));            
        }

        private void Grid_DragEnter(object sender, System.Windows.DragEventArgs e) {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.All;
            else
                e.Effects = System.Windows.DragDropEffects.None;


        }

        private void Grid_Drop(object sender, System.Windows.DragEventArgs e) {
            var paths = ((System.Array)e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop));

            dataParse = new StdfParse(paths.GetValue(0).ToString());
            dataParse.ExtractStdf();

            generateReport();

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            dataParse = new StdfParse(@"C:\Users\Harlin\Documents\Projects\801\Data\GT-issue lots\FRF678.1-PTD210\CP1-CP-FRF678.1-PTD210-63KCM146.1-FRF678-10C5-20191228132342.stdf");
            dataParse.ExtractStdf();

            generateReport();

        }
    }
}
