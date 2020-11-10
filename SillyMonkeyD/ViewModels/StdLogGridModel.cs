using DataInterface;
using FastWpfGrid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SillyMonkeyD.ViewModels {
    public class StdLogGridModel : IFastGridModel, IFastGridCell {

        Dictionary<TestID, IItemStatistic> _ststistic;
        Dictionary<TestID, IItemInfo> _itemInfo;
        List<IChipInfo> _chipInfo;
        float?[][] _rst;
        int _colCount;
        int _rowCount;
        int _filterId;
        IDataAcquire _dataAcquire;

        const int colFixedLength = 11;



        private List<IFastGridView> _grids = new List<IFastGridView>();
        private int? _requestedRow;
        private int? _requestedColumn;
        private HashSet<int> _frozenRows = new HashSet<int>();
        private HashSet<int> _hiddenRows = new HashSet<int>();
        private HashSet<int> _frozenColumns = new HashSet<int>();
        private HashSet<int> _hiddenColumns = new HashSet<int>();



        public StdLogGridModel(IDataAcquire dataAcquire, int filterId) {
            _dataAcquire = dataAcquire;
            _filterId = filterId;

            Update();

        }

        public void Update() {
            _ststistic = _dataAcquire.GetFilteredStatistic(_filterId);
            _itemInfo = _dataAcquire.GetFilteredTestIDs_Info(_filterId);

            _chipInfo = _dataAcquire.GetFilteredChipsInfo(_filterId);

            _rst = new float?[_itemInfo.Count][];
            for (int i = 0; i < _itemInfo.Count; i++) {
                _rst[i] = _dataAcquire.GetFilteredItemData(_itemInfo.ElementAt(i).Key, _filterId);
            }

            _colCount = _dataAcquire.GetFilteredChipSummary(_filterId).TotalCount;
            _rowCount = _itemInfo.Count;

            NotifyRefresh();
        }

        public int ColumnCount { get { return _colCount; } }

        public int RowCount { get { return _rowCount; } }

        string GetCellText(int row, int column) {
            if (column < colFixedLength) {
                switch (column) {
                    case 0:
                        return _itemInfo.ElementAt(row).Key.ToString();
                    case 1:
                        return _itemInfo.ElementAt(row).Value.TestText;
                    case 2:
                        return _itemInfo.ElementAt(row).Value.LoLimit.ToString();
                    case 3:
                        return _itemInfo.ElementAt(row).Value.HiLimit.ToString();
                    case 4:
                        return _itemInfo.ElementAt(row).Value.Unit;
                    case 5: {
                            var v = _ststistic[_itemInfo.ElementAt(row).Key].MinValue;
                            return v.HasValue ? v.Value.ToString("F4") : "";
                        }
                    case 6: {
                            var v = _ststistic[_itemInfo.ElementAt(row).Key].MaxValue;
                            return v.HasValue ? v.Value.ToString("F4") : "";
                        }
                    case 7: {
                            var v = _ststistic[_itemInfo.ElementAt(row).Key].MeanValue;
                            return v.HasValue ? v.Value.ToString("F4") : "";
                        }
                    case 8: {
                            var v = _ststistic[_itemInfo.ElementAt(row).Key].Sigma;
                            return v.HasValue ? v.Value.ToString("F4") : "";
                        }
                    case 9: {
                            var v = _ststistic[_itemInfo.ElementAt(row).Key].Cpk;
                            return v.HasValue ? v.Value.ToString("F4") : "";
                        }
                    case 10:
                        return _ststistic[_itemInfo.ElementAt(row).Key].PassCount.ToString();
                    default:
                        throw new Exception("Out of Range");
                }
            } else {
                return _rst[row][column - colFixedLength].ToString();
            }
        }

        public IFastGridCell GetCell(IFastGridView view, int row, int column) {
            _requestedRow = row;
            _requestedColumn = column;
            return this;
        }

        public string GetRowHeaderText(int row) {
            return (row+1).ToString();
        }

        public string GetColumnHeaderText(int column) {
            if (column < colFixedLength) {
                switch (column) {
                    case 0:
                        return "Test Num";
                    case 1:
                        return "Test Text";
                    case 2:
                        return "Low Limit";
                    case 3:
                        return "Hi Limit";
                    case 4:
                        return "Unit";
                    case 5:
                        return "Min";
                    case 6:
                        return "Max";
                    case 7:
                        return "Mean";
                    case 8:
                        return "Sigma";
                    case 9:
                        return "CPK";
                    case 10:
                        return "Pass Cnt";
                    default:
                        throw new Exception("Out of Range");
                }
            } else {
                var v = _chipInfo[column - colFixedLength];
                return $"ID:{v.PartId}\n{v.WaferCord}\nHB:{v.HardBin}\nSB:{v.SoftBin}\nPF:{v.Result}\nS:{v.Site}\nT:{v.TestTime}ms";
            }
        }

        public IFastGridCell GetRowHeader(IFastGridView view, int row) {
            _requestedRow = row;
            _requestedColumn = null;
            return this;
        }

        public IFastGridCell GetColumnHeader(IFastGridView view, int column) {
            _requestedColumn = column;
            _requestedRow = null;
            return this;
        }

        public IFastGridCell GetGridHeader(IFastGridView view) {
            return null;
        }


        public void AttachView(IFastGridView view) {
            _grids.Add(view);
        }

        public void DetachView(IFastGridView view) {
            _grids.Remove(view);
        }

        public void HandleCommand(IFastGridView view, FastGridCellAddress address, object commandParameter, ref bool handled) {
        }

        public HashSet<int> GetHiddenColumns(IFastGridView view) {
            return _hiddenColumns;
        }

        public HashSet<int> GetFrozenColumns(IFastGridView view) {
            return _frozenColumns;
        }

        public HashSet<int> GetHiddenRows(IFastGridView view) {
            return _hiddenRows;
        }

        public HashSet<int> GetFrozenRows(IFastGridView view) {
            return _frozenRows;
        }

        public int GetRowHeaderWidth() {
            return 100;
        }

        public int GetColumnHeaderHeight() {
            return 140;
        }

        public void SetColumnArrange(HashSet<int> hidden, HashSet<int> frozen) {
            _hiddenColumns = hidden;
            _frozenColumns = frozen;
            NotifyColumnArrangeChanged();
        }

        public void SetRowArrange(HashSet<int> hidden, HashSet<int> frozen) {
            _hiddenRows = hidden;
            _frozenRows = frozen;
            NotifyRowArrangeChanged();
        }

        public void InvalidateAll() {
            _grids.ForEach(x => x.InvalidateAll());
        }

        public void InvalidateCell(int row, int column) {
            _grids.ForEach(x => x.InvalidateModelCell(row, column));
        }

        public void InvalidateRowHeader(int row) {
            _grids.ForEach(x => x.InvalidateModelRowHeader(row));
        }

        public void InvalidateColumnHeader(int column) {
            _grids.ForEach(x => x.InvalidateModelColumnHeader(column));
        }

        public void NotifyAddedRows() {
            _grids.ForEach(x => x.NotifyAddedRows());
        }

        public void NotifyRefresh() {
            _grids.ForEach(x => x.NotifyRefresh());
        }

        public void NotifyColumnArrangeChanged() {
            _grids.ForEach(x => x.NotifyColumnArrangeChanged());
        }

        public void NotifyRowArrangeChanged() {
            _grids.ForEach(x => x.NotifyRowArrangeChanged());
        }

        public void HandleSelectionCommand(IFastGridView view, string command) {
        }

        public int? SelectedRowCountLimit {
            get { return null; }
        }

        public int? SelectedColumnCountLimit {
            get { return null; }
        }





        public Color? BackgroundColor {
            get { return null; }
        }
        public virtual Color? FontColor {
            get {
                if (_requestedRow.HasValue && _requestedColumn.HasValue)
                    return GetCellFontColor(_requestedRow.Value, _requestedColumn.Value);
                else
                    return null;
            }
        }
        public Color? GetCellFontColor(int row, int column) {
            //if (column < colFixedLength) {
            //    return null;
            //} else {
            //    var v = _rst[row][column - colFixedLength];
            //    if (v.HasValue && !v.IsPass) {
            //        if (v.IsLessLL) return Colors.Blue;
            //        if (v.IsGreaterHL) return Colors.Red;
            //        return null;
            //    } else
            return null;
            //}
        }


        public bool IsItalic {
            get { return false; }
        }

        public bool IsBold {
            get { return false; }
        }

        public string ToolTipText {
            get { return null; }
        }
        public TooltipVisibilityMode ToolTipVisibility {
            get { return TooltipVisibilityMode.OnlyWhenTrimmed; }
        }

        public string TextData {
            get {
                if (_requestedColumn == null && _requestedRow == null) return null;
                if (_requestedColumn != null && _requestedRow != null) return GetCellText(_requestedRow.Value, _requestedColumn.Value);
                if (_requestedColumn != null) return GetColumnHeaderText(_requestedColumn.Value);
                if (_requestedRow != null) return GetRowHeaderText(_requestedRow.Value);
                return null;
            }
        }


        public MouseHoverBehaviours MouseHoverBehaviour {
            get { return MouseHoverBehaviours.ShowAllWhenMouseOut; }
        }

        public object CommandParameter {
            get { return null; }
        }

        public CellDecoration Decoration {
            get { return CellDecoration.None; }
        }

        public Color? DecorationColor {
            get { return null; }
        }
    }
}
