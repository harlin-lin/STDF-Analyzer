﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public enum DuplicateSelectMode {
        First,
        Last,
        OnlyDuplicated
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
        public List<byte> MaskSites { get; set; }
        public List<ushort> MaskSoftBins { get; set; }
        public List<ushort> MaskHardBins { get; set; }
        public List<string> MaskChips { get; set; }
        public List<Tuple<ushort, ushort>> MaskCords { get; set; }
        public bool IfmaskDuplicateChips { get; set; }
        public bool IfMaskOrEnableIds { get; set; }
        public bool IfMaskOrEnableCords { get; set; }
        public DuplicateSelectMode DuplicateSelectMode{ get; set; }

        public string Comment { get; private set; }
        /// <summary>
        /// true is Part ID, defult
        /// </summary>
        public DuplicateJudgeMode DuplicateJudgeMode { get; set; }

        public List<string> MaskTestIDs { get; set; }

        public FilterSetup(string comment) {
            MaskSites = new List<byte>();
            MaskSoftBins = new List<ushort>();
            MaskHardBins = new List<ushort>();
            MaskTestIDs = new List<string>();
            MaskChips = new List<string>();
            MaskCords = new List<Tuple<ushort, ushort>>();
            IfmaskDuplicateChips = false;
            IfMaskOrEnableIds = false;
            IfMaskOrEnableCords = false;
            DuplicateSelectMode = DuplicateSelectMode.First;
            DuplicateJudgeMode = DuplicateJudgeMode.ID;

            Comment = comment;
        }

        public FilterSetup(IEnumerable<byte> sites, byte enSite, string comment) {
            MaskSites = new List<byte>();
            foreach (var v in sites) {
                if (v != enSite)
                    MaskSites.Add(v);
            }

            MaskSoftBins = new List<ushort>();
            MaskHardBins = new List<ushort>();
            MaskTestIDs = new List<string>();
            MaskChips = new List<string>();
            MaskCords = new List<Tuple<ushort, ushort>>();
            IfmaskDuplicateChips = false;
            IfMaskOrEnableIds = false;
            IfMaskOrEnableCords = false;
            DuplicateSelectMode = DuplicateSelectMode.First;
            DuplicateJudgeMode = DuplicateJudgeMode.ID;

            Comment = comment;
        }


        public void EnableSingleSite(byte[] sites, byte enSite) {
            MaskSites.Clear();
            foreach(var v in sites) {
                if (v != enSite)
                    MaskSites.Add(v);
            }
        }

        public void ClearAllFilter() {
            MaskSites.Clear();
            MaskSoftBins.Clear();
            MaskHardBins.Clear();
            MaskTestIDs.Clear();
            MaskChips.Clear();
            MaskCords.Clear();
            IfmaskDuplicateChips = false;
            IfMaskOrEnableIds = false;
            IfMaskOrEnableCords = false;
            DuplicateSelectMode = DuplicateSelectMode.First;
            DuplicateJudgeMode = DuplicateJudgeMode.ID;
        }

    }
}
