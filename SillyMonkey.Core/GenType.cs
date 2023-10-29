using DataContainer;
using System.Collections.Generic;
using System.Windows;

namespace SillyMonkey.Core {
    public enum TabType {
        RawDataTab,
        RawDataCorTab,
        SiteDataCorTab
    }

    public enum CorrItemType {
        All,
        Mean,
        Min,
        Max,
        Cp,
        Cpk,
        Sigma
    }

    public interface IDataView {
        SubData? CurrentData{ get; }
        List<SubData> SubDataList { get; }
        TabType CurrentTabType { get; }
    }

    public enum ChartAxisType {
        Sigma,
        MinMax,
        Limit
    }

    public enum UidType {
        TestNumber,
        TestName,
        TestNumberAndTestName
    }

    public enum SigmaRangeType {
        Sigma6,
        Sigma5,
        Sigma4,
        Sigma3,
        Sigma2,
        Sigma1
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
