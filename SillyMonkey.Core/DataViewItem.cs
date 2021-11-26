using DataContainer;
using System.Windows;

namespace SillyMonkey.Core {
    public enum TabType {
        RawDataTab,
        RawDataCorTab
    }

    public interface IDataView {
        SubData? CurrentData{ get; }
        TabType CurrentTabType { get; }
    }

    

    public class BindingProxy : Freezable {
        protected override Freezable CreateInstanceCore() {
            return new BindingProxy();
        }

        public object Data {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}
