﻿using System;
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
    /// <summary>
    /// MapBaseControl.xaml 的交互逻辑
    /// </summary>
    public partial class MapBaseControl : UserControl {
        public MapBaseControl() {
            InitializeComponent();
        }

        private WriteableBitmap _drawBuffer;
        private double _waferDiameter = DEFAULT_WAFER_DIAMETER;
        //private double _initWaferDiameter = DEFAULT_WAFER_DIAMETER;
        private Color[,] _waferColor;

        private int _zoomShiftX = -5;
        private int _zoomShiftY = -5;

        const int MIN_DIE_PIXELS = 2;
        const double DEFAULT_WAFER_DIAMETER = 300;

        public Tuple<int, int> FocusedCord { get; private set; }
        public event CordChangedHandler CordChanged;

        private bool ValidateWaferDiameter() {
            var longerRank = _waferColor.GetLength(0) > _waferColor.GetLength(1) ? _waferColor.GetLength(0) : _waferColor.GetLength(1);
            if (_waferDiameter / longerRank < MIN_DIE_PIXELS) {
                _waferDiameter = longerRank * MIN_DIE_PIXELS;
                //_initWaferDiameter = _waferDiameter;
                return false;
            }
            return true;
        }
        private void ZoomMap(int stepPixel) {
            var tgtDiameter = _waferDiameter + stepPixel;

            int width = (int)imageGrid.ActualWidth - 0;
            int height = (int)imageGrid.ActualHeight - 0;

            int pixelWidth = (int)Math.Ceiling(width * DpiXKoef);
            int pixelHeight = (int)Math.Ceiling(height * DpiYKoef);

            var min = GetMinDiameter(pixelWidth, pixelHeight);
            if (tgtDiameter < min) {
                tgtDiameter = min;
            }

            if(tgtDiameter != _waferDiameter) {
                _waferDiameter = tgtDiameter;
                Render();
            }
        }

        private int _colLen;
        private int _rowLen;
        private int _dieWidth;
        private int _dieHeight;

        private void Render() {
            if (_drawBuffer is null || _waferColor is null) return;
            using (_drawBuffer.GetBitmapContext()) {
                _drawBuffer.Clear(Colors.White);
                _colLen = _waferColor.GetLength(0);
                _rowLen = _waferColor.GetLength(1);

                ValidateWaferDiameter();

                _dieWidth = (int)Math.Floor(_waferDiameter / _colLen);
                _dieHeight = (int)Math.Floor(_waferDiameter / _rowLen);


                int x = 0, y = 0;
                for (int cPixel = 0; cPixel < _drawBuffer.PixelWidth + _zoomShiftX; cPixel += _dieWidth) {
                    for (int rPixel = 0; rPixel < _drawBuffer.PixelHeight + _zoomShiftY; rPixel += _dieHeight) {
                        if(_dieWidth > 4 && _dieHeight > 4) {
                            _drawBuffer.DrawRectangle(cPixel - _zoomShiftX, rPixel - _zoomShiftY, cPixel + _dieWidth - _zoomShiftX, rPixel + _dieHeight - _zoomShiftY, Colors.White);
                            _drawBuffer.FillRectangle(cPixel + 1 - _zoomShiftX, rPixel + 1 - _zoomShiftY, cPixel + _dieWidth - _zoomShiftX, rPixel + _dieHeight - _zoomShiftY, _waferColor[x, y]);
                        } else {
                            _drawBuffer.FillRectangle(cPixel - _zoomShiftX, rPixel - _zoomShiftY, cPixel + _dieWidth - _zoomShiftX, rPixel + _dieHeight - _zoomShiftY, _waferColor[x, y]);
                        }
                        if ((++y) >= _rowLen) break;
                    }
                    if ((++x) >= _colLen) break;
                    y = 0;
                }

            }
        }

        private int GetMinDiameter(int ctrWidth, int ctrHeight) {
            if (ctrWidth > DEFAULT_WAFER_DIAMETER && ctrHeight > DEFAULT_WAFER_DIAMETER) {
                return (ctrWidth > ctrHeight ? ctrHeight : ctrWidth);
            } else {
                return (int)DEFAULT_WAFER_DIAMETER;
            }
        }

        private void InitWaferDiameter(int ctrWidth, int ctrHeight) {
            if (ctrWidth > DEFAULT_WAFER_DIAMETER && ctrHeight > DEFAULT_WAFER_DIAMETER) {
                _waferDiameter = ctrWidth > ctrHeight ? ctrHeight : ctrWidth;
                //_initWaferDiameter = _waferDiameter;
            } else {
                _waferDiameter = DEFAULT_WAFER_DIAMETER;
            }
            _zoomShiftX = -5;
            _zoomShiftY = -5;
        }

        private void imageGridResized(object sender, SizeChangedEventArgs e) {
            //bool renderNeeded = false;
            int width = (int)imageGrid.ActualWidth - 0;
            int height = (int)imageGrid.ActualHeight - 0;
            if (width > 0 && height > 0) {
                //To avoid flicker (blank image) while resizing, crop the current buffer and set it as the image source instead of using a new one.
                //This will be shown during the refresh.
                int pixelWidth = (int)Math.Ceiling(width * DpiXKoef);
                int pixelHeight = (int)Math.Ceiling(height * DpiYKoef);
                InitWaferDiameter(pixelWidth, pixelHeight);

                if (_drawBuffer == null) {
                    _drawBuffer = BitmapFactory.New(pixelWidth, pixelHeight);

                    Render();

                    //renderNeeded = true;
                } else if (_drawBuffer.Width >= width && _drawBuffer.Height >= height) {
                    var oldBuffer = _drawBuffer;
                    _drawBuffer = oldBuffer.Crop(0, 0, pixelWidth, pixelHeight);
                    Render();

                    //The unmanaged memory when crating new WritableBitmaps doesn't reliably garbage collect and can still cause out of memory exceptions
                    //Profiling revealed handles on the object that aren't able to be collected.
                    //Freezing the object removes all handles and should help in garbage collection.
                    oldBuffer.Freeze();
                } else {
                    var oldBuffer = _drawBuffer;
                    _drawBuffer = oldBuffer.Resize(pixelWidth, pixelHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
                    //renderNeeded = true;
                    Render();

                    //The unmanaged memory when crating new WritableBitmaps doesn't reliably garbage collect and can still cause out of memory exceptions
                    //Profiling revealed handles on the object that aren't able to be collected.
                    //Freezing the object removes all handles and should help in garbage collection.
                    oldBuffer.Freeze();
                }
            } else {
                _drawBuffer = null;
            }
            image.Source = _drawBuffer;
            image.Margin = new Thickness(0);
            image.Width = Math.Max(0, width);
            image.Height = Math.Max(0, height);

            //if (renderNeeded) {
            //    Render();
            //}
        }

        private void imageMouseWheel(object sender, MouseWheelEventArgs e) {
            if (EnableZoom) {
                Point imagePoint = e.GetPosition(imageGrid);
                ZoomMap(e.Delta);
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnMouseDoubleClick(e);
            if (EnableZoom || EnableDrag) {
                int width = (int)imageGrid.ActualWidth - 0;
                int height = (int)imageGrid.ActualHeight - 0;

                int pixelWidth = (int)Math.Ceiling(width * DpiXKoef);
                int pixelHeight = (int)Math.Ceiling(height * DpiYKoef);
                InitWaferDiameter(pixelWidth, pixelHeight);

                Render();
            }
        }

        private bool _dragFlg = false;
        private Point _dragStartPoint;
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e) {
            //Debug.WriteLine("----->OnMouseMove Enter");
            base.OnMouseMove(e);
            var pt = e.GetPosition(image);

            if (_dragFlg) {
                _zoomShiftX = (int)Math.Floor(_dragStartPoint.X- pt.X);
                _zoomShiftY = (int)Math.Floor(_dragStartPoint.Y - pt.Y);

                Render();
            } else {
                var actPt = new Point(pt.X + _zoomShiftX, pt.Y + _zoomShiftY);
                int x, y;

                x = (int)Math.Floor(actPt.X / _dieWidth);
                y = (int)Math.Floor(actPt.Y / _dieHeight);

                if (x>=0 && x<_colLen && y>=0 && y<_rowLen && _waferColor[x, y] != new Color()) {
                    //_lastCordX = x;
                    //_lastCordY = y;
                    FocusedCord = new Tuple<int, int>(x, y);
                    CordChanged?.Invoke(x, y, _waferColor[x,y]);
                } else {
                    FocusedCord = new Tuple<int, int>(int.MinValue, int.MinValue);
                    CordChanged?.Invoke(int.MinValue, int.MinValue, Colors.White);
                }
            }

        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (EnableDrag) {
                Cursor = Cursors.SizeAll;

                _dragFlg = true;
                _dragStartPoint = e.GetPosition(image);
                _dragStartPoint.X -= -_zoomShiftX;
                _dragStartPoint.Y -= -_zoomShiftY;
            }
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);

            //if (EnableDrag) {
                Cursor = Cursors.Arrow;
                _dragFlg = false;
            //}

        }




        private static double? _dpiXKoef;

        public static double DpiXKoef {
            get {
                if (_dpiXKoef == null) {
                    using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) {
                        _dpiXKoef = graphics.DpiX / 96.0;
                    }
                }
                return _dpiXKoef ?? 1;
            }
        }

        private static double? _dpiYKoef;

        public static double DpiYKoef {
            get {
                if (_dpiYKoef == null) {
                    using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) {
                        _dpiYKoef = graphics.DpiY / 96.0;
                    }
                }
                return _dpiYKoef ?? 1;

                //return Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.WorkArea.Width;
                //WantGlobalTransformMatrix();
                //if (_globalTransformPatrix.HasValue) return _globalTransformPatrix.Value.M22;
                //return 1;
            }
        }

    }
}
