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
    public class WaferMapGridModel : IFastGridModel, IFastGridCell {
        List<IChipInfo> _chipInfo;
        Dictionary<CordType, IChipInfo> _waferMap;
        int _colCount;
        int _rowCount;
        bool valid;
        int _filterId;
        IDataAcquire _dataAcquire;



        private List<IFastGridView> _grids = new List<IFastGridView>();
        private int? _requestedRow;
        private int? _requestedColumn;
        private HashSet<int> _frozenRows = new HashSet<int>();
        private HashSet<int> _hiddenRows = new HashSet<int>();
        private HashSet<int> _frozenColumns = new HashSet<int>();
        private HashSet<int> _hiddenColumns = new HashSet<int>();



        public WaferMapGridModel(IDataAcquire dataAcquire, int filterId) {

            _dataAcquire = dataAcquire;
            _filterId = filterId;
            _waferMap = new Dictionary<CordType, IChipInfo>();

            foreach (var v in dataAcquire.GetChipsInfo()) {
                var c = new CordType(v.WaferCord.CordX, v.WaferCord.CordY);
                if (_colCount <= c.CordX) _colCount = c.CordX + 1;
                if (_rowCount <= c.CordY) _rowCount = c.CordY + 1;
            }

            Update();
        }

        public void Update() {
            _chipInfo = _dataAcquire.GetFilteredChipsInfo(_filterId);
            valid = true;
            _waferMap.Clear();

            //check if can be plot into wafer map
            int i = 0;
            foreach (var v in _chipInfo) {
                if (v.WaferCord.CordX != 0 && v.WaferCord.CordY != 0) break;
                else i++;

                if (i >= 3) {
                    _colCount = 0;
                    _rowCount = 0;
                    valid = false;
                }
            }

            //get the cord dict
            foreach (var v in _chipInfo) {
                var c = new CordType(v.WaferCord.CordX, v.WaferCord.CordY);
                if (_waferMap.ContainsKey(c)) {
                    _waferMap[c] = v;
                } else {
                    _waferMap.Add(c, v);
                }
                if (_colCount <= c.CordX) _colCount = c.CordX + 1;
                if (_rowCount <= c.CordY) _rowCount = c.CordY + 1;
            }
            NotifyRefresh();
        }


        public int ColumnCount { get { return _colCount; } }

        public int RowCount { get { return _rowCount; } }

        public string GetCellText(int row, int column) {
            if (!valid) return "";

            CordType c = new CordType((short)(column - 1), (short)(row - 1));

            if (!_waferMap.ContainsKey(c)) return "";

            if (_waferMap[c].Result == ResultType.Null) {
                return "";
            } else {
                return _waferMap[c].SoftBin.ToString();
            }
        }

        public IFastGridCell GetCell(IFastGridView view, int row, int column) {
            _requestedRow = row;
            _requestedColumn = column;
            return this;
        }

        public string GetRowHeaderText(int row) {
            if (!valid) return "";

            return $"{row}";
        }

        public string GetColumnHeaderText(int column) {
            if (!valid) return "";

            return $"{column}";
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
            return 50;
        }

        public int GetColumnHeaderHeight() {
            return 50;
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
            get {
                if (_requestedRow.HasValue && _requestedColumn.HasValue)
                    return GetCellColor(_requestedRow.Value, _requestedColumn.Value);
                else
                    return null;
            }
        }
        Color? GetCellColor(int row, int column) {
            if (!valid) return null;

            CordType c = new CordType((short)(column - 1), (short)(row - 1));

            if (!_waferMap.ContainsKey(c)) return null;

            switch (_waferMap[c].Result) {
                case ResultType.Pass: return Colors.Green;
                case ResultType.Fail: return Colors.Red;
                case ResultType.Abort: return Colors.Yellow;
                default: return null;
            }

        }

        public virtual Color? FontColor {
            get {
                return null;
            }
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
