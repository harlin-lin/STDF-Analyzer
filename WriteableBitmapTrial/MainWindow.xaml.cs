using GalaSoft.MvvmLight;
using MapBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WriteableBitmapTrial {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            this.DataContext = this;
            InitializeComponent();
            InitData();
        }

        private void InitData() {
            var waferData = new WaferData();
            OnDataSourceChanged(waferData);

        }

        public Color[,] WaferColor { get; set; }


        private Dictionary<short?, Color[,]> _sBinMaps= new Dictionary<short?, Color[,]>();
        private Dictionary<short?, Color[,]> _hBinMaps = new Dictionary<short?, Color[,]>();
        private Dictionary<short?, Color[,]> _freshSBinMaps = new Dictionary<short?, Color[,]>();
        private Dictionary<short?, Color[,]> _freshHBinMaps = new Dictionary<short?, Color[,]>();
        private Dictionary<short?, bool> _containRtFlg = new Dictionary<short?, bool>();

        private MapViewMode _mapViewMode = MapViewMode.Single;
        private MapBinMode _mapBinMode = MapBinMode.SBin;
        private MapRtDataMode _mapRtDataMode = MapRtDataMode.OverWrite;

        private Dictionary<ushort, int> _sBinIdx= null;
        private Dictionary<ushort, int> _hBinIdx = null;

        private List<string> _logList = new List<string>();

        private short? _selectedSingleWafer=null;

        private IWaferData _waferData;

        private int _xCnt, _yCnt;

        private Dictionary<short?, MapBaseControl> _mapControlList= new Dictionary<short?, MapBaseControl>();
        private MapBaseControl _mapControlStack;

        private void SwitchSingleView(MapBaseControl mapBaseControl) {
            viewGrid.RowDefinitions.Clear();
            viewGrid.ColumnDefinitions.Clear();
            mapBaseControl.CordChanged += MapBaseControl_CordChanged;
            viewGrid.Children.Add(mapBaseControl);
        }

        private void MapBaseControl_CordChanged(int x, int y) {
            if(x==int.MinValue || y == int.MinValue) {
                infoBlock.Text = "";
                infoBlock.Visibility = Visibility.Hidden;
            } else {
                infoBlock.Text = $"X:{x} Y:{y}";
                infoBlock.Visibility = Visibility.Visible;
                var pt = Mouse.GetPosition(viewGrid);
                infoBlock.Margin = new Thickness {
                    Left = pt.X + 12,
                    Top = pt.Y
                };
            }
        }

        private void SwitchSplitView(int splitColCnt, List<MapBaseControl> mapBaseControls) {

            viewGrid.RowDefinitions.Clear();
            viewGrid.ColumnDefinitions.Clear();
            viewGrid.ShowGridLines = true;

            int rowCnt = mapBaseControls.Count / splitColCnt + (mapBaseControls.Count % splitColCnt == 0 ? 0 : 1);

            for (int i=0; i< splitColCnt; i++) {
                viewGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < rowCnt; i++) {
                viewGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < mapBaseControls.Count; i++) {                
                viewGrid.Children.Add(mapBaseControls[i]);
                Grid.SetRow(mapBaseControls[i], i / splitColCnt);
                Grid.SetColumn(mapBaseControls[i], i % splitColCnt);
            }

        }

        

        private void UpdateView() {
            if (_sBinMaps.Count == 0) return;

            Dictionary<short?, Color[,]> maps;

            if (_mapBinMode == MapBinMode.HBin) {
                if(_mapRtDataMode == MapRtDataMode.FirstOnly) {
                    maps = _freshHBinMaps;
                } else {
                    maps = _hBinMaps;
                }
            } else {
                if (_mapRtDataMode == MapRtDataMode.FirstOnly) {
                    maps = _freshSBinMaps;
                } else {
                    maps = _sBinMaps;
                }
            }

            //gen stack map
            Color[,] stack = new Color[_xCnt, _yCnt];
            int?[,] stackCnt = new int?[_xCnt, _yCnt];

            foreach (var wafer in maps) {
                for(int x=0; x < _xCnt; x++) {
                    for(int y=0; y<_yCnt; y++) {
                        if(wafer.Value[x,y] != BinColor.GetPassBinColor() && wafer.Value[x, y] != new Color()) {
                            if (stackCnt[x, y] is null) stackCnt[x, y] = 0;
                            stackCnt[x, y]++;
                        }else if(wafer.Value[x, y] == BinColor.GetPassBinColor()) {
                            if (stackCnt[x, y] is null) stackCnt[x, y] = 0;
                        }
                    }
                }
            }
            for (int x = 0; x < _xCnt; x++) {
                for (int y = 0; y < _yCnt; y++) {
                    if(stackCnt[x, y].HasValue) {
                        stack[x, y] = BinColor.GetStackWaferBinColor(stackCnt[x, y].Value, maps.Count);
                    } else {
                        stack[x, y] = Colors.White;
                    }
                }
            }

            _mapControlList.Clear();
            foreach (var wafer in maps) {
                var map = new MapBaseControl();
                map.MapDataSource = wafer.Value;
                _mapControlList.Add(wafer.Key, map);
            }
            _mapControlStack = new MapBaseControl();
            _mapControlStack.MapDataSource = stack;

            SwitchSingleView(_mapControlList[1]);
            //SwitchSingleView(_mapControlStack);
        }

        private void UpdateData() {
            _logList.Clear();

            _hBinIdx = new Dictionary<ushort, int>();
            _sBinIdx = new Dictionary<ushort, int>();

            _xCnt = _waferData.XUbound - _waferData.XLbound + 1;
            _yCnt = _waferData.YUbound - _waferData.YLbound + 1;

            foreach (var die in _waferData.DieInfoList) {
                if (!_sBinIdx.ContainsKey(die.SBin) && !die.PassOrFail) _sBinIdx.Add(die.SBin, _sBinIdx.Count);
                if (!_hBinIdx.ContainsKey(die.HBin) && !die.PassOrFail) _hBinIdx.Add(die.HBin, _hBinIdx.Count);

                if (!_sBinMaps.ContainsKey(die.WaferId)) {
                    _sBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _hBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _freshSBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _freshHBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _containRtFlg.Add(die.WaferId, false);
                }
                //ignore the unreasonable point
                if (die.X > _waferData.XUbound || die.X < _waferData.XLbound || die.Y > _waferData.YUbound || die.Y < _waferData.XLbound) {
                    _logList.Add($"Cord X:{die.X} Y:{die.Y} out of wafer");
                    continue;
                }
                
                if (_sBinMaps[die.WaferId][die.X, die.Y] == new Color()) {
                    if (die.PassOrFail) {
                        _sBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetPassBinColor();
                        _hBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetPassBinColor();
                        _freshSBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetPassBinColor();
                        _freshHBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetPassBinColor();

                    } else {
                        _sBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetFailBinColor(_sBinIdx[die.SBin]);
                        _hBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetFailBinColor(_hBinIdx[die.HBin]);
                        _freshSBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetFailBinColor(_sBinIdx[die.SBin]);
                        _freshHBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetFailBinColor(_hBinIdx[die.HBin]);
                    }
                } else {
                    _containRtFlg[die.WaferId] = true;
                    if (die.PassOrFail) {
                        _freshSBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetPassBinColor();
                        _freshHBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetPassBinColor();

                    } else {
                        _freshSBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetFailBinColor(_sBinIdx[die.SBin]);
                        _freshHBinMaps[die.WaferId][die.X, die.Y] = BinColor.GetFailBinColor(_hBinIdx[die.HBin]);
                    }
                }

            }

            UpdateView();
        }


        private void OnDataSourceChanged(IWaferData waferData) {
            _waferData = waferData;

            UpdateData();
            //Task.Run(() => { UpdateData(); });
        }



    }
}
