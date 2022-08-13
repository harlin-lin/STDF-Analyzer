using Prism.Commands;
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
using System.Windows.Shapes;

namespace UI_DataList.Views {
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window {
        public AboutWindow() {
            DataContext = this;
            InitializeComponent();
        }

        private DelegateCommand _apply;
        public DelegateCommand Apply =>
            _apply ?? (_apply = new DelegateCommand(ExecuteApply));

        void ExecuteApply() {
            this.Close();
        }
    }
}
