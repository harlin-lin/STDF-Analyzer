using Prism.Commands;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using SillyMonkey.Core.Properties;

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

        private string[] _chartAxisTypeList = Enum.GetNames(typeof(ChartAxisType));
        public string[] ChartAxisTypeList {
            get { return _chartAxisTypeList; }
        }

        private string[] _sigmaRangeTypeList = Enum.GetNames(typeof(SigmaRangeType));
        public string[] SigmaRangeTypeList {
            get { return _sigmaRangeTypeList; }
        }


        public string SelectedUidMode {
            get { return SA.UidMode.ToString(); }
            set { SA.UidMode = (UidType)Enum.Parse(typeof(UidType), value); }
        }

        public string SelectedHistogramChartAxis {
            get { return SA.HistogramChartAxis.ToString(); }
            set { SA.HistogramChartAxis = (ChartAxisType)Enum.Parse(typeof(ChartAxisType), value); }
        }

        public string SelectedHistogramChartAxisSigmaRange {
            get { return SA.HistogramChartAxisSigmaRange.ToString(); }
            set { SA.HistogramChartAxisSigmaRange = (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), value); } 
        }

        public string SelectedHistogramOutlierFilterRange {
            get { return SA.HistogramOutlierFilterRange.ToString(); }
            set { SA.HistogramOutlierFilterRange = (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), value); }
        }

        public bool HistogramEnableOutlierFilter {
            get { return SA.HistogramEnableOutlierFilter; }
            set { SA.HistogramEnableOutlierFilter = value; }
        }

        public bool HistogramEnableLimitLine {
            get { return SA.HistogramEnableLimitLine; }
            set { SA.HistogramEnableLimitLine = value; }
        }

        public bool HistogramEnableSigma6Line {
            get { return SA.HistogramEnableSigma6Line; }
            set { SA.HistogramEnableSigma6Line = value; }
        }

        public bool HistogramEnableSigma3Line {
            get { return SA.HistogramEnableSigma3Line; }
            set { SA.HistogramEnableSigma3Line = value; }
        }

        public bool HistogramEnableMinMaxLine {
            get { return SA.HistogramEnableMinMaxLine; }
            set { SA.HistogramEnableMinMaxLine = value; }
        }

        public bool HistogramEnableMeanLine {
            get { return SA.HistogramEnableMeanLine; }
            set { SA.HistogramEnableMeanLine = value; }
        }

        public bool HistogramEnableMedianLine {
            get { return SA.HistogramEnableMedianLine; }
            set { SA.HistogramEnableMedianLine = value; }
        }


        public string SelectedTrendChartAxis {
            get { return SA.TrendChartAxis.ToString(); }
            set { SA.TrendChartAxis = (ChartAxisType)Enum.Parse(typeof(ChartAxisType), value); }
        }

        public string SelectedTrendChartAxisSigmaRange {
            get { return SA.TrendChartAxisSigmaRange.ToString(); }
            set { SA.TrendChartAxisSigmaRange = (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), value); }
        }

        public string SelectedTrendOutlierFilterRange {
            get { return SA.TrendOutlierFilterRange.ToString(); }
            set { SA.TrendOutlierFilterRange = (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), value); }
        }

        public bool TrendEnableOutlierFilter {
            get { return SA.TrendEnableOutlierFilter; }
            set { SA.TrendEnableOutlierFilter = value; }
        }

        public bool TrendEnableLimitLine {
            get { return SA.TrendEnableLimitLine; }
            set { SA.TrendEnableLimitLine = value; }
        }

        public bool TrendEnableSigma6Line {
            get { return SA.TrendEnableSigma6Line; }
            set { SA.TrendEnableSigma6Line = value; }
        }

        public bool TrendEnableSigma3Line {
            get { return SA.TrendEnableSigma3Line; }
            set { SA.TrendEnableSigma3Line = value; }
        }

        public bool TrendEnableMinMaxLine {
            get { return SA.TrendEnableMinMaxLine; }
            set { SA.TrendEnableMinMaxLine = value; }
        }

        public bool TrendEnableMeanLine {
            get { return SA.TrendEnableMeanLine; }
            set { SA.TrendEnableMeanLine = value; }
        }

        public bool TrendEnableMedianLine {
            get { return SA.TrendEnableMedianLine; }
            set { SA.TrendEnableMedianLine = value; }
        }


        public string SelectedCorrHistogramChartAxis {
            get { return SA.CorrHistogramChartAxis.ToString(); }
            set { SA.CorrHistogramChartAxis = (ChartAxisType)Enum.Parse(typeof(ChartAxisType), value); }
        }

        public string SelectedCorrHistogramOutlierFilterRange {
            get { return SA.CorrHistogramOutlierFilterRange.ToString(); }
            set { SA.CorrHistogramOutlierFilterRange = (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), value); }
        }

        public bool CorrHistogramEnableOutlierFilter {
            get { return SA.CorrHistogramEnableOutlierFilter; }
            set { SA.CorrHistogramEnableOutlierFilter = value; }
        }

        public bool CorrHistogramEnableLimitLine {
            get { return SA.CorrHistogramEnableLimitLine; }
            set { SA.CorrHistogramEnableLimitLine = value; }
        }

        public bool CorrHistogramEnableSigmaLine {
            get { return SA.CorrHistogramEnableSigmaLine; }
            set { SA.CorrHistogramEnableSigmaLine = value; }
        }

        public bool CorrHistogramEnableMinMaxLine {
            get { return SA.CorrHistogramEnableMinMaxLine; }
            set { SA.CorrHistogramEnableMinMaxLine = value; }
        }



        public string SelectedItemCorrOutlierFilterRange {
            get { return SA.ItemCorrOutlierFilterRange.ToString(); }
            set { SA.ItemCorrOutlierFilterRange = (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), value); }
        }

        public bool ItemCorrEnableOutlierFilter {
            get { return SA.ItemCorrEnableOutlierFilter; }
            set { SA.ItemCorrEnableOutlierFilter = value; }
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

        private void Window_Closing(object sender, CancelEventArgs e) {
            SA.ApplyAndSave();
        }
    }


}
