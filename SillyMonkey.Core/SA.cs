using Microsoft.Win32;
using SillyMonkey.Core.Properties;
using System;
using System.Windows.Media;

namespace SillyMonkey.Core {

    public static class SA {

        private static Color[] ColorList = {
                Color.FromRgb(0, 0, 255),
                Color.FromRgb(255, 182, 193),
                
                Color.FromRgb(166, 86, 40),
                Color.FromRgb(153, 153, 153),

                //Color.FromRgb(220, 20, 60),
                //Color.FromRgb(255, 20, 147),

                Color.FromRgb(255, 0, 255),
                Color.FromRgb(148, 0, 211),
                Color.FromRgb(72, 61, 139),
                Color.FromRgb(100, 149, 237),
                Color.FromRgb(70, 130, 180),
                Color.FromRgb(95, 158, 160),
                Color.FromRgb(0, 255, 255),
                Color.FromRgb(47, 79, 79),
                Color.FromRgb(46, 139, 87),
                Color.FromRgb(85, 107, 47),
                Color.FromRgb(255, 255, 0),
                Color.FromRgb(255, 165, 0)
        };

        public static void Init() {
            DataContainer.ParseConfig.UpdateCconfig(SA.UidMode == UidType.TestNumberAndTestName);
        }
        
        public static void ApplyAndSave() {
            Properties.Settings.Default.Save();
        }

        public static UidType UidMode { 
            get { return (UidType)Enum.Parse(typeof(UidType), Settings.Default.UidMode); }
            set { Settings.Default.UidMode = value.ToString(); }
        }

        public static ChartAxisType HistogramChartAxis {
            get { return (ChartAxisType)Enum.Parse(typeof(ChartAxisType), Settings.Default.HistogramChartAxis); }
            set { Settings.Default.HistogramChartAxis = value.ToString(); }
        }

        public static SigmaRangeType HistogramChartAxisSigmaRange {
            get { return (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), Settings.Default.HistogramChartAxisSigmaRange); }
            set { Settings.Default.HistogramChartAxisSigmaRange = value.ToString(); }
        }

        public static SigmaRangeType HistogramOutlierFilterRange {
            get { return (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), Settings.Default.HistogramOutlierFilterRange); }
            set { Settings.Default.HistogramOutlierFilterRange = value.ToString(); }
        }

        public static bool HistogramEnableOutlierFilter {
            get { return Settings.Default.HistogramEnableOutlierFilter; }
            set { Settings.Default.HistogramEnableOutlierFilter = value; }
        }

        public static bool HistogramEnableLimitLine {
            get { return Settings.Default.HistogramEnableLimitLine; }
            set { Settings.Default.HistogramEnableLimitLine = value; }
        }

        public static bool HistogramEnableSigma6Line {
            get { return Settings.Default.HistogramEnableSigma6Line; }
            set { Settings.Default.HistogramEnableSigma6Line = value; }
        }

        public static bool HistogramEnableSigma3Line {
            get { return Settings.Default.HistogramEnableSigma3Line; }
            set { Settings.Default.HistogramEnableSigma3Line = value; }
        }

        public static bool HistogramEnableMinMaxLine {
            get { return Settings.Default.HistogramEnableMinMaxLine; }
            set { Settings.Default.HistogramEnableMinMaxLine = value; }
        }

        public static bool HistogramEnableMeanLine {
            get { return Settings.Default.HistogramEnableMeanLine; }
            set { Settings.Default.HistogramEnableMeanLine = value; }
        }

        public static bool HistogramEnableMedianLine {
            get { return Settings.Default.HistogramEnableMedianLine; }
            set { Settings.Default.HistogramEnableMedianLine = value; }
        }

        public static ChartAxisType TrendChartAxis {
            get { return (ChartAxisType)Enum.Parse(typeof(ChartAxisType), Settings.Default.TrendChartAxis); }
            set { Settings.Default.TrendChartAxis = value.ToString(); }
        }

        public static SigmaRangeType TrendChartAxisSigmaRange {
            get { return (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), Settings.Default.TrendChartAxisSigmaRange); }
            set { Settings.Default.TrendChartAxisSigmaRange = value.ToString(); }
        }

        public static SigmaRangeType TrendOutlierFilterRange {
            get { return (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), Settings.Default.TrendOutlierFilterRange); }
            set { Settings.Default.TrendOutlierFilterRange = value.ToString(); }
        }

        public static bool TrendEnableOutlierFilter {
            get { return Settings.Default.TrendEnableOutlierFilter; }
            set { Settings.Default.TrendEnableOutlierFilter = value; }
        }

        public static bool TrendEnableLimitLine {
            get { return Settings.Default.TrendEnableLimitLine; }
            set { Settings.Default.TrendEnableLimitLine = value; }
        }

        public static bool TrendEnableSigma6Line {
            get { return Settings.Default.TrendEnableSigma6Line; }
            set { Settings.Default.TrendEnableSigma6Line = value; }
        }

        public static bool TrendEnableSigma3Line {
            get { return Settings.Default.TrendEnableSigma3Line; }
            set { Settings.Default.TrendEnableSigma3Line = value; }
        }

        public static bool TrendEnableMinMaxLine {
            get { return Settings.Default.TrendEnableMinMaxLine; }
            set { Settings.Default.TrendEnableMinMaxLine = value; }
        }

        public static bool TrendEnableMeanLine {
            get { return Settings.Default.TrendEnableMeanLine; }
            set { Settings.Default.TrendEnableMeanLine = value; }
        }

        public static bool TrendEnableMedianLine {
            get { return Settings.Default.TrendEnableMedianLine; }
            set { Settings.Default.TrendEnableMedianLine = value; }
        }


        public static ChartAxisType CorrHistogramChartAxis {
            get { return (ChartAxisType)Enum.Parse(typeof(ChartAxisType), Settings.Default.CorrHistogramChartAxis); }
            set { Settings.Default.CorrHistogramChartAxis = value.ToString(); }
        }

        public static SigmaRangeType CorrHistogramOutlierFilterRange {
            get { return (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), Settings.Default.CorrHistogramOutlierFilterRange); }
            set { Settings.Default.CorrHistogramOutlierFilterRange = value.ToString(); }
        }

        public static bool CorrHistogramEnableOutlierFilter {
            get { return Settings.Default.CorrHistogramEnableOutlierFilter; }
            set { Settings.Default.CorrHistogramEnableOutlierFilter = value; }
        }

        public static bool CorrHistogramEnableLimitLine {
            get { return Settings.Default.CorrHistogramEnableLimitLine; }
            set { Settings.Default.CorrHistogramEnableLimitLine = value; }
        }

        public static bool CorrHistogramEnableSigmaLine {
            get { return Settings.Default.CorrHistogramEnableSigmaLine; }
            set { Settings.Default.CorrHistogramEnableSigmaLine = value; }
        }

        public static bool CorrHistogramEnableMinMaxLine {
            get { return Settings.Default.CorrHistogramEnableMinMaxLine; }
            set { Settings.Default.CorrHistogramEnableMinMaxLine = value; }
        }

        public static SigmaRangeType ItemCorrOutlierFilterRange {
            get { return (SigmaRangeType)Enum.Parse(typeof(SigmaRangeType), Settings.Default.ItemCorrOutlierFilterRange); }
            set { Settings.Default.ItemCorrOutlierFilterRange = value.ToString(); }
        }

        public static bool ItemCorrEnableOutlierFilter {
            get { return Settings.Default.ItemCorrEnableOutlierFilter; }
            set { Settings.Default.ItemCorrEnableOutlierFilter = value; }
        }

        public static Color GetColor(int idx) {
            if (idx >= 16) idx = 15;
            return ColorList[idx];
        }

        public static Color GetHistogramOutlierColor() {
            //return Color.FromRgb(239,0,0);
            return Properties.Settings.Default.HistogramOutlierColor;
        }

        private static bool WriteToReg(string keyName, object val) {

            var regSubKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\SillyMonkeyStdfAnalyzer");
            if (regSubKey is null) {
                return false;
            } else {
                try {
                    regSubKey.SetValue(keyName, val);
                }
                catch {
                    return false;
                }
            }

            return true;
        }

        private static bool ReadFromReg(string keyName, out object val) {
            var regSubKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\SillyMonkeyStdfAnalyzer");
            if (regSubKey is null) {
                val = null;
                return false;
            } else {
                try {
                    val = regSubKey.GetValue(keyName).ToString();
                }
                catch {
                    val = null;
                    return false;
                }
            }

            return true;
        }


        /// <summary>
		/// 设置文件默认打开程序 前提是程序支持参数启动打开文件
		/// 特殊说明:txt后缀比较特殊,还需要从注册表修改userchoie的键值才行
		/// </summary>
		/// <param name="fileExtension">文件拓展名 示例:'.slnc'</param>
		/// <param name="appPath">默认程序绝对路径 示例:'c:\\test.exe'</param>
		/// <param name="fileIconPath">文件默认图标绝对路径 示例:'c:\\test.ico'</param>
		public static void SetFileOpenApp(string fileExtension, string appPath, string fileIconPath) {
            //slnc示例 注册表中tree node path
            //|-.slnc				默认		"slncfile"
            //|--slncfile
            //|---DefaultIcon		默认		"fileIconPath"			默认图标
            //|----shell
            //|-----open
            //|------command		默认		"fileExtension \"%1\""	默认打开程序路径
            var fileExtensionKey = Registry.ClassesRoot.OpenSubKey(fileExtension);
            if (fileExtensionKey != null)
                Registry.ClassesRoot.DeleteSubKeyTree(fileExtension, false);
            fileExtensionKey = Registry.ClassesRoot.CreateSubKey(fileExtension);
            using (fileExtensionKey) {
                var fileKeyName = $"{fileExtension.Substring(1)}file";
                fileExtensionKey.SetValue(null, fileKeyName, RegistryValueKind.String);
                using (var fileKey = fileExtensionKey.CreateSubKey(fileKeyName)) {
                    using (var defaultIcon = fileKey.CreateSubKey("DefaultIcon")) {
                        defaultIcon.SetValue(null, fileIconPath);
                    }
                    using (var shell = fileKey.CreateSubKey("Shell")) {
                        using (var open = shell.CreateSubKey("Open")) {
                            using (var command = open.CreateSubKey("Command")) {
                                command.SetValue(null, $"{appPath} \"%1\"");
                            }
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Create an associaten for a file extension in the windows registry
        /// CreateAssociation(@"vendor.application",".tmf","Tool file",@"C:\Windows\SYSWOW64\notepad.exe",@"%SystemRoot%\SYSWOW64\notepad.exe,0");
        /// </summary>
        /// <param name="ProgID">e.g. vendor.application</param>
        /// <param name="extension">e.g. .tmf</param>
        /// <param name="description">e.g. Tool file</param>
        /// <param name="application">e.g.  @"C:\Windows\SYSWOW64\notepad.exe"</param>
        /// <param name="icon">@"%SystemRoot%\SYSWOW64\notepad.exe,0"</param>
        /// <param name="hive">e.g. The user-specific settings have priority over the computer settings. KeyHive.LocalMachine  need admin rights</param>
        public static void CreateAssociation(string ProgID, string extension, string description, string application, string icon/*, KeyHiveSmall hive = KeyHiveSmall.CurrentUser*/) {
            RegistryKey selectedKey = null;

            //switch (hive) {
            //    case KeyHiveSmall.ClassesRoot:
            //        Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(extension).SetValue("", ProgID);
            //        selectedKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(ProgID);
            //        break;

            //    case KeyHiveSmall.CurrentUser:
                    Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + extension).SetValue("", ProgID);
                    selectedKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + ProgID);
            //        break;

            //    case KeyHiveSmall.LocalMachine:
            //        Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Classes\" + extension).SetValue("", ProgID);
            //        selectedKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Classes\" + ProgID);
            //        break;
            //}

            if (selectedKey != null) {
                if (description != null) {
                    selectedKey.SetValue("", description);
                }
                if (icon != null) {
                    selectedKey.CreateSubKey("DefaultIcon").SetValue("", icon, RegistryValueKind.ExpandString);
                    selectedKey.CreateSubKey(@"Shell\Open").SetValue("icon", icon, RegistryValueKind.ExpandString);
                }
                if (application != null) {
                    selectedKey.CreateSubKey(@"Shell\Open\command").SetValue("", "\"" + application + "\"" + " \"%1\"", RegistryValueKind.ExpandString);
                }
            }
            selectedKey.Flush();
            selectedKey.Close();
        }
        /// <summary>
        /// Creates a association for current running executable
        /// </summary>
        /// <param name="extension">e.g. .tmf</param>
        /// <param name="hive">e.g. KeyHive.LocalMachine need admin rights</param>
        /// <param name="description">e.g. Tool file. Displayed in explorer</param>
        public static void SelfCreateAssociation(string extension/*, KeyHiveSmall hive = KeyHiveSmall.CurrentUser*/, string description = "") {
            //string ProgID = System.Reflection.Assembly.GetExecutingAssembly().EntryPoint.DeclaringType.FullName;
            //string FileLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string ProgID = "SA_StdfAnalyzer";
            string FileLocation = System.AppDomain.CurrentDomain.BaseDirectory + "\\SA_StdfAnalyzer.exe";

            CreateAssociation(ProgID, extension, description, FileLocation, FileLocation/* + ",0"*//*, hive*/);
        }

    }
}
