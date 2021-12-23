using DataContainer;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using System.Text;

namespace UI_Chart.ViewModels {
    public class SummaryViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;


        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
                _subData = data;


                UpdateSummary();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];

            return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {

        }

        public SummaryViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;

            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateFilter);

        }

        private string summary;
        public string Summary {
            get { return summary; }
            set { SetProperty(ref summary, value); }
        }



        void UpdateFilter(SubData subData) {
            if (subData.Equals(_subData)) {
                UpdateSummary();
            }

        }

        void UpdateSummary() {
            Summary = GetSummary(StdDB.GetDataAcquire(_subData.StdFilePath), _subData.FilterId);
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
