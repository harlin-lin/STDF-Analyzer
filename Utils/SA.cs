using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;

namespace Utils {

    public static class SA {
        public static UserSetup SaUserSetup;
        static string SetupPath = @"\SaUserSetup.json";

        private static bool _ifCmpTextInUid = false;

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
            SetupPath = System.Environment.CurrentDirectory + SetupPath;
            if (File.Exists(SetupPath)) {
                try {
                    SaUserSetup = JsonConvert.DeserializeObject<UserSetup>(File.ReadAllText(SetupPath));
                    return;
                } catch {
                    SaUserSetup = new UserSetup();
                }
            }
            SaUserSetup = new UserSetup();
            SaveSetup();
        }
        
        private static void SaveSetup() {
            return;
            try {
                string output = JsonConvert.SerializeObject(SaUserSetup);
                File.WriteAllText(SetupPath, output);
            } catch {
                MessageBox.Show("Setup save failed");
            }
        }

        public static void ApplyAndSave() {
            SaveSetup();
        }

        public static bool IfCmpTextInUid { 
            get {
                return _ifCmpTextInUid;
            }
            set {
                _ifCmpTextInUid = value;
            }
        }

        public static Color GetColor(int idx) {
            if (idx >= 16) idx = 15;
            return ColorList[idx];
        }

        public static Color GetHistogramOutlierColor() {
            return Color.FromRgb(239,0,0);
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
                    using (var shell = fileKey.CreateSubKey("shell")) {
                        using (var open = shell.CreateSubKey("open")) {
                            using (var command = open.CreateSubKey("command")) {
                                command.SetValue(null, $"{appPath} \"%1\"");
                            }
                        }
                    }
                }
            }
        }


    }
}
