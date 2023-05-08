using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using DataContainer;
using FastWpfGrid;

namespace UI_Data.ViewModels {
    public class DataRaw_FastDataGridModel : FastGridModelBase {
        private SubData _subData;

        private Color? _cellColor;

        private HashSet<int> _frozenCols = new HashSet<int>();
        private HashSet<int> _frozenRows = new HashSet<int>();

        private List<Item> _testItems = null;


        readonly string[] ColNames = { "Idx", "TestNumber", "TestText          ", "LoLimit", "HiLimit", "Unit", "PassCnt", "FailCnt", "FailPer", "MeanValue", "MedianValue", "MinValue", "MaxValue", "Cp", "Cpk", "Sigma" };


        public DataRaw_FastDataGridModel(SubData subData) {
            _subData = subData;

            var _da = StdDB.GetDataAcquire(_subData.StdFilePath);
            _testItems = new List<Item>(_da.GetFilteredItemStatistic(_subData.FilterId));

            _frozenCols.Add(0);

            sorted = Enumerable.Range(0, _da.GetTestIDs().Count()).ToList();
        }

        private SortMode sortMode = SortMode.Default;
        private int sortCol = -1;

        private List<int> sorted = null;

        public void SortColumn(int column) {

            if (column != sortCol) sortMode = SortMode.Default;
            sortCol = column;

            Dictionary<int, object> colData = new Dictionary<int, object>(RowCount);
            for (int i = 0; i < RowCount; i++) {
                colData.Add(i, GetCell(i, column));
            }

            if (sortMode == SortMode.Default) {
                sorted = (from pair in colData orderby pair.Value ascending select pair.Key).ToList();
                sortMode = SortMode.MinToMax;
            } else if (sortMode == SortMode.MinToMax) {
                sorted = (from pair in colData orderby pair.Value descending select pair.Key).ToList();
                sortMode = SortMode.MaxToMin;

            } else {
                sorted = colData.Keys.ToList();
                sortMode = SortMode.Default;
            }

            _hiddenRows.Clear();
            if (sortMode == SortMode.Default) {
                foreach (var v in hid) {
                    _hiddenRows.Add(v);
                }
            } else {
                foreach (var v in hid) {
                    _hiddenRows.Add(sorted.FindIndex((x) => x == v));
                }
            }

            NotifyRefresh();
        }

        private HashSet<int> _hiddenRows = new HashSet<int>();
        private List<int> hid = new List<int>(400);

        public void FilterColumn(int column, string filterPat) {
            hid.Clear();
            if (!string.IsNullOrWhiteSpace(filterPat)) {
                for (int i = 0; i < RowCount; i++) {
                    if(!Regex.IsMatch(GetCell(i, column).ToString(), filterPat, RegexOptions.IgnoreCase)){
                        hid.Add(i);
                    }
                }
            }

            _hiddenRows.Clear();
            if(sortMode == SortMode.Default) {
                foreach (var v in hid) {
                    _hiddenRows.Add(v);
                }
            } else {
                foreach(var v in hid) {
                    _hiddenRows.Add(sorted.FindIndex((x) => x == v));
                }
            }

            NotifyRefresh();
        }


        public void UpdateView() {
            var _da = StdDB.GetDataAcquire(_subData.StdFilePath);
            _testItems = new List<Item>(_da.GetFilteredItemStatistic(_subData.FilterId));

            NotifyRefresh();
        }

        public string GetTestId(int row) {
            if (row >= _testItems.Count) return "";
            row = sorted[row];
            return _testItems[row].TestNumber;
        }

        public override HashSet<int> GetFrozenColumns(IFastGridView view) {
            return _frozenCols;
        }

        public override HashSet<int> GetFrozenRows(IFastGridView view) {
            return _frozenRows;
        }

        public override HashSet<int> GetHiddenRows(IFastGridView view) {
            return _hiddenRows;
        }

        public override int ColumnHeaderHeight => 20;
        public override int RowHeaderWidth => 33;

        public override string GetColumnHeaderText(int column) {
            return ColNames[column];
        }

        public override string GetRowHeaderText(int row) {
            return (row + 1).ToString();
        }

        public override int ColumnCount {
            get { return ColNames.Length; }
        }

        public override int RowCount {
            get { return _testItems.Count(); }
        }

        public override Color? FontColor {
            get { return _cellColor; }
        }

        object GetCell(int row, int column) {

            switch (column) {
                case 0:
                    return _testItems[row].Idx;
                case 1:
                    return _testItems[row].TestNumber;
                case 2:
                    return _testItems[row].TestText;
                case 3:
                    return _testItems[row].LoLimit;
                case 4:
                    return _testItems[row].HiLimit;
                case 5:
                    return _testItems[row].Unit;
                case 6:
                    return _testItems[row].PassCnt;
                case 7:
                    return _testItems[row].FailCnt;
                case 8:
                    return _testItems[row].FailPer;
                case 9:
                    return _testItems[row].MeanValue;
                case 10:
                    return _testItems[row].MedianValue;
                case 11:
                    return _testItems[row].MinValue;
                case 12:
                    return _testItems[row].MaxValue;
                case 13:
                    return _testItems[row].Cp;
                case 14:
                    return _testItems[row].Cpk;
                case 15:
                    return _testItems[row].Sigma;
            }
            return null;
        }


        public override string GetCellText(int row, int column) {
            row = sorted[row];

            switch (column) {
                case 0:
                    return _testItems[row].Idx.ToString();
                case 1:
                    return _testItems[row].TestNumber.ToString();
                case 2:
                    return _testItems[row].TestText.ToString();
                case 3:
                    return getstr(_testItems[row].LoLimit);
                case 4:
                    return getstr(_testItems[row].HiLimit);
                case 5:
                    return _testItems[row].Unit.ToString();
                case 6:
                    return _testItems[row].PassCnt.ToString();
                case 7:
                    if (_testItems[row].FailCnt>0) _cellColor = Colors.Red;
                    return _testItems[row].FailCnt.ToString();
                case 8:
                    return _testItems[row].FailPer.ToString();
                case 9:
                    return getstr(_testItems[row].MeanValue);
                case 10:
                    return getstr(_testItems[row].MedianValue);
                case 11:
                    return getstr(_testItems[row].MinValue);
                case 12:
                    return getstr(_testItems[row].MaxValue);
                case 13:
                    return getstr(_testItems[row].Cp);
                case 14:
                    return getstr(_testItems[row].Cpk);
                case 15:
                    return getstr(_testItems[row].Sigma);
            }
            return "";
        }

        string getstr(float val) {
            if (float.IsPositiveInfinity(val)) {
                return "Inf+";
            } else if (float.IsNegativeInfinity(val)) {
                return "Inf-";
            } else if (float.IsNaN(val)) {
                return "NaN";
            }
            return val.ToString();
        }

        string getstr(float? val) {
            if (val is null) return "";
            return getstr(val.Value);
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
