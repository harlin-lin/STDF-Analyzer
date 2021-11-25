using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public class PartStatistic {
        public Dictionary<byte, int> SiteCnt;
        public Dictionary<byte, Dictionary<UInt16, int>> HardBinBySite;
        public Dictionary<byte, Dictionary<UInt16, int>> SoftBinBySite;
        public Dictionary<byte, int> PassCntBySite;
        public Dictionary<byte, int> FailCntBySite;
        public Dictionary<byte, int> AbortCntBySite;
        public Dictionary<byte, int> NullCntBySite;
        public Dictionary<byte, int> FreshCntBySite;
        public Dictionary<byte, int> RtByIdCntBySite;
        public Dictionary<byte, int> RtByCordCntBySite;

        public int TotalCnt;
        public Dictionary<UInt16, int> HardBin;
        public Dictionary<UInt16, int> SoftBin;
        public int PassCnt;
        public int FailCnt;
        public int AbortCnt;
        public int NullCnt;
        public int FreshCnt;
        public int RtByIdCnt;
        public int RtByCordCnt;
        public UInt32 AverageTestTime;
        public UInt32 AverageTestTimePassOnly;

        public PartStatistic(IEnumerable<byte> sites) {
            SiteCnt = new Dictionary<byte, int>();
            HardBinBySite = new Dictionary<byte, Dictionary<ushort, int>>(); ;
            SoftBinBySite = new Dictionary<byte, Dictionary<ushort, int>>();
            PassCntBySite = new Dictionary<byte, int>();
            FailCntBySite = new Dictionary<byte, int>();
            AbortCntBySite = new Dictionary<byte, int>();
            NullCntBySite = new Dictionary<byte, int>();
            FreshCntBySite = new Dictionary<byte, int>();
            RtByIdCntBySite = new Dictionary<byte, int>();
            RtByCordCntBySite = new Dictionary<byte, int>();

            foreach (var s in sites) {
                SiteCnt.Add(s, 0);
                HardBinBySite.Add(s, new Dictionary<ushort, int>());
                SoftBinBySite.Add(s, new Dictionary<ushort, int>());
                PassCntBySite.Add(s, 0);
                FailCntBySite.Add(s, 0);
                AbortCntBySite.Add(s, 0);
                NullCntBySite.Add(s, 0);
                FreshCntBySite.Add(s, 0);
                RtByIdCntBySite.Add(s, 0);
                RtByCordCntBySite.Add(s, 0);
            }
        }

        public PartStatistic(PartStatistic copy) {
            SiteCnt = new Dictionary<byte, int>(copy.SiteCnt);
            HardBinBySite = new Dictionary<byte, Dictionary<ushort, int>>(copy.HardBinBySite); ;
            SoftBinBySite = new Dictionary<byte, Dictionary<ushort, int>>(copy.SoftBinBySite);
            PassCntBySite = new Dictionary<byte, int>(copy.PassCntBySite);
            FailCntBySite = new Dictionary<byte, int>(copy.FailCntBySite);
            AbortCntBySite = new Dictionary<byte, int>(copy.AbortCntBySite);
            NullCntBySite = new Dictionary<byte, int>(copy.NullCntBySite);
            FreshCntBySite = new Dictionary<byte, int>(copy.FreshCntBySite);
            RtByIdCntBySite = new Dictionary<byte, int>(copy.RtByIdCntBySite);
            RtByCordCntBySite = new Dictionary<byte, int>(copy.RtByCordCntBySite);

            TotalCnt = copy.TotalCnt;
            HardBin = new Dictionary<ushort, int>(copy.HardBin);
            SoftBin = new Dictionary<ushort, int>(copy.SoftBin);
            PassCnt = copy.PassCnt;
            FailCnt = copy.FailCnt;
            AbortCnt = copy.AbortCnt;
            NullCnt = copy.NullCnt;
            FreshCnt = copy.FreshCnt;
            RtByIdCnt = copy.RtByIdCnt;
            RtByCordCnt = copy.RtByCordCnt;
            AverageTestTime = copy.AverageTestTime;
            AverageTestTimePassOnly = copy.AverageTestTimePassOnly;
        }

    }

}
