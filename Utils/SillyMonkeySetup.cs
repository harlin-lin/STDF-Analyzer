using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;


namespace Utils {
    public static class SillyMonkeySetup {
        private static bool _ifCmpTextInUid = false;

        private static Color[] ColorList = {
                Color.FromRgb(0, 0, 255),
                Color.FromRgb(255, 182, 193),
                Color.FromRgb(220, 20, 60),
                Color.FromRgb(255, 20, 147),
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
            object val = null;
            try {
                if(ReadFromReg("IfCmpTextInUid", out val)) {
                    if(!(val is null)) {
                        _ifCmpTextInUid = (bool)val;
                    }
                }
            }
            catch {
                System.Diagnostics.Debug.Print("Init Setup Fail");
            }
        }

        public static void ApplyAndSave() {
            try {
                if(!WriteToReg("IfCmpTextInUid", _ifCmpTextInUid)) {
                    System.Diagnostics.Debug.Print("Save Setup Fail");
                }
            }
            catch {
                System.Diagnostics.Debug.Print("Init Setup Fail");
            }

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

    }
}
