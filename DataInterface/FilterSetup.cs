using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public enum DuplicateSelectMode {
        SelectFirst,
        SelectLast
    }

    /// <summary>
    /// just for datahelper to define the filter content
    /// </summary>
    public class FilterSetup {
        public List<byte> maskSites { get; set; }
        public List<ushort> maskSoftBins { get; set; }
        public List<ushort> maskHardBins { get; set; }
        public List<int> maskChips { get; set; }
        public List<CordType> maskCords { get; set; }
        public bool ifmaskDuplicateChips { get; set; }
        public DuplicateSelectMode DuplicateSelectMode{ get; set; }

        public string Comment { get; private set; }
        /// <summary>
        /// true is Part ID, defult
        /// </summary>
        public bool DuplicateJudgeByIdOrCord { get; set; }

        public List<TestID> maskTestIDs { get; set; }

        public FilterSetup(string comment) {
            maskSites = new List<byte>();
            maskSoftBins = new List<ushort>();
            maskHardBins = new List<ushort>();
            maskTestIDs = new List<TestID>();
            maskChips = new List<int>();
            maskCords = new List<CordType>();
            ifmaskDuplicateChips = false;
            DuplicateSelectMode = DuplicateSelectMode.SelectFirst;
            DuplicateJudgeByIdOrCord = true;

            Comment = comment;
        }

        //public FilterSetup(FilterSetup f, string comment) {
        //    maskSites = new List<byte>(f.maskSites);
        //    maskSoftBins = new List<ushort>(f.maskSoftBins);
        //    maskHardBins = new List<ushort>(f.maskHardBins);
        //    maskTestIDs = new List<TestID>(f.maskTestIDs);
        //    maskChips = new List<int>(f.maskChips);
        //    maskCords = new List<CordType>(f.maskCords);
        //    ifmaskDuplicateChips = f.ifmaskDuplicateChips;
        //    DuplicateSelectMode = f.DuplicateSelectMode;
        //    DuplicateJudgeByIdOrCord = f.DuplicateJudgeByIdOrCord; 

        //    Comment = comment;
        //}

        public FilterSetup(List<byte> sites, byte enSite, string comment) {
            maskSites = new List<byte>();
            foreach (var v in sites) {
                if (v != enSite)
                    maskSites.Add(v);
            }

            maskSoftBins = new List<ushort>();
            maskHardBins = new List<ushort>();
            maskTestIDs = new List<TestID>();
            maskChips = new List<int>();
            maskCords = new List<CordType>();
            ifmaskDuplicateChips = false;
            DuplicateSelectMode = DuplicateSelectMode.SelectFirst;
            DuplicateJudgeByIdOrCord = true;

            Comment = comment;
        }


        public void EnableSingleSite(List<byte> sites, byte enSite) {
            maskSites.Clear();
            foreach(var v in sites) {
                if (v != enSite)
                    maskSites.Add(v);
            }
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
            DuplicateJudgeByIdOrCord = true;
        }

    }
}
