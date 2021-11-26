using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public partial class SubContainer {
        private ConcurrentDictionary<string, ItemStatistic> _itemStatistics;


        private void Initialize_ItemStatistic() {
            //_itemStatistics = new ConcurrentDictionary<string, ItemStatistic>(from r in _itemContainer
            //                                                                  let v = new KeyValuePair<string, ItemStatistic>(r.Key, null)
            //                                                                  select v);
        }

        private void AnalyseItems() {
            _itemStatistics = new ConcurrentDictionary<string, ItemStatistic>(from r in _itemContainer
                                                                              let v = new KeyValuePair<string, ItemStatistic>(r.Key, null)
                                                                              select v);

            Parallel.For(0, _itemStatistics.Count, (x) => {
                var key = _itemStatistics.ElementAt((int)x).Key;
                _itemStatistics[key] = new ItemStatistic(GetItemVal(key), _itemContainer[key].LoLimit, _itemContainer[key].HiLimit);
            });
        }

        private void AnalyseItems_Filtered(Filter filter) {
            filter.FilterItemStatistics = new ConcurrentDictionary<string, ItemStatistic>(from i in Enumerable.Range(0, _itemContainer.Count)
                                                                                          let v = new KeyValuePair<string, ItemStatistic>(_itemContainer.ElementAt(i).Key, null)
                                                                                          select v);

            Parallel.For(0, filter.FilterItemStatistics.Count, (x) => {
                var key = filter.FilterItemStatistics.ElementAt((int)x).Key;
                filter.FilterItemStatistics[key] = new ItemStatistic(GetItemVal(key, filter), _itemContainer[key].LoLimit, _itemContainer[key].HiLimit);
            });
        }

    }
}
