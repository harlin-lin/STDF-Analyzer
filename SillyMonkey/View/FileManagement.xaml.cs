using SillyMonkey.ViewModel;
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
    /// FileManagement.xaml 的交互逻辑
    /// </summary>
    public partial class FileManagement : UserControl {
        public FileManagement() {
            InitializeComponent();
        }

        private void evSelectedChange(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (e.NewValue is FileInfo) {
                var s = e.NewValue as FileInfo;
                //_stdFiles.ChangeFileSelected(s.FilePath.GetHashCode(), null);
            } else {

                var s = (KeyValuePair<byte, KeyValuePair<int, string>>)e.NewValue;
                //_stdFiles.ChangeFileSelected(s.Value.Value.GetHashCode(), s.Key);
            }
        }

    }
}
