using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapBase {
    public class DieInfo {
        public short X { get; }
        public short Y { get; }
        public short? WaferId { get; }
        public ushort HBin { get; }
        public ushort SBin { get; }
        public bool PassOrFail { get; }
        public byte Site { get; }
        public int Idx { get; }

        public DieInfo(int idx, short x, short y, ushort hbin, ushort sbin, byte site, bool passOrFail, short? waferId) {
            X = x;
            Y = y;
            WaferId = waferId;
            HBin = hbin;
            SBin = sbin;
            Site = site;
            Idx = idx;
            PassOrFail = passOrFail;
        }
    }

    public enum MapViewMode{
        Single,
        Split
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
        Dictionary<ushort, Tuple<string, string>> HBinInfo { get; }
        Dictionary<ushort, Tuple<string, string>> SBinInfo { get; }
        short XUbound { get; }
        short YUbound { get; }
        short XLbound { get; }
        short YLbound { get; }
    }
}
