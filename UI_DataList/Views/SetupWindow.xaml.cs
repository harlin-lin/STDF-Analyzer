using Prism.Commands;
using Prism.Mvvm;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UI_DataList.Views {
    /// <summary>
    /// SetupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetupWindow : Window, INotifyPropertyChanged {
        public SetupWindow() {
            DataContext = this;
            InitializeComponent();
        }

        private string[] _uidTypeList = Enum.GetNames(typeof(UidType));
        public string[] UidTypeList {
            get { return _uidTypeList; }
        }

        public string SelectedUidMode { get; set; }


        private string[] _chartAxisTypeList = Enum.GetNames(typeof(ChartAxisType));
        public string[] ChartAxisTypeList {
            get { return _chartAxisTypeList; }
        }

        public string SelectedHistogramChartAxis { get; set; }

        private string[] _sigmaRangeTypeList = Enum.GetNames(typeof(SigmaRangeType));
        public string[] SigmaRangeTypeList {
            get { return _sigmaRangeTypeList; }
        }



        private DelegateCommand _apply;

        public DelegateCommand Apply =>
            _apply ?? (_apply = new DelegateCommand(ExecuteApply));

        void ExecuteApply() {



            SA.ApplyAndSave();
            this.Close();
        }





        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <param name="onChanged">Action that is called after the property value has been changed.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);

            return true;
        }


        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="args">The PropertyChangedEventArgs</param>
        void OnPropertyChanged(PropertyChangedEventArgs args) {
            PropertyChanged?.Invoke(this, args);
        }


    }


}
