using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class OpenFiles{
        public string File { get; set; }
        public List<string> Sites { get; set; }

        public OpenFiles(string file) {
            File = file;
            Sites = new List<string>();
            Enumerable.Range(1, 10).ToList().ForEach(x => Sites.Add("Site:" + x));
        }

    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            Setup();
            //SetupDatatable();
        }

        void Setup(){
            var data = new ObservableCollection<OpenFiles>();
            Enumerable.Range(0, 9).ToList().ForEach(x => data.Add(new OpenFiles(x.ToString())));

            tvFiles.DataContext =data;

        }
    }
}
