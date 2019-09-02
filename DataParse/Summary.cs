using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse
{
    public class ChipSummary{
        public int TotalCount { get; private set; }
        public int RetestCount { get; private set; }
        public int FreshCount { get; private set; }
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

        public void AddPassChip(bool markRetest = false) {
            PassCount++;
            if (markRetest)
                RetestCount++;
            else
                FreshCount++;

            TotalCount++;
        }
        public void AddFailChip(bool markRetest = false) {
            FailCount++;
            if (markRetest)
                RetestCount++;
            else
                FreshCount++;

            TotalCount++;
        }
        public void AddAbortChip(bool markRetest = false) {
            AbortCount++;
            if (markRetest)
                RetestCount++;
            else
                FreshCount++;

            TotalCount++;
        }

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

    }

    public class TestSummary{


    }

}
