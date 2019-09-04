using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public enum DuplicateSelectMode {
        SelectFirst,
        SelectLast
    }


    public class Filter {
        public List<byte> maskSites { get; set; }
        public List<ushort> maskSoftBins { get; set; }
        public List<ushort> maskHardBins { get; set; }
        public List<int> maskChips { get; set; }
        public List<CordType> maskCords { get; set; }
        public bool ifmaskDuplicateChips { get; set; }
        public DuplicateSelectMode DuplicateSelectMode{ get; set; }

        public List<TestID> maskTestIDs { get; set; }

        public Filter() {
            maskSites = new List<byte>();
            maskSoftBins = new List<ushort>();
            maskHardBins = new List<ushort>();
            maskTestIDs = new List<TestID>();
            maskChips = new List<int>();
            maskCords = new List<CordType>();
            ifmaskDuplicateChips = false;
            DuplicateSelectMode = DuplicateSelectMode.SelectFirst;
        }

        public void ClearAllFilter() {
            maskSites.Clear();
            maskSoftBins.Clear();
            maskHardBins.Clear();
            maskTestIDs.Clear();
            maskChips.Clear();
            maskCords.Clear();
            ifmaskDuplicateChips = false;
            DuplicateSelectMode = DuplicateSelectMode.SelectFirst;
        }

    }
}
