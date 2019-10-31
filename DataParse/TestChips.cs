using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse
{
    public class TestChips{
        private List<IChipInfo> _testChips;
        private List<int> _chipIndexes;

        public TestChips(int capacity) {
            _testChips = new List<IChipInfo>(capacity);
            _chipIndexes = new List<int>(capacity);
        }

        public void AddChip(ChipInfo chipInfo) {
            _testChips.Add(chipInfo);
            _chipIndexes.Add(_testChips.Count - 1);
        }

        public int ChipsCount {
            get {
                return _testChips.Count;
            }
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
        public List<int> GetFilteredChipsIndexes(bool[] chipsFilter) {
            List<int> indexes = new List<int>(_testChips.Count);
            for (int i = 0; i < _testChips.Count; i++) {
                if (!chipsFilter[i])
                    indexes.Add(_chipIndexes[i]);
            }
            return indexes;
        }
        public List<IChipInfo> GetFilteredChipsInfo(bool[] chipsFilter) {
            List<IChipInfo> infos = new List<IChipInfo>(_testChips.Count);
            for (int i = 0; i < _testChips.Count; i++) {
                if (!chipsFilter[i])
                    infos.Add(_testChips[i]);
            }
            return infos;
        }
        public void UpdateSummaryFiltered(bool[] chipsFilter, ref Dictionary<byte, IChipSummary> summary) {
            summary.Clear();

            for (int i = 0; i < _testChips.Count; i++) {
                if (chipsFilter[i]) continue;
                if (!summary.ContainsKey(_testChips[i].Site))
                    summary.Add(_testChips[i].Site, new ChipSummary());

                ((ChipSummary)summary[_testChips[i].Site]).AddChip(_testChips[i]);
            }
        }

        public void UpdateSummary(ref Dictionary<byte, IChipSummary> summary) {

            for (int i = 0; i < _testChips.Count; i++) {
                if (!summary.ContainsKey(_testChips[i].Site))
                    summary.Add(_testChips[i].Site, new ChipSummary());

                ((ChipSummary)summary[_testChips[i].Site]).AddChip(_testChips[i]);
            }
        }

        public void UpdateChipFilter(FilterSetup filter, ref bool[] chipsFilter) {


            for (int i = 0; i < _testChips.Count; i++) {
                //init
                chipsFilter[i] = false;

                //site
                if (filter.maskSites.Contains(_testChips[i].Site)) {
                    chipsFilter[i] = true;
                    continue;
                }

                //softbin
                if (filter.maskSoftBins.Contains(_testChips[i].SoftBin)) {
                    chipsFilter[i] = true;
                    continue;
                }

                //hardbin
                if (filter.maskHardBins.Contains(_testChips[i].HardBin)) {
                    chipsFilter[i] = true;
                    continue;
                }

                //cords
                if (filter.maskCords.Contains(_testChips[i].WaferCord)) {
                    chipsFilter[i] = true;
                    continue;
                }

            }
            //index
            foreach (int i in filter.maskChips) {
                chipsFilter[i] = true;
            }

            if (filter.ifmaskDuplicateChips) {
                //dupicate chip
                if (filter.DuplicateSelectMode == DuplicateSelectMode.SelectFirst) {
                    for (int i = 0; i < _testChips.Count; i++) {
                        for (int j = i + 1; j < _testChips.Count; j++) {
                            if (filter.DuplicateJudgeMode== DuplicateJudgeMode.ID) {
                                if (_testChips[i].PartId == _testChips[j].PartId)
                                    chipsFilter[j] = true;
                            } else {
                                if (_testChips[i].WaferCord == _testChips[j].WaferCord)
                                    chipsFilter[j] = true;
                            }
                        }
                    }
                } else {
                    for (int i = _testChips.Count - 1; i >= 0; i--) {
                        for (int j = i - 1; j >= 0; j--) {
                            if (filter.DuplicateJudgeMode == DuplicateJudgeMode.ID) {
                                if (_testChips[i].PartId == _testChips[j].PartId)
                                    chipsFilter[j] = true;
                            } else {
                                if (_testChips[i].WaferCord == _testChips[j].WaferCord)
                                    chipsFilter[j] = true;
                            }
                        }
                    }
                }
            }

        }

        public void UpdateChipFilter(byte site, ref bool[] chipsFilter) {
            for (int i = 0; i < _testChips.Count; i++) {
                //init
                chipsFilter[i] = false;

                //site
                if (_testChips[i].Site != site) {
                    chipsFilter[i] = true;
                    continue;
                }
            }
        }


    }
}
