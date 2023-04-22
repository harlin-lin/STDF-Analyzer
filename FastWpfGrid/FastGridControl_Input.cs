using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FastWpfGrid
{
    partial class FastGridControl
    {
        public static readonly object ToggleAllowFlexibleRowsCommand = new object();
        public static readonly object SelectAllCommand = new object();
        public static readonly object AdjustColumnSizesCommand = new object();

        public class ActiveRegion
        {
            public IntRect Rect;
            public object CommandParameter;
            public string Tooltip;
        }

        public event Action<object, ColumnClickEventArgs> ColumnHeaderClick;
        public event Action<object, ColumnClickEventArgs> ColumnHeaderDoubleClick;
        public event Action<object, RowClickEventArgs> RowHeaderClick;
        public List<ActiveRegion> CurrentCellActiveRegions = new List<ActiveRegion>();
        public ActiveRegion CurrentHoverRegion;
        private Point? _mouseCursorPoint;
        private ToolTip _tooltip;
        private object _tooltipTarget;
        private string _tooltipText;
        private DispatcherTimer _tooltipTimer;
        private DispatcherTimer _dragTimer;
        private FastGridCellAddress _dragStartCell;
        private FastGridCellAddress _mouseOverCell;
        private bool _mouseOverCellIsTrimmed;
        private int? _mouseOverRow;
        private int? _mouseOverRowHeader;
        private int? _mouseOverColumnHeader;
        private FastGridCellAddress _shiftDragStartCell;
        public event EventHandler ScrolledModelRows;
        public event EventHandler ScrolledModelColumns;

        // mouse is scrolled and captured out of control are - force scroll
        private bool _mouseIsBehindBottom;
        private bool _mouseIsBehindTop;
        private bool _mouseIsBehindLeft;
        private bool _mouseIsBehindRight;

        private int? _mouseMoveColumn;
        private int? _mouseMoveRow;

        private int? _resizingColumn;
        private Point? _resizingColumnOrigin;
        private int? _resizingColumnStartSize;
        private int? _lastResizingColumn;
        private DateTime _lastResizingColumnSet;
        private DateTime? _lastDblClickResize;

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var pt = e.GetPosition(image);
            pt.X *= DpiDetector.DpiXKoef;
            pt.Y *= DpiDetector.DpiYKoef;
            var cell = GetCellAddress(pt);

            var currentRegion = CurrentCellActiveRegions.FirstOrDefault(x => x.Rect.Contains(pt));
            if (currentRegion != null)
            {
                HandleCommand(cell, currentRegion.CommandParameter);
                return;
            }

            using (var ctx = CreateInvalidationContext())
            {
                int? resizingColumn = GetResizingColumn(pt);
                if (resizingColumn != null)
                {
                    Cursor = Cursors.SizeWE;
                    _resizingColumn = resizingColumn;
                    _resizingColumnOrigin = pt;
                    _resizingColumnStartSize = _columnSizes.GetSizeByRealIndex(_resizingColumn.Value);
                    CaptureMouse();
                }

                bool isHeaderClickHandled = false;
                if (_resizingColumn == null && cell.IsColumnHeader)
                {
                    isHeaderClickHandled = OnModelColumnClick(_columnSizes.RealToModel(cell.Column.Value));
                }
                if (cell.IsRowHeader)
                {
                    isHeaderClickHandled = OnModelRowClick(_rowSizes.RealToModel(cell.Row.Value));
                }

                if (!isHeaderClickHandled && (_lastDblClickResize == null || DateTime.Now - _lastDblClickResize.Value > TimeSpan.FromSeconds(1))){
                    if (_resizingColumn == null && cell.IsColumnHeader) {
                        _selectedCells.ToList().ForEach(InvalidateCell);
                        _selectedCells.Clear();
                        _selectedRowRange.ToList().ForEach(x => InvalidateRow(x));
                        _selectedRowRange.Clear();

                        if (ControlPressed) {
                            if (_selectedColumnRange.Contains(cell.Column.Value)) 
                                _selectedColumnRange.Remove(cell.Column.Value);
                            else
                                _selectedColumnRange.Add(cell.Column.Value);
                                InvalidateColumn(cell.Column.Value);
                        } else if (ShiftPressed) {
                            if (_currentCell.Column.HasValue) {
                                _selectedColumnRange.ToList().ForEach(x=> InvalidateColumn(x));
                                ClearSelectedCells();

                                var start = Math.Min(_currentCell.Column.Value, cell.Column.Value);
                                var stop = Math.Max(_currentCell.Column.Value, cell.Column.Value);
                                for (int c = start; c <= stop; c++) {
                                    _selectedColumnRange.Add(c);
                                    InvalidateColumn(c);
                                }
                            }
                        } else {
                            if (_currentCell.IsCell) {
                                SetCurrentCell(cell);
                            }
                            _selectedColumnRange.ToList().ForEach(x => InvalidateColumn(x));
                            ClearSelectedCells();

                            _selectedColumnRange.Add(cell.Column.Value);
                            InvalidateColumn(cell.Column.Value);

                            _dragStartCell = cell;
                            _dragTimer.IsEnabled = true;
                            CaptureMouse();
                        }
                        OnChangeSelectedCells(true);

                    }
                    if (cell.IsRowHeader) {
                        _selectedCells.ToList().ForEach(InvalidateCell);
                        _selectedCells.Clear();
                        _selectedColumnRange.ToList().ForEach(x => InvalidateColumn(x));
                        _selectedColumnRange.Clear();

                        if (ControlPressed) {
                            if (_selectedRowRange.Contains(cell.Row.Value))
                                _selectedRowRange.Remove(cell.Row.Value);
                            else
                                _selectedRowRange.Add(cell.Row.Value);
                            InvalidateRow(cell.Row.Value);
                        } else if (ShiftPressed) {
                            if (_currentCell.Row.HasValue) {
                                _selectedRowRange.ToList().ForEach(x => InvalidateRow(x));
                                ClearSelectedCells();

                                var start = Math.Min(_currentCell.Row.Value, cell.Row.Value);
                                var stop = Math.Max(_currentCell.Row.Value, cell.Row.Value);
                                for (int c = start; c <= stop; c++) {
                                    _selectedRowRange.Add(c);
                                    InvalidateRow(c);
                                }
                            }
                        } else {
                            if (_currentCell.IsCell) {
                                SetCurrentCell(cell);
                            }
                            _selectedRowRange.ToList().ForEach(x => InvalidateRow(x));
                            ClearSelectedCells();

                            _selectedRowRange.Add(cell.Row.Value);
                            InvalidateRow(cell.Row.Value);

                            _dragStartCell = cell;
                            _dragTimer.IsEnabled = true;
                            CaptureMouse();
                        }
                        OnChangeSelectedCells(true);

                    }
                }

                if (cell.IsCell)
                {
                    if(_selectionMode == SelectionModeType.CellMode) {
                        _selectedRowRange.ToList().ForEach(x => InvalidateRow(x));
                        _selectedRowRange.Clear();
                        _selectedColumnRange.ToList().ForEach(x => InvalidateColumn(x));
                        _selectedColumnRange.Clear();
                        if (ControlPressed) {
                            if (_selectedCells.Contains(cell)) RemoveSelectedCell(cell);
                            else AddSelectedCell(cell);
                            InvalidateCell(cell);
                        } else if (ShiftPressed) {
                            _selectedCells.ToList().ForEach(InvalidateCell);
                            ClearSelectedCells();

                            foreach (var cellItem in GetCellRange(_currentCell, cell)) {
                                AddSelectedCell(cellItem);
                                InvalidateCell(cellItem);
                            }
                        } else {
                            _selectedCells.ToList().ForEach(InvalidateCell);
                            ClearSelectedCells();
                            if (_currentCell != cell) {
                                SetCurrentCell(cell);
                                _shiftDragStartCell = FastGridCellAddress.Empty;
                            }
                            AddSelectedCell(cell);
                            _dragStartCell = cell;
                            _dragTimer.IsEnabled = true;
                            CaptureMouse();
                        }

                    } else if(_selectionMode == SelectionModeType.RowMode) {
                        _selectedCells.ToList().ForEach(InvalidateCell);
                        _selectedCells.Clear();
                        _selectedColumnRange.ToList().ForEach(x => InvalidateColumn(x));
                        _selectedColumnRange.Clear();

                        if (ControlPressed) {
                            if (_selectedRowRange.Contains(cell.Row.Value))
                                _selectedRowRange.Remove(cell.Row.Value);
                            else
                                _selectedRowRange.Add(cell.Row.Value);
                            InvalidateRow(cell.Row.Value);
                        } else if (ShiftPressed) {
                            if (_currentCell.Row.HasValue) {
                                _selectedRowRange.ToList().ForEach(x => InvalidateRow(x));
                                ClearSelectedCells();

                                var start = Math.Min(_currentCell.Row.Value, cell.Row.Value);
                                var stop = Math.Max(_currentCell.Row.Value, cell.Row.Value);
                                for (int c = start; c <= stop; c++) {
                                    _selectedRowRange.Add(c);
                                    InvalidateRow(c);
                                }
                            }
                        } else {
                            if (_currentCell.IsCell) {
                                SetCurrentCell(cell);
                            }
                            _selectedRowRange.ToList().ForEach(x => InvalidateRow(x));
                            ClearSelectedCells();

                            _selectedRowRange.Add(cell.Row.Value);
                            InvalidateRow(cell.Row.Value);

                            _dragStartCell = cell;
                            _dragTimer.IsEnabled = true;
                            CaptureMouse();
                        }

                    } else {
                        _selectedCells.ToList().ForEach(InvalidateCell);
                        _selectedCells.Clear();
                        _selectedRowRange.ToList().ForEach(x => InvalidateRow(x));
                        _selectedRowRange.Clear();

                        if (ControlPressed) {
                            if (_selectedColumnRange.Contains(cell.Column.Value))
                                _selectedColumnRange.Remove(cell.Column.Value);
                            else
                                _selectedColumnRange.Add(cell.Column.Value);
                            InvalidateColumn(cell.Column.Value);
                        } else if (ShiftPressed) {
                            if (_currentCell.Column.HasValue) {
                                _selectedColumnRange.ToList().ForEach(x => InvalidateColumn(x));
                                ClearSelectedCells();

                                var start = Math.Min(_currentCell.Column.Value, cell.Column.Value);
                                var stop = Math.Max(_currentCell.Column.Value, cell.Column.Value);
                                for (int c = start; c <= stop; c++) {
                                    _selectedColumnRange.Add(c);
                                    InvalidateColumn(c);
                                }
                            }
                        } else {
                            if (_currentCell.IsCell) {
                                SetCurrentCell(cell);
                            }
                            _selectedColumnRange.ToList().ForEach(x => InvalidateColumn(x));
                            ClearSelectedCells();

                            _selectedColumnRange.Add(cell.Column.Value);
                            InvalidateColumn(cell.Column.Value);

                            _dragStartCell = cell;
                            _dragTimer.IsEnabled = true;
                            CaptureMouse();
                        }

                    }

                    OnChangeSelectedCells(true);
                }

                SetCurrentCell(cell);

            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.ChangedButton == MouseButton.Left && (_lastResizingColumn.HasValue && (DateTime.Now - _lastResizingColumnSet) < TimeSpan.FromSeconds(1)))
            {
                _lastDblClickResize = DateTime.Now;
                int col = _lastResizingColumn.Value;

                _columnSizes.RemoveSizeOverride(col);

                if (_model == null) return;
                int rowCount = _modelRowCount;
                int colCount = _modelColumnCount;
                {
                    var cell = _model.GetColumnHeader(this, col);
                    _columnSizes.PutSizeOverride(col, GetCellContentWidth(cell) + 2 * CellPaddingHorizontal);
                }
                int visRows = VisibleRowCount;
                int row0 = FirstVisibleRowScrollIndex + _rowSizes.FrozenCount;
                for (int row = row0; row < Math.Min(row0 + visRows, rowCount); row++)
                {
                    var cell = _model.GetCell(this, row, col);
                    _columnSizes.PutSizeOverride(col, GetCellContentWidth(cell, _columnSizes.MaxSize) + 2 * CellPaddingHorizontal);
                }

                _columnSizes.BuildIndex();
                AdjustScrollbars();
                SetScrollbarMargin();
                FixScrollPosition();
                InvalidateAll();
            } else 
            {
                var pt = e.GetPosition(image);
                pt.X *= DpiDetector.DpiXKoef;
                pt.Y *= DpiDetector.DpiYKoef;
                var cell = GetCellAddress(pt);

                if (cell.IsColumnHeader && cell.Column.HasValue) {
                    var column = cell.Column.Value;
                    if (column >= 0 && column < _modelColumnCount) {
                        var args = new ColumnClickEventArgs {
                            Grid = this,
                            Column = column,
                        };
                        if (ColumnHeaderDoubleClick != null) {
                            ColumnHeaderDoubleClick(this, args);
                        }
                    }

                }
            }
        }

        private void _dragTimer_Tick(object sender, EventArgs e)
        {
            using (var ctx = CreateInvalidationContext())
            {
                if (_mouseIsBehindBottom)
                {
                    int newRow = FirstVisibleRowScrollIndex + 1;
                    if (!_rowSizes.IsWholeInView(FirstVisibleRowScrollIndex, _rowSizes.ScrollCount - 1, GridScrollAreaHeight))
                    {
                        ScrollContent(newRow, FirstVisibleColumnScrollIndex);
                        var row = FirstVisibleRowScrollIndex + VisibleRowCount - 1 + _rowSizes.FrozenCount;
                        SetSelectedRectangle(_dragStartCell, new FastGridCellAddress(row, _mouseMoveColumn ?? _currentCell.Column));
                        AdjustScrollBarPositions();
                    }
                }
                if (_mouseIsBehindTop)
                {
                    int newRow = FirstVisibleRowScrollIndex - 1;
                    if (newRow >= 0)
                    {
                        ScrollContent(newRow, FirstVisibleColumnScrollIndex);
                        var row = newRow + _rowSizes.FrozenCount;
                        SetSelectedRectangle(_dragStartCell, new FastGridCellAddress(row, _mouseMoveColumn ?? _currentCell.Column));
                        AdjustScrollBarPositions();
                    }
                }
            }
            using (var ctx = CreateInvalidationContext())
            {
                if (_mouseIsBehindRight)
                {
                    int newColumn = FirstVisibleColumnScrollIndex + 1;
                    if (!_columnSizes.IsWholeInView(FirstVisibleColumnScrollIndex, _columnSizes.ScrollCount - 1, GridScrollAreaWidth))
                    {
                        ScrollContent(FirstVisibleRowScrollIndex, newColumn);
                        var col = FirstVisibleColumnScrollIndex + VisibleColumnCount - 1 + _columnSizes.FrozenCount;
                        SetSelectedRectangle(_dragStartCell, new FastGridCellAddress(_mouseMoveRow ?? _currentCell.Row, col));
                        AdjustScrollBarPositions();
                    }
                }
                if (_mouseIsBehindLeft)
                {
                    int newColumn = FirstVisibleColumnScrollIndex - 1;
                    if (newColumn >= 0)
                    {
                        ScrollContent(FirstVisibleRowScrollIndex, newColumn);
                        var col = newColumn + _columnSizes.FrozenCount;
                        SetSelectedRectangle(_dragStartCell, new FastGridCellAddress(_mouseMoveRow ?? _currentCell.Row, col));
                        AdjustScrollBarPositions();
                    }
                }
            }
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!_dragStartCell.IsEmpty)
            {
                _dragStartCell = new FastGridCellAddress();
                _dragTimer.IsEnabled = false;
                ReleaseMouseCapture();
            }
            //bool wasColumnResizing = false;
            if (_resizingColumn.HasValue)
            {
                _lastResizingColumnSet = DateTime.Now;
                _lastResizingColumn = _resizingColumn;
                _resizingColumn = null;
                _resizingColumnOrigin = null;
                _resizingColumnStartSize = null;
                //wasColumnResizing = true;
                ReleaseMouseCapture();
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            //var pt = e.GetPosition(image);
            //pt.X *= DpiDetector.DpiXKoef;
            //pt.Y *= DpiDetector.DpiYKoef;
            //var cell = GetCellAddress(pt);

            //if (!_selectedCells.Contains(cell))
            //{
            //    using (var ctx = CreateInvalidationContext())
            //    {
            //        InvalidateCell(_currentCell);
            //        _selectedCells.ToList().ForEach(InvalidateCell);
            //        ClearSelectedCells();
            //        AddSelectedCell(cell);
            //        _currentCell = cell;
            //        InvalidateCell(_currentCell);
            //        OnChangeSelectedCells(true);
            //    }
            //}
        }

        private bool OnModelColumnClick(int column)
        {
            if (column >= 0 && column < _modelColumnCount)
            {
                var args = new ColumnClickEventArgs
                {
                    Grid = this,
                    Column = column,
                };
                if (ColumnHeaderClick != null)
                {
                    ColumnHeaderClick(this, args);
                }
                return args.Handled;
            }
            return false;
        }

        private bool OnModelRowClick(int row)
        {
            if (row >= 0 && row < _modelRowCount)
            {
                var args = new RowClickEventArgs
                {
                    Grid = this,
                    Row = row,
                };
                if (RowHeaderClick != null)
                {
                    RowHeaderClick(this, args);
                }
                return args.Handled;
            }
            return false;
        }

        private void imageMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ControlPressed)
            {
                if (e.Delta < 0 && CellFontSize < 20) CellFontSize++;
                if (e.Delta > 0 && CellFontSize > 6) CellFontSize--;

                RecountColumnWidths();
                RecountRowHeights();
                AdjustScrollbars();
                SetScrollbarMargin();
                FixScrollPosition();
                InvalidateAll();
            }
            else
            {
                if (e.Delta < 0) vscroll.Value = vscroll.Value + vscroll.LargeChange / 2;
                if (e.Delta > 0) vscroll.Value = vscroll.Value - vscroll.LargeChange / 2;
                ScrollChanged();
            }
        }

        private static bool ControlPressed
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Control) != 0; }
        }

        private static bool ShiftPressed
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Shift) != 0; }
        }

        private bool HandleCursorMove(KeyEventArgs e, bool isInTextBox = false)
        {
            if (e.Key == Key.Up && ControlPressed) return MoveCurrentCell(0, _currentCell.Column, e);
            if (e.Key == Key.Down && ControlPressed) return MoveCurrentCell(_realRowCount - 1, _currentCell.Column, e);
            if (e.Key == Key.Left && ControlPressed) return MoveCurrentCell(_currentCell.Row, 0, e);
            if (e.Key == Key.Right && ControlPressed) return MoveCurrentCell(_currentCell.Row, _realColumnCount - 1, e);

            if (e.Key == Key.Up) return MoveCurrentCell(_currentCell.Row - 1, _currentCell.Column, e);
            if (e.Key == Key.Down) return MoveCurrentCell(_currentCell.Row + 1, _currentCell.Column, e);
            if (e.Key == Key.Left && !isInTextBox) return MoveCurrentCell(_currentCell.Row, _currentCell.Column - 1, e);
            if (e.Key == Key.Right && !isInTextBox) return MoveCurrentCell(_currentCell.Row, _currentCell.Column + 1, e);

            if (e.Key == Key.Home && ControlPressed) return MoveCurrentCell(0, 0, e);
            if (e.Key == Key.End && ControlPressed) return MoveCurrentCell(_realRowCount - 1, _realColumnCount - 1, e);
            if (e.Key == Key.PageDown && ControlPressed) return MoveCurrentCell(_realRowCount - 1, _currentCell.Column, e);
            if (e.Key == Key.PageUp && ControlPressed) return MoveCurrentCell(0, _currentCell.Column, e);
            if (e.Key == Key.Home && !isInTextBox) return MoveCurrentCell(_currentCell.Row, 0, e);
            if (e.Key == Key.End && !isInTextBox) return MoveCurrentCell(_currentCell.Row, _realColumnCount - 1, e);
            if (e.Key == Key.PageDown) return MoveCurrentCell(_currentCell.Row + VisibleRowCount, _currentCell.Column, e);
            if (e.Key == Key.PageUp) return MoveCurrentCell(_currentCell.Row - VisibleRowCount, _currentCell.Column, e);

            if (e.Key == Key.Enter && !isInTextBox) return MoveCurrentCell(_currentCell.Row + 1, _currentCell.Column, e);
            if (e.Key == Key.Return && !isInTextBox) return MoveCurrentCell(_currentCell.Row + 1, _currentCell.Column, e);
            if (e.Key == Key.Tab) return MoveCurrentCell(_currentCell.Row, _currentCell.Column + 1, e);
            return false;
        }

        private void imageKeyDown(object sender, KeyEventArgs e)
        {
            using (var ctx = CreateInvalidationContext())
            {
                if (ShiftPressed)
                {
                    if (!_shiftDragStartCell.IsCell)
                    {
                        _shiftDragStartCell = _currentCell;
                    }
                }
                else
                {
                    _shiftDragStartCell = FastGridCellAddress.Empty;
                }

                bool moved = HandleCursorMove(e);
                if (ShiftPressed && moved) SetSelectedRectangle(_shiftDragStartCell, _currentCell);

                if (e.Key == Key.A && ControlPressed && AllowSelectAll)
                {
                    SelectAll();
                }
            }
        }

        private void imageMouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(image);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            using (var ctx = CreateInvalidationContext())
            {
                var pt = e.GetPosition(image);

                _mouseIsBehindLeft = pt.X < 0;
                _mouseIsBehindRight = pt.X > image.ActualWidth;
                _mouseIsBehindTop = pt.Y < 0;
                _mouseIsBehindBottom = pt.Y > image.ActualHeight;

                pt.X *= DpiDetector.DpiXKoef;
                pt.Y *= DpiDetector.DpiYKoef;
                _mouseCursorPoint = pt;
                var cell = GetCellAddress(pt);
                _mouseMoveRow = GetSeriesIndexOnPosition(pt.Y, HeaderHeight, _rowSizes, FirstVisibleRowScrollIndex);
                _mouseMoveColumn = GetSeriesIndexOnPosition(pt.X, HeaderWidth, _columnSizes, FirstVisibleColumnScrollIndex);

                if (_resizingColumn.HasValue)
                {
                    int newSize = _resizingColumnStartSize.Value + (int)Math.Round(pt.X - _resizingColumnOrigin.Value.X);
                    if (newSize < MinColumnWidth) newSize = MinColumnWidth;
                    if (newSize > GridScrollAreaWidth) newSize = GridScrollAreaWidth;
                    _columnSizes.Resize(_resizingColumn.Value, newSize);
                    if (_resizingColumn < _columnSizes.FrozenCount)
                    {
                        SetScrollbarMargin();
                    }
                    AdjustScrollbars();
                    InvalidateAll();
                }
                else
                {
                    int? column = GetResizingColumn(pt);
                    if (column != null) Cursor = Cursors.SizeWE;
                    else Cursor = Cursors.Arrow;
                }

                if (_dragStartCell.IsCell && cell.IsCell
                    || _dragStartCell.IsRowHeader && cell.Row.HasValue
                    || _dragStartCell.IsColumnHeader && cell.Column.HasValue)
                {
                    SetSelectedRectangle(_dragStartCell, cell);
                }

                SetHoverRow(cell.IsCell ? cell.Row.Value : (int?)null);
                SetHoverRowHeader(cell.IsRowHeader ? cell.Row.Value : (int?)null);
                SetHoverColumnHeader(cell.IsColumnHeader ? cell.Column.Value : (int?)null);
                SetHoverCell(cell);

                var currentRegion = CurrentCellActiveRegions.FirstOrDefault(x => x.Rect.Contains(pt));
                if (currentRegion != CurrentHoverRegion)
                {
                    InvalidateCell(cell);
                }
            }

            HandleMouseMoveTooltip();
        }

        private void HandleMouseMoveTooltip()
        {
            if (CurrentHoverRegion != null && CurrentHoverRegion.Tooltip != null)
            {
                ShowTooltip(CurrentHoverRegion, CurrentHoverRegion.Tooltip);
                return;
            }

            if (CurrentHoverRegion == null)
            {
                var modelCell = GetCell(_mouseOverCell);
                if (modelCell != null)
                {
                    if (modelCell.ToolTipVisibility == TooltipVisibilityMode.Always || _mouseOverCellIsTrimmed)
                    {
                        string tooltip = modelCell.ToolTipText;
                        if (tooltip != null)
                        {
                            ShowTooltip(_mouseOverCell, tooltip);
                            return;
                        }
                    }
                }
            }

            HideTooltip();
        }

        private void HideTooltip()
        {
            if (_tooltip != null && _tooltip.IsOpen)
            {
                _tooltip.IsOpen = false;
            }
            _tooltipTarget = null;
            if (_tooltipTimer != null)
            {
                _tooltipTimer.IsEnabled = false;
            }
        }

        private void ShowTooltip(object tooltipTarget, string text)
        {
            if (Equals(tooltipTarget, _tooltipTarget) && _tooltipText == text) return;
            HideTooltip();

            if (_tooltip == null)
            {
                _tooltip = new ToolTip();
            }
            if (_tooltipTimer == null)
            {
                _tooltipTimer = new DispatcherTimer();
                _tooltipTimer.Interval = TimeSpan.FromSeconds(0.5);
                _tooltipTimer.Tick += _tooltipTimer_Tick;
            }

            _tooltipText = text;
            _tooltipTarget = tooltipTarget;
            _tooltip.Content = text;
            _tooltipTimer.IsEnabled = true;
        }

        private void _tooltipTimer_Tick(object sender, EventArgs e)
        {
            _tooltip.IsOpen = true;
            _tooltipTimer.IsEnabled = false;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            using (var ctx = CreateInvalidationContext())
            {
                SetHoverRow(null);
                SetHoverRowHeader(null);
                SetHoverColumnHeader(null);
            }
        }

        public void HandleCommand(FastGridCellAddress address, object commandParameter)
        {
            bool handled = false;
            if (Model != null)
            {
                var addressModel = RealToModel(address);
                Model.HandleCommand(this, addressModel, commandParameter, ref handled);
            }
            if (handled) return;
            if (commandParameter == ToggleAllowFlexibleRowsCommand)
            {
                AllowFlexibleRows = !AllowFlexibleRows;
            }
            if (commandParameter == SelectAllCommand)
            {
                DoSelectAll();
            }
            if (commandParameter == AdjustColumnSizesCommand)
            {
                RecountColumnWidths();
                AdjustScrollbars();
                SetScrollbarMargin();
                FixScrollPosition();
                InvalidateAll();
            }
        }

        private void SelectAll()
        {
            HandleCommand(FastGridCellAddress.Empty, SelectAllCommand);
        }

        private void DoSelectAll()
        {
            SetSelectedRectangle(new FastGridCellAddress(0, 0), new FastGridCellAddress(_rowSizes.RealCount - 1, _columnSizes.RealCount - 1));
        }

        //public void SelectAll(int? rowCountLimit, int? columnCountLimit)
        //{
        //    var rows = _rowSizes.RealCount;
        //    var cols = _columnSizes.RealCount;
        //    if (rowCountLimit != null)
        //    {
        //        if (rowCountLimit.Value < rows) rows = rowCountLimit.Value;
        //    }
        //    if (columnCountLimit != null)
        //    {
        //        if (columnCountLimit.Value < cols) cols = columnCountLimit.Value;
        //    }
        //    SetSelectedRectangle(new FastGridCellAddress(0, 0), new FastGridCellAddress(rows - 1, cols - 1));
        //}

        private void imageMouseLeave(object sender, MouseEventArgs e)
        {
            HideTooltip();
        }

        private void OnScrolledModelRows()
        {
            if (ScrolledModelRows != null) ScrolledModelRows(this, EventArgs.Empty);
        }

        private void OnScrolledModelColumns()
        {
            if (ScrolledModelColumns != null) ScrolledModelColumns(this, EventArgs.Empty);
        }

        private void selectionCommandClick(object sender, RoutedEventArgs e)
        {
            dynamic button = sender;
            SelectionQuickCommand cmd = button.DataContext;
            cmd.Model.HandleSelectionCommand(this, cmd.Text);
        }
    }
}
