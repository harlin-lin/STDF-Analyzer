using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public enum RetestSelectMode{
        SelectFirst,
        SelectLast
    }
    public enum DuplicateSelectMode {
        SelectFirst,
        SelectLast
    }


    public class Filter {
        public List<byte> maskSites { get; set; }
        public List<ushort> maskSoftBins { get; set; }
        public List<ushort> maskHardBins { get; set; }
        public List<TestNumber> maskTestNumbers { get; set; }
        public List<int> maskChips { get; set; }
        public List<CordType> maskCords { get; set; }
        public bool ifmaskRtChips { get; set; }
        public bool ifmaskDuplicateChips { get; set; }
        public RetestSelectMode RetestSelectMode { get; set; }
        public DuplicateSelectMode DuplicateSelectMode{ get; set; }


        public Filter() {
            maskSites = new List<byte>();
            maskSoftBins = new List<ushort>();
            maskHardBins = new List<ushort>();
            maskTestNumbers = new List<TestNumber>();
            maskChips = new List<int>();
            maskCords = new List<CordType>();
            ifmaskRtChips = false;
            ifmaskDuplicateChips = false;
            RetestSelectMode = RetestSelectMode.SelectFirst;
            DuplicateSelectMode = DuplicateSelectMode.SelectFirst;
        }

        public void ClearAllFilter() {
            maskSites.Clear();
            maskSoftBins.Clear();
            maskHardBins.Clear();
            maskTestNumbers.Clear();
            maskChips.Clear();
            maskCords.Clear();
            ifmaskRtChips = false;
            ifmaskDuplicateChips = false;
            RetestSelectMode = RetestSelectMode.SelectFirst;
            DuplicateSelectMode = DuplicateSelectMode.SelectFirst;
        }

    }
}
