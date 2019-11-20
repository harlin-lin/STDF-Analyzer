using C1.WPF;
using DataInterface;
using FileHelper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SillyMonkey.ViewModel {
    public class DataGridTabModel : ViewModelBase {
        const int MinPerPageCount = 30;
        const int DefaultPerPageCount = 100;

        private StdFileHelper _fileHelper;
        private int _fileHash;
        private int _filterId { get; set; }
        private FilterSetup _filter;
        private IDataAcquire _dataAcquire;
        private int _countPerPage;
        private DuplicateSelectMode _duplicateSelectMode;
        private DuplicateJudgeMode _judgeMode;

        public string TabTitle {get; private set;}
        public string FilePath { get; private set; }

        public DataTable Data { get; private set; }
        public int CountPerPage {
            get { return _countPerPage; }
            set {
                if (value > MinPerPageCount)
                    _countPerPage = value;
                else
                    _countPerPage = MinPerPageCount;
            }
        }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }
        public int CurrentPageIndex { get; private set; }


        public ObservableCollection<byte> AllSites { get; private set; }
        public ObservableCollection<ushort> AllHBins { get; private set; }
        public ObservableCollection<ushort> AllSBins { get; private set; }
        public ObservableCollection<byte> EnabledSites { get; private set; }
        public ObservableCollection<ushort> EnabledHBins { get; private set; }
        public ObservableCollection<ushort> EnabledSBins { get; private set; }
        public string MaskEnableChips { get; set; }
        public string MaskEnableCords { get; set; }
        public bool ifmaskDuplicateChips { get; set; }
        public bool MaskOrEnableIds { get; set; }
        public bool MaskOrEnableCords { get; set; }
        public List<string> JudgeMode { get; set; }
        public List<string> DuplicateMode { get; set; }



        public RelayCommand JumpStartPage { get; private set; }
        public RelayCommand JumpLastPage { get; private set; }
        public RelayCommand JumpNextPage { get; private set; }
        public RelayCommand JumpEndPage { get; private set; }
        public RelayCommand ClearIDs { get; private set; }
        public RelayCommand ClearCords { get; private set; }
        public RelayCommand ExportToExcel { get; private set; }

        public RelayCommand<SelectionChangedEventArgs> DuplicateSelectModeChanged { get; private set; }
        public RelayCommand<SelectionChangedEventArgs> JudgeModeChanged { get; private set; }
        public RelayCommand ApplyFilter { get; private set; }
        public RelayCommand<MouseButtonEventArgs> RemoveSite { get; private set; }
        public RelayCommand<MouseButtonEventArgs> AddSite { get; private set; }
        public RelayCommand<MouseButtonEventArgs> RemoveHBin { get; private set; }
        public RelayCommand<MouseButtonEventArgs> AddHBin { get; private set; }
        public RelayCommand<MouseButtonEventArgs> RemoveSBin { get; private set; }
        public RelayCommand<MouseButtonEventArgs> AddSBin { get; private set; }
        public RelayCommand<RoutedEventArgs> CreateHistogram { get; private set; }

        public DataGridTabModel(StdFileHelper stdFileHelper, int fileHash, int filterId) {
            _fileHelper = stdFileHelper;
            _fileHash = fileHash;
            _filterId = filterId;

            _dataAcquire =stdFileHelper.GetFile(fileHash);
            _filter = _dataAcquire.GetFilterSetup(_filterId);
            _duplicateSelectMode = DuplicateSelectMode.SelectFirst;
            _judgeMode = DuplicateJudgeMode.ID;

            if (_dataAcquire.FileName.Length > 15) 
                TabTitle = _dataAcquire.FileName.Substring(0, 15) + "...";
            else
                TabTitle = _dataAcquire.FileName;
            FilePath = _dataAcquire.FilePath;

            CountPerPage = DefaultPerPageCount;
            TotalCount = _dataAcquire.GetFilteredChipSummary(_filterId).TotalCount;
            TotalPages = TotalCount / CountPerPage + 1;
            CurrentPageIndex = 1;
            UpdateDataToStartPage();


            AllSites = new ObservableCollection<byte>(_dataAcquire.GetSites());
            AllHBins = new ObservableCollection<ushort>(_dataAcquire.GetHardBins());
            AllSBins = new ObservableCollection<ushort>(_dataAcquire.GetSoftBins());
            EnabledSites = new ObservableCollection<byte>(AllSites.Except(_filter.maskSites));
            EnabledHBins = new ObservableCollection<ushort>(AllHBins.Except(_filter.maskHardBins));
            EnabledSBins = new ObservableCollection<ushort>(AllSBins.Except(_filter.maskSoftBins));
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

            JumpStartPage = new RelayCommand(() => { UpdateDataToStartPage(); });
            JumpLastPage = new RelayCommand(() => { UpdateDataToLastPage(); });
            JumpNextPage = new RelayCommand(() => { UpdateDataToNextPage(); });
            JumpEndPage = new RelayCommand(() => { UpdateDataToEndPage(); });
            ClearIDs = new RelayCommand(() => { MaskEnableChips = ""; RaisePropertyChanged("MaskEnableChips"); });
            ClearCords = new RelayCommand(() => { MaskEnableCords = ""; RaisePropertyChanged("MaskEnableCords"); });
            ExportToExcel = new RelayCommand(()=> { ExportToExcelAction(); });

            RemoveSite = new RelayCommand<MouseButtonEventArgs>((e) => {
                var v = ((ListBox)(e.Source));
                if(v.Items.Count>1 && v.SelectedIndex >= 0)
                    EnabledSites.RemoveAt(v.SelectedIndex);
            });
            AddSite = new RelayCommand<MouseButtonEventArgs>((e) => {
                var v = ((ListBox)(e.Source));
                if (v.SelectedIndex >= 0 && !EnabledSites.Contains((byte)v.SelectedItem))
                    EnabledSites.Add((byte)v.SelectedItem);
            });
            RemoveHBin= new RelayCommand<MouseButtonEventArgs>((e) => {
                var v = ((ListBox)(e.Source));
                if (v.Items.Count > 1 && v.SelectedIndex >= 0)
                    EnabledHBins.RemoveAt(v.SelectedIndex);
            });
            AddHBin = new RelayCommand<MouseButtonEventArgs>((e) => {
                var v = ((ListBox)(e.Source));
                if (v.SelectedIndex >= 0 && !EnabledHBins.Contains((ushort)v.SelectedItem))
                    EnabledHBins.Add((ushort)v.SelectedItem);
            });
            RemoveSBin = new RelayCommand<MouseButtonEventArgs>((e) => {
                var v = ((ListBox)(e.Source));
                if (v.Items.Count > 1 && v.SelectedIndex >= 0)
                    EnabledSBins.RemoveAt(v.SelectedIndex);
            });
            AddSBin = new RelayCommand<MouseButtonEventArgs>((e) => {
                var v = ((ListBox)(e.Source));
                if (v.SelectedIndex >= 0 && !EnabledSBins.Contains((ushort)v.SelectedItem))
                    EnabledSBins.Add((ushort)v.SelectedItem);
            });

            DuplicateSelectModeChanged = new RelayCommand<SelectionChangedEventArgs>((e)=> {
                var v=((ComboBox)e.Source).SelectedItem;
                _duplicateSelectMode = (DuplicateSelectMode)Enum.Parse(typeof(DuplicateSelectMode), v.ToString());
            });
            JudgeModeChanged = new RelayCommand<SelectionChangedEventArgs>((e) => {
                var v = ((ComboBox)e.Source).SelectedItem;
                _judgeMode = (DuplicateJudgeMode)Enum.Parse(typeof(DuplicateJudgeMode), v.ToString());
            });

            ApplyFilter = new RelayCommand(() => { UpdateFilter(); });

            CreateHistogram = new RelayCommand<RoutedEventArgs>((e)=> {
                ;
            });
        }

        private void UpdateDataToStartPage() {
            CurrentPageIndex = 1;
            if (TotalPages > 1)
                Data = _dataAcquire.GetFilteredItemData(0, CountPerPage, _filterId, true);
            else
                Data = _dataAcquire.GetFilteredItemData(0, TotalCount, _filterId, true);
            
            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }
        private void UpdateDataToLastPage() {
            if (CurrentPageIndex > 1) {
                CurrentPageIndex--;
                Data = _dataAcquire.GetFilteredItemData((CurrentPageIndex-1) * CountPerPage, CountPerPage, _filterId, true);
            }
            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }
        private void UpdateDataToNextPage() {
            if (CurrentPageIndex < TotalPages) {
                CurrentPageIndex++;
                int leftCnt = TotalCount - (CurrentPageIndex-1) * CountPerPage;
                if(leftCnt > CountPerPage)
                    Data = _dataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, CountPerPage, _filterId, true);
                else
                    Data = _dataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, leftCnt, _filterId, true);
            }
            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }
        private void UpdateDataToEndPage() {
            CurrentPageIndex = TotalPages;
            int leftCnt = TotalCount - (CurrentPageIndex - 1) * CountPerPage;
            Data = _dataAcquire.GetFilteredItemData((CurrentPageIndex - 1) * CountPerPage, leftCnt, _filterId, true);

            RaisePropertyChanged("Data");
            RaisePropertyChanged("CurrentPageIndex");
        }
        private void UpdateFilter() {
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

            CountPerPage = DefaultPerPageCount;
            TotalCount = _dataAcquire.GetFilteredChipSummary(_filterId).TotalCount;
            TotalPages = TotalCount / CountPerPage + 1;
            CurrentPageIndex = 1;
            RaisePropertyChanged("TotalPages");
            RaisePropertyChanged("TotalCount");

            UpdateDataToStartPage();
        }

        private List<string> ParseMaskEnableIds() {
            List<string> rst = new List<string>();
            var ss = MaskEnableChips.Split(';');
            foreach(string s in ss) {
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

        private void ExportToExcelAction() {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            sfd.Filter = "ExcelFile|*.xlsx";
            
            if (sfd.ShowDialog() == true) {



                MessageBox.Show("Done");
            } 
        }

        public override void Cleanup() {
            base.Cleanup();

            JumpStartPage=null;
            JumpLastPage=null;
            JumpNextPage=null;
            JumpEndPage=null;

            DuplicateSelectModeChanged=null;
            JudgeModeChanged=null;
            ApplyFilter=null;
            RemoveSite=null;
            AddSite=null;
            RemoveHBin=null;
            AddHBin=null;
            RemoveSBin=null;
            AddSBin=null;

            _fileHelper=null;
            _filter = null;
            _dataAcquire = null;

            Data = null;

            AllSites=null;
            AllHBins=null;
            AllSBins=null;
            EnabledSites=null;
            EnabledHBins=null;
            EnabledSBins=null;
            JudgeMode = null;
            DuplicateMode = null;

            GC.Collect();
        }
    }
}
