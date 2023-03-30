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
    public delegate void CordChangedHandler(int x, int y, Color color);
    public delegate void MapSelectedHandler(MapBaseControl map);
    /// <summary>
    /// MapBaseControl.xaml 的交互逻辑
    /// </summary>
    public partial class MapBaseControl : UserControl {
        public MapBaseControl() {
            InitializeComponent();
        }

        private BitmapSource _drawBuffer;
        private WriteableBitmap _rawBuffer;

        private Color[,] _waferColor;

        private int _rawWidth, _rawHeight;
        private int _zoomDiameter;
        private int _zoomShiftX = 0;
        private int _zoomShiftY = 0;

        const int MIN_WAFER_DIAMETER = 50;
        const int DEFAULT_WAFER_DIAMETER = 1000;

        public Tuple<int, int> FocusedCord { get; private set; }
        public event CordChangedHandler CordChanged;
        public event MapSelectedHandler MapSelected;


        private void ZoomMap(int stepPixel) {
            var tgtDiameter = _zoomDiameter + stepPixel;

            if (tgtDiameter < MIN_WAFER_DIAMETER) {
                _zoomDiameter = MIN_WAFER_DIAMETER;
            } else {
                _zoomDiameter = tgtDiameter;
            }

            UpdateMap();
        }

        private int _colLen;
        private int _rowLen;
        //private int _dieWidth;
        //private int _dieHeight;



        private void imageGridResized(object sender, SizeChangedEventArgs e) {
            var width = (int)imageGrid.ActualWidth - 0;
            var height = (int)imageGrid.ActualHeight - 0;
            int pixelWidth = (int)Math.Ceiling(width * DpiXKoef);
            int pixelHeight = (int)Math.Ceiling(height * DpiYKoef);

            var diameter = pixelWidth > pixelHeight ? pixelHeight : pixelWidth;
            _zoomDiameter = diameter;

            _zoomShiftX = 0;
            _zoomShiftY = 0;

            image.Margin = new Thickness(0);
            image.Width = Math.Max(0, width);
            image.Height = Math.Max(0, height);

            UpdateMap();
        }

        private void imageMouseWheel(object sender, MouseWheelEventArgs e) {
            if (EnableZoom) {
                ZoomMap(e.Delta);
            }
        }

        private bool ModeChangeFlg=false;

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnMouseDoubleClick(e);
            if (EnableZoom) {
                var width = (int)imageGrid.ActualWidth - 0;
                var height = (int)imageGrid.ActualHeight - 0;
                int pixelWidth = (int)Math.Ceiling(width * DpiXKoef);
                int pixelHeight = (int)Math.Ceiling(height * DpiYKoef);

                var diameter = pixelWidth > pixelHeight ? pixelHeight : pixelWidth;
                _zoomDiameter = diameter;

                _zoomShiftX = 0;
                _zoomShiftY = 0;

                UpdateMap();
            } else {
                MapSelected?.Invoke(this);
                ModeChangeFlg = true;
            }
        }

        private bool _dragFlg = false;
        private Point _dragStartPoint;
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e) {
            //Debug.WriteLine("----->OnMouseMove Enter");
            base.OnMouseMove(e);

            if (_dragFlg) {
                var pt = e.GetPosition(image);

                pt.X *= DpiXKoef;
                pt.Y *= DpiYKoef;

                _zoomShiftX = (int)Math.Floor(pt.X - _dragStartPoint.X);
                _zoomShiftY = (int)Math.Floor(pt.Y - _dragStartPoint.Y);

                if (_zoomShiftX < 0) {
                    var w = (int)(_rawWidth * _zoomDiameter * 1.0 / DEFAULT_WAFER_DIAMETER);
                    if(w < -_zoomShiftX) _zoomShiftX = -w + 20;
                }
                if (_zoomShiftX > 0 && (_drawBuffer.PixelWidth < _zoomShiftX)) {
                    _zoomShiftX = (_drawBuffer.PixelWidth - 20);
                }

                if (_zoomShiftY < 0) {
                    var h = (int)(_rawHeight * _zoomDiameter * 1.0 / DEFAULT_WAFER_DIAMETER);
                    if (h < -_zoomShiftY) _zoomShiftY = -h + 20;
                }
                if (_zoomShiftY > 0 && (_drawBuffer.PixelHeight < _zoomShiftY)) {
                    _zoomShiftY = (_drawBuffer.PixelHeight - 20);
                }

                //System.Diagnostics.Debug.WriteLine($"MOV SX:{_zoomShiftX} SY:{_zoomShiftY}");

                UpdateMap();

            } else {
                var pt = e.GetPosition(image);

                pt.X *= DpiXKoef;
                pt.Y *= DpiYKoef;

                var actPt = new Point(pt.X - _zoomShiftX, pt.Y - _zoomShiftY);
                int x, y;
                
                var scale = _zoomDiameter * 1.0 / DEFAULT_WAFER_DIAMETER;

                x = (int)Math.Floor(actPt.X * _colLen / (scale * _rawWidth));
                y = (int)Math.Floor(actPt.Y * _rowLen / (scale * _rawHeight));

                if (x >= 0 && x < _colLen && y >= 0 && y < _rowLen && _waferColor[x, y] != new Color()) {
                    //_lastCordX = x;
                    //_lastCordY = y;
                    FocusedCord = new Tuple<int, int>(x, y);
                    CordChanged?.Invoke(x, y, _waferColor[x, y]);
                } else {
                    FocusedCord = new Tuple<int, int>(int.MinValue, int.MinValue);
                    CordChanged?.Invoke(int.MinValue, int.MinValue, Colors.White);
                }
            }

        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (EnableZoom && !ModeChangeFlg) {
                Cursor = Cursors.SizeAll;

                _dragStartPoint = e.GetPosition(image);
                _dragFlg = true;
                _dragStartPoint.X -= _zoomShiftX;
                _dragStartPoint.Y -= _zoomShiftY;

                CordChanged?.Invoke(int.MinValue, int.MinValue, Colors.White);

            } else {
                ModeChangeFlg = false;
            }
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);

            if (_dragFlg) {
                Cursor = Cursors.Arrow;
                _dragFlg = false;
            }

        }

        private void image_MouseLeave(object sender, MouseEventArgs e) {
            if (_dragFlg) {
                //var pt = e.GetPosition(image);
                ////System.Diagnostics.Debug.WriteLine($"LEAVE SX:{pt.X} SY:{pt.Y}");
                //if (pt.X < 0 || pt.X > image.ActualWidth || pt.Y<0 || pt.Y > image.ActualHeight) {
                    Cursor = Cursors.Arrow;
                    _dragFlg = false;
                //}
            }
        }


        private void CreateRawBuffer() {

            int diameter = DEFAULT_WAFER_DIAMETER;

            _rawBuffer = BitmapFactory.New(diameter, diameter);

            using (_rawBuffer.GetBitmapContext()) {
                _rawBuffer.Clear(Colors.White);
                _colLen = _waferColor.GetLength(0);
                _rowLen = _waferColor.GetLength(1);

                var dieWidth = (int)Math.Floor(diameter * 1.0 / _colLen);
                var dieHeight = (int)Math.Floor(diameter * 1.0 / _rowLen);

                _rawWidth = dieWidth * _colLen;
                _rawHeight = dieHeight * _rowLen;

                _zoomShiftX = 0;
                _zoomShiftY = 0;

                Color die;
                int x = 0, y = 0;
                for (int cPixel = 0; cPixel < _rawBuffer.PixelWidth + _zoomShiftX; cPixel += dieWidth) {
                    for (int rPixel = 0; rPixel < _rawBuffer.PixelHeight + _zoomShiftY; rPixel += dieHeight) {
                        die = _waferColor[x, y];
                        if (die == new Color()) die = Colors.White;

                        _rawBuffer.DrawRectangle(cPixel, rPixel, cPixel + dieWidth, rPixel + dieHeight, Colors.White);
                        _rawBuffer.FillRectangle(cPixel + 1, rPixel + 1, cPixel + dieWidth, rPixel + dieHeight, die);
                        if ((++y) >= _rowLen) break;
                    }
                    if ((++x) >= _colLen) break;
                    y = 0;
                }

            }

            waferIdTag.Text = WaferNo;
        }

        private void UpdateMap() {
            var buf = PicResize(_rawBuffer, _zoomDiameter);
            _drawBuffer = PicMove(buf, _zoomShiftX, _zoomShiftY);

            image.Source = _drawBuffer;
            //System.Diagnostics.Debug.WriteLine($"Update: X:{_zoomShiftX} Y:{_zoomShiftY}");

            buf.Freeze();
        }


        private BitmapSource PicResize(WriteableBitmap origin, int diameter) {
            var scale = (double)diameter / DEFAULT_WAFER_DIAMETER;

            TransformedBitmap tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = origin;
            ScaleTransform st = new ScaleTransform(scale, scale);
            tb.Transform = st;
            tb.EndInit();
            return tb;

            //var oldBuf = origin;
            //origin = oldBuf.Resize(diameter, diameter, WriteableBitmapExtensions.Interpolation.Bilinear);
            //return origin;
        }

        private BitmapSource PicMove(BitmapSource origin, int x, int y) {
            var width = (int)imageGrid.ActualWidth - 0;
            var height = (int)imageGrid.ActualHeight - 0;
            int pixelWidth = (int)Math.Ceiling(width * DpiXKoef);
            int pixelHeight = (int)Math.Ceiling(height * DpiYKoef);

            double destx, desty, destw, desth;
            double srcx, srcy;

            if (x < 0) {
                destx = 0;
                destw = origin.PixelWidth + x;
                srcx = -x;
            } else {
                destx = x;
                destw = origin.PixelWidth;
                srcx = 0;
            }
            if (y < 0) {
                desty = 0;
                desth = origin.PixelHeight + y;
                srcy = -y;
            } else {
                desty = y;
                desth = origin.PixelHeight;
                srcy = 0;
            }

            if (destw < 0) destw = 0;
            if (desth < 0) desth = 0;

            var srcRect = new Rect(srcx,srcy, destw, desth);
            var destRect = new Rect(destx, desty, destw, desth);

            var bs = BitmapFactory.New(pixelWidth, pixelHeight);
            using (bs.GetBitmapContext()) {
                bs.Clear();
                bs.Blit(destRect, new WriteableBitmap(origin), srcRect, WriteableBitmapExtensions.BlendMode.None);
            }

            return bs;
        }


        public BitmapSource GetBitmapSource() {
            return _rawBuffer.Clone();
        }

        private static double? _dpiXKoef;

        public static double DpiXKoef {
            get {
                //if (_dpiXKoef == null) {
                //    using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) {
                //        _dpiXKoef = graphics.DpiX / 96.0;
                //    }
                //}
                //return _dpiXKoef ?? 1;
                return 1;
            }
        }

        private static double? _dpiYKoef;

        public static double DpiYKoef {
            get {
                //if (_dpiYKoef == null) {
                //    using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) {
                //        _dpiYKoef = graphics.DpiY / 96.0;
                //    }
                //}
                //return _dpiYKoef ?? 1;
                return 1;
            }
        }

    }
}
