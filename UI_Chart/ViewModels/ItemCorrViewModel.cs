using DataContainer;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace UI_Chart.ViewModels {
    public class ItemCorrViewModel : BindableBase, INavigationAware {
        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;
        string _selectedX;
        string _selectedY;


        public ItemCorrViewModel(IRegionManager regionManager, IEventAggregator ea) {
            _regionManager = regionManager;
            _ea = ea;
            _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateChart);

            InitUi();
        }

        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
                _subData = data;

                var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);
                _items = dataAcquire.GetFilteredItemStatistic(_subData.FilterId);
                RaisePropertyChanged("ItemX");
                RaisePropertyChanged("ItemY");

                UpdateData();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];

            return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {

        }


        void UpdateData() {
            if (_selectedX == null || _selectedY == null) {
                _ea.GetEvent<Event_Log>().Publish("Please Select Two Items");
                return;
            }

            var da = StdDB.GetDataAcquire(_subData.StdFilePath);

            var xs = da.GetFilteredItemData(_selectedX, _subData.FilterId);
            var ys = da.GetFilteredItemData(_selectedY, _subData.FilterId);

            var infoX = da.GetTestInfo(_selectedX);
            var infoY = da.GetTestInfo(_selectedY);

            ItemTitle = $"{_selectedX}:{infoX.TestText}\n{_selectedY}:{infoY.TestText}\n";

            CorrSeries = new XyDataSeries<float, float>();
            CorrSeries.AcceptsUnsortedData = true;
            CorrSeries.Append(xs, ys);
            RaisePropertyChanged("CorrSeries");

            _ea.GetEvent<Event_Log>().Publish("");
        }


        void UpdateChart(SubData subData) {
            if (subData.Equals(_subData)) {
                UpdateData();
            }
        }

        private IEnumerable<Item> _items;
        public IEnumerable<Item> ItemX {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public IEnumerable<Item> ItemY {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }
        public DataSeries<float, float> _corrSeries;
        public DataSeries<float, float> CorrSeries {
            get { return _corrSeries; }
            set { SetProperty(ref _corrSeries, value); }
        }

        private IAxisViewModel _xAxis;
        public IAxisViewModel XAxis {
            get { return _xAxis; }
            set { SetProperty(ref _xAxis, value); }
        }
        private IAxisViewModel _yAxis;
        public IAxisViewModel YAxis {
            get { return _yAxis; }
            set { SetProperty(ref _yAxis, value); }
        }

        private string _itemTitle;
        public string ItemTitle {
            get { return _itemTitle; }
            set { SetProperty(ref _itemTitle, value); }
        }

        void InitUi() {
            XAxis = new NumericAxisViewModel {
                //AxisTitle = "XAxis",
                DrawMinorGridLines = false,
                DrawMajorBands = false,
                DrawMajorGridLines = true,
                TextFormatting = "#",
                FontSize = 10,
                TickTextBrush = Brushes.Black,
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(400),
                VisibleRange = new DoubleRange(1, 1),
            };
            YAxis = new NumericAxisViewModel {
                AxisAlignment = AxisAlignment.Right,
                //AxisTitle = "YAxis",
                DrawMinorGridLines = false,
                DrawMajorBands = false,
                DrawMajorGridLines = true,
                TextFormatting = "f3",
                FontSize = 10,
                TickTextBrush = Brushes.Black,
                FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(400),
                VisibleRange = new DoubleRange(0, 1),
            };
        }

        private DelegateCommand<Item> cmdSelectItemX;
        public DelegateCommand<Item> CmdSelectItemX =>
            cmdSelectItemX ?? (cmdSelectItemX = new DelegateCommand<Item>(ExecuteCmdSelectItemX));

        void ExecuteCmdSelectItemX(Item parameter) {
            _selectedX = parameter.TestNumber;
            UpdateData();
        }

        private DelegateCommand<Item> cmdSelectItemY;
        public DelegateCommand<Item> CmdSelectItemY =>
            cmdSelectItemY ?? (cmdSelectItemY = new DelegateCommand<Item>(ExecuteCmdSelectItemY));

        void ExecuteCmdSelectItemY(Item parameter) {
            _selectedY = parameter.TestNumber;
            UpdateData();
        }

        ///<summary>
        /// Check if file is Good for writing
        ///</summary>
        ///<param name="filePath">File path</param>
        ///<returns></returns>
        public static bool IsFileGoodForWriting(string filePath) {
            FileStream stream = null;
            FileInfo file = new FileInfo(filePath);

            try {
                stream = file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            }
            catch (Exception) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return false;
            } finally {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return true;
        }

        public SaveFileDialog CreateFileDialog(string filter) {
            var saveFileDialog = new SaveFileDialog {
                Filter = filter,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            return saveFileDialog;
        }

        private bool GetAndCheckPath(string filter, string dftName, out string path) {
            var ret = false;
            var isGoodPath = false;
            var saveFileDialog = CreateFileDialog(filter);
            saveFileDialog.FileName = dftName;
            path = null;

            while (!isGoodPath) {
                if (saveFileDialog.ShowDialog() == true) {
                    if (IsFileGoodForWriting(saveFileDialog.FileName)) {
                        path = saveFileDialog.FileName;
                        isGoodPath = true;
                        ret = true;
                    } else {
                        System.Windows.MessageBox.Show(
                            "File is inaccesible for writing or you can not create file in this location, please choose another one.");
                    }
                } else {
                    isGoodPath = true;
                }
            }

            return ret;
        }

        private DelegateCommand<object> _CmdSave;
        public DelegateCommand<object> CmdSave =>
            _CmdSave ?? (_CmdSave = new DelegateCommand<object>(ExecuteCmdSave));

        void ExecuteCmdSave(object e) {
            string filePath;
            var txtX = StdDB.GetDataAcquire(_subData.StdFilePath).GetTestInfo(_selectedX).TestText;
            var txtY = StdDB.GetDataAcquire(_subData.StdFilePath).GetTestInfo(_selectedY).TestText;

            string dftName = $"{_selectedX}_{txtX}_{_selectedY}_{txtY}_Corr";
            if (GetAndCheckPath("PNG | *.png", dftName, out filePath)) {
                (e as SciChartSurface).ExportToFile(filePath, SciChart.Core.ExportType.Png, false);
            }

        }

        private DelegateCommand<object> _CmdCopy;
        public DelegateCommand<object> CmdCopy =>
            _CmdCopy ?? (_CmdCopy = new DelegateCommand<object>(ExecuteCmdCopy));

        void ExecuteCmdCopy(object e) {
            if (_selectedX == null || _selectedY == null) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            var image = (e as SciChartSurface).ExportToBitmapSource();
            System.Windows.Clipboard.SetImage(image);
            _ea.GetEvent<Event_Log>().Publish("Copied to clipboard");
        }

    }
}
