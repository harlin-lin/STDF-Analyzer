using DataContainer;
using FileReader;
using Newtonsoft.Json;
using SillyMonkey.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ProdLogAnalyzer
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        /// <summary>
        /// 程序入口。
        /// - 无参数：启动引导 UI（选择配置文件、后台执行分析）
        /// - 带参数：命令行模式，args[0] 为 JSON 配置文件路径（兼容原有用法）
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // ★ 必须在任何 SkiaSharp/ScottPlot 代码之前提取原生 DLL
            ExtractNativeDlls();

            if (args != null && args.Length > 0)
            {
                // WinExe 模式下默认无控制台：优先附加父进程控制台（如 cmd），失败则分配新控制台
                if (!AttachConsole(-1))
                {
                    AllocConsole();
                }
                RunAnalysis(args[0]);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// 执行数据分析主流程（供 UI 后台线程或命令行直接调用）
        /// </summary>
        public static void RunAnalysis(string configPath)
        {
            if (!File.Exists(configPath))
            {
                Console.Error.WriteLine($"Config file not found: {configPath}");
                return;
            }

            ProdLogConfiguration config;
            try
            {
                // 使用 Newtonsoft.Json 读取配置文件（支持可读性更好、兼容性更强）
                var json = File.ReadAllText(configPath, Encoding.UTF8);
                config = JsonConvert.DeserializeObject<ProdLogConfiguration>(json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to read config: {ex.Message}");
                return;
            }

            if (config?.DataFiles == null || config.DataFiles.Count == 0)
            {
                Console.WriteLine("No data files defined in configuration.");
                return;
            }

            List<SubData> subDataList = new List<SubData>();

            foreach (var df in config.DataFiles)
            {
                var path = df.Path;
                try
                {
                    Console.WriteLine($"Start loading: {path}");
                    if (!File.Exists(path))
                    {
                        Console.Error.WriteLine($"Data file not found: {path}");
                    }

                    if (StdDB.IfExsistFile(path))
                    {
                        var existing = StdDB.GetDataAcquire(path);
                        var existFilter = existing.CreateFilter();
                        Console.WriteLine($"Already loaded: {path} -> filter {existFilter}");
                    }

                    var da = StdDB.CreateSubContainer(path);
                    using (var reader = new StdReader(path, StdFileType.STD))
                    {
                        reader.ExtractStdf();
                    }

                    var filterId = da.CreateFilter();

                    if (df.Filter != null)
                    {
                        // 将外部 FilterSetup 应用到新建的 filter
                        try
                        {
                            da.UpdateFilter(filterId, df.Filter);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Failed to apply FilterSetup for {path}: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"Loaded {path} -> filter {filterId}");

                    subDataList.Add(new SubData(da.FilePath, filterId, da.GetFilterIndex(filterId)));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to load {path}: {ex.Message}");
                }
            }

            Console.WriteLine("Loading completed. Summary:");

            if (!StdDB.MergeSubData(config.Name, subDataList))
            {
                Console.Error.WriteLine("Failing at file mergering");
                return;
            }
            foreach (var df in config.DataFiles)
            {
                StdDB.RemoveFile(df.Path);
            }

            var dataAcquire = StdDB.GetDataAcquire(config.Name);

            int filterId_raw = dataAcquire.CreateFilter();
            int filterId_pass = dataAcquire.CreateFilter();
            var allBins = new List<ushort>(dataAcquire.GetHardBins());
            allBins.Remove((ushort)(config.PassBin));
            dataAcquire.UpdateFilter(filterId_pass, new FilterSetup("PassOnly") { MaskHardBins = allBins });

            DataParser.ParseDataFile(config, dataAcquire, filterId_raw, filterId_pass);
        }

        /// <summary>
        /// 从嵌入资源中提取原生 DLL（libSkiaSharp.dll、libHarfBuzzSharp.dll）到 EXE 所在目录。
        /// 必须在使用任何 SkiaSharp/ScottPlot API 之前调用，确保原生库就位。
        /// </summary>
        private static void ExtractNativeDlls()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var resourceNames = new[] { "libSkiaSharp.dll", "libHarfBuzzSharp.dll" };

            foreach (var name in resourceNames)
            {
                var destPath = Path.Combine(exeDir, name);

                // 如果目标文件已存在且大小匹配（幂等操作），跳过
                using (var stream = assembly.GetManifestResourceStream(name))
                {
                    if (stream == null) continue; // 资源不存在（开发调试时可能没有嵌入）

                    if (File.Exists(destPath) && new FileInfo(destPath).Length == stream.Length)
                        continue;

                    // 写入文件
                    var tempPath = destPath + ".tmp";
                    using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                    }

                    // 原子替换（避免写入中途文件被读取）
                    if (File.Exists(destPath))
                        File.Delete(destPath);
                    File.Move(tempPath, destPath);
                }
            }
        }
    }
}
