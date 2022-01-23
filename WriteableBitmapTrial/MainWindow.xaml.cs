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

        private Color[,] _waferMap;
        private int _xCnt, _yCnt;
        

        private void OnDataSourceChanged(IWaferData waferData) {

        }

    }
}
