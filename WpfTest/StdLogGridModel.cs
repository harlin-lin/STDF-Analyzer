using DataInterface;
using FastWpfGrid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfTest {
    public class StdLogGridModel : IFastGridModel, IFastGridCell {

        private StdLogTable _dataSource;

        public StdLogTable DataSource {
            get { return _dataSource; }
            set {
                _dataSource = value;
                NotifyRefresh();
            }
        }

        private List<IFastGridView> _grids = new List<IFastGridView>();
        private int? _requestedRow;
        private int? _requestedColumn;
        private HashSet<int> _frozenRows = new HashSet<int>();
        private HashSet<int> _hiddenRows = new HashSet<int>();
        private HashSet<int> _frozenColumns = new HashSet<int>();
        private HashSet<int> _hiddenColumns = new HashSet<int>();



        public StdLogGridModel(StdLogTable logTable) {
            _dataSource = logTable;
            NotifyRefresh();
        }

        public void ChangePage(int startIndex, int count) {
            _dataSource.Update(startIndex, count);
            NotifyRefresh();
        }


        public int ColumnCount { get { return _dataSource.ColumnCount; } }

        public int RowCount { get { return _dataSource.RowCount; }}

        string GetCellText(int row, int column) {
            return _dataSource.GetCellText(row,column);
        }

        public IFastGridCell GetCell(IFastGridView view, int row, int column) {
            _requestedRow = row;
            _requestedColumn = column;
            return this;
        }

        public string GetRowHeaderText(int row) {
            return GetCellText(row,0);
        }

        public string GetColumnHeaderText(int column) {
            return _dataSource.GetColHeader(column);
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
                    return _dataSource.GetCellFontColor(_requestedRow.Value, _requestedColumn.Value);
                else
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
