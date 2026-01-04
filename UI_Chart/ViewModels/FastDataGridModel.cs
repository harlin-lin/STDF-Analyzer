using DataContainer;
using FastWpfGrid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace UI_Chart.ViewModels {
    public class FastDataGridModel : FastGridModelBase {
        private IDataAcquire _da;
        private SubData _subData;

        private Color? _cellColor;

        private HashSet<int> _frozenCols = new HashSet<int>();
        private HashSet<int> _frozenRows = new HashSet<int>();

        public FastDataGridModel(SubData subData) {
            _subData = subData;
            _da = StdDB.GetDataAcquire(subData.StdFilePath);

            _frozenCols.Add(0);
            _frozenCols.Add(1);
        }

        private HashSet<int> _hiddenRows = new HashSet<int>();
        private List<int> hid = new List<int>(400);

        public void FilterColumn(int column, string filterPat)
        {
            hid.Clear();
            if (!string.IsNullOrWhiteSpace(filterPat))
            {
                //TestName Filter
                for (int i = 0; i < RowCount; i++)
                {
                    if (!Regex.IsMatch(GetCellText(i, column), filterPat, RegexOptions.IgnoreCase))
                    {
                        hid.Add(i);
                    }
                }

                //TestNumber Filter
                if (hid.Count == RowCount)
                {
                    hid.Clear();
                    for (int i = 0; i < RowCount; i++)
                    {
                        if (!Regex.IsMatch(GetCellText(i, 0), filterPat, RegexOptions.IgnoreCase))
                        {
                            hid.Add(i);
                        }
                    }
                }
            }

            _hiddenRows.Clear();

            foreach (var v in hid)
            {
                _hiddenRows.Add(v);
            }


            NotifyRefresh();
        }


        public override HashSet<int> GetFrozenColumns(IFastGridView view) {
            return _frozenCols;
        }

        public override HashSet<int> GetFrozenRows(IFastGridView view) {
            return _frozenRows;
        }

        public override HashSet<int> GetHiddenRows(IFastGridView view)
        {
            return _hiddenRows;
        }

        public override int ColumnHeaderHeight => 90;
        public override int RowHeaderWidth => 33;

        public override string GetColumnHeaderText(int column) {
            if (column == 0) {
                return $"Index     \nCord\nTime\nHBin\nSBin\nSite";
            } else if (column == 1) {
                return $"TestText          ";
            } else {
                    var idx = _da.GetFilteredPartIndex(_subData.FilterId).ElementAt(column - 2);
                return $"{idx.ToString()}\n{_da.GetWaferCord(idx)}\n{_da.GetTestTime(idx).ToString()}\n{_da.GetHardBin(idx).ToString()}\n{_da.GetSoftBin(idx).ToString()}\n{_da.GetSite(idx).ToString()}";
            }
        }

        public override string GetRowHeaderText(int row) {
            return (row + 1).ToString();
        }

        public override int ColumnCount {
            get { return _da.GetFilteredChipsCount(_subData.FilterId) + 2; }
        }

        public override int RowCount {
            get { return _da.GetTestIDs().Count(); }
        }

        public override Color? FontColor {
            get { return _cellColor; }
        }

        public override string GetCellText(int row, int column) {
            _cellColor = null;

            if (column == 0) {
                return _da.GetTestIDs().ElementAt(row);
            }else if (column == 1) {
                return _da.GetTestIDs_Info().ElementAt(row).Value.TestText;
            } else {
                var idx = _da.GetFilteredPartIndex(_subData.FilterId).ElementAt(column - 2);
                
                var uid = _da.GetTestIDs().ElementAt(row);
                var val = _da.GetItemData(uid, idx);
                var limit = _da.GetTestInfo(uid);

                if(limit.LoLimit.HasValue && val < limit.LoLimit)
                    _cellColor = Colors.Blue;
                else if(limit.HiLimit.HasValue && val > limit.HiLimit)
                    _cellColor = Colors.Red;

                return getstr(val);
            }
        }

        public override string GetCellPassFail(int row, int column)
        {
            _cellColor = null;

            if (column == 0)
            {
                return _da.GetTestIDs().ElementAt(row);
            }
            else if (column == 1)
            {
                return _da.GetTestIDs_Info().ElementAt(row).Value.TestText;
            }
            else
            {
                var idx = _da.GetFilteredPartIndex(_subData.FilterId).ElementAt(column - 2);

                var uid = _da.GetTestIDs().ElementAt(row);
                var val = _da.GetItemData(uid, idx);
                var limit = _da.GetTestInfo(uid);

                if (limit.LoLimit.HasValue && limit.HiLimit.HasValue )
                {
                    if  (float.IsNaN(val))
                    {
                        return "";
                    }
                    else if  ( val > limit.LoLimit && val < limit.HiLimit)
                    {
                        return ("1");
                    }
                    else if (val < limit.LoLimit || val > limit.HiLimit)
                    {
                        return ("0");
                    }
                    else return ("NA");
                }
                else return ("NA");

            }
        }
        string getstr(float val) {
            if (float.IsPositiveInfinity(val)) {
                return "Inf+";
            } else if (float.IsNegativeInfinity(val)) {
                return "Inf-";
            } else if (float.IsNaN(val)) {
                return "";
            }
            return ((double)val).ToString("0.#######");
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
