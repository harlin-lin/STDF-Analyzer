using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProdLogAnalyzer
{
    /// <summary>
    /// 数据分析引导界面：选择 JSON 配置文件，后台异步执行分析，日志实时回显。
    /// </summary>
    public class MainForm : Form
    {
        private TextBox _txtConfigPath;
        private Button _btnBrowse;
        private Button _btnStart;
        private TextBox _txtLog;
        private ProgressBar _progressBar;
        private bool _isRunning = false;

        /// <summary>
        /// UI 设置存储路径（%APPDATA%\ProdLogAnalyzer\settings.json），用于记忆上次配置路径
        /// </summary>
        private static string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ProdLogAnalyzer", "settings.json");

        public MainForm()
        {
            BuildUi();
            LoadSettings();
        }

        // ==================== UI 构建 ====================

        private void BuildUi()
        {
            Text = "生产测试数据分析系统";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(860, 600);
            MinimumSize = new Size(760, 480);
            Font = new Font("Microsoft YaHei UI", 9F);

            // 配置文件选择区
            var grpConfig = new GroupBox
            {
                Text = "JSON 配置文件",
                Location = new Point(12, 12),
                Size = new Size(ClientSize.Width - 24, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _txtConfigPath = new TextBox
            {
                Location = new Point(15, 28),
                Size = new Size(grpConfig.Width - 130, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _btnBrowse = new Button
            {
                Text = "浏览...",
                Location = new Point(grpConfig.Width - 105, 27),
                Size = new Size(90, 27),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnBrowse.Click += OnBrowseClicked;

            grpConfig.Controls.Add(_txtConfigPath);
            grpConfig.Controls.Add(_btnBrowse);

            // 启动按钮 + 进度条
            _btnStart = new Button
            {
                Text = "开始分析",
                Location = new Point(12, 92),
                Size = new Size(120, 38),
                Font = new Font(Font, FontStyle.Bold)
            };
            _btnStart.Click += OnStartClicked;

            _progressBar = new ProgressBar
            {
                Location = new Point(142, 96),
                Size = new Size(ClientSize.Width - 154, 30),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // 日志显示区
            _txtLog = new TextBox
            {
                Location = new Point(12, 140),
                Size = new Size(ClientSize.Width - 24, ClientSize.Height - 152),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGray,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.Add(grpConfig);
            Controls.Add(_btnStart);
            Controls.Add(_progressBar);
            Controls.Add(_txtLog);
        }

        // ==================== 事件处理 ====================

        private void OnBrowseClicked(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog
            {
                Title = "选择 JSON 配置文件",
                Filter = "JSON 配置文件 (*.json)|*.json|所有文件 (*.*)|*.*"
            })
            {
                if (!string.IsNullOrEmpty(_txtConfigPath.Text) && File.Exists(_txtConfigPath.Text))
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(_txtConfigPath.Text);
                }

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _txtConfigPath.Text = dlg.FileName;
                }
            }
        }

        private async void OnStartClicked(object sender, EventArgs e)
        {
            var configPath = _txtConfigPath.Text.Trim();
            if (!File.Exists(configPath))
            {
                MessageBox.Show(this, "配置文件不存在，请重新选择。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_isRunning) return;

            // 进入后台执行状态（UI 保持响应），三个操作控件永久禁用
            _isRunning = true;
            _btnStart.Enabled = false;
            _btnBrowse.Enabled = false;
            _txtConfigPath.Enabled = false;
            _progressBar.Visible = true;
            _txtLog.Clear();

            // 记录本次路径，下次启动默认填充
            SaveSettings(configPath);

            var consoleOut = Console.Out;
            var consoleError = Console.Error;
            var writer = new UiConsoleWriter(AppendLog);

            try
            {
                // 将控制台输出重定向到 UI 日志框，后台线程执行分析，不阻塞 UI
                Console.SetOut(writer);
                Console.SetError(writer);

                await Task.Run(() => Program.RunAnalysis(configPath));
            }
            catch (Exception ex)
            {
                AppendLog($"✗ 分析失败: {ex.Message}");
                if (ex.InnerException != null)
                {
                    AppendLog($"  详情: {ex.InnerException.Message}");
                }
            }
            finally
            {
                Console.SetOut(consoleOut);
                Console.SetError(consoleError);

                // 分析完成，恢复前台；三个操作控件保持禁用（再次分析需重新启动程序）
                _isRunning = false;
                _progressBar.Visible = false;
                AppendLog(string.Empty);
                AppendLog("========== 分析完成 ==========");
                AppendLog("如需进行下一次分析，请重新启动程序。");
            }
        }

        // ==================== 设置存取 ====================

        private class UiSettings
        {
            public string LastConfigPath { get; set; }
        }

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return;

                var settings = JsonConvert.DeserializeObject<UiSettings>(
                    File.ReadAllText(SettingsPath, Encoding.UTF8));
                if (!string.IsNullOrEmpty(settings?.LastConfigPath))
                {
                    _txtConfigPath.Text = settings.LastConfigPath;
                }
            }
            catch
            {
                // 设置文件损坏时忽略，按默认空路径启动
            }
        }

        private void SaveSettings(string configPath)
        {
            try
            {
                var dir = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(
                    new UiSettings { LastConfigPath = configPath }, Formatting.Indented);
                File.WriteAllText(SettingsPath, json, Encoding.UTF8);
            }
            catch
            {
                // 无法写入设置时不影响主流程
            }
        }

        // ==================== 日志回显 ====================

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendLog), message);
                return;
            }

            _txtLog.AppendText(message + Environment.NewLine);
            _txtLog.SelectionStart = _txtLog.TextLength;
            _txtLog.ScrollToCaret();
        }
    }

    /// <summary>
    /// 将 Console 输出重定向到 UI 日志框的 TextWriter
    /// </summary>
    public class UiConsoleWriter : TextWriter
    {
        private readonly Action<string> _onWrite;

        public UiConsoleWriter(Action<string> onWrite)
        {
            _onWrite = onWrite;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(string value)
        {
            _onWrite(value ?? string.Empty);
        }

        public override void WriteLine(string value)
        {
            _onWrite(value ?? string.Empty);
        }

        public override void WriteLine()
        {
            _onWrite(string.Empty);
        }
    }
}
