using DataContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SillyMonkey.Core {
    public enum TabType {
        RawDataTab,
        RawDataCorTab
    }
    public class DataViewItem {
        string TabTitle { get; }
        List<Tuple<IDataAcquire,int>> DataList { get; }
        TabType TabType { get; }
    }
}
