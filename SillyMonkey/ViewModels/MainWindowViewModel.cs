using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SillyMonkey.Core;
using UI_Data.ViewModels;

namespace SillyMonkey.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        IEventAggregator _ea;
        IRegionManager _regionManager;
        
        private string _title = "SillyMonkey";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator ea)
        {
            _regionManager = regionManager;
            _ea = ea;

        }

        private DelegateCommand<object> selectSubdata;
        public DelegateCommand<object> SelectTab =>
            selectSubdata ?? (selectSubdata = new DelegateCommand<object>(ExecuteSelectSubData));

        void ExecuteSelectSubData(object parameter) {
            if(parameter is UI_Data.Views.DataRaw) {
                var data = ((parameter as UI_Data.Views.DataRaw).DataContext as DataRawViewModel).CurrentData;
                if(data.HasValue)   _ea.GetEvent<Event_SubDataTabSelected>().Publish(data.Value);
            }
        }
    }
}
