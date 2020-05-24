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
    /// Interaction logic for ItemChartTab.xaml
    /// </summary>
    public partial class ItemChartTab : DXTabItem {
        ItemChartTabViewModel itemChartTab;
        public ItemChartTab(IDataAcquire dataAcquire, int filterId) {
            InitializeComponent();
            itemChartTab = new ItemChartTabViewModel(dataAcquire, filterId);
            DataContext = itemChartTab;
        }
    }
}
