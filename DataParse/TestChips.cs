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
        private Dictionary<byte, ChipSummary> _chipSummaryFiltered;
        private Dictionary<byte, ChipSummary> _chipSummaryDefault;
        private bool NeedUpdateAfterFilterChanged;
        private bool DefaultSummaryDone;

        public TestChips(int capacity) {
            _testChips = new List<ChipInfo>(capacity);
            _chipIndexes = new List<int>(capacity);
            _ChipFilter = new List<bool>(capacity);
            _chipSummaryFiltered = new Dictionary<byte, ChipSummary>();
            _chipSummaryDefault = new Dictionary<byte, ChipSummary>();
            NeedUpdateAfterFilterChanged = true; //true for first time call
            DefaultSummaryDone = false;
        }

        public void AddChip(ChipInfo chipInfo) {
            _testChips.Add(chipInfo);
            _chipIndexes.Add(_testChips.Count - 1);
            _ChipFilter.Add(true);
        }

        public int ChipsCount {
            get {
                return _testChips.Count;
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

        private void UpdateSummaryFiltered() {

            if (!NeedUpdateAfterFilterChanged) return;

            for (int i = 0; i < _testChips.Count; i++) {
                if (!_ChipFilter[i]) continue;
                if (!_chipSummaryFiltered.ContainsKey(_testChips[i].Site)) 
                    _chipSummaryFiltered.Add(_testChips[i].Site, new ChipSummary());

                _chipSummaryFiltered[_testChips[i].Site].AddChip(_testChips[i]);
            }
            NeedUpdateAfterFilterChanged = false;
        }

        public Dictionary<byte, ChipSummary> GetChipSummaryBySiteFiltered() {
            UpdateSummaryFiltered();
            return new Dictionary<byte, ChipSummary>(_chipSummaryFiltered);
        }

        public Dictionary<byte, ChipSummary> GetChipSummaryBySiteFiltered(List<byte> sites) {
            Dictionary<byte, ChipSummary> summary = new Dictionary<byte, ChipSummary>();

            UpdateSummaryFiltered();
            foreach (byte s in sites) {
                if (_chipSummaryFiltered.ContainsKey(s))
                    summary.Add(s, _chipSummaryFiltered[s]);
                else
                    summary.Add(s, new ChipSummary());
            }

            return summary;
        }

        public ChipSummary GetChipSummaryFiltered() {
            UpdateSummaryFiltered();
            return ChipSummary.Combine(_chipSummaryFiltered);
        }

        public ChipSummary GetChipSummaryFiltered(List<byte> sites) {
            Dictionary<byte, ChipSummary> summary = new Dictionary<byte, ChipSummary>();

            UpdateSummaryFiltered();
            foreach (byte s in sites) {
                if (_chipSummaryFiltered.ContainsKey(s))
                    summary.Add(s, _chipSummaryFiltered[s]);
                else
                    summary.Add(s, new ChipSummary());
            }

            return ChipSummary.Combine(_chipSummaryFiltered);
        }


        private void UpdateSummaryDefault() {
            if (DefaultSummaryDone) return;
            for (int i = 0; i < _testChips.Count; i++) {
                if (!_chipSummaryDefault.ContainsKey(_testChips[i].Site))
                    _chipSummaryDefault.Add(_testChips[i].Site, new ChipSummary());

                _chipSummaryDefault[_testChips[i].Site].AddChip(_testChips[i]);
            }
            DefaultSummaryDone = true;
        }

        public Dictionary<byte, ChipSummary> GetChipSummaryBySiteDefault() {
            UpdateSummaryDefault();
            return new Dictionary<byte, ChipSummary>(_chipSummaryDefault);
        }

        public Dictionary<byte, ChipSummary> GetChipSummaryBySiteDefault(List<byte> sites) {
            Dictionary<byte, ChipSummary> summary = new Dictionary<byte, ChipSummary>();

            UpdateSummaryDefault();
            foreach(byte s in sites) {
                if (_chipSummaryDefault.ContainsKey(s))
                    summary.Add(s, _chipSummaryDefault[s]);
                else
                    summary.Add(s, new ChipSummary());
            }

            return summary;
        }

        public ChipSummary GetChipSummaryDefault() {
            UpdateSummaryDefault();
            return ChipSummary.Combine(_chipSummaryDefault);
        }

        public ChipSummary GetChipSummaryDefault(List<byte> sites) {
            Dictionary<byte, ChipSummary> summary = new Dictionary<byte, ChipSummary>();

            UpdateSummaryDefault();
            foreach (byte s in sites) {
                if (_chipSummaryDefault.ContainsKey(s))
                    summary.Add(s, _chipSummaryDefault[s]);
                else
                    summary.Add(s, new ChipSummary());
            }

            return ChipSummary.Combine(_chipSummaryDefault);
        }


        public void UpdateChipFilter(Filter filter) {

            for(int i=0; i< _testChips.Count; i++) {
                _ChipFilter[i] = true;

                //site
                if (filter.maskSites.Contains(_testChips[i].Site)) {
                    _ChipFilter[i] = false;
                    continue;
                }

                //softbin
                if (filter.maskSoftBins.Contains(_testChips[i].SoftBin)) {
                    _ChipFilter[i] = false;
                    continue;
                }

                //hardbin
                if (filter.maskHardBins.Contains(_testChips[i].HardBin)) {
                    _ChipFilter[i] = false;
                    continue;
                }

                //cords
                if (filter.maskCords.Contains(_testChips[i].WaferCord)) {
                    _ChipFilter[i] = false;
                    continue;
                }

            }
            //index
            foreach (int i in filter.maskChips) {
                _ChipFilter[i] = false;
            }

            //dupicate chip
            if (filter.DuplicateSelectMode == DuplicateSelectMode.SelectFirst) {
                for(int i = 0; i < _testChips.Count; i++) {
                    for(int j = i + 1; j < _testChips.Count; j++) {
                        if(_testChips[i].PartId==_testChips[j].PartId || _testChips[i].WaferCord == _testChips[j].WaferCord) 
                            _ChipFilter[j] = false;
                    }
                }
            } 
            else {
                for (int i = _testChips.Count-1; i >= 0; i--) {
                    for (int j = i - 1; j >= 0; j--) {
                        if (_testChips[i].PartId == _testChips[j].PartId || _testChips[i].WaferCord == _testChips[j].WaferCord)
                            _ChipFilter[j] = false;
                    }
                }
            }

            NeedUpdateAfterFilterChanged = true;
        }

    }
}
