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
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            this.DataContext = this;
            InitializeComponent();
            InitData();
        }




        private void InitData() {
            Random r = new Random();

            int i = 0;
            WaferColor = new Color[50, 70]; //y: row, x: col
            for (int x = 0; x < WaferColor.GetLength(0); x++) {
                for (int y = 0; y < WaferColor.GetLength(1); y++) {
                    WaferColor[x, y] = Color.FromRgb((byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255));
                    //WaferColor[x, y] = BinColor.GetFailBinColor(i++);
                }
            }
            //WaferColor[1, 2] = BinColor.GetFailBinColor(1);
            OnPropertyChanged("WaferColor");
        }

        void OnPropertyChanged(string propertyName) {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Color[,] WaferColor { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public bool IfContainRt { 
            get { return false; } 
        }

        private Dictionary<short?, Color[,]> _waferMaps= new Dictionary<short?, Color[,]>();
        private Dictionary<short?, Color[,]> _rtMaps = new Dictionary<short?, Color[,]>();

        private MapViewMode _mapViewMode = MapViewMode.Single;
        private MapBinMode _mapBinMode = MapBinMode.SBin;
        private MapRtDataMode _mapRtDataMode = MapRtDataMode.OverWrite;

        private Dictionary<ushort, int> _sBinIdx= null;
        private Dictionary<ushort, int> _hBinIdx = null;

        private List<string> _logList = new List<string>();

        private short? _selectedSingleWafer=null;

        private IWaferData _waferData;

        private int _xCnt, _yCnt;

        private void OnDataSourceChanged(IWaferData waferData) {
            _waferData = waferData;

            _logList.Clear();

            _hBinIdx = new Dictionary<ushort, int>();
            _sBinIdx = new Dictionary<ushort, int>();

            _xCnt = waferData.XUbound - waferData.XLbound + 1;
            _yCnt = waferData.YUbound -waferData.YLbound + 1;

            foreach (var die in waferData.DieInfoList) {
                if (!_hBinIdx.ContainsKey(die.SBin)) _hBinIdx.Add(die.SBin, _hBinIdx.Count - 1);
                if (!_sBinIdx.ContainsKey(die.HBin)) _sBinIdx.Add(die.HBin, _hBinIdx.Count - 1);

                if (!_waferMaps.ContainsKey(die.WaferId)) {
                    _waferMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                    _rtMaps.Add(die.WaferId, new Color[_xCnt, _yCnt]);
                }
                if (die.X> waferData.XUbound || die.X < waferData.XLbound || die.Y> waferData.YUbound || die.Y < waferData.XLbound) {
                    _logList.Add($"Cord X:{die.X} Y:{die.Y} out of wafer");
                    continue;
                }
                if(_mapRtDataMode == MapRtDataMode.OverWrite || _waferMaps[die.WaferId][die.X, die.Y] == new Color()) {
                    if(_mapBinMode == MapBinMode.HBin) {
                        _waferMaps[die.WaferId][die.X, die.Y] = BinColor.GetFailBinColor(_hBinIdx[die.HBin]);
                    } else {
                        _waferMaps[die.WaferId][die.X, die.Y] = BinColor.GetFailBinColor(_sBinIdx[die.SBin]);
                    }
                } else {
                    if (_mapBinMode == MapBinMode.HBin) {
                        _rtMaps[die.WaferId][die.X, die.Y] = BinColor.GetFailBinColor(_hBinIdx[die.HBin]);
                    } else {
                        _rtMaps[die.WaferId][die.X, die.Y] = BinColor.GetFailBinColor(_sBinIdx[die.SBin]);
                    }
                }

            }
            
        }



    }
}
