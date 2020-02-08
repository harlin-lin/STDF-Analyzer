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
        Filter filter;

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

        private void ShowFilter() {
            filter = new Filter(dataParse, dataParse.GetAllFilter().Keys.ToList()[0]);
            filter.Show();

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            dataParse = new StdfParse(@"D:\ASRProj\STDF\Data\CP3-CP-FRB098.1-PTD211I-63KAL138.1-FRB098-01F6-20191015003425.stdf");
            dataParse.ExtractStdf();

            ShowFilter();
            sum.IsEnabled = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            generateReport();
        }
    }
}
