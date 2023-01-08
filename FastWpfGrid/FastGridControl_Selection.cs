using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FastWpfGrid
{
    public enum SelectionModeType {
        CellMode,
        RowMode,
        ColumnMode
    }
    partial class FastGridControl
    {
        public event EventHandler<SelectionChangedEventArgs> SelectedCellsChanged;

        private HashSet<FastGridCellAddress> _selectedCells = new HashSet<FastGridCellAddress>();

        private HashSet<int> _selectedRowRange = new HashSet<int>();
        private HashSet<int> _selectedColumnRange = new HashSet<int>();


        private void ClearSelectedCells()
        {
            _selectedCells.Clear();
            _selectedRowRange.Clear();
            _selectedColumnRange.Clear();
        }

        private void AddSelectedCell(FastGridCellAddress cell)
        {
            if (!cell.IsCell) return;

            if (_selectedCells.Contains(cell)) return;

            _selectedCells.Add(cell);

        }

        private void RemoveSelectedCell(FastGridCellAddress cell)
        {
            if (!cell.IsCell) return;

            if (!_selectedCells.Contains(cell)) return;

            _selectedCells.Remove(cell);

        }

        private void SetSelectedRectangle(FastGridCellAddress origin, FastGridCellAddress cell)
        {
            if (origin.IsColumnHeader || _selectionMode == SelectionModeType.ColumnMode) {
                if (cell.Column.HasValue) {
                    var start = Math.Min(cell.Column.Value, origin.Column.Value);
                    var stop = Math.Max(cell.Column.Value, origin.Column.Value);
                    var newSelected = Enumerable.Range(start, stop - start + 1);
                    foreach (var added in newSelected) {
                        if (_selectedColumnRange.Contains(added)) continue;
                        InvalidateColumn(added);
                        _selectedColumnRange.Add(added);
                    }
                    foreach (var removed in _selectedColumnRange.ToList()) {
                        if (newSelected.Contains(removed)) continue;
                        InvalidateColumn(removed);
                        _selectedColumnRange.Remove(removed);
                    }
                }

            }else if (origin.IsRowHeader || _selectionMode == SelectionModeType.RowMode) {
                if (cell.Row.HasValue) {
                    var start = Math.Min(cell.Row.Value, origin.Row.Value);
                    var stop = Math.Max(cell.Row.Value, origin.Row.Value);
                    var newSelected = Enumerable.Range(start, stop - start + 1);
                    foreach (var added in newSelected) {
                        if (_selectedRowRange.Contains(added)) continue;
                        InvalidateRow(added);
                        _selectedRowRange.Add(added);
                    }
                    foreach (var removed in _selectedRowRange.ToList()) {
                        if (newSelected.Contains(removed)) continue;
                        InvalidateRow(removed);
                        _selectedRowRange.Remove(removed);
                    }
                }

            } else {
                var newSelected = GetCellRange(origin, cell);
                foreach (var added in newSelected)
                {
                    if (_selectedCells.Contains(added)) continue;
                    InvalidateCell(added);
                    AddSelectedCell(added);
                }
                foreach (var removed in _selectedCells.ToList())
                {
                    if (newSelected.Contains(removed)) continue;
                    InvalidateCell(removed);
                    RemoveSelectedCell(removed);
                }
            }

            SetCurrentCell(cell);
            OnChangeSelectedCells(true);
        }

        private void OnChangeSelectedCells(bool isInvokedByUser)
        {
            if (SelectedCellsChanged != null) SelectedCellsChanged(this, new SelectionChangedEventArgs { IsInvokedByUser = isInvokedByUser });
        }
    }
}
