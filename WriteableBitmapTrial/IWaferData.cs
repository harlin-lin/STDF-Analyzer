using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteableBitmapTrial {
    public class DieInfo {
        public short X { get; }
        public short Y { get; }
        public ushort HBin { get; }
        public ushort SBin { get; }
        public byte Site { get; }
        public int Idx { get; }

        public DieInfo(short x, short y, ushort hbin, ushort sbin, byte site, int idx) {
            X = x;
            Y = y;
            HBin = hbin;
            SBin = sbin;
            Site = site;
            Idx = idx;
        }
    }

    public interface IWaferData {
        IEnumerable<DieInfo> dieInfoList { get; }
        Dictionary<ushort, string> HBinInfo { get; }
        Dictionary<ushort, string> SBinInfo { get; }

    }
}
