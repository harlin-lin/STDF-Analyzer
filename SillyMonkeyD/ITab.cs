using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SillyMonkeyD {
    public enum TabType {
        RawDataTab,
        SummaryTab,
        ChartTab,
        WaferMapTab,
        RawDataCorTab,
    }
    public interface ITab {
        string TabTitle { get; }
        int FilterId { get; }
        int WindowFlag { get; }
        TabType TabType { get; }
        bool IsMainTab { get; }
        Thickness LocationInTablist { get; }
        IDataAcquire DataAcquire { get; }
        TabItem CorrespondingTab { get; }
        void UpdateFilter();
    }
}
