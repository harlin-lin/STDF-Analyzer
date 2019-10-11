using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse
{
    public class ChipSummary: IChipSummary {
        public int TotalCount { get; private set; }
        public int RetestCount { get; private set; }
        public int FreshCount { get; private set; }

        public int NullCount { get; private set; }
        public int AbortCount { get; private set; }
        public int PassCount { get; private set; }
        public int FailCount { get; private set; }

        private Dictionary<UInt16, int> _hardBins;
        private Dictionary<UInt16, int> _softBins;

        public Dictionary<UInt16, int> GetHardBins() {
            return new Dictionary<UInt16, int>(_hardBins);
        }
        public Dictionary<UInt16, int> GetSoftBins() {
            return new Dictionary<UInt16, int>(_softBins);
        }

        public ChipSummary() {
            _hardBins = new Dictionary<ushort, int>();
            _softBins = new Dictionary<ushort, int>();
        }

        [Obsolete]
        public void AddChipCount(DeviceType deviceType, ResultType resultType) {
            switch (deviceType) {
                case DeviceType.Fresh:
                    FreshCount++; break;
                case DeviceType.RT_ID:
                case DeviceType.RT_Cord:
                    RetestCount++;
                    break;
            }
            switch (resultType) {
                case ResultType.Pass:
                    PassCount++;
                    break;
                case ResultType.Fail:
                    FailCount++;
                    break;
                case ResultType.Abort:
                    AbortCount++;
                    break;
                case ResultType.Null:
                    NullCount++;
                    break;
            }
            TotalCount++;
        }

        [Obsolete]
        public void AddBinCount(UInt16 hardBin, UInt16 softBin) {
            if (_hardBins.ContainsKey(hardBin))
                _hardBins[hardBin]++;
            else
                _hardBins.Add(hardBin, 1);

            
            if (_softBins.ContainsKey(softBin))
                _softBins[softBin]++;
            else
                _softBins.Add(softBin, 1);

        }

        public void AddChip(IChipInfo chipInfo) {
            switch (chipInfo.ChipType) {
                case DeviceType.Fresh:
                    FreshCount++; break;
                case DeviceType.RT_ID:
                case DeviceType.RT_Cord:
                    RetestCount++;
                    break;
            }
            switch (chipInfo.Result) {
                case ResultType.Pass:
                    PassCount++;
                    break;
                case ResultType.Fail:
                    FailCount++;
                    break;
                case ResultType.Abort:
                    AbortCount++;
                    break;
                case ResultType.Null:
                    NullCount++;
                    break;
            }
            TotalCount++;

            if (_hardBins.ContainsKey(chipInfo.HardBin))
                _hardBins[chipInfo.HardBin]++;
            else
                _hardBins.Add(chipInfo.HardBin, 1);


            if (_softBins.ContainsKey(chipInfo.SoftBin))
                _softBins[chipInfo.SoftBin]++;
            else
                _softBins.Add(chipInfo.SoftBin, 1);
        }

        public void Add(ChipSummary summary) {
            this.TotalCount += summary.TotalCount;
            this.FreshCount += summary.FreshCount;
            this.RetestCount += summary.RetestCount;
            this.NullCount += summary.NullCount;
            this.PassCount += summary.PassCount;
            this.FailCount += summary.FailCount;
            this.AbortCount += summary.AbortCount;

            foreach(var v in summary.GetHardBins()) {
                if (_hardBins.ContainsKey(v.Key))
                    _hardBins[v.Key] += v.Value;
                else
                    _hardBins.Add(v.Key, v.Value);
            }

            foreach (var v in summary.GetSoftBins()) {
                if (_softBins.ContainsKey(v.Key))
                    _softBins[v.Key] += v.Value;
                else
                    _softBins.Add(v.Key, v.Value);
            }

        }

        public static IChipSummary Combine(Dictionary<byte, IChipSummary> summaryBySite) {
            ChipSummary summary = new ChipSummary();

            foreach(var v in summaryBySite) {
                summary.Add((ChipSummary)v.Value);
            }

            return summary;
        }

    }

}
