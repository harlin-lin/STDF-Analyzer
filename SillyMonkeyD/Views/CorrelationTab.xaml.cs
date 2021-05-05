using DataInterface;
using SillyMonkeyD.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SillyMonkeyD.Views {
    /// <summary>
    /// Interaction logic for CorrelationTab.xaml
    /// </summary>
    public partial class CorrelationTab : TabItem {
        CorrelationTabViewModel corrTab;
        public CorrelationTab(List<SubData> dataFilterTuple) {
            InitializeComponent();

            corrTab = new CorrelationTabViewModel(dataFilterTuple, this);
            DataContext = corrTab;

        }

    }
}
