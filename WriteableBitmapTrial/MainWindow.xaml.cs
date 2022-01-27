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

        private MapViewMode _mapViewMode = MapViewMode.Split;
        private MapBinMode _mapBinMode = MapBinMode.SBin;
        private MapRtDataMode _mapRtDataMode = MapRtDataMode.OverWrite;

        private Dictionary<ushort, Color> _sBinColors= new Dictionary<ushort, Color>();
        private Dictionary<ushort, Color> _hBinColors = new Dictionary<ushort, Color>();

        private Dictionary<ushort, int> _sBinDieCnt = new Dictionary<ushort, int>();
        private Dictionary<ushort, int> _hBinDieCnt = new Dictionary<ushort, int>();
        private Dictionary<short?, Dictionary<ushort, int>> _perWaferSBinDieCnt = new Dictionary<short?, Dictionary<ushort, int>>();
        private Dictionary<short?, Dictionary<ushort, int>> _perWaferHBinDieCnt = new Dictionary<short?, Dictionary<ushort, int>>();

        private int _totalDieCnt = 0;
        private Dictionary<short?, int> _perWaferTotalDieCnt = new Dictionary<short?, int>();

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

            UpdateBinInfo();
        }

        private void MapBaseControl_CordChanged(int x, int y, Color color) {
            if(x==int.MinValue || y == int.MinValue) {
                infoBlock.Text = "";
                infoBlock.Visibility = Visibility.Hidden;
            } else {
                string append="";
                if(_mapViewMode != MapViewMode.Stack) {
                    if(_mapBinMode == MapBinMode.HBin) {
                        var bin = _hBinColors.FirstOrDefault(a => a.Value == color ).Key;
                        append = $"HBIN {bin}";
                    } else {
                        var bin = _sBinColors.FirstOrDefault(a => a.Value == color).Key;
                        append = $"SBIN {bin}";
                    }
                } 

                infoBlock.Text = $"X:{x} Y:{y}\n{append}";
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
                mapBaseControls[i].CordChanged -= MapBaseControl_CordChanged;
                viewGrid.Children.Add(mapBaseControls[i]);
                Grid.SetRow(mapBaseControls[i], i / splitColCnt);
                Grid.SetColumn(mapBaseControls[i], i % splitColCnt);
            }

            UpdateBinInfo();
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
            //SwitchSplitView(1, _mapControlList.Values.ToList());
            //SwitchSingleView(_mapControlStack);
        }

        private void UpdateBinInfo() {

            binInfo.Items.Clear();

            if (_mapBinMode == MapBinMode.HBin) {
                foreach(var b in _hBinColors) {
                    TextBox textBlock = new TextBox();
                    textBlock.Text = $"BIN:{b.Key} Qty:{_hBinDieCnt[b.Key]} Per:{_hBinDieCnt[b.Key] * 100.0 / _totalDieCnt : f3}%";
                    textBlock.BorderBrush = new SolidColorBrush(b.Value);
                    textBlock.BorderThickness = new Thickness(5);
                    textBlock.Width = binInfo.ActualWidth;
                    binInfo.Items.Add(textBlock);
                }
            } else {
                foreach (var b in _sBinColors) {
                    TextBox textBlock = new TextBox();
                    textBlock.Text = $"BIN:{b.Key, -5} {(_sBinDieCnt[b.Key] * 100.0 / _totalDieCnt).ToString("f3")+ "%",-7} {_sBinDieCnt[b.Key],-10} ";
                    textBlock.BorderBrush = new SolidColorBrush(b.Value);
                    textBlock.BorderThickness = new Thickness(5);
                    //textBlock.Width = binInfo.ActualWidth;
                    textBlock.IsReadOnly = true;
                    binInfo.Items.Add(textBlock);
                }
            }


        }

        private void UpdateData() {
            _logList.Clear();
            _sBinColors.Clear();
            _hBinColors.Clear();
            _sBinDieCnt.Clear();
            _hBinDieCnt.Clear();
            _perWaferTotalDieCnt.Clear();
            _totalDieCnt = 0;

            _xCnt = _waferData.XUbound - _waferData.XLbound + 1;
            _yCnt = _waferData.YUbound - _waferData.YLbound + 1;

            bool hbFlg = false;
            bool sbFlg = false;
            if(_waferData.HBinInfo != null) {
                int i = 0;
                foreach (var hb in _waferData.HBinInfo) {
                    _hBinDieCnt.Add(hb.Key, 0);
                    if (hb.Value.Item2.Contains("P")) {
                        _hBinColors.Add(hb.Key, BinColor.GetPassBinColor());
                    } else {
                        _hBinColors.Add(hb.Key, BinColor.GetFailBinColor(i++));
                    }
                }
                hbFlg = true;
            }
            if (_waferData.SBinInfo != null) {
                int i = 0;
                foreach (var sb in _waferData.SBinInfo) {
                    _sBinDieCnt.Add(sb.Key, 0);
                    if (sb.Value.Item2.Contains("P")) {
                        _sBinColors.Add(sb.Key, BinColor.GetPassBinColor());
                    } else {
                        _sBinColors.Add(sb.Key, BinColor.GetFailBinColor(i++));
                    }
                }
                sbFlg = true;
            }

            int fsbCnt = 0;
            int fhbCnt = 0;
            foreach (var die in _waferData.DieInfoList) {
                if (!sbFlg && !_sBinColors.ContainsKey(die.SBin)) {
                    if (die.PassOrFail) {
                        _sBinColors.Add(die.SBin, BinColor.GetPassBinColor());
                    } else { 
                        _sBinColors.Add(die.SBin, BinColor.GetFailBinColor(fsbCnt++));
                    }
                    _sBinDieCnt.Add(die.SBin, 0);
                }
                if (!hbFlg && !_hBinColors.ContainsKey(die.HBin)) { 
                    if (die.PassOrFail) {
                        _hBinColors.Add(die.HBin, BinColor.GetPassBinColor());
                    } else {
                        _hBinColors.Add(die.HBin, BinColor.GetFailBinColor(fhbCnt++));
                    }
                    _hBinDieCnt.Add(die.HBin, 0);
                } 


                _totalDieCnt++;
                _sBinDieCnt[die.SBin]++;
                _hBinDieCnt[die.HBin]++;

                if (!_sBinMaps.ContainsKey(die.WaferId)) {
                    _sBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _hBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _freshSBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _freshHBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _containRtFlg.Add(die.WaferId, false);
                    _perWaferTotalDieCnt.Add(die.WaferId, 0);
                }

                _perWaferTotalDieCnt[die.WaferId]++;

                //ignore the unreasonable point
                if (die.X > _waferData.XUbound || die.X < _waferData.XLbound || die.Y > _waferData.YUbound || die.Y < _waferData.XLbound) {
                    _logList.Add($"Cord X:{die.X} Y:{die.Y} out of wafer");
                    continue;
                }
                
                if (_sBinMaps[die.WaferId][die.X, die.Y] == new Color()) {
                    _sBinMaps[die.WaferId][die.X, die.Y] = _sBinColors[die.SBin];
                    _hBinMaps[die.WaferId][die.X, die.Y] = _hBinColors[die.HBin];
                    _freshSBinMaps[die.WaferId][die.X, die.Y] = _sBinColors[die.SBin];
                    _freshHBinMaps[die.WaferId][die.X, die.Y] = _hBinColors[die.HBin];
                } else {
                    _containRtFlg[die.WaferId] = true;
                    _freshSBinMaps[die.WaferId][die.X, die.Y] = _sBinColors[die.SBin];
                    _freshHBinMaps[die.WaferId][die.X, die.Y] = _hBinColors[die.HBin];
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
