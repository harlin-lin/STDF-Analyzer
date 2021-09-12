using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public partial class SubContainer {

        Dictionary<int, Filter> _filterContainer;
        private void Initialize_Filter() {
            _filterContainer = new Dictionary<int, Filter>();
        }

        public int CreateFilter() {
            int key = System.DateTime.UtcNow.Ticks.GetHashCode();
            while (_filterContainer.ContainsKey(key)) key++;

            _filterContainer.Add(key, new Filter(_partIdx+1, _itemContainer.Count));

            _filterContainer[key].FilterPartStatistic = new PartStatistic(_partStatistic);
            _filterContainer[key].FilterItemStatistics = new ConcurrentDictionary<string, ItemStatistic>(_itemStatistics);
            return key;
        }

        public void ResetFilter(int filterId) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");
            _filterContainer[filterId].ResetFilter();
        }
        
        public void UpdateFilter(int filterId, int[] maskIds, int[] maskItemIds) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");
            try {
                foreach(var id in maskIds) {
                    _filterContainer[filterId].FilterIdxFlag[id]=true;
                }
                foreach (var id in maskItemIds) {
                    _filterContainer[filterId].FilterItemFlag[id] = true;
                }
            }
            catch {
                _filterContainer[filterId].ResetFilter();
                throw new Exception("Id not in idx list, Reset Filter");
            }

            CurrentLoadingProgress = 0;
            OnPropertyChanged("CurrentLoadingProgress");

            Task[] asyncTask = new Task[2];
            asyncTask[0] = Task.Run(() => {
                AnalyseParts_Filtered(_filterContainer[filterId]);
                CurrentLoadingProgress += 50;
                OnPropertyChanged("CurrentLoadingProgress");

                Console.WriteLine("Parts Done");
            });
            asyncTask[1] = Task.Run(() => {
                AnalyseItems_Filtered(_filterContainer[filterId]);
                CurrentLoadingProgress += 50;
                OnPropertyChanged("CurrentLoadingProgress");

                Console.WriteLine("Items Done");
            });

            Task.WaitAll(asyncTask);

        }

        public void RemoveFilter(int filterId) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");
            _filterContainer.Remove(filterId);
        }
    }
}
