using DataInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

namespace TestWPF {
    /// <summary>
    /// Filter.xaml 的交互逻辑
    /// </summary>
    public partial class Filter : Window {
        public Filter(IDataAcquire dataAcquire, int filterId) {

            InitializeComponent();
            this.DataContext = this;
            UpdateFilter(dataAcquire, filterId);
        }

        private int _filterId { get; set; }
        private FilterSetup _filter;
        private IDataAcquire _dataAcquire;
        private DuplicateSelectMode _duplicateSelectMode;
        private DuplicateJudgeMode _judgeMode;

        public ObservableCollection<string> AllItems { get; private set; }
        public ObservableCollection<string> EnabledItems { get; private set; }
        public ObservableCollection<byte> AllSites { get; private set; }
        public ObservableCollection<byte> EnabledSites { get; private set; }
        public ObservableCollection<ushort> AllHBins { get; private set; }
        public ObservableCollection<ushort> EnabledHBins { get; private set; }
        public ObservableCollection<ushort> AllSBins { get; private set; }
        public ObservableCollection<ushort> EnabledSBins { get; private set; }
        public string MaskEnableChips { get; set; }
        public string MaskEnableCords { get; set; }
        public bool ifmaskDuplicateChips { get; set; }
        public bool MaskOrEnableIds { get; set; }
        public bool MaskOrEnableCords { get; set; }
        public List<string> JudgeMode { get; set; }
        public List<string> DuplicateMode { get; set; }



        public void UpdateFilter(IDataAcquire dataAcquire, int filterId) {
            _dataAcquire = dataAcquire;
            _filterId = filterId;
            _filter = _dataAcquire.GetFilterSetup(_filterId);
            _duplicateSelectMode = DuplicateSelectMode.SelectFirst;
            _judgeMode = DuplicateJudgeMode.ID;

            var allItems = (from r in _dataAcquire.GetTestIDs_Info()
                         let v = r.Key.GetGeneralTestNumber() + "<>" + r.Value.TestText
                         orderby v
                         select v);
            var enabledItems = (from r in _dataAcquire.GetFilteredTestIDs_Info(_filterId)
                            let v = r.Key.GetGeneralTestNumber() + "<>" + r.Value.TestText
                            orderby v
                            select v);

            AllItems = new ObservableCollection<string>(allItems);
            EnabledItems = new ObservableCollection<string>(enabledItems);
            AllSites = new ObservableCollection<byte>(_dataAcquire.GetSites().OrderBy(x =>x));
            AllHBins = new ObservableCollection<ushort>(_dataAcquire.GetHardBins().OrderBy(x => x));
            AllSBins = new ObservableCollection<ushort>(_dataAcquire.GetSoftBins().OrderBy(x => x));
            EnabledSites = new ObservableCollection<byte>(AllSites.Except(_filter.maskSites).OrderBy(x => x));
            EnabledHBins = new ObservableCollection<ushort>(AllHBins.Except(_filter.maskHardBins).OrderBy(x => x));
            EnabledSBins = new ObservableCollection<ushort>(AllSBins.Except(_filter.maskSoftBins).OrderBy(x => x));

            ifmaskDuplicateChips = _filter.ifmaskDuplicateChips;
            DuplicateMode = new List<string>();
            DuplicateMode.Add(DuplicateSelectMode.SelectFirst.ToString());
            DuplicateMode.Add(DuplicateSelectMode.SelectLast.ToString());
            DuplicateMode.Add(DuplicateSelectMode.AllDuplicate.ToString());
            JudgeMode = new List<string>();
            JudgeMode.Add(DuplicateJudgeMode.ID.ToString());
            JudgeMode.Add(DuplicateJudgeMode.Cord.ToString());
            MaskEnableCords = "";
            MaskEnableChips = "";

            MaskOrEnableIds = false;
            MaskOrEnableCords = false;

            //OnPropertyChanged("MaskOrEnableIds");
            //OnPropertyChanged("MaskOrEnableCords");
        }
        #region UI
        private void LbAllItems_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var v = ((ListBox)(e.Source));
            if (v.SelectedIndex >= 0 && !AllItems.Contains((string)v.SelectedItem))
                EnabledItems.Add((string)v.SelectedItem);

            EnabledItems.OrderBy(x => x);
        }

        private void LbEnabledItems_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var v = ((ListBox)(e.Source));
            if (v.Items.Count > 0 && v.SelectedIndex >= 0)
                EnabledItems.RemoveAt(v.SelectedIndex);
        }

        private void AddAllItems(object sender, RoutedEventArgs e) {
            EnabledItems.Clear();
            foreach(var v in AllItems)
                EnabledItems.Add(v);
        }

        private void AddItems(object sender, RoutedEventArgs e) {
            if (lbAllItems.SelectedItems.Count >= 0) {
                foreach (var v in lbAllItems.SelectedItems)
                    if (!EnabledItems.Contains((string)v))
                        EnabledItems.Add((string)v);
            }

            EnabledItems.OrderBy(x => x);
        }

        private void RemoveAllItems(object sender, RoutedEventArgs e) {
            EnabledItems.Clear();
        }

        private void RemoveItems(object sender, RoutedEventArgs e) {
            if (lbAllItems.SelectedItems.Count >= 0)
                foreach (var v in lbAllItems.SelectedItems)
                    EnabledItems.Remove((string)v);
        }

        private void LbAllSites_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var v = ((ListBox)(e.Source));
            if (v.SelectedIndex >= 0 && !EnabledSites.Contains((byte)v.SelectedItem))
                EnabledSites.Add((byte)v.SelectedItem);

            EnabledSites.OrderBy(x => x);
        }

        private void LbEnabledSites_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var v = ((ListBox)(e.Source));
            if (v.Items.Count > 0 && v.SelectedIndex >= 0)
                EnabledSites.RemoveAt(v.SelectedIndex);
        }

        private void AddAllSites(object sender, RoutedEventArgs e) {
            EnabledSites.Clear();
            foreach (var v in AllSites)
                EnabledSites.Add(v);
        }

        private void AddSites(object sender, RoutedEventArgs e) {
            if (lbAllSites.SelectedItems.Count >= 0) {
                foreach (var v in lbAllSites.SelectedItems)
                    if (!EnabledSites.Contains((byte)v))
                        EnabledSites.Add((byte)v);
            }

            EnabledSites.OrderBy(x => x);
        }

        private void RemoveAllSites(object sender, RoutedEventArgs e) {
            EnabledSites.Clear();
        }

        private void RemoveSites(object sender, RoutedEventArgs e) {
            if (lbAllSites.SelectedItems.Count >= 0)
                foreach (var v in lbAllSites.SelectedItems)
                    EnabledSites.Remove((byte)v);
        }

        private void LbAllHBins_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var v = ((ListBox)(e.Source));
            if (v.SelectedIndex >= 0 && !EnabledHBins.Contains((ushort)v.SelectedItem))
                EnabledHBins.Add((ushort)v.SelectedItem);

            EnabledHBins.OrderBy(x => x);
        }

        private void LbEnabledHBins_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var v = ((ListBox)(e.Source));
            if (v.Items.Count > 0 && v.SelectedIndex >= 0)
                EnabledHBins.RemoveAt(v.SelectedIndex);
        }

        private void AddAllHBins(object sender, RoutedEventArgs e) {
            EnabledHBins.Clear();
            foreach (var v in AllHBins)
                EnabledHBins.Add(v);
        }

        private void AddHBins(object sender, RoutedEventArgs e) {
            if (lbAllHBins.SelectedItems.Count >= 0) {
                foreach (var v in lbAllHBins.SelectedItems)
                    if (!EnabledHBins.Contains((ushort)v))
                        EnabledHBins.Add((ushort)v);
            }

            EnabledHBins.OrderBy(x => x);
        }

        private void RemoveAllHBins(object sender, RoutedEventArgs e) {
            EnabledHBins.Clear();
        }

        private void RemoveHBins(object sender, RoutedEventArgs e) {
            if (lbAllHBins.SelectedItems.Count >= 0)
                foreach (var v in lbAllHBins.SelectedItems)
                    EnabledHBins.Remove((ushort)v);
        }

        private void LbAllSBins_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var v = ((ListBox)(e.Source));
            if (v.SelectedIndex >= 0 && !EnabledSBins.Contains((ushort)v.SelectedItem))
                EnabledSBins.Add((ushort)v.SelectedItem);

            EnabledSBins.OrderBy(x => x);
        }

        private void LbEnabledSBins_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var v = ((ListBox)(e.Source));
            if (v.Items.Count > 0 && v.SelectedIndex >= 0)
                EnabledSBins.RemoveAt(v.SelectedIndex);
        }

        private void AddAllSBins(object sender, RoutedEventArgs e) {
            EnabledSBins.Clear();
            foreach (var v in AllSBins)
                EnabledSBins.Add(v);
        }

        private void AddSBins(object sender, RoutedEventArgs e) {
            if (lbAllSBins.SelectedItems.Count >= 0) {
                foreach (var v in lbAllSBins.SelectedItems)
                    if (!EnabledSBins.Contains((ushort)v))
                        EnabledSBins.Add((ushort)v);
            }

            EnabledSBins.OrderBy(x => x);
        }

        private void RemoveAllSBins(object sender, RoutedEventArgs e) {
            EnabledSBins.Clear();
        }

        private void RemoveSBins(object sender, RoutedEventArgs e) {
            if (lbAllSBins.SelectedItems.Count >= 0)
                foreach (var v in lbAllSBins.SelectedItems)
                    EnabledSBins.Remove((ushort)v);
        }
        #endregion

        private void ApplyFilter(object sender, RoutedEventArgs e) {
            _filter.DuplicateSelectMode = _duplicateSelectMode;
            _filter.DuplicateJudgeMode = _judgeMode;
            _filter.ifmaskDuplicateChips = ifmaskDuplicateChips;
            _filter.ifMaskOrEnableIds = MaskOrEnableIds;
            _filter.ifMaskOrEnableCords = MaskOrEnableCords;

            _filter.maskSites = AllSites.Except(EnabledSites).ToList();
            _filter.maskHardBins = AllHBins.Except(EnabledHBins).ToList();
            _filter.maskSoftBins = AllSBins.Except(EnabledSBins).ToList();
            _filter.maskChips = ParseMaskEnableIds();
            _filter.maskCords = ParseMaskEnableCords();
            _dataAcquire.UpdateFilter(_filterId, _filter);

        }

        private List<string> ParseMaskEnableIds() {
            List<string> rst = new List<string>();
            var ss = MaskEnableChips.Split(';');
            foreach (string s in ss) {
                try { rst.Add(s); } catch { continue; }
            }
            return rst;
        }
        private List<CordType> ParseMaskEnableCords() {
            List<CordType> rst = new List<CordType>();
            var ss = MaskEnableCords.Split(';');
            foreach (string s in ss) {
                var xy = s.Split(',');
                if (xy.Length != 2) continue;
                try {
                    short x = short.Parse(xy[0]);
                    short y = short.Parse(xy[1]);
                    rst.Add(new CordType(x, y));
                } catch { continue; }
            }

            return rst;
        }

        private void ClearCords(object sender, RoutedEventArgs e) {
            MaskEnableCords = "";
        }

        private void ClearIDs(object sender, RoutedEventArgs e) {
            MaskEnableChips = "";
        }

    }

    //public class Convter : IValueConverter {
    //    //当值从绑定源传播给绑定目标时，调用方法Convert
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    //        if (value == null)
    //            return DependencyProperty.UnsetValue;
    //        DateTime date = (DateTime)value;
    //        return date.ToString("yyyy-MM-dd");
    //    }
    //    //当值从绑定目标传播给绑定源时，调用此方法ConvertBack
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    //        string str = value as string;
    //        DateTime txtDate;
    //        if (DateTime.TryParse(str, out txtDate)) {
    //            return txtDate;
    //        }
    //        return DependencyProperty.UnsetValue;
    //    }
    //}

}
