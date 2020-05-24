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
    /// Interaction logic for WaferMapTab.xaml
    /// </summary>
    public partial class WaferMapTab : DXTabItem {
        WaferMapViewModel mapgridTab;

        public WaferMapTab(IDataAcquire dataAcquire, int filterId) {
            InitializeComponent();

            mapgridTab = new WaferMapViewModel(dataAcquire, filterId);
            DataContext = mapgridTab;

        }
    }
}
