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

namespace MapBase {
    /// <summary>
    /// WaferMapControl.xaml 的交互逻辑
    /// </summary>
    public partial class WaferMapControl : UserControl {
        public WaferMapControl() {
            InitializeComponent();
        }

        private Dictionary<short?, Color[,]> _sBinMaps = new Dictionary<short?, Color[,]>();
        private Dictionary<short?, Color[,]> _hBinMaps = new Dictionary<short?, Color[,]>();
        private Dictionary<short?, Color[,]> _freshSBinMaps = new Dictionary<short?, Color[,]>();
        private Dictionary<short?, Color[,]> _freshHBinMaps = new Dictionary<short?, Color[,]>();
        private Dictionary<short?, bool> _containRtFlg = new Dictionary<short?, bool>();

        private Dictionary<ushort, Color> _sBinColors = new Dictionary<ushort, Color>();
        private Dictionary<ushort, Color> _hBinColors = new Dictionary<ushort, Color>();

        private Dictionary<ushort, int> _sBinDieCnt = new Dictionary<ushort, int>();
        private Dictionary<ushort, int> _hBinDieCnt = new Dictionary<ushort, int>();
        //private Dictionary<short?, Dictionary<ushort, int>> _perWaferSBinDieCnt = new Dictionary<short?, Dictionary<ushort, int>>();
        //private Dictionary<short?, Dictionary<ushort, int>> _perWaferHBinDieCnt = new Dictionary<short?, Dictionary<ushort, int>>();

        private int _totalDieCnt = 0;
        private Dictionary<short?, int> _perWaferTotalDieCnt = new Dictionary<short?, int>();

        private List<string> _logList = new List<string>();


        private IWaferData _waferData;

        private int _xCnt, _yCnt;

        private Dictionary<short?, MapBaseControl> _mapControlList = new Dictionary<short?, MapBaseControl>();
        private MapBaseControl _mapControlStack;
        private MapBaseControl _selectedMap = null;

        private void MapBaseControl_CordChanged(int x, int y, Color color) {
            if (x == int.MinValue || y == int.MinValue) {
                infoBlock.Text = "";
                infoBlock.Visibility = Visibility.Hidden;
            } else {
                string append = "";
                if (ViewMode != MapViewMode.Stack) {
                    if (BinMode == MapBinMode.HBin) {
                        var bin = _hBinColors.FirstOrDefault(a => a.Value == color).Key;
                        append = $"HBIN {bin}";

                        if (_waferData.HBinInfo != null) {
                            var binName = _waferData.HBinInfo[bin].Item2;
                            if (binName.Length > 0) {
                                append += $" {binName}";
                            }
                        }
                    } else {
                        var bin = _sBinColors.FirstOrDefault(a => a.Value == color).Key;
                        append = $"SBIN {bin}";

                        if (_waferData.SBinInfo != null) {
                            var binName = _waferData.SBinInfo[bin].Item2;
                            if (binName.Length > 0) {
                                append += $" {binName}";
                            }
                        }
                    }
                }

                infoBlock.Text = $"XY[{x + _waferData.YLbound},{y + _waferData.YLbound}]\n{append}";
                infoBlock.Visibility = Visibility.Visible;
                var pt = Mouse.GetPosition(viewGrid);
                infoBlock.Margin = new Thickness {
                    Left = pt.X + 12,
                    Top = pt.Y
                };
            }
        }

        private void SwitchSingleView() {
            if (_selectedMap is null) return;

            viewGrid.Children.Clear();
            viewGrid.RowDefinitions.Clear();
            viewGrid.ColumnDefinitions.Clear();
            if(_selectedMap == _mapControlStack) {
                _selectedMap.CordChanged -= MapBaseControl_CordChanged;
            } else {
                _selectedMap.CordChanged += MapBaseControl_CordChanged;
            }
            viewGrid.Children.Add(_selectedMap);

            UpdateBinInfo();
        }

        private void SwitchSplitView() {
            int splitColCnt = 3;

            viewGrid.Children.Clear();
            viewGrid.RowDefinitions.Clear();
            viewGrid.ColumnDefinitions.Clear();
            viewGrid.ShowGridLines = true;

            int rowCnt = _mapControlList.Count / splitColCnt + (_mapControlList.Count % splitColCnt == 0 ? 0 : 1);

            for (int i = 0; i < splitColCnt; i++) {
                viewGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < rowCnt; i++) {
                viewGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < _mapControlList.Count; i++) {
                _mapControlList.ElementAt(i).Value.CordChanged -= MapBaseControl_CordChanged;
                _mapControlList.ElementAt(i).Value.EnableDrag = false;
                _mapControlList.ElementAt(i).Value.EnableZoom = false;

                viewGrid.Children.Add(_mapControlList.ElementAt(i).Value);
                Grid.SetRow(_mapControlList.ElementAt(i).Value, i / splitColCnt);
                Grid.SetColumn(_mapControlList.ElementAt(i).Value, i % splitColCnt);
            }

            UpdateBinInfo();
        }



        private void UpdateView() {
            if (_sBinMaps.Count == 0) return;

            _selectedMap = null; 

            Dictionary<short?, Color[,]> maps;

            if (BinMode == MapBinMode.HBin) {
                if (RtDataMode == MapRtDataMode.FirstOnly) {
                    maps = _freshHBinMaps;
                } else {
                    maps = _hBinMaps;
                }
            } else {
                if (RtDataMode == MapRtDataMode.FirstOnly) {
                    maps = _freshSBinMaps;
                } else {
                    maps = _sBinMaps;
                }
            }

            //gen stack map
            Color[,] stack = new Color[_xCnt, _yCnt];
            int?[,] stackCnt = new int?[_xCnt, _yCnt];

            foreach (var wafer in maps) {
                for (int x = 0; x < _xCnt; x++) {
                    for (int y = 0; y < _yCnt; y++) {
                        if (wafer.Value[x, y] != BinColor.GetPassBinColor() && wafer.Value[x, y] != new Color()) {
                            if (stackCnt[x, y] is null) stackCnt[x, y] = 0;
                            stackCnt[x, y]++;
                        } else if (wafer.Value[x, y] == BinColor.GetPassBinColor()) {
                            if (stackCnt[x, y] is null) stackCnt[x, y] = 0;
                        }
                    }
                }
            }
            for (int x = 0; x < _xCnt; x++) {
                for (int y = 0; y < _yCnt; y++) {
                    if (stackCnt[x, y].HasValue) {
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

            UpdateViewMode();
        }

        private void UpdateBinInfo() {

            binInfo.Children.Clear();

            if (BinMode == MapBinMode.HBin) {
                foreach (var b in _hBinColors) {
                    TextBox textBlock = new TextBox();
                    textBlock.Text = $"BIN{b.Key,-2} {(_hBinDieCnt[b.Key] * 100.0 / _totalDieCnt).ToString("f2") + "%",-6} {_hBinDieCnt[b.Key],-7}";
                    textBlock.Background = new SolidColorBrush(b.Value);
                    textBlock.BorderThickness = new Thickness(0);
                    textBlock.Height = 18;
                    textBlock.IsReadOnly = true;
                    textBlock.Foreground = new SolidColorBrush(Colors.White);

                    if (_waferData.HBinInfo != null) {
                        textBlock.ToolTip = _waferData.HBinInfo[b.Key].Item2;
                    }
                    binInfo.Children.Add(textBlock);
                }
            } else {
                foreach (var b in _sBinColors) {
                    TextBox textBlock = new TextBox();
                    textBlock.Text = $"BIN{b.Key,-5} {(_sBinDieCnt[b.Key] * 100.0 / _totalDieCnt).ToString("f2") + "%",-6} {_sBinDieCnt[b.Key],-7}";
                    textBlock.Background = new SolidColorBrush(b.Value);
                    textBlock.BorderThickness = new Thickness(0);
                    textBlock.Height = 18;
                    textBlock.IsReadOnly = true;
                    textBlock.Foreground = new SolidColorBrush(Colors.White);

                    if (_waferData.SBinInfo != null) {
                        textBlock.ToolTip = _waferData.SBinInfo[b.Key].Item2;
                    }
                    binInfo.Children.Add(textBlock);
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
            
            _sBinMaps.Clear();
            _hBinMaps.Clear();
            
            _freshSBinMaps.Clear();
            _freshHBinMaps.Clear();
            _containRtFlg.Clear();

            _xCnt = _waferData.XUbound - _waferData.XLbound + 1;
            _yCnt = _waferData.YUbound - _waferData.YLbound + 1;

            bool hbFlg = false;
            bool sbFlg = false;
            if (_waferData.HBinInfo != null) {
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
                if (die.X > _waferData.XUbound || die.X < _waferData.XLbound || die.Y > _waferData.YUbound || die.Y < _waferData.YLbound) {
                    _logList.Add($"Cord X:{die.X} Y:{die.Y} out of wafer");
                    continue;
                }

                if (_sBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] == new Color()) {
                    _sBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _sBinColors[die.SBin];
                    _hBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _hBinColors[die.HBin];
                    _freshSBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _sBinColors[die.SBin];
                    _freshHBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _hBinColors[die.HBin];
                } else {
                    _containRtFlg[die.WaferId] = true;
                    _freshSBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _sBinColors[die.SBin];
                    _freshHBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _hBinColors[die.HBin];
                }

            }

            UpdateView();
        }


        private void OnDataSourceChanged(IWaferData waferData) {
            _waferData = waferData;

            if (_waferData.DieInfoList is null || _waferData.DieInfoList.Count() == 0 || _waferData.XUbound==0 || _waferData.YUbound==0) return;

            UpdateData();
            //Task.Run(() => { UpdateData(); });
        }

        private void UpdateBinMode() {
            UpdateView();
        }

        private void UpdateViewMode() {
            switch (ViewMode) {
                case MapViewMode.Split:
                    SwitchSplitView();
                    break;
                case MapViewMode.Single:
                    if ((_selectedMap is null || _selectedMap == _mapControlStack) && _mapControlList!=null && _mapControlList.Count>0) _selectedMap = _mapControlList.ElementAt(0).Value;
                    SwitchSingleView();
                    break;
                case MapViewMode.Stack:
                    _selectedMap = _mapControlStack;
                    SwitchSingleView();
                    break;
                default: break;
            }
        }

        private void UpdateRtDataMode() {
            UpdateView();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnMouseDoubleClick(e);

            if (ViewMode != MapViewMode.Split) return;
            foreach(var v in _mapControlList) {
                if (v.Value.IsFocused) {
                    _selectedMap = v.Value;
                    SwitchSingleView();
                }
            }
        }

    }
}
