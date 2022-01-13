using Prism.Commands;
using Prism.Mvvm;
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
using Utils;

namespace UI_DataList.Views {
    /// <summary>
    /// SetupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetupWindow : Window {
        public SetupWindow() {
            IfCmpTextInUid = SillyMonkeySetup.IfCmpTextInUid;
            DataContext = this;
            InitializeComponent();
        }

        public bool IfCmpTextInUid {
            get;
            set;
        }

        private DelegateCommand _apply;
        public DelegateCommand Apply =>
            _apply ?? (_apply = new DelegateCommand(ExecuteApply));

        void ExecuteApply() {
            SillyMonkeySetup.IfCmpTextInUid = IfCmpTextInUid;

            SillyMonkeySetup.ApplyAndSave();
            this.Close();
        }
    }


}
