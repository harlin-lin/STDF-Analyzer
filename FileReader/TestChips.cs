using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader
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
        public List<IChipInfo> GetFilteredChipsInfo(List<int> chipsFilter) {
            List<IChipInfo> infos = new List<IChipInfo>(chipsFilter.Count);
            foreach(int i in chipsFilter) {
                infos.Add(_testChips[i]);
            }
            return infos;
        }
        public List<IChipInfo> GetChipsInfo() {
            //List<IChipInfo> infos = new List<IChipInfo>(_testChips);
            //return infos;
            return _testChips;
        }

        public void UpdateSummaryFiltered(List<int> chipsFilter, ref Dictionary<byte, IChipSummary> summary) {
            summary.Clear();

            //for (int i = 0; i < _testChips.Count; i++) {
            foreach(int i in chipsFilter) { 
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

        public void UpdateChipFilter(FilterSetup filter, ref List<int> chipsFilter) {
            chipsFilter.Clear();
            List<int> dupSelected=null;
            if (filter.ifmaskDuplicateChips) {
                dupSelected = new List<int>(_testChips.Count);
                List<IChipInfo> dup = null;
                if(filter.DuplicateSelectMode== DuplicateSelectMode.OnlyDuplicated) {
                    if (filter.DuplicateJudgeMode == DuplicateJudgeMode.ID) {
                        var dupb = _testChips.GroupBy(x => x.PartId)
                                      .Where(g => g.Count() > 1)
                                      .Select(a=>a.ToList())
                                      .ToList();
                        dup = new List<IChipInfo>(dupb.Count*2);
                        foreach (var v in dupb) {
                            dup.AddRange(v);
                        }

                    } else {
                        var dupb = _testChips.GroupBy(x => x.WaferCord)
                                      .Where(g => g.Count() > 1)
                                      .Select(a => a.ToList())
                                      .ToList();
                        dup = new List<IChipInfo>(dupb.Count * 2);
                        foreach (var v in dupb) {
                            dup.AddRange(v);
                        }
                    }

                } else if (filter.DuplicateSelectMode == DuplicateSelectMode.First) {
                    if (filter.DuplicateJudgeMode == DuplicateJudgeMode.ID) {
                        var od = _testChips.GroupBy(x => x.PartId);
                        var dupf = od.Where(g => g.Count() > 1)
                                      .Select(a=>a.First())
                                      .ToList();
                        var dupa = od.Where(g => g.Count() ==1)
                                      .Select(a => a.FirstOrDefault())
                                      .ToList();
                        dupa.AddRange(dupf);
                        dup = dupa;
                    } else {
                        var od = _testChips.GroupBy(x => x.WaferCord);
                        var dupf = od.Where(g => g.Count() > 1)
                                      .Select(a => a.First())
                                      .ToList();
                        var dupa = od.Where(g => g.Count() == 1)
                                      .Select(a => a.FirstOrDefault())
                                      .ToList();
                        dupa.AddRange(dupf);
                        dup = dupa;
                    }

                } else if (filter.DuplicateSelectMode == DuplicateSelectMode.Last) {
                    if (filter.DuplicateJudgeMode == DuplicateJudgeMode.ID) {
                        var od = _testChips.GroupBy(x => x.PartId);
                        var dupf = od.Where(g => g.Count() > 1)
                                      .Select(a => a.Last())
                                      .ToList();
                        var dupa = od.Where(g => g.Count() == 1)
                                      .Select(a => a.FirstOrDefault())
                                      .ToList();
                        dupa.AddRange(dupf);
                        dup = dupa;
                    } else {
                        var od = _testChips.GroupBy(x => x.WaferCord);
                        var dupf = od.Where(g => g.Count() > 1)
                                      .Select(a => a.Last())
                                      .ToList();
                        var dupa = od.Where(g => g.Count() == 1)
                                      .Select(a => a.FirstOrDefault())
                                      .ToList();
                        dupa.AddRange(dupf);
                        dup = dupa;
                    }

                }

                foreach (var v in dup) {
                    dupSelected.Add(v.InternalId);
                }

                dupSelected.OrderBy(x => x);
            } else {
                dupSelected = Enumerable.Range(0, _testChips.Count).ToList();
            }
            //for (int i = 0; i < _testChips.Count; i++) {
            foreach(int i in dupSelected) { 
                //init
                if (!filter.ifMaskOrEnableIds) {
                    if (filter.maskChips.Contains(_testChips[i].PartId)) continue;
                } else {
                    if (!filter.maskChips.Contains(_testChips[i].PartId)) continue;
                }

                if (!filter.ifMaskOrEnableCords) {
                    if (filter.maskCords.Contains(_testChips[i].WaferCord)) continue;
                } else {
                    if (!filter.maskCords.Contains(_testChips[i].WaferCord)) continue;
                }

                //site
                if (filter.maskSites.Contains(_testChips[i].Site)) continue;
                //softbin
                if (filter.maskSoftBins.Contains(_testChips[i].SoftBin)) continue;
                //hardbin
                if (filter.maskHardBins.Contains(_testChips[i].HardBin)) continue;

                chipsFilter.Add(i);
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
