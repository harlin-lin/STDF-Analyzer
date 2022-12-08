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

        private Dictionary<Color, int> _colorDieCnt = new Dictionary<Color, int>();
        //private Dictionary<ushort, int> _sBinDieCnt = new Dictionary<ushort, int>();
        //private Dictionary<ushort, int> _hBinDieCnt = new Dictionary<ushort, int>();
        //private int _totalDieCnt = 0;
        //private Dictionary<short?, int> _perWaferTotalDieCnt = new Dictionary<short?, int>();

        private List<string> _logList = new List<string>();


        private IWaferData _waferData;

        private int _xCnt, _yCnt;

        private Dictionary<short?, MapBaseControl> _mapControlList = new Dictionary<short?, MapBaseControl>();
        private MapBaseControl _selectedMap = null;

        private Color NullColor = new Color();

        private void MapBaseControl_CordChanged(int x, int y, Color color) {
            if (x == int.MinValue || y == int.MinValue) {
                infoBlock.Text = "";
                infoBlock.Visibility = Visibility.Hidden;
            } else {
                string append = "";
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
            viewGrid.ShowGridLines = false;
            scrollBar.VerticalScrollBarVisibility =  ScrollBarVisibility.Disabled;

            _selectedMap.Width = viewGrid.ActualWidth;
            _selectedMap.Height = viewGrid.ActualHeight;
            _selectedMap.CordChanged += MapBaseControl_CordChanged;
            _selectedMap.EnableZoom = true;
            viewGrid.Children.Add(_selectedMap);
            
            _colorDieCnt.Clear();

            foreach (var die in _selectedMap.MapDataSource) {
                if (die != NullColor) {
                    if (_colorDieCnt.ContainsKey(die)) {
                        _colorDieCnt[die]++;
                    } else {
                        _colorDieCnt.Add(die, 1);
                    }
                }
            }

            UpdateBinInfo();
        }

        private void SwitchSplitView() {
            int splitColCnt = 2;

            infoBlock.Visibility = Visibility.Hidden;

            viewGrid.Children.Clear();
            viewGrid.RowDefinitions.Clear();
            viewGrid.ColumnDefinitions.Clear();
            viewGrid.ShowGridLines = true;
            scrollBar.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            int rowCnt = _mapControlList.Count / splitColCnt + (_mapControlList.Count % splitColCnt == 0 ? 0 : 1);

            for (int i = 0; i < splitColCnt; i++) {
                viewGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < rowCnt; i++) {
                viewGrid.RowDefinitions.Add(new RowDefinition());
            }

            var width = viewGrid.ActualWidth / splitColCnt;

            _colorDieCnt.Clear();

            for (int i = 0; i < _mapControlList.Count; i++) {
                _mapControlList.ElementAt(i).Value.CordChanged -= MapBaseControl_CordChanged;
                _mapControlList.ElementAt(i).Value.MapSelected += SplitView_MapSelected;
                _mapControlList.ElementAt(i).Value.EnableZoom = false;

                _mapControlList.ElementAt(i).Value.Width = width;
                _mapControlList.ElementAt(i).Value.Height = width;

                viewGrid.Children.Add(_mapControlList.ElementAt(i).Value);
                Grid.SetRow(_mapControlList.ElementAt(i).Value, i / splitColCnt);
                Grid.SetColumn(_mapControlList.ElementAt(i).Value, i % splitColCnt);

                //calc the bin yield based on the map
                foreach(var die in _mapControlList.ElementAt(i).Value.MapDataSource) {
                    if(die != NullColor) {
                        if (_colorDieCnt.ContainsKey(die)) {
                            _colorDieCnt[die]++;
                        } else {
                            _colorDieCnt.Add(die, 1);
                        }
                    }
                }
            }

            UpdateBinInfo();
        }

        private void SplitView_MapSelected(MapBaseControl map) {
            _selectedMap = map;

            ViewMode = MapViewMode.Single;
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

            _mapControlList.Clear();
            foreach (var wafer in maps) {
                var map = new MapBaseControl();
                map.WaferNo = $"NO:{wafer.Key.ToString()}";
                map.MapDataSource = wafer.Value;
                _mapControlList.Add(wafer.Key, map);
            }

            UpdateViewMode();
        }

        private void UpdateBinInfo() {

            int totalCnt = 0;
            Dictionary<ushort, int> binCnt = new Dictionary<ushort, int>();
            foreach(var c in _colorDieCnt) {
                totalCnt += c.Value;
                if (BinMode == MapBinMode.HBin) {
                    var bin = _hBinColors.FirstOrDefault(a => a.Value == c.Key).Key;
                    binCnt.Add(bin, c.Value);
                } else {
                    var bin = _sBinColors.FirstOrDefault(a => a.Value == c.Key).Key;
                    binCnt.Add(bin, c.Value);
                }
            }

            textBlockDieCnt.Text = $"Total:{totalCnt}";

            binInfo.Children.Clear();
            var orderBincnt = binCnt.OrderBy(x=>x.Key);
            if (BinMode == MapBinMode.HBin) {
                foreach (var b in orderBincnt) {
                    TextBox textBlock = new TextBox();
                    textBlock.Text = $"BIN{b.Key,-2} {(b.Value * 100.0 / totalCnt).ToString("f2") + "%",-6} {b.Value,-7}";

                    textBlock.Background = new SolidColorBrush(_hBinColors[b.Key]);
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
                foreach (var b in orderBincnt) {
                    TextBox textBlock = new TextBox();
                    textBlock.Text = $"BIN{b.Key,-5} {(b.Value * 100.0 / totalCnt).ToString("f2") + "%",-6} {b.Value,-7}";
                    textBlock.Background = new SolidColorBrush(_sBinColors[b.Key]);
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
                }
                if (!hbFlg && !_hBinColors.ContainsKey(die.HBin)) {
                    if (die.PassOrFail) {
                        _hBinColors.Add(die.HBin, BinColor.GetPassBinColor());
                    } else {
                        _hBinColors.Add(die.HBin, BinColor.GetFailBinColor(fhbCnt++));
                    }
                }

                if (!_sBinMaps.ContainsKey(die.WaferId)) {
                    _sBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _hBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _freshSBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _freshHBinMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _containRtFlg.Add(die.WaferId, false);
                }

                //ignore the unreasonable point
                if (die.X > _waferData.XUbound || die.X < _waferData.XLbound || die.Y > _waferData.YUbound || die.Y < _waferData.YLbound) {
                    _logList.Add($"Cord X:{die.X} Y:{die.Y} out of wafer");
                    continue;
                }

                if (_sBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] == NullColor) {
                    _sBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _sBinColors[die.SBin];
                    _hBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _hBinColors[die.HBin];
                    _freshSBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _sBinColors[die.SBin];
                    _freshHBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _hBinColors[die.HBin];
                } else {
                    _containRtFlg[die.WaferId] = true;
                    _sBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _sBinColors[die.SBin];
                    _hBinMaps[die.WaferId][die.X - _waferData.XLbound, die.Y - _waferData.YLbound] = _hBinColors[die.HBin];
                }

            }

            UpdateView();
        }


        private void OnDataSourceChanged(IWaferData waferData) {
            _waferData = waferData;

            if (_waferData.DieInfoList is null || _waferData.DieInfoList.Count() == 0 || _waferData.XUbound==0 || _waferData.YUbound==0) return;

            UpdateData();
        }

        private void UpdateBinMode() {
            UpdateView();
        }

        private void UpdateViewMode() {
            if (_mapControlList is null || _mapControlList.Count == 0) return;
            switch (ViewMode) {
                case MapViewMode.Split:
                    SwitchSplitView();
                    break;
                case MapViewMode.Single:
                    if (_selectedMap is null) _selectedMap = _mapControlList.ElementAt(0).Value;
                    SwitchSingleView();
                    break;
                default: break;
            }
        }

        private void UpdateRtDataMode() {
            UpdateView();
        }

        private void viewGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (_mapControlList is null || _mapControlList.Count == 0) return;
            var width = viewGrid.ActualWidth / 2;

            if (ViewMode== MapViewMode.Split) {
                for (int i = 0; i < _mapControlList.Count; i++) {
                    _mapControlList.ElementAt(i).Value.Width = width;
                    _mapControlList.ElementAt(i).Value.Height = width;
                }

            } else {
                _selectedMap.Width = viewGrid.ActualWidth;
                _selectedMap.Height = viewGrid.ActualHeight;
            }
        }

        public BitmapSource GetWaferMap() {
            if (_selectedMap is null) return null;

            return _selectedMap.GetBitmapSource();
        }

    }
}
