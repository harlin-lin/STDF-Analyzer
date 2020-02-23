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
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class SummaryTab : DXTabItem {
        SummaryTabViewModel summaryTab;
        public SummaryTab(IDataAcquire dataAcquire, int filterId) {
            InitializeComponent();
            summaryTab = new SummaryTabViewModel(dataAcquire, filterId);
            DataContext = summaryTab;
            rtbSum.Document = summaryTab.Summary;

            summaryTab.SummaryUpdateEvent += (()=> {
                rtbSum.Document = summaryTab.Summary;
            });
        }
    }
}
