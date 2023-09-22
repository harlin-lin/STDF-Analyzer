using SillyMonkey.ViewModels;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace SillyMonkey.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > 1) {
                Process process = RunningInstance();
                if (process != null) {
                    //MessageBox.Show("应用程序已经在运行中。。。");
                    HandleRunningInstance(process);

                    string s = commandLineArgs[1];
                    foreach (var v in commandLineArgs.Skip(2)) {
                        s += "\n";
                        s += v;
                    }
                    SendMessage(s);
                    System.Environment.Exit(1);
                }
            }

            InitializeComponent();

            ////读取配置文件
            //try {
            //    //设置位置、大小
            //    Rect restoreBounds = Properties.Settings.Default.MainRestoreBounds;
            //    this.WindowState = WindowState.Normal;
            //    this.Left = restoreBounds.Left;
            //    this.Top = restoreBounds.Top;
            //    this.Width = restoreBounds.Width;
            //    this.Height = restoreBounds.Height;
            //    //设置窗口状态
            //    this.WindowState = Properties.Settings.Default.MainWindowState;
            //} catch { }  

        }

        private void Window_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;

        }

        private Process RunningInstance() {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] Processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (Process process in Processes) {
                if (process.Id != currentProcess.Id) {
                    if (System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == currentProcess.MainModule.FileName) {
                        return process;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        /// 该函数设置由不同线程产生的窗口的显示状态  
        /// </summary>  
        /// <param name="hWnd">窗口句柄</param>  
        /// <param name="cmdShow">指定窗口如何显示。查看允许值列表，请查阅ShowWlndow函数的说明部分</param>  
        /// <returns>如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零</returns>  
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        /// <summary>  
        ///  该函数将创建指定窗口的线程设置到前台，并且激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。  
        ///  系统给创建前台窗口的线程分配的权限稍高于其他线程。   
        /// </summary>  
        /// <param name="hWnd">将被激活并被调入前台的窗口句柄</param>  
        /// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零</returns>  
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private const int SW_HIDE = 0;
        private const int SW_NORMAL = 1;
        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;

        private void HandleRunningInstance(Process instance) {
            ShowWindowAsync(instance.MainWindowHandle, SW_SHOW);   //显示  
            SetForegroundWindow(instance.MainWindowHandle); //当到最前端  
        }

        public struct COPYDATASTRUCT {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
        int hWnd, // handle to destination window
        int Msg, // message
        int wParam, // first message parameter
        ref COPYDATASTRUCT lParam // second message parameter
        );

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string lpClassName, string
                lpWindowName);

        public void SendMessage(string str) {
            try {
                int WINDOW_HANDLER = FindWindow(null, @"StdfAnalyzer");
                if (WINDOW_HANDLER != 0) {
                    COPYDATASTRUCT cds;
                    cds.dwData = (IntPtr)100;
                    cds.lpData = str;
                    cds.cbData = System.Text.Encoding.Default.GetByteCount(str)+1;//str.Length + 1;
                    SendMessage(WINDOW_HANDLER, 0x004A, 0, ref cds);

                }

            } catch (Exception ex) {

            }
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            // Handle messages...
            switch (msg) {
                case 0x004A:
                    var data = Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));
                    var files = ((COPYDATASTRUCT)data).lpData.Split('\n');

                    ((MainWindowViewModel)this.DataContext).LoadStdFiles(files);

                    handled = true;
                    break;
                default:
                    break;
            }

            return IntPtr.Zero;
        }

        private void StdfAnalyzer_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            //保存当前位置、大小和状态，到配置文件
            Properties.Settings.Default.MainRestoreBounds = this.RestoreBounds;
            Properties.Settings.Default.MainWindowState = this.WindowState;
            Properties.Settings.Default.Save();
        }
    }
}
