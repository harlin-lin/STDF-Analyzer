using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public static class ParseConfig {

        public static bool IfCmpTextInUid=false;

        public static void UpdateCconfig(bool ifCmpTextInUid) {
            IfCmpTextInUid = ifCmpTextInUid;
        }
    }
}
