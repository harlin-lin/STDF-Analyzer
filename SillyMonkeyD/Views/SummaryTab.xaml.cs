using DataInterface;
using SillyMonkeyD.ViewModels;
using System.Windows.Controls;

namespace SillyMonkeyD.Views {
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class SummaryTab : TabItem {
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
