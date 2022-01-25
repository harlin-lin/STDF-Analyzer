using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteableBitmapTrial {
    public class DieInfo {
        public short X { get; }
        public short Y { get; }
        public short? WaferId { get; }
        public ushort HBin { get; }
        public ushort SBin { get; }
        public byte Site { get; }
        public int Idx { get; }

        public DieInfo(short x, short y, short? waferId,ushort hbin, ushort sbin, byte site, int idx) {
            X = x;
            Y = y;
            WaferId = waferId;
            HBin = hbin;
            SBin = sbin;
            Site = site;
            Idx = idx;
        }
    }

    public enum MapViewMode{
        Single,
        Split,
        Stack
    }

    public enum MapBinMode {
        SBin,
        HBin
    }

    public enum MapRtDataMode {
        OverWrite,
        FirstOnly
    }

    public interface IWaferData {
        IEnumerable<DieInfo> DieInfoList { get; }
        Dictionary<ushort, string> HBinInfo { get; }
        Dictionary<ushort, string> SBinInfo { get; }
        short XUbound { get; }
        short YUbound { get; }
        short XLbound { get; }
        short YLbound { get; }
    }
}
