using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataInterface;
using DevExpress.Mvvm;


namespace SillyMonkeyD.ViewModels {
    public class FilterEditorViewModel : ViewModelBase {
        public FilterEditorViewModel(IDataAcquire dataAcquire, int filterId) {
            UpdateFilter(dataAcquire, filterId);

            InitUiCtr();
        }


        private int _filterId { get; set; }
        private FilterSetup _filter;
        private IDataAcquire _dataAcquire;

        public ObservableCollection<string> AllItems { get; private set; }
        public ObservableCollection<string> EnabledItems { get; private set; }
        public ObservableCollection<byte> AllSites { get; private set; }
        public ObservableCollection<byte> EnabledSites { get; private set; }
        public ObservableCollection<ushort> AllHBins { get; private set; }
        public ObservableCollection<ushort> EnabledHBins { get; private set; }
        public ObservableCollection<ushort> AllSBins { get; private set; }
        public ObservableCollection<ushort> EnabledSBins { get; private set; }
        public string MaskEnableChips { get { return GetProperty(() => MaskEnableChips); } set { SetProperty(() => MaskEnableChips, value); } }
        public string MaskEnableCords { get { return GetProperty(() => MaskEnableCords); } set { SetProperty(() => MaskEnableCords, value); } }
        public bool ifmaskDuplicateChips { get { return GetProperty(() => ifmaskDuplicateChips); } set { SetProperty(() => ifmaskDuplicateChips, value); } }
        public bool MaskOrEnableIds { get { return GetProperty(() => MaskOrEnableIds); } set { SetProperty(() => MaskOrEnableIds, value); } }
        public bool MaskOrEnableCords { get { return GetProperty(() => MaskOrEnableCords); } set { SetProperty(() => MaskOrEnableCords, value); } }
        public DuplicateSelectMode DuplicateSelectMode { get { return GetProperty(() => DuplicateSelectMode); } set { SetProperty(() => DuplicateSelectMode, value); } }
        public DuplicateJudgeMode JudgeMode { get { return GetProperty(() => JudgeMode); } set { SetProperty(() => JudgeMode, value); } }



        public void UpdateFilter(IDataAcquire dataAcquire, int filterId) {
            _dataAcquire = dataAcquire;
            _filterId = filterId;
            _filter = _dataAcquire.GetFilterSetup(_filterId);

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
            AllSites = new ObservableCollection<byte>(_dataAcquire.GetSites().OrderBy(x => x));
            AllHBins = new ObservableCollection<ushort>(_dataAcquire.GetHardBins().OrderBy(x => x));
            AllSBins = new ObservableCollection<ushort>(_dataAcquire.GetSoftBins().OrderBy(x => x));
            EnabledSites = new ObservableCollection<byte>(AllSites.Except(_filter.maskSites).OrderBy(x => x));
            EnabledHBins = new ObservableCollection<ushort>(AllHBins.Except(_filter.maskHardBins).OrderBy(x => x));
            EnabledSBins = new ObservableCollection<ushort>(AllSBins.Except(_filter.maskSoftBins).OrderBy(x => x));

            ifmaskDuplicateChips = _filter.ifmaskDuplicateChips;
            DuplicateSelectMode = DuplicateSelectMode.First;
            JudgeMode = DuplicateJudgeMode.ID;

            MaskEnableCords = "";
            MaskEnableChips = "";

            MaskOrEnableIds = false;
            MaskOrEnableCords = false;
        }
        #region UI
        public ICommand ApplyFilter { get; private set;}
        public ICommand<ListBox> RemoveItem { get; private set;}
        public ICommand<ListBox> AddItem { get; private set;}
        public ICommand AddAllItems { get; private set;}
        public ICommand<ListBox> AddItems { get; private set;}
        public ICommand RemoveAllItems { get; private set;}
        public ICommand<ListBox> RemoveItems { get; private set;}
        public ICommand<ListBox> RemoveSite { get; private set;} 
        public ICommand<ListBox> AddSite { get; private set;} 
        public ICommand AddAllSites { get; private set;}
        public ICommand<ListBox> AddSites { get; private set;}
        public ICommand RemoveAllSites { get; private set;}
        public ICommand<ListBox> RemoveSites { get; private set;}
        public ICommand<ListBox> RemoveHBin { get; private set;} 
        public ICommand<ListBox> AddHBin { get; private set;} 
        public ICommand AddAllHBins { get; private set;}
        public ICommand<ListBox> AddHBins { get; private set;}
        public ICommand RemoveAllHBins { get; private set;}
        public ICommand<ListBox> RemoveHBins { get; private set;}
        public ICommand<ListBox> RemoveSBin { get; private set;} 
        public ICommand<ListBox> AddSBin { get; private set;}
        public ICommand AddAllSBins { get; private set;}
        public ICommand<ListBox> AddSBins { get; private set;}
        public ICommand RemoveAllSBins { get; private set;}
        public ICommand<ListBox> RemoveSBins { get; private set;}
        public ICommand ClearIds { get; private set; }
        public ICommand ClearCords { get; private set; }

        private void InitUiCtr() {
            ApplyFilter = new DelegateCommand(() => {
                _filter.DuplicateSelectMode = DuplicateSelectMode;
                _filter.DuplicateJudgeMode = JudgeMode;
                _filter.ifmaskDuplicateChips = ifmaskDuplicateChips;
                _filter.ifMaskOrEnableIds = MaskOrEnableIds;
                _filter.ifMaskOrEnableCords = MaskOrEnableCords;

                _filter.maskSites = AllSites.Except(EnabledSites).ToList();
                _filter.maskHardBins = AllHBins.Except(EnabledHBins).ToList();
                _filter.maskSoftBins = AllSBins.Except(EnabledSBins).ToList();
                _filter.maskChips = ParseMaskEnableIds();
                _filter.maskCords = ParseMaskEnableCords();
                _dataAcquire.UpdateFilter(_filterId, _filter);
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
                if (e.SelectedItems.Count >= 0)
                    foreach (var v in e.SelectedItems)
                        EnabledItems.Remove((string)v);
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
                if (e.SelectedItems.Count >= 0)
                    foreach (var v in e.SelectedItems)
                        EnabledSites.Remove((byte)v);
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
                if (e.SelectedItems.Count >= 0)
                    foreach (var v in e.SelectedItems)
                        EnabledHBins.Remove((ushort)v);
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
                if (e.SelectedItems.Count >= 0)
                    foreach (var v in e.SelectedItems)
                        EnabledSBins.Remove((ushort)v);
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
        #endregion


    }
}