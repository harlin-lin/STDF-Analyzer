using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public enum DuplicateSelectMode {
        First,
        Last,
        Both
    }
    public enum DuplicateJudgeMode {
        ID,
        Cord
    }

    /// <summary>
    /// just for datahelper to define the filter content
    /// </summary>
    [Serializable]
    public class FilterSetup {
        public List<byte> maskSites { get; set; }
        public List<ushort> maskSoftBins { get; set; }
        public List<ushort> maskHardBins { get; set; }
        public List<string> maskChips { get; set; }
        public List<CordType> maskCords { get; set; }
        public bool ifmaskDuplicateChips { get; set; }
        public bool ifMaskOrEnableIds { get; set; }
        public bool ifMaskOrEnableCords { get; set; }
        public DuplicateSelectMode DuplicateSelectMode{ get; set; }

        public string Comment { get; private set; }
        /// <summary>
        /// true is Part ID, defult
        /// </summary>
        public DuplicateJudgeMode DuplicateJudgeMode { get; set; }

        public List<TestID> maskTestIDs { get; set; }

        public FilterSetup(string comment) {
            maskSites = new List<byte>();
            maskSoftBins = new List<ushort>();
            maskHardBins = new List<ushort>();
            maskTestIDs = new List<TestID>();
            maskChips = new List<string>();
            maskCords = new List<CordType>();
            ifmaskDuplicateChips = false;
            ifMaskOrEnableIds = false;
            ifMaskOrEnableCords = false;
            DuplicateSelectMode = DuplicateSelectMode.First;
            DuplicateJudgeMode = DuplicateJudgeMode.ID;

            Comment = comment;
        }

        public FilterSetup(List<byte> sites, byte enSite, string comment) {
            maskSites = new List<byte>();
            foreach (var v in sites) {
                if (v != enSite)
                    maskSites.Add(v);
            }

            maskSoftBins = new List<ushort>();
            maskHardBins = new List<ushort>();
            maskTestIDs = new List<TestID>();
            maskChips = new List<string>();
            maskCords = new List<CordType>();
            ifmaskDuplicateChips = false;
            ifMaskOrEnableIds = false;
            ifMaskOrEnableCords = false;
            DuplicateSelectMode = DuplicateSelectMode.First;
            DuplicateJudgeMode = DuplicateJudgeMode.ID;

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
            ifMaskOrEnableIds = false;
            ifMaskOrEnableCords = false;
            DuplicateSelectMode = DuplicateSelectMode.First;
            DuplicateJudgeMode = DuplicateJudgeMode.ID;
        }

    }
}
