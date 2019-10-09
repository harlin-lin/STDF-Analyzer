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
using DataAnalyser;
using SillyMonkey.View;

namespace SillyMonkey {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        private Analyser _stdFiles;

        public MainWindow() {
            InitializeComponent();
            Setup();
        }

        void Setup(){
            _stdFiles = new Analyser();
            this.DataContext = _stdFiles;
            AddDataTab("123");
            AddDataTab("321");
        }

        private void evDragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;
        }

        private async void evDrop(object sender, DragEventArgs e) {
            var paths = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
            foreach (string path in paths) {
                var ext = System.IO.Path.GetExtension(path).ToLower();
                if (ext == ".stdf" || ext == ".std") {
                    _stdFiles.AddFile(path);
                } else {
                    //log message not supported
                }
            }

            //extract the files
            await Task.Run(new Action(() => _stdFiles.ExtractFiles(new List<string>(paths.OfType<string>())))) ;

        }


        private void evSelectedChange(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if(e.NewValue is FileInfo) {
                var s = e.NewValue as FileInfo;
                _stdFiles.ChangeFileSelected(s.FilePath.GetHashCode(), null);
            } else {

                var s = (KeyValuePair<byte, KeyValuePair<int, string>>)e.NewValue;
                _stdFiles.ChangeFileSelected(s.Value.Value.GetHashCode(), s.Key);
            }
        }

        private void AddDataTab(string name) {
            DataGridTab dataGridTab = new DataGridTab();

            TabItem tabItem = new TabItem();
            tabItem.Header = name;
            tabItem.Content= dataGridTab;
            dataTab.Items.Add(tabItem);
        }


    }
}
