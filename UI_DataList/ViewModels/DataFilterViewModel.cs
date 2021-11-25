using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using SillyMonkey.Core;
using DataContainer;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls;

namespace UI_DataList.ViewModels {
    public class DataFilterViewModel : BindableBase {
        IEventAggregator _ea;

        private ObservableCollection<string> _allItems;
        public ObservableCollection<string> AllItems {
            get { return _allItems; }
            set { SetProperty(ref _allItems, value); }
        }

        private ObservableCollection<string> _enabledItems;
        public ObservableCollection<string> EnabledItems {
            get { return _enabledItems; }
            set { SetProperty(ref _enabledItems, value); }
        }

        private ObservableCollection<byte> _allSites;
        public ObservableCollection<byte> AllSites {
            get { return _allSites; }
            set { SetProperty(ref _allSites, value); }
        }

        private ObservableCollection<byte> _enabledSites;
        public ObservableCollection<byte> EnabledSites {
            get { return _enabledSites; }
            set { SetProperty(ref _enabledSites, value); }
        }

        private ObservableCollection<ushort> _allHBins;
        public ObservableCollection<ushort> AllHBins {
            get { return _allHBins; }
            set { SetProperty(ref _allHBins, value); }
        }

        private ObservableCollection<ushort> _enabledHBins;
        public ObservableCollection<ushort> EnabledHBins {
            get { return _enabledHBins; }
            set { SetProperty(ref _enabledHBins, value); }
        }

        private ObservableCollection<ushort> _allSBins;
        public ObservableCollection<ushort> AllSBins {
            get { return _allSBins; }
            set { SetProperty(ref _allSBins, value); }
        }

        private ObservableCollection<ushort> _enabledSBins;
        public ObservableCollection<ushort> EnabledSBins {
            get { return _enabledSBins; }
            set { SetProperty(ref _enabledSBins, value); }
        }

        private string _maskEnableChips;
        public string MaskEnableChips {
            get { return _maskEnableChips; }
            set { SetProperty(ref _maskEnableChips, value); }
        }

        private string _maskEnableCords;
        public string MaskEnableCords {
            get { return _maskEnableCords; }
            set { SetProperty(ref _maskEnableCords, value); }
        }

        private bool _ifmaskDuplicateChips;
        public bool IfmaskDuplicateChips {
            get { return _ifmaskDuplicateChips; }
            set { SetProperty(ref _ifmaskDuplicateChips, value); }
        }

        private bool _maskOrEnableIds;
        public bool MaskOrEnableIds {
            get { return _maskOrEnableIds; }
            set { SetProperty(ref _maskOrEnableIds, value); }
        }

        private bool _maskOrEnableCords;
        public bool MaskOrEnableCords {
            get { return _maskOrEnableCords; }
            set { SetProperty(ref _maskOrEnableCords, value); }
        }

        private DuplicateSelectMode _duplicateSelectMode;
        public DuplicateSelectMode DuplicateSelectMode {
            get { return _duplicateSelectMode; }
            set { SetProperty(ref _duplicateSelectMode, value); }
        }

        private DuplicateJudgeMode _judgeMode;
        public DuplicateJudgeMode JudgeMode {
            get { return _judgeMode; }
            set { SetProperty(ref _judgeMode, value); }
        }

        string _filePath;
        int? _filterId;
        FilterSetup _filter;

        public DataFilterViewModel(IEventAggregator ea) {
            _ea = ea;
            UpdateFilter();
            _ea.GetEvent<Event_SubDataSelected>().Subscribe(UpdateDisplay);
            _ea.GetEvent<Event_DataSelected>().Subscribe(UpdateFilter);
            InitUiCtr();
        }

        private void UpdateDisplay(SubData selectedData) {

            var dataAcquire = StdDB.GetDataAcquire(selectedData.StdFilePath);
            _filePath = selectedData.StdFilePath;
            _filterId = selectedData.FilterId;

            _filter = dataAcquire.GetFilterSetup(selectedData.FilterId);

            var items = dataAcquire.GetTestIDs_Info();
            var allItems = (from r in items
                            let v = r.Key + "<>" + r.Value.TestText
                            orderby v
                            select v);
            var enabledItems = (from r in items.Keys.Except(_filter.MaskTestIDs)
                                let v = r + "<>" + items[r].TestText
                                orderby v
                                select v);

            AllItems = new ObservableCollection<string>(allItems);
            EnabledItems = new ObservableCollection<string>(enabledItems);
            AllSites = new ObservableCollection<byte>(dataAcquire.GetSites().OrderBy(x => x));
            EnabledSites = new ObservableCollection<byte>(AllSites.Except(_filter.MaskSites).OrderBy(x => x));
            AllHBins = new ObservableCollection<ushort>(dataAcquire.GetHardBins().OrderBy(x => x));
            EnabledHBins = new ObservableCollection<ushort>(AllHBins.Except(_filter.MaskHardBins).OrderBy(x => x));
            AllSBins = new ObservableCollection<ushort>(dataAcquire.GetSoftBins().OrderBy(x => x));
            EnabledSBins = new ObservableCollection<ushort>(AllSBins.Except(_filter.MaskSoftBins).OrderBy(x => x));

            IfmaskDuplicateChips = _filter.IfmaskDuplicateChips;
            DuplicateSelectMode = _filter.DuplicateSelectMode;
            JudgeMode = _filter.DuplicateJudgeMode;

            StringBuilder sb = new StringBuilder();
            foreach (var v in _filter.MaskChips) {
                sb.Append($"{v},");
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);
            MaskEnableChips = sb.ToString();

            sb.Clear();
            foreach (var v in _filter.MaskCords) {
                sb.Append($"{v.Item1},{v.Item2};");
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);
            MaskEnableCords = sb.ToString();


            MaskOrEnableIds = _filter.IfMaskOrEnableIds;
            MaskOrEnableCords = _filter.IfMaskOrEnableCords;

        }

        public void UpdateFilter(string path) {
            UpdateFilter();
        }

        public void UpdateFilter() {
            _filePath = null;
            _filterId = null;
            _filter = null;

            AllItems = null;
            EnabledItems = null;
            AllSites = null;
            EnabledSites = null;
            AllHBins = null;
            EnabledHBins = null;
            AllSBins = null;
            EnabledSBins = null;

            IfmaskDuplicateChips = false;
            DuplicateSelectMode = DuplicateSelectMode.First;
            JudgeMode = DuplicateJudgeMode.ID;

            MaskEnableChips = "";
            MaskEnableCords = "";

            MaskOrEnableIds = false;
            MaskOrEnableCords = false;
        }

        #region UI
        public DelegateCommand ApplyFilter { get; private set; }
        public DelegateCommand<ListBox> RemoveItem { get; private set; }
        public DelegateCommand<ListBox> AddItem { get; private set; }
        public DelegateCommand AddAllItems { get; private set; }
        public DelegateCommand<ListBox> AddItems { get; private set; }
        public DelegateCommand RemoveAllItems { get; private set; }
        public DelegateCommand<ListBox> RemoveItems { get; private set; }
        public DelegateCommand<ListBox> RemoveSite { get; private set; }
        public DelegateCommand<ListBox> AddSite { get; private set; }
        public DelegateCommand AddAllSites { get; private set; }
        public DelegateCommand<ListBox> AddSites { get; private set; }
        public DelegateCommand RemoveAllSites { get; private set; }
        public DelegateCommand<ListBox> RemoveSites { get; private set; }
        public DelegateCommand<ListBox> RemoveHBin { get; private set; }
        public DelegateCommand<ListBox> AddHBin { get; private set; }
        public DelegateCommand AddAllHBins { get; private set; }
        public DelegateCommand<ListBox> AddHBins { get; private set; }
        public DelegateCommand RemoveAllHBins { get; private set; }
        public DelegateCommand<ListBox> RemoveHBins { get; private set; }
        public DelegateCommand<ListBox> RemoveSBin { get; private set; }
        public DelegateCommand<ListBox> AddSBin { get; private set; }
        public DelegateCommand AddAllSBins { get; private set; }
        public DelegateCommand<ListBox> AddSBins { get; private set; }
        public DelegateCommand RemoveAllSBins { get; private set; }
        public DelegateCommand<ListBox> RemoveSBins { get; private set; }
        public DelegateCommand ClearIds { get; private set; }
        public DelegateCommand ClearCords { get; private set; }

        private void InitUiCtr() {
            ApplyFilter = new DelegateCommand(() => {
                if (_filter is null) {
                    return;
                }
                _filter.DuplicateSelectMode = DuplicateSelectMode;
                _filter.DuplicateJudgeMode = JudgeMode;
                _filter.IfmaskDuplicateChips = IfmaskDuplicateChips;
                _filter.IfMaskOrEnableIds = MaskOrEnableIds;
                _filter.IfMaskOrEnableCords = MaskOrEnableCords;

                _filter.MaskSites = AllSites.Except(EnabledSites).ToList();
                _filter.MaskHardBins = AllHBins.Except(EnabledHBins).ToList();
                _filter.MaskSoftBins = AllSBins.Except(EnabledSBins).ToList();
                _filter.MaskChips = ParseMaskEnableIds();
                _filter.MaskCords = ParseMaskEnableCords();
                var dataAcquire = StdDB.GetDataAcquire(_filePath);
                dataAcquire.UpdateFilter(_filterId.Value, _filter);
                _ea.GetEvent<Event_FilterUpdated>().Publish(new SubData(_filePath, _filterId.Value));
            });

            RemoveItem = new DelegateCommand<ListBox>((e) => {
                var v = ((ListBox)(e));
                if (v.Items.Count > 0 && v.SelectedIndex >= 0)
                    EnabledItems.RemoveAt(v.SelectedIndex);
            });
            AddItem = new DelegateCommand<ListBox>((e) => {
                var v = ((ListBox)(e));
                if (v.SelectedIndex >= 0 && !AllItems.Contains((string)v.SelectedItem))
                    EnabledItems.Add((string)v.SelectedItem);

                EnabledItems.OrderBy(x => x);
            });
            AddAllItems = new DelegateCommand(() => {
                EnabledItems.Clear();
                foreach (var v in AllItems)
                    EnabledItems.Add(v);
            });
            AddItems = new DelegateCommand<ListBox>((e) => {
                if (e.SelectedItems.Count >= 0) {
                    foreach (var v in e.SelectedItems)
                        if (!EnabledItems.Contains((string)v))
                            EnabledItems.Add((string)v);
                }

                EnabledItems.OrderBy(x => x);
            });
            RemoveAllItems = new DelegateCommand(() => {
                EnabledItems.Clear();
            });
            RemoveItems = new DelegateCommand<ListBox>((e) => {
                if (e.SelectedItems.Count >= 0) {
                    for(int i= EnabledItems.Count-1; i>=0; i--) {
                        if (e.SelectedItems.Contains(EnabledItems[i])) EnabledItems.RemoveAt(i);
                    }
                }
            });

            RemoveSite = new DelegateCommand<ListBox>((e) => {
                var v = ((ListBox)(e));
                if (v.Items.Count > 0 && v.SelectedIndex >= 0)
                    EnabledSites.RemoveAt(v.SelectedIndex);
            });
            AddSite = new DelegateCommand<ListBox>((e) => {
                var v = ((ListBox)(e));
                if (v.SelectedIndex >= 0 && !EnabledSites.Contains((byte)v.SelectedItem))
                    EnabledSites.Add((byte)v.SelectedItem);

                EnabledSites.OrderBy(x => x);
            });
            AddAllSites = new DelegateCommand(() => {
                EnabledSites.Clear();
                foreach (var v in AllSites)
                    EnabledSites.Add(v);
            });
            AddSites = new DelegateCommand<ListBox>((e) => {
                if (e.SelectedItems.Count >= 0) {
                    foreach (var v in e.SelectedItems)
                        if (!EnabledSites.Contains((byte)v))
                            EnabledSites.Add((byte)v);
                }

                EnabledSites.OrderBy(x => x);
            });
            RemoveAllSites = new DelegateCommand(() => {
                EnabledSites.Clear();
            });
            RemoveSites = new DelegateCommand<ListBox>((e) => {
                if (e.SelectedItems.Count >= 0) {
                    for(int i= EnabledSites.Count-1; i>=0; i--) {
                        if (e.SelectedItems.Contains(EnabledSites[i])) EnabledSites.RemoveAt(i);
                    }
                }
            });

            RemoveHBin = new DelegateCommand<ListBox>((e) => {
                var v = ((ListBox)(e));
                if (v.Items.Count > 0 && v.SelectedIndex >= 0)
                    EnabledHBins.RemoveAt(v.SelectedIndex);
            });
            AddHBin = new DelegateCommand<ListBox>((e) => {
                var v = ((ListBox)(e));
                if (v.SelectedIndex >= 0 && !EnabledHBins.Contains((ushort)v.SelectedItem))
                    EnabledHBins.Add((ushort)v.SelectedItem);

                EnabledHBins.OrderBy(x => x);
            });
            AddAllHBins = new DelegateCommand(() => {
                EnabledHBins.Clear();
                foreach (var v in AllHBins)
                    EnabledHBins.Add(v);
            });
            AddHBins = new DelegateCommand<ListBox>((e) => {
                if (e.SelectedItems.Count >= 0) {
                    foreach (var v in e.SelectedItems)
                        if (!EnabledHBins.Contains((ushort)v))
                            EnabledHBins.Add((ushort)v);
                }

                EnabledHBins.OrderBy(x => x);
            });
            RemoveAllHBins = new DelegateCommand(() => {
                EnabledHBins.Clear();
            });
            RemoveHBins = new DelegateCommand<ListBox>((e) => {
                if (e.SelectedItems.Count >= 0) {
                    for (int i = EnabledHBins.Count - 1; i >= 0; i--) {
                        if (e.SelectedItems.Contains(EnabledHBins[i])) EnabledHBins.RemoveAt(i);
                    }
                }
            });

            RemoveSBin = new DelegateCommand<ListBox>((e) => {
                var v = ((ListBox)(e));
                if (v.Items.Count > 0 && v.SelectedIndex >= 0)
                    EnabledSBins.RemoveAt(v.SelectedIndex);
            });
            AddSBin = new DelegateCommand<ListBox>((e) => {
                var v = ((ListBox)(e));
                if (v.SelectedIndex >= 0 && !EnabledSBins.Contains((ushort)v.SelectedItem))
                    EnabledSBins.Add((ushort)v.SelectedItem);

                EnabledSBins.OrderBy(x => x);
            });
            AddAllSBins = new DelegateCommand(() => {
                EnabledSBins.Clear();
                foreach (var v in AllSBins)
                    EnabledSBins.Add(v);
            });
            AddSBins = new DelegateCommand<ListBox>((e) => {
                if (e.SelectedItems.Count >= 0) {
                    foreach (var v in e.SelectedItems)
                        if (!EnabledSBins.Contains((ushort)v))
                            EnabledSBins.Add((ushort)v);
                }

                EnabledSBins.OrderBy(x => x);
            });
            RemoveAllSBins = new DelegateCommand(() => {
                EnabledSBins.Clear();
            });
            RemoveSBins = new DelegateCommand<ListBox>((e) => {
                if (e.SelectedItems.Count >= 0) {
                    for (int i = EnabledSBins.Count - 1; i >= 0; i--) {
                        if (e.SelectedItems.Contains(EnabledSBins[i])) EnabledSBins.RemoveAt(i);
                    }
                }
            });

            ClearIds = new DelegateCommand(() => {
                MaskEnableChips = "";
            });

            ClearCords = new DelegateCommand(() => {
                MaskEnableCords = "";
            });

        }

        private List<string> ParseMaskEnableIds() {
            List<string> rst = new List<string>();
            var ss = MaskEnableChips.Split(';');
            foreach (string s in ss) {
                try { rst.Add(s); } catch { continue; }
            }
            return rst;
        }
        private List<Tuple<ushort, ushort>> ParseMaskEnableCords() {
            List<Tuple<ushort, ushort>> rst = new List<Tuple<ushort, ushort>>();
            var ss = MaskEnableCords.Split(';');
            foreach (string s in ss) {
                var xy = s.Split(',');
                if (xy.Length != 2) continue;
                try {
                    ushort x = ushort.Parse(xy[0]);
                    ushort y = ushort.Parse(xy[1]);
                    rst.Add(new Tuple<ushort, ushort>(x, y));
                } catch { continue; }
            }

            return rst;
        }
        #endregion


    }
}
