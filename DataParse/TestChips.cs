using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse
{
    public class TestChips{
        private List<ChipInfo> _testChips;
        private List<int> _chipIndexes;
        private List<bool> _ChipFilter;
        Dictionary<byte, ChipSummary> _chipSummary;

        public TestChips(int capacity) {
            _testChips = new List<ChipInfo>(capacity);
            _chipIndexes = new List<int>(capacity);
            _ChipFilter = new List<bool>(capacity);
            _chipSummary = new Dictionary<byte, ChipSummary>();
        }

        public void AddChip(ChipInfo chipInfo) {
            _testChips.Add(chipInfo);
            _chipIndexes.Add(_testChips.Count - 1);
            _ChipFilter.Add(true);
        }

        public int ChipsCount {
            get {
                return _chipIndexes.Count;
            }
        }

        public List<bool> GetChipFilter() {
            return _ChipFilter;
        }

        public List<bool> GetChipFilter(List<byte> sites) {
            List<bool> filter = new List<bool>(_testChips.Count);
            for (int i = 0; i < _testChips.Count; i++) {
                if (sites.Contains(_testChips[i].Site))
                    filter.Add(_ChipFilter[i]);
            }
            return filter;
        }

        public List<int> GetChipsIndexes() {
            return _chipIndexes;
        }
        public List<int> GetChipsIndexes(List<byte> sites) {
            List<int> indexes = new List<int>(_testChips.Count);
            for (int i = 0; i < _testChips.Count; i++) {
                if (sites.Contains(_testChips[i].Site))
                    indexes.Add(_chipIndexes[i]);
            }
            return indexes;
        }
        public List<int> GetFilteredChipsIndexes() {
            List<int> indexes = new List<int>(_testChips.Count);
            for (int i = 0; i < _testChips.Count; i++) {
                if (_ChipFilter[i])
                    indexes.Add(_chipIndexes[i]);
            }
            return indexes;
        }
        public List<int> GetFilteredChipsIndexes(List<byte> sites) {
            List<int> indexes = new List<int>(_testChips.Count);
            for (int i = 0; i < _testChips.Count; i++) {
                if (_ChipFilter[i] && sites.Contains(_testChips[i].Site)) 
                    indexes.Add(_chipIndexes[i]);
            }
            return indexes;
        }
        public List<ChipInfo> GetFilteredChipsInfo() {
            return _testChips;
        }
        public List<ChipInfo> GetFilteredChipsInfo(List<byte> sites) {
            List<ChipInfo> infos = new List<ChipInfo>(_testChips.Count);
            for (int i = 0; i < _testChips.Count; i++) {
                if (sites.Contains(_testChips[i].Site))
                    infos.Add(_testChips[i]);
            }
            return infos;
        }

        private void UpdateSummary() {

            for (int i = 0; i < _testChips.Count; i++) {
                if (!_ChipFilter[i]) continue;
                if (!_chipSummary.ContainsKey(_testChips[i].Site)) 
                    _chipSummary.Add(_testChips[i].Site, new ChipSummary());

                _chipSummary[_testChips[i].Site].AddChip(_testChips[i]);
            }

        }

        public Dictionary<byte, ChipSummary> GetChipSummaryBySite() {
            UpdateSummary();
            return new Dictionary<byte, ChipSummary>(_chipSummary);
        }

        public Dictionary<byte, ChipSummary> GetChipSummaryBySite(List<byte> sites) {
            Dictionary<byte, ChipSummary> summary = new Dictionary<byte, ChipSummary>();

            UpdateSummary();
            foreach(byte s in sites) {
                if (_chipSummary.ContainsKey(s))
                    summary.Add(s, _chipSummary[s]);
                else
                    summary.Add(s, new ChipSummary());
            }

            return summary;
        }

        public ChipSummary GetChipSummary() {
            UpdateSummary();
            return ChipSummary.Combine(_chipSummary);
        }

        public ChipSummary GetChipSummary(List<byte> sites) {
            Dictionary<byte, ChipSummary> summary = new Dictionary<byte, ChipSummary>();

            UpdateSummary();
            foreach (byte s in sites) {
                if (_chipSummary.ContainsKey(s))
                    summary.Add(s, _chipSummary[s]);
                else
                    summary.Add(s, new ChipSummary());
            }

            return ChipSummary.Combine(_chipSummary);
        }


    }
}
