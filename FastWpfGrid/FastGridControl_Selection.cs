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
        private Dictionary<int, int> _selectedRows = new Dictionary<int, int>();
        private Dictionary<int, int> _selectedColumns = new Dictionary<int, int>();

        private void ClearSelectedCells()
        {
            _selectedCells.Clear();
            _selectedRows.Clear();
            _selectedColumns.Clear();

        }

        private void AddSelectedCell(FastGridCellAddress cell)
        {
            if (!cell.IsCell) return;

            if (_selectedCells.Contains(cell)) return;

            _selectedCells.Add(cell);

            if (!_selectedRows.ContainsKey(cell.Row.Value)) _selectedRows[cell.Row.Value] = 0;
            _selectedRows[cell.Row.Value]++;

            if (!_selectedColumns.ContainsKey(cell.Column.Value)) _selectedColumns[cell.Column.Value] = 0;
            _selectedColumns[cell.Column.Value]++;

        }

        private void RemoveSelectedCell(FastGridCellAddress cell)
        {
            if (!cell.IsCell) return;

            if (!_selectedCells.Contains(cell)) return;

            _selectedCells.Remove(cell);

            if (_selectedRows.ContainsKey(cell.Row.Value))
            {
                _selectedRows[cell.Row.Value]--;
                if (_selectedRows[cell.Row.Value] == 0) _selectedRows.Remove(cell.Row.Value);
            }

            if (_selectedColumns.ContainsKey(cell.Column.Value))
            {
                _selectedColumns[cell.Column.Value]--;
                if (_selectedColumns[cell.Column.Value] == 0) _selectedColumns.Remove(cell.Column.Value);
            }

        }

        private void SetSelectedRectangle(FastGridCellAddress origin, FastGridCellAddress cell)
        {
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
            SetCurrentCell(cell);
            OnChangeSelectedCells(true);
        }

        private void OnChangeSelectedCells(bool isInvokedByUser)
        {
            if (SelectedCellsChanged != null) SelectedCellsChanged(this, new SelectionChangedEventArgs { IsInvokedByUser = isInvokedByUser });
        }
    }
}
