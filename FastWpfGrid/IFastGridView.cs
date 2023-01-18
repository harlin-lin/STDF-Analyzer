using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastWpfGrid
{
    public interface IFastGridView
    {
        /// <summary>
        /// invalidates whole grid
        /// </summary>
        void InvalidateAll();

        /// <summary>
        /// invalidates given cell
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        void InvalidateModelCell(int row, int column);
        
        /// <summary>
        /// invalidates given row header
        /// </summary>
        /// <param name="row"></param>
        void InvalidateModelRowHeader(int row);

        /// <summary>
        /// invalidates given row (all cells including header)
        /// </summary>
        /// <param name="row"></param>
        void InvalidateModelRow(int row);

        /// <summary>
        /// invalidates given column header
        /// </summary>
        /// <param name="column"></param>
        void InvalidateModelColumnHeader(int column);

        /// <summary>
        /// invalidates given column (all cells including header)
        /// </summary>
        /// <param name="column"></param>
        void InvalidateModelColumn(int column);

        /// <summary>
        /// invalidates grid header (top-left header cell)
        /// </summary>
        void InvalidateGridHeader();

        /// <summary>
        /// forces grid to refresh all data
        /// </summary>
        void NotifyRefresh();

        /// <summary>
        /// notifies grid about new rows added to the end
        /// </summary>
        void NotifyAddedRows();

        /// <summary>
        /// notifies grid, that result of GetHiddenColumns() or GetFrozenColumns() is changed
        /// </summary>
        void NotifyColumnArrangeChanged();

        /// <summary>
        /// notifies grid, that result of GetHiddenRows() or GetFrozenRows() is changed
        /// </summary>
        void NotifyRowArrangeChanged();

        /// <summary>
        /// gets whether flexible rows (real rows) are curently used
        /// </summary>
        bool FlexibleRows { get; }

        /// <summary>
        /// gets or sets whereher flexible rows are allows
        /// </summary>
        bool AllowFlexibleRows { get; set; }

        /// <summary>
        /// gets selected model cells
        /// </summary>
        /// <returns></returns>
        HashSet<FastGridCellAddress> GetSelectedModelCells();

        /// <summary>
        /// gets selected model rows
        /// </summary>
        /// <returns></returns>
        HashSet<int> GetSelectedModelRows();

        /// <summary>
        /// gets selected model columns
        /// </summary>
        /// <returns></returns>
        HashSet<int> GetSelectedModelColumns();

        /// <summary>
        /// handles command
        /// </summary>
        /// <param name="address"></param>
        /// <param name="command"></param>
        void HandleCommand(FastGridCellAddress address, object command);

        ///// <summary>
        ///// selects all cells in grid (with given limits)
        ///// </summary>
        ///// <param name="rowCountLimit"></param>
        ///// <param name="columnCountLimit"></param>
        //void SelectAll(int? rowCountLimit, int? columnCountLimit);
    }
}
