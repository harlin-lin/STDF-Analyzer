using DataInterface;
using DevExpress.Xpf.Core;
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
using DevExpress.Mvvm;
using SillyMonkeyD.ViewModels;

namespace SillyMonkeyD.Views {
    /// <summary>
    /// Interaction logic for GridTab.xaml
    /// </summary>
    public partial class RawGridTab : DXTabItem {
        RawGridTabViewModel rawgridTab;
        public RawGridTab(IDataAcquire dataAcquire, int filterId) {
            InitializeComponent();

            rawgridTab = new RawGridTabViewModel(dataAcquire, filterId);
            DataContext = rawgridTab;

        }

    }
}
