using Prism.Commands;
using Prism.Mvvm;
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
using Utils;

namespace UI_DataList.Views {
    /// <summary>
    /// SetupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetupWindow : Window, INotifyPropertyChanged {
        public SetupWindow() {
            DataContext = this;
            InitializeComponent();
        }

        private IEnumerable<UidType> _uidTypeList = Enum.GetValues(typeof(UidType)).OfType<UidType>();
        public IEnumerable<UidType> UidTypeList {
            get { return _uidTypeList; }
        }

        private IEnumerable<ChartAxisType> _chartAxisTypeList = Enum.GetValues(typeof(ChartAxisType)).OfType<ChartAxisType>();
        public IEnumerable<ChartAxisType> ChartAxisTypeList {
            get { return _chartAxisTypeList; }
        }

        private IEnumerable<SigmaRangeType> _sigmaRangeTypeList = Enum.GetValues(typeof(SigmaRangeType)).OfType<SigmaRangeType>();
        public IEnumerable<SigmaRangeType> SigmaRangeTypeList {
            get { return _sigmaRangeTypeList; }
        }


        private UidType _selectedUidMode = SA.SaUserSetup.UidMode;
        public UidType SelectedUidMode {
            get { return _selectedUidMode; }
            set { SetProperty(ref _selectedUidMode, value); }
        }

        //histogram
        private ChartAxisType _selectedHistogramChartAxis = SA.SaUserSetup.SetupHistogramChartAxis;
        public ChartAxisType SelectedHistogramChartAxis {
            get { return _selectedHistogramChartAxis; }
            set { SetProperty(ref _selectedHistogramChartAxis, value); }
        }

        private SigmaRangeType _selectedHistogramChartAxisSigmaRange = SA.SaUserSetup.SetupHistogramChartAxisSigmaRange;
        public SigmaRangeType SelectedHistogramChartAxisSigmaRange {
            get { return _selectedHistogramChartAxisSigmaRange; }
            set { SetProperty(ref _selectedHistogramChartAxisSigmaRange, value); }
        }

        private SigmaRangeType _selectedHistogramOutlierFilterRange = SA.SaUserSetup.SetupHistogramOutlierFilterRange;
        public SigmaRangeType SelectedHistogramOutlierFilterRange {
            get { return _selectedHistogramOutlierFilterRange; }
            set { SetProperty(ref _selectedHistogramOutlierFilterRange, value); }
        }

        private bool _HistogramEnableOutlierFilter = SA.SaUserSetup.SetupHistogramEnableOutlierFilter;
        public bool HistogramEnableOutlierFilter {
            get { return _HistogramEnableOutlierFilter; }
            set { SetProperty(ref _HistogramEnableOutlierFilter, value); }
        }

        private bool _HistogramEnableLimitLine = SA.SaUserSetup.SetupHistogramEnableLimitLine;
        public bool HistogramEnableLimitLine {
            get { return _HistogramEnableLimitLine; }
            set { SetProperty(ref _HistogramEnableLimitLine, value); }
        }

        private bool _HistogramEnableSigma6Line = SA.SaUserSetup.SetupHistogramEnableSigma6Line;
        public bool HistogramEnableSigma6Line {
            get { return _HistogramEnableSigma6Line; }
            set { SetProperty(ref _HistogramEnableSigma6Line, value); }
        }

        private bool _HistogramEnableSigma3Line = SA.SaUserSetup.SetupHistogramEnableSigma3Line;
        public bool HistogramEnableSigma3Line {
            get { return _HistogramEnableSigma3Line; }
            set { SetProperty(ref _HistogramEnableSigma3Line, value); }
        }

        private bool _HistogramEnableMinMaxLine = SA.SaUserSetup.SetupHistogramEnableMinMaxLine;
        public bool HistogramEnableMinMaxLine {
            get { return _HistogramEnableMinMaxLine; }
            set { SetProperty(ref _HistogramEnableMinMaxLine, value); }
        }

        private bool _HistogramEnableMeanLine = SA.SaUserSetup.SetupHistogramEnableMeanLine;
        public bool HistogramEnableMeanLine {
            get { return _HistogramEnableMeanLine; }
            set { SetProperty(ref _HistogramEnableMeanLine, value); }
        }

        private bool _HistogramEnableMedianLine = SA.SaUserSetup.SetupHistogramEnableMedianLine;
        public bool HistogramEnableMedianLine {
            get { return _HistogramEnableMedianLine; }
            set { SetProperty(ref _HistogramEnableMedianLine, value); }
        }

        //trend
        private ChartAxisType _selectedTrendChartAxis = SA.SaUserSetup.SetupTrendChartAxis;
        public ChartAxisType SelectedTrendChartAxis {
            get { return _selectedTrendChartAxis; }
            set { SetProperty(ref _selectedTrendChartAxis, value); }
        }

        private SigmaRangeType _selectedTrendChartAxisSigmaRange = SA.SaUserSetup.SetupTrendChartAxisSigmaRange;
        public SigmaRangeType SelectedTrendChartAxisSigmaRange {
            get { return _selectedTrendChartAxisSigmaRange; }
            set { SetProperty(ref _selectedTrendChartAxisSigmaRange, value); }
        }

        private SigmaRangeType _selectedTrendOutlierFilterRange = SA.SaUserSetup.SetupTrendOutlierFilterRange;
        public SigmaRangeType SelectedTrendOutlierFilterRange {
            get { return _selectedTrendOutlierFilterRange; }
            set { SetProperty(ref _selectedTrendOutlierFilterRange, value); }
        }

        private bool _TrendEnableOutlierFilter = SA.SaUserSetup.SetupTrendEnableOutlierFilter;
        public bool TrendEnableOutlierFilter {
            get { return _TrendEnableOutlierFilter; }
            set { SetProperty(ref _TrendEnableOutlierFilter, value); }
        }

        private bool _TrendEnableLimitLine = SA.SaUserSetup.SetupTrendEnableLimitLine;
        public bool TrendEnableLimitLine {
            get { return _TrendEnableLimitLine; }
            set { SetProperty(ref _TrendEnableLimitLine, value); }
        }

        private bool _TrendEnableSigma6Line = SA.SaUserSetup.SetupTrendEnableSigma6Line;
        public bool TrendEnableSigma6Line {
            get { return _TrendEnableSigma6Line; }
            set { SetProperty(ref _TrendEnableSigma6Line, value); }
        }

        private bool _TrendEnableSigma3Line = SA.SaUserSetup.SetupTrendEnableSigma3Line;
        public bool TrendEnableSigma3Line {
            get { return _TrendEnableSigma3Line; }
            set { SetProperty(ref _TrendEnableSigma3Line, value); }
        }

        private bool _TrendEnableMinMaxLine = SA.SaUserSetup.SetupTrendEnableMinMaxLine;
        public bool TrendEnableMinMaxLine {
            get { return _TrendEnableMinMaxLine; }
            set { SetProperty(ref _TrendEnableMinMaxLine, value); }
        }

        private bool _TrendEnableMeanLine = SA.SaUserSetup.SetupTrendEnableMeanLine;
        public bool TrendEnableMeanLine {
            get { return _TrendEnableMeanLine; }
            set { SetProperty(ref _TrendEnableMeanLine, value); }
        }

        private bool _TrendEnableMedianLine = SA.SaUserSetup.SetupTrendEnableMedianLine;
        public bool TrendEnableMedianLine {
            get { return _TrendEnableMedianLine; }
            set { SetProperty(ref _TrendEnableMedianLine, value); }
        }


        //CorrHistogram
        private ChartAxisType _selectedCorrHistogramChartAxis = SA.SaUserSetup.SetupCorrHistogramChartAxis;
        public ChartAxisType SelectedCorrHistogramChartAxis {
            get { return _selectedCorrHistogramChartAxis; }
            set { SetProperty(ref _selectedCorrHistogramChartAxis, value); }
        }

        private SigmaRangeType _selectedCorrHistogramChartAxisSigmaRange = SA.SaUserSetup.SetupCorrHistogramChartAxisSigmaRange;
        public SigmaRangeType SelectedCorrHistogramChartAxisSigmaRange {
            get { return _selectedCorrHistogramChartAxisSigmaRange; }
            set { SetProperty(ref _selectedCorrHistogramChartAxisSigmaRange, value); }
        }

        private SigmaRangeType _selectedCorrHistogramOutlierFilterRange = SA.SaUserSetup.SetupCorrHistogramOutlierFilterRange;
        public SigmaRangeType SelectedCorrHistogramOutlierFilterRange {
            get { return _selectedCorrHistogramOutlierFilterRange; }
            set { SetProperty(ref _selectedCorrHistogramOutlierFilterRange, value); }
        }

        private bool _CorrHistogramEnableOutlierFilter = SA.SaUserSetup.SetupCorrHistogramEnableOutlierFilter;
        public bool CorrHistogramEnableOutlierFilter {
            get { return _CorrHistogramEnableOutlierFilter; }
            set { SetProperty(ref _CorrHistogramEnableOutlierFilter, value); }
        }


        //ItemCorr
        private SigmaRangeType _selectedItemCorrOutlierFilterRange = SA.SaUserSetup.SetupItemCorrOutlierFilterRange;
        public SigmaRangeType SelectedItemCorrOutlierFilterRange {
            get { return _selectedItemCorrOutlierFilterRange; }
            set { SetProperty(ref _selectedItemCorrOutlierFilterRange, value); }
        }

        private bool _ItemCorrEnableOutlierFilter = SA.SaUserSetup.SetupItemCorrEnableOutlierFilter;
        public bool ItemCorrEnableOutlierFilter {
            get { return _ItemCorrEnableOutlierFilter; }
            set { SetProperty(ref _ItemCorrEnableOutlierFilter, value); }
        }

        private DelegateCommand _apply;

        public DelegateCommand Apply =>
            _apply ?? (_apply = new DelegateCommand(ExecuteApply));

        void ExecuteApply() {
            SA.SaUserSetup.UidMode = _selectedUidMode;
            //histogram
            SA.SaUserSetup.SetupHistogramChartAxis = _selectedHistogramChartAxis;
            SA.SaUserSetup.SetupHistogramChartAxisSigmaRange = _selectedHistogramChartAxisSigmaRange;
            SA.SaUserSetup.SetupHistogramOutlierFilterRange = _selectedHistogramOutlierFilterRange;
            SA.SaUserSetup.SetupHistogramEnableOutlierFilter = _HistogramEnableOutlierFilter;
            SA.SaUserSetup.SetupHistogramEnableLimitLine = _HistogramEnableLimitLine;
            SA.SaUserSetup.SetupHistogramEnableSigma6Line = _HistogramEnableSigma6Line;
            SA.SaUserSetup.SetupHistogramEnableSigma3Line = _HistogramEnableSigma3Line;
            SA.SaUserSetup.SetupHistogramEnableMinMaxLine = _HistogramEnableMinMaxLine;
            SA.SaUserSetup.SetupHistogramEnableMeanLine = _HistogramEnableMeanLine;
            SA.SaUserSetup.SetupHistogramEnableMedianLine = _HistogramEnableMedianLine;
            //trend
            SA.SaUserSetup.SetupTrendChartAxis = _selectedTrendChartAxis;
            SA.SaUserSetup.SetupTrendChartAxisSigmaRange = _selectedTrendChartAxisSigmaRange;
            SA.SaUserSetup.SetupTrendOutlierFilterRange = _selectedTrendOutlierFilterRange;
            SA.SaUserSetup.SetupTrendEnableOutlierFilter = _TrendEnableOutlierFilter;
            SA.SaUserSetup.SetupTrendEnableLimitLine = _TrendEnableLimitLine;
            SA.SaUserSetup.SetupTrendEnableSigma6Line = _TrendEnableSigma6Line;
            SA.SaUserSetup.SetupTrendEnableSigma3Line = _TrendEnableSigma3Line;
            SA.SaUserSetup.SetupTrendEnableMinMaxLine = _TrendEnableMinMaxLine;
            SA.SaUserSetup.SetupTrendEnableMeanLine = _TrendEnableMeanLine;
            SA.SaUserSetup.SetupTrendEnableMedianLine = _TrendEnableMedianLine;
            //CorrHistogram
            SA.SaUserSetup.SetupCorrHistogramChartAxis = _selectedCorrHistogramChartAxis;
            SA.SaUserSetup.SetupCorrHistogramChartAxisSigmaRange = _selectedCorrHistogramChartAxisSigmaRange;
            SA.SaUserSetup.SetupCorrHistogramOutlierFilterRange = _selectedCorrHistogramOutlierFilterRange;
            SA.SaUserSetup.SetupCorrHistogramEnableOutlierFilter = _CorrHistogramEnableOutlierFilter;
            //ItemCorr
            SA.SaUserSetup.SetupItemCorrOutlierFilterRange = _selectedItemCorrOutlierFilterRange;
            SA.SaUserSetup.SetupItemCorrEnableOutlierFilter = _ItemCorrEnableOutlierFilter;

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
