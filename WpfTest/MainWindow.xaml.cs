using FastWpfGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataInterface;
using DataParse;

namespace WpfTest {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        //private GridModel1 _model1;
        private StdLogGridModel gridModel;
        public MainWindow() {
            InitializeComponent();
            InitData();
            //grid1.Model = _model1 = new GridModel1();
        }

        void InitData() {
            IDataAcquire dataAcquire = new StdfParse(@"E:\Data\12345678.stdf");

            dataAcquire.ExtractStdf();

            gridModel = new StdLogGridModel(new StdLogTable(dataAcquire, 0, 100, dataAcquire.GetAllFilter().ElementAt(0).Key));

            grid1.Model = gridModel;
        }

        private void grid1_SelectedCellsChanged(object sender, FastWpfGrid.SelectionChangedEventArgs e) {

        }
    }

    public class GridModel1 : FastGridModelBase {
        private Dictionary<Tuple<int, int>, string> _editedCells = new Dictionary<Tuple<int, int>, string>();
        private static string[] _columnBasicNames = new[] { "", "Value:", "Long column value:" };

        public override int ColumnCount {
            get { return 100; }
        }

        public override int RowCount {
            get { return 1000; }
        }

        public override string GetCellText(int row, int column) {
            var key = Tuple.Create(row, column);
            if (_editedCells.ContainsKey(key)) return _editedCells[key];


            return String.Format("{0}{1},{2}", _columnBasicNames[column % _columnBasicNames.Length], row + 1, column + 1);
        }

        //public override void SetCellText(int row, int column, string value) {
        //    var key = Tuple.Create(row, column);
        //    _editedCells[key] = value;
        //}

        //public override IFastGridCell GetGridHeader(IFastGridView view) {
        //    var impl = new FastGridCellImpl();
        //    //var btn = impl.AddImageBlock(view.IsTransposed ? "/Images/flip_horizontal_small.png" : "/Images/flip_vertical_small.png");
        //    //btn.CommandParameter = FastWpfGrid.FastGridControl.ToggleTransposedCommand;
        //    //btn.ToolTip = "Swap rows and columns";
        //    //impl.AddImageBlock("/Images/foreign_keysmall.png").CommandParameter = "FK";
        //    //impl.AddImageBlock("/Images/primary_keysmall.png").CommandParameter = "PK";
        //    return impl;
        //}

        public override void HandleCommand(IFastGridView view, FastGridCellAddress address, object commandParameter, ref bool handled) {
            base.HandleCommand(view, address, commandParameter, ref handled);
            if (commandParameter is string) MessageBox.Show(commandParameter.ToString());
        }

        public override int? SelectedRowCountLimit {
            get { return 100; }
        }

        public override void HandleSelectionCommand(IFastGridView view, string command) {
            MessageBox.Show(command);
        }
    }

}
