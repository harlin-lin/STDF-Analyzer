using DataContainer;
using Microsoft.Win32;
using Prism.Events;
using Prism.Regions;
using ScottPlot.Plottable;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace UI_Chart.Views {
    /// <summary>
    /// Interaction logic for ItemCorr
    /// </summary>
    public partial class ItemCorr : UserControl, INavigationAware {
        public ItemCorr(IRegionManager regionManager, IEventAggregator ea) {
            InitializeComponent();

            _regionManager = regionManager;
            _ea = ea;

            scatterChart.RightClicked -= scatterChart.DefaultRightClickEvent;
            scatterChart.Configuration.DoubleClickBenchmark = false;

            toggleOutlier.IsChecked = SA.ItemCorrEnableOutlierFilter;

            switch (SA.ItemCorrOutlierFilterRange) {
                case SigmaRangeType.Sigma6:
                    comboboxSigmaOutlier.SelectedIndex = 0;
                    break;
                case SigmaRangeType.Sigma5:
                    comboboxSigmaOutlier.SelectedIndex = 1;
                    break;
                case SigmaRangeType.Sigma4:
                    comboboxSigmaOutlier.SelectedIndex = 2;
                    break;
                case SigmaRangeType.Sigma3:
                    comboboxSigmaOutlier.SelectedIndex = 3;
                    break;
                case SigmaRangeType.Sigma2:
                    comboboxSigmaOutlier.SelectedIndex = 4;
                    break;
                case SigmaRangeType.Sigma1:
                    comboboxSigmaOutlier.SelectedIndex = 5;
                    break;
            }
        }

        IRegionManager _regionManager;
        IEventAggregator _ea;

        SubData _subData;
        string _selectedX;
        string _selectedY;

        public void OnNavigatedTo(NavigationContext navigationContext) {
            var data = (SubData)navigationContext.Parameters["subData"];
            if (!_subData.Equals(data)) {
                _subData = data;

                var dataAcquire = StdDB.GetDataAcquire(_subData.StdFilePath);
                var items = dataAcquire.GetFilteredItemStatistic(_subData.FilterId);

                cbItemX.ItemsSource = items;
                cbItemY.ItemsSource = items;

                _ea.GetEvent<Event_FilterUpdated>().Subscribe(UpdateChart);

                UpdateData();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) {
            return false;
            //var data = (SubData)navigationContext.Parameters["subData"];

            //return data.Equals(_subData);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) {
            _ea.GetEvent<Event_FilterUpdated>().Unsubscribe(UpdateChart);
        }

        int SigmaByIdx(int idx) {
            return 6 - idx;
        }

        bool IfValidData(float f) {
            if (float.IsNaN(f) || float.IsInfinity(f))
                return false;
            return true;
        }

        void UpdateData() {
            if (_selectedX == null || _selectedY == null) {
                _ea.GetEvent<Event_Log>().Publish("Please Select Two Items");
                return;
            }

            var da = StdDB.GetDataAcquire(_subData.StdFilePath);

            var x = da.GetFilteredItemData(_selectedX, _subData.FilterId).ToArray();
            var y = da.GetFilteredItemData(_selectedY, _subData.FilterId).ToArray();

            scatterChart.Plot.Clear();

            List<int> idx = new List<int>(x.Length);
            for (int i = 0; i < x.Length; i++) {
                if (IfValidData(x[i]) && IfValidData(y[i])) idx.Add(i);
            }

            var xs = (from r in idx
                      select (double)x[r]).ToArray();

            var ys = (from r in idx
                      select (double)y[r]).ToArray();

            var color = SA.GetColor(0);
            if (xs.Count() > 0 && ys.Count() > 0) {
                scatterChart.Plot.AddScatterPoints(xs, ys, Color.FromArgb(color.A, color.R, color.G, color.B), 4);
            } else {
                scatterChart.Refresh();
                return;
            }
            var infoX = da.GetTestInfo(_selectedX);
            var infoY = da.GetTestInfo(_selectedY);

            var itemTitle = $"{_selectedX}:{infoX.TestText}\n{_selectedY}:{infoY.TestText}\n";

            scatterChart.Plot.XLabel(_selectedX);
            scatterChart.Plot.YLabel(_selectedY);
            scatterChart.Plot.Title(itemTitle, true, null, 12);

            UpdateViewRange();


            _ea.GetEvent<Event_Log>().Publish("");
        }

        void UpdateViewRange() {
            if (_selectedX == null || _selectedY == null) return;

            var da = StdDB.GetDataAcquire(_subData.StdFilePath);

            var statistic_rawX = da.GetFilteredStatistic(_subData.FilterId, _selectedX);
            var statistic_rawY = da.GetFilteredStatistic(_subData.FilterId, _selectedY);
            ItemStatistic statisticX, statisticY;

            if (toggleOutlier.IsChecked.Value) {
                statisticX = da.GetFilteredStatisticIgnoreOutlier(_subData.FilterId, _selectedX, SigmaByIdx(comboboxSigmaOutlier.SelectedIndex));
                statisticY = da.GetFilteredStatisticIgnoreOutlier(_subData.FilterId, _selectedY, SigmaByIdx(comboboxSigmaOutlier.SelectedIndex));
            } else {
                statisticX = statistic_rawX;
                statisticY = statistic_rawY;
            }
            //var xll = IfValidData(statisticX.GetSigmaRangeLow(6)) ? statisticX.GetSigmaRangeLow(6) : statisticX.MinValue;
            //var xhl = IfValidData(statisticX.GetSigmaRangeHigh(6)) ? statisticX.GetSigmaRangeHigh(6) : statisticX.MaxValue;
            var xll = statisticX.MinValue;
            var xhl = statisticX.MaxValue;
            var xov = 0.1 * (xhl - xll);
            if (xov == 0) xov = 1;
            scatterChart.Plot.SetAxisLimitsX(xll-xov, xhl+xov);

            //var yll = IfValidData(statisticY.GetSigmaRangeLow(6)) ? statisticY.GetSigmaRangeLow(6) : statisticY.MinValue;
            //var yhl = IfValidData(statisticY.GetSigmaRangeHigh(6)) ? statisticY.GetSigmaRangeHigh(6) : statisticY.MaxValue;
            var yll = statisticY.MinValue;
            var yhl = statisticY.MaxValue;
            var yov = 0.1 * (yhl - yll);
            if (yov == 0) yov = 1;
            scatterChart.Plot.SetAxisLimitsY(yll-yov, yhl+yov);

            scatterChart.Refresh();
        }


        void UpdateChart(SubData subData) {
            if (subData.Equals(_subData)) {
                UpdateData();
            }
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

        private void scatterChart_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            UpdateViewRange();
        }

        private void buttonSave_Click(object sender, System.Windows.RoutedEventArgs e) {
            string filePath;
            var txtX = StdDB.GetDataAcquire(_subData.StdFilePath).GetTestInfo(_selectedX).TestText;
            var txtY = StdDB.GetDataAcquire(_subData.StdFilePath).GetTestInfo(_selectedY).TestText;

            string dftName = $"{_selectedX}_{txtX}_{_selectedY}_{txtY}_Corr";
            if (GetAndCheckPath("PNG | *.png", dftName, out filePath)) {
                scatterChart.Plot.SaveFig(filePath);
            }

        }

        private void buttonCopy_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (_selectedX == null || _selectedY == null) {
                System.Windows.MessageBox.Show("Select at list one item");
                return;
            }
            var image = scatterChart.Plot.GetBitmap();
            System.Windows.Clipboard.SetImage(System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            image.GetHbitmap(),
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()));

            _ea.GetEvent<Event_Log>().Publish("Copied to clipboard");

        }

        private void cbItemX_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _selectedX = ((Item)cbItemX.SelectedItem).TestNumber;
            UpdateData();

        }

        private void cbItemY_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _selectedY = ((Item)cbItemY.SelectedItem).TestNumber;
            UpdateData();

        }

        private void toggleOutlier_Click(object sender, System.Windows.RoutedEventArgs e) {
            UpdateViewRange();
        }

        private void comboboxSigmaOutlier_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            UpdateViewRange();
        }
    }
}
