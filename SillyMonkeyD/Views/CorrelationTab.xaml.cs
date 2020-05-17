using DataInterface;
using DevExpress.Xpf.Core;
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
    /// Interaction logic for CorrelationTab.xaml
    /// </summary>
    public partial class CorrelationTab : DXTabItem {
        CorrelationTabViewModel corrTab;
        public CorrelationTab(List<Tuple<IDataAcquire, int>> dataFilterTuple) {
            InitializeComponent();

            corrTab = new CorrelationTabViewModel(dataFilterTuple);
            DataContext = corrTab;

        }

    }
}
