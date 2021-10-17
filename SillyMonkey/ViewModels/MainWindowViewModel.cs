using Prism.Mvvm;

namespace SillyMonkey.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "SillyMonkey";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
