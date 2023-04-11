using DataContainer;
using Prism.Events;
using Prism.Regions;
using SillyMonkey.Core;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace UI_Chart.Views {
    /// <summary>
    /// Interaction logic for Summary
    /// </summary>
    public partial class Summary : UserControl, INavigationAware {
        public Summary(IRegionManager regionManager, IEventAggregator ea) {
            InitializeComponent();

            _regionManager = regionManager;
            _ea = ea;

        }

        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;


        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
                _subData = data;

                _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateFilter);

                UpdateSummary();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return false;
            //var data = (SubData)navigationContext.Parameters["subData"];

            //return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            _ea.GetEvent<Event_FilterUpdated>().Unsubscribe(UpdateFilter);
        }

        void UpdateFilter(SubData subData) {
            if (subData.Equals(_subData)) {
                UpdateSummary();
            }

        }

        void UpdateSummary() {
            summary.Text = GetSummary(StdDB.GetDataAcquire(_subData.StdFilePath), _subData.FilterId);
        }

        public string GetSummary(IDataAcquire dataAcquire, int filterId) {
            StringBuilder sb = new StringBuilder();

            var statistic = dataAcquire.GetFilteredPartStatistic(filterId);

            SummaryHelper.AppendBasicInfo(ref sb, dataAcquire);
            SummaryHelper.AppendCounters(ref sb, statistic);
            SummaryHelper.AppendSoftbin(ref sb, dataAcquire, statistic);
            SummaryHelper.AppendHardbin(ref sb, dataAcquire, statistic);
            //AppendItems();

            return sb.ToString();
        }



    }
}
