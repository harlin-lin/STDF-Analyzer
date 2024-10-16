using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DataContainer;
using FastWpfGrid;

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

        public override HashSet<int> GetFrozenColumns(IFastGridView view) {
            return _frozenCols;
        }

        public override HashSet<int> GetFrozenRows(IFastGridView view) {
            return _frozenRows;
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
