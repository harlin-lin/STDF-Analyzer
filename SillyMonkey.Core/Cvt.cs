using DataContainer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SillyMonkey.Core {
    public class BooleanNegationConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.GetType().Name == "Boolean") {
                if ((bool)value)
                    return false;
                else
                    return true;
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.GetType().Name == "Boolean") {
                if ((bool)value)
                    return false;
                else
                    return true;
            }

            throw new NotSupportedException();
        }
    }

    public class ImgVisibilityCtr : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.GetType().Name == "Boolean") {
                if ((bool)value)
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

    public class SubDataCvtStr : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.GetType().Name == "SubData") {
                var d = (SubData)value;
                if (d.FilterId_index == 0)
                {
                    return $"F:{d.FilterId:X8}_Root  Data:{System.IO.Path.GetFileName(d.StdFilePath)}"; 
                }else 
                {
                    return $"F:{d.FilterId:X8}_S{d.FilterId_index - 1}  Data:{System.IO.Path.GetFileName(d.StdFilePath)}";
                }
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }


}
