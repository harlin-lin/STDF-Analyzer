using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DataContainer;
using FastWpfGrid;

namespace UI_Data.ViewModels {
    public class SiteDataCorr_FastDataGridModel : FastGridModelBase {
        private SubData _subData;

        private Color? _cellColor;

        private HashSet<int> _frozenCols = new HashSet<int>();
        private HashSet<int> _frozenRows = new HashSet<int>();

        private List<Item> _testItems = null;

        List<string> _colNames = new List<string>();

        private void UpdateColumnRow() {
            _colNames.Clear();
            var da = StdDB.GetDataAcquire(_subData.StdFilePath);

            _testItems = da.GetFilteredItemStatistic(_subData.FilterId).ToList();

            _colNames.Add("Idx");
            _colNames.Add("TestNO");
            _colNames.Add("TestText");
            _colNames.Add("LoLimit");
            _colNames.Add("HiLimit");
            _colNames.Add("Unit");

            var sites = da.GetSites();

            for (int i = 0; i < sites.Length; i++) {
                _colNames.Add("Mean S:" + sites[i]);
            }
            for (int i = 0; i < sites.Length; i++) {
                _colNames.Add("Min S:" + sites[i]);
            }
            for (int i = 0; i < sites.Length; i++) {
                _colNames.Add("Max S:" + sites[i]);
            }
            for (int i = 0; i < sites.Length; i++) {
                _colNames.Add("Cp S:" + sites[i]);
            }
            for (int i = 0; i < sites.Length; i++) {
                _colNames.Add("Cpk S:" + sites[i]);
            }
            for (int i = 0; i < sites.Length; i++) {
                _colNames.Add("Sigma S:" + sites[i]);
            }
        }

        public SiteDataCorr_FastDataGridModel(SubData subData) {
            _subData = subData;
            UpdateColumnRow();

            _frozenCols.Add(0);


        }

        public void UpdateView() {
            UpdateColumnRow();

            NotifyRefresh();
        }

        public string GetTestId(int row) {
            if (row >= _testItems.Count) return "";
            return _testItems[row].TestNumber;
        }

        public override HashSet<int> GetFrozenColumns(IFastGridView view) {
            return _frozenCols;
        }

        public override HashSet<int> GetFrozenRows(IFastGridView view) {
            return _frozenRows;
        }

        public override int ColumnHeaderHeight => 20;
        public override int RowHeaderWidth => 33;

        public override string GetColumnHeaderText(int column) {
            return _colNames[column];
        }

        public override string GetRowHeaderText(int row) {
            return (row + 1).ToString();
        }

        public override int ColumnCount {
            get { return _colNames.Count; }
        }

        public override int RowCount {
            get { return _testItems.Count(); }
        }

        public override Color? FontColor {
            get { return _cellColor; }
        }

        public override string GetCellText(int row, int column) {
            var da = StdDB.GetDataAcquire(_subData.StdFilePath);
            var sites = da.GetSites();
            int cnt = sites.Length;

            switch (column) {
                case 0:
                    return _testItems[row].Idx.ToString();
                case 1:
                    return _testItems[row].TestNumber.ToString();
                case 2:
                    return _testItems[row].TestText.ToString();
                case 3:
                    return _testItems[row].LoLimit.ToString();
                case 4:
                    return _testItems[row].HiLimit.ToString();
                case 5:
                    return _testItems[row].Unit.ToString();
                default:
                    var si = (column - 6) % cnt;
                    var d = (column - 6) / cnt;
                    var s = da.GetFilteredStatisticBySite(_subData.FilterId, _testItems[row].TestNumber, sites[si]);
                    switch (d) {
                        case 0:
                            return s.MeanValue.ToString();
                        case 1:
                            return s.MinValue.ToString();
                        case 2:
                            return s.MaxValue.ToString();
                        case 3:
                            return s.Cp.ToString();
                        case 4:
                            return s.Cpk.ToString();
                        case 5:
                            return s.Sigma.ToString();
                    }
                    break;

            }
            return "";
        }

        public override IFastGridCellBlock GetBlock(int blockIndex) {
            _cellColor = null;

            return this;
        }

        //public override void HandleCommand(IFastGridView view, FastGridCellAddress address, object commandParameter, ref bool handled) {
        //    //base.HandleCommand(view, address, commandParameter, ref handled);
        //    //if (commandParameter is string) MessageBox.Show(commandParameter.ToString());
        //}

        //public override void HandleSelectionCommand(IFastGridView view, string command) {
        //    //MessageBox.Show(command);
        //}


    }
}
