using DataContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SillyMonkey.Core {
    public enum TabType {
        RawDataTab,
        RawDataCorTab
    }
    public struct SubData : IEquatable<SubData> {
        public string StdFilePath { get; }
        public int FilterId { get; }

        public SubData(string filePath, int filterId) {
            StdFilePath = filePath;
            FilterId = filterId;
        }

        public bool Equals(SubData other) {
            return StdFilePath == other.StdFilePath && FilterId == other.FilterId;
        }


    }

    public interface IDataView {
        SubData CurrentData{ get; }
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
