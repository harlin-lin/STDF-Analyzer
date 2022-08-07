using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public partial class SubContainer {

        Dictionary<int, Filter> _filterContainer;
        Dictionary<int, FilterSetup> _filterSetupContainer;

        private void Initialize_Filter() {
            _filterContainer = new Dictionary<int, Filter>();
            _filterSetupContainer = new Dictionary<int, FilterSetup>();
        }

        public int CreateFilter() {
            int key = System.DateTime.UtcNow.Ticks.GetHashCode();
            while (_filterContainer.ContainsKey(key)) key++;

            _filterContainer.Add(key, new Filter(_partIdx+1, _itemContainer.Count));
            _filterSetupContainer.Add(key, new FilterSetup(""));

            ResetFilterStatistic(key);

            return key;
        }

        public int CreateFilter(byte enSite) {
            if (!_siteContainer.ContainsKey(enSite)) throw new Exception("No Such site");
            int key = System.DateTime.UtcNow.Ticks.GetHashCode();
            while (_filterContainer.ContainsKey(key)) key++;

            _filterContainer.Add(key, new Filter(_partIdx + 1, _itemContainer.Count));
            _filterSetupContainer.Add(key, new FilterSetup(_siteContainer.Keys, enSite, "Site:" + enSite));

            _filterContainer[key].FilterPartStatistic = new PartStatistic(_siteContainer.Keys);
            //_filterContainer[key].FilterItemStatistics = new ConcurrentDictionary<string, ItemStatistic>();

            var maskIds = from i in Enumerable.Range(0, _partIdx + 1)
                    where _site_PartContainer[i] != enSite
                    select i;

            UpdateFilter(key, maskIds);
            return key;
        }


        public void ResetFilter(int filterId) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");
            _filterContainer[filterId].ResetFilter();
            ResetFilterStatistic(filterId);
        }

        void ResetFilterStatistic(int filterId) {
            _filterContainer[filterId].FilterPartStatistic = new PartStatistic(_partStatistic);
            _filterContainer[filterId].FilterItemStatistics = new ConcurrentDictionary<string, ItemStatistic>(_itemStatistics);
            _filterContainer[filterId].FilteredPartIdx = _allIndex;
        }

        void UpdateFilterStatistic(int filterId) {

            CurrentLoadingProgress = 0;
            OnPropertyChanged("CurrentLoadingProgress");

            _filterContainer[filterId].FilteredPartIdx = from i in Enumerable.Range(0, _partIdx + 1)
                                                         where !_filterContainer[filterId].FilterIdxFlag[i]
                                                         select i;

            Task[] asyncTask = new Task[2];
            asyncTask[0] = Task.Run(() => {
                AnalyseParts_Filtered(_filterContainer[filterId]);
                CurrentLoadingProgress += 50;
                OnPropertyChanged("CurrentLoadingProgress");

                //Console.WriteLine("Parts Done");
            });
            asyncTask[1] = Task.Run(() => {
                AnalyseItems_Filtered(_filterContainer[filterId]);
                CurrentLoadingProgress += 50;
                OnPropertyChanged("CurrentLoadingProgress");

                //Console.WriteLine("Items Done");
            });

            Task.WaitAll(asyncTask);


        }

        public void UpdateFilter(int filterId, IEnumerable<int> maskIds) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");
            try {
                _filterContainer[filterId].IfNoChanged = false;
                foreach (var id in maskIds) {
                    _filterContainer[filterId].FilterIdxFlag[id]=true;
                }

                UpdateFilterStatistic(filterId);
            }
            catch {
                _filterContainer[filterId].ResetFilter();
                ResetFilterStatistic(filterId);
                throw new Exception("Id not in idx list, Reset Filter");
            }

        }

        public void UpdateFilter(int filterId, FilterSetup externalFilter) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");
            CurrentLoadingProgress = 0;
            OnPropertyChanged("CurrentLoadingProgress");

            _filterSetupContainer[filterId] = DeepCopy(externalFilter);

            var maskIds = GetMaskIds(externalFilter);

            _filterContainer[filterId].ResetFilter();
            try {
                _filterContainer[filterId].IfNoChanged = false;
                foreach (var id in maskIds) {
                    _filterContainer[filterId].FilterIdxFlag[id] = true;
                }
    
                UpdateFilterStatistic(filterId);
            }
            catch {
                _filterContainer[filterId].ResetFilter();
                ResetFilterStatistic(filterId);
                throw new Exception("Id not in idx list, Reset Filter");
            }

        }


        public void RemoveFilter(int filterId) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");
            _filterContainer.Remove(filterId);
            _filterSetupContainer.Remove(filterId);
        }

        private List<int> GetMaskIds(FilterSetup filter) {
            List<int> chipsFilter = new List<int>();
            //List<int> dupSelected = null;
            //if (filter.ifmaskDuplicateChips) {
            //    dupSelected = new List<int>(_partIdx+1);
            //    List<int> dup = null;
            //    if (filter.DuplicateSelectMode == DuplicateSelectMode.OnlyDuplicated) {
            //        if (filter.DuplicateJudgeMode == DuplicateJudgeMode.ID) {

            //            var dupb = _partId_PartContainer.GroupBy(x => x)
            //                          .Where(g => g.Count() > 1)
            //                          .Select(a => a.ToList())
            //                          .ToList();
            //            dup = new List<string>(dupb.Count * 2);
            //            foreach (var v in dupb) {
            //                dup.AddRange(v);
            //            }

            //        } else if(_ifCordValid){
            //            var dupb = _testChips.GroupBy(x => x.WaferCord)
            //                          .Where(g => g.Count() > 1)
            //                          .Select(a => a.ToList())
            //                          .ToList();
            //            dup = new List<IChipInfo>(dupb.Count * 2);
            //            foreach (var v in dupb) {
            //                dup.AddRange(v);
            //            }
            //        }

            //    } else if (filter.DuplicateSelectMode == DuplicateSelectMode.First) {
            //        if (filter.DuplicateJudgeMode == DuplicateJudgeMode.ID) {
            //            var od = _testChips.GroupBy(x => x.PartId);
            //            var dupf = od.Where(g => g.Count() > 1)
            //                          .Select(a => a.First())
            //                          .ToList();
            //            var dupa = od.Where(g => g.Count() == 1)
            //                          .Select(a => a.FirstOrDefault())
            //                          .ToList();
            //            dupa.AddRange(dupf);
            //            dup = dupa;
            //        } else {
            //            var od = _testChips.GroupBy(x => x.WaferCord);
            //            var dupf = od.Where(g => g.Count() > 1)
            //                          .Select(a => a.First())
            //                          .ToList();
            //            var dupa = od.Where(g => g.Count() == 1)
            //                          .Select(a => a.FirstOrDefault())
            //                          .ToList();
            //            dupa.AddRange(dupf);
            //            dup = dupa;
            //        }

            //    } else if (filter.DuplicateSelectMode == DuplicateSelectMode.Last) {
            //        if (filter.DuplicateJudgeMode == DuplicateJudgeMode.ID) {
            //            var od = _testChips.GroupBy(x => x.PartId);
            //            var dupf = od.Where(g => g.Count() > 1)
            //                          .Select(a => a.Last())
            //                          .ToList();
            //            var dupa = od.Where(g => g.Count() == 1)
            //                          .Select(a => a.FirstOrDefault())
            //                          .ToList();
            //            dupa.AddRange(dupf);
            //            dup = dupa;
            //        } else {
            //            var od = _testChips.GroupBy(x => x.WaferCord);
            //            var dupf = od.Where(g => g.Count() > 1)
            //                          .Select(a => a.Last())
            //                          .ToList();
            //            var dupa = od.Where(g => g.Count() == 1)
            //                          .Select(a => a.FirstOrDefault())
            //                          .ToList();
            //            dupa.AddRange(dupf);
            //            dup = dupa;
            //        }

            //    }

            //    foreach (var v in dup) {
            //        dupSelected.Add(v.InternalId);
            //    }

            //    dupSelected.OrderBy(x => x);
            //} else {
            //    dupSelected = Enumerable.Range(0, _testChips.Count).ToList();
            //}

            for (int i = 0; i <= _partIdx; i++) {
                //foreach (int i in dupSelected) {

                //site
                if (filter.MaskSites.Contains(_site_PartContainer[i])) {
                    chipsFilter.Add(i);
                    continue;
                }
                //softbin
                if (filter.MaskSoftBins.Contains(_softBin_PartContainer[i])) {
                    chipsFilter.Add(i);
                    continue;
                }
                //hardbin
                if (filter.MaskHardBins.Contains(_hardBin_PartContainer[i])) {
                    chipsFilter.Add(i);
                    continue;
                }

                //init
                if (!filter.IfMaskOrEnableIds) {
                    if (filter.MaskChips.Contains(i)) {
                        chipsFilter.Add(i);
                        continue;
                    }
                } else {
                    if (!filter.MaskChips.Contains(i)) {
                        chipsFilter.Add(i);
                        continue;
                    }
                }

                if (_ifCordValid) {
                    if (!filter.IfMaskOrEnableCords) {
                        if (filter.MaskCords.Contains((_xCord_PartContainer[i], _yCord_PartContainer[i]))) {
                            chipsFilter.Add(i);
                            continue;
                        }
                    } else {
                        if (!filter.MaskCords.Contains((_xCord_PartContainer[i], _yCord_PartContainer[i]))) {
                            chipsFilter.Add(i);
                            continue;
                        }
                    }
                }

                if (filter.ItemFilters.Count > 0) {
                    foreach(var itemFilter in filter.ItemFilters) {
                        var rst = GetItemVal(itemFilter.TestNumber, i);
                        if(rst<itemFilter.LowRange || rst > itemFilter.HighRange || float.IsNaN(rst) || float.IsInfinity(rst)) {
                            chipsFilter.Add(i);
                            break;
                        }
                    }
                }

            }

            return chipsFilter;
        }


        static T DeepCopy<T>(T obj) {
            using (var ms = new MemoryStream()) {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
