using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DataContainer;
using FastWpfGrid;

namespace UI_Chart.ViewModels {
    public class FastDataGridModel : FastGridModelBase{
        private IDataAcquire _da;
        private SubData _subData;

        private Color? _cellColor;

        public FastDataGridModel(SubData subData) {
            _subData = subData;
            _da = StdDB.GetDataAcquire(subData.StdFilePath);
        }

        public override HashSet<int> GetFrozenColumns(IFastGridView view) {
            var col = new HashSet<int>();
            col.Add(0);
            col.Add(1);
            return col;
        }

        public override string GetColumnHeaderText(int column) {
            return (column+1).ToString();
        }

        public override string GetRowHeaderText(int row) {
            return string.Empty;
        }

        public override int ColumnCount {
            get { return _da.GetFilteredChipsCount(_subData.FilterId) + 2; }
        }

        public override int RowCount {
            get { return _da.GetTestIDs().Count() + 6; }
        }

        public override Color? FontColor {
            get { return _cellColor; }
        }

        public override string GetCellText(int row, int column) {
            _cellColor = null;

            if (column == 0) {
                if (row < 6) {
                    return string.Empty;
                }
                return _da.GetTestIDs().ElementAt(row-6);
            } else if (column == 1){
                if (row == 0) return "Index";
                if (row == 1) return "Cord";
                if (row == 2) return "Time";
                if (row == 3) return "HBin";
                if (row == 4) return "SBin";
                if (row == 5) return "Site";

                var uid = _da.GetTestIDs().ElementAt(row-6);
                return _da.GetTestIDs_Info()[uid].TestText;

            } else if(column > 1){
                var idx = _da.GetFilteredPartIndex(_subData.FilterId).ElementAt(column - 2);
                if (row == 0) return idx.ToString();
                if (row == 1) return _da.GetWaferCord(idx);
                if (row == 2) return _da.GetTestTime(idx).ToString();
                if (row == 3) return _da.GetHardBin(idx).ToString();
                if (row == 4) return _da.GetSoftBin(idx).ToString();
                if (row == 5) return _da.GetSite(idx).ToString();
                
                var uid = _da.GetTestIDs().ElementAt(row - 6);
                var val = _da.GetItemData(uid, idx);
                var limit = _da.GetTestInfo(uid);

                if(limit.LoLimit.HasValue && val < limit.LoLimit)
                    _cellColor = Colors.Blue;
                else if(limit.HiLimit.HasValue && val > limit.HiLimit)
                    _cellColor = Colors.Red;

                return val.ToString();
            }
            return string.Empty;
        }

        //public override void SetCellText(int row, int column, string value) {
        //    var key = Tuple.Create(row, column);
        //    _editedCells[key] = value;
        //}

        //public override IFastGridCell GetGridHeader(IFastGridView view) {
        //    var impl = new FastGridCellImpl();
        //    var btn = impl.AddImageBlock(view.IsTransposed ? "/Images/flip_horizontal_small.png" : "/Images/flip_vertical_small.png");
        //    btn.CommandParameter = FastWpfGrid.FastGridControl.ToggleTransposedCommand;
        //    btn.ToolTip = "Swap rows and columns";
        //    impl.AddImageBlock("/Images/foreign_keysmall.png").CommandParameter = "FK";
        //    impl.AddImageBlock("/Images/primary_keysmall.png").CommandParameter = "PK";
        //    return impl;
        //}

        //public override void HandleCommand(IFastGridView view, FastGridCellAddress address, object commandParameter, ref bool handled) {
        //    //base.HandleCommand(view, address, commandParameter, ref handled);
        //    //if (commandParameter is string) MessageBox.Show(commandParameter.ToString());
        //}

        //public override int? SelectedRowCountLimit {
        //    get { return 100; }
        //}

        //public override void HandleSelectionCommand(IFastGridView view, string command) {
        //    //MessageBox.Show(command);
        //}


    }
}
