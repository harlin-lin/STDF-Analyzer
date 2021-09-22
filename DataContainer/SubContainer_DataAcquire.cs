using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public partial class SubContainer {
        public byte[] GetSites() {
            return _siteContainer.Keys.ToArray();
        }

        public int[] GetAllIndex() {
            return Enumerable.Range(0, _partIdx + 1).ToArray();
        }

        public int[] GetSiteIndex(byte site) {
            return (from i in Enumerable.Range(0, _partIdx + 1)
                    where _site_PartContainer[i] == site
                    select i).ToArray();
        }


        public Dictionary<byte, int> GetSitesChipCount() {
            return new Dictionary<byte, int>(_partStatistic.SiteCnt);
        }

        public ushort[] GetSoftBins() {
            return _softBin_PartContainer.ToArray();
        }

        public Dictionary<ushort, int> GetSoftBinsCount() {
            return new Dictionary<ushort, int>(_partStatistic.SoftBin);
        }

        public ushort[] GetHardBins() {
            return _hardBin_PartContainer.ToArray();
        }

        public Dictionary<ushort, int> GetHardBinsCount() {
            return new Dictionary<ushort, int>(_partStatistic.HardBin);
        }

        public string[] GetTestIDs() {
            return _itemContainer.Keys.ToArray();
        }

        public ItemInfo GetTestInfo(string id) {
            if (!_itemContainer.ContainsKey(id)) throw new Exception("No required Test Id");
            return new ItemInfo(_itemContainer[id]);
        }

        public Dictionary<string, ItemInfo> GetTestIDs_Info() {
            return new Dictionary<string, ItemInfo>(_itemContainer);
        }

        public Dictionary<ushort, Tuple<string, string>> GetSBinInfo() {
            return new Dictionary<ushort, Tuple<string, string>>(_softBinNames);
        }

        public Dictionary<ushort, Tuple<string, string>> GetHBinInfo() {
            return new Dictionary<ushort, Tuple<string, string>>(_hardBinNames);
        }

        public Dictionary<string, ItemStatistic> GetStatistic() {
            return new Dictionary<string, ItemStatistic>(_itemStatistics);
        }

        public int ChipsCount { 
            get {
                return _partIdx + 1;
            } 
        }

        public string FileName {
            get {
                return Path.GetFileName(FilePath);
            }
        }


        public float[] GetFilteredItemData(string testID, int filterId) {
            return GetItemVal(testID, _filterContainer[filterId]).ToArray();
        }
        public float[] GetFilteredItemData(string testID, int startIndex, int count, int filterId) {
            return GetItemVal(testID, startIndex, count, _filterContainer[filterId]).ToArray();
        }

        public IEnumerable<Item> GetFilteredItems(int filterId) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");

            return from r in _filterContainer[filterId].FilterItemStatistics
                    let item = new Item(r.Key, _itemContainer[r.Key], r.Value)
                    select item;

        }

        public FilterSetup GetFilterSetup(int filterId){
            return _filterSetupContainer[filterId];
        }

        public int[] GetAllFilterId() {
            return _filterContainer.Keys.ToArray();
        }

        public int GetFilterIndex(int filterId) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");
            int idx =0;
            foreach(var i in Enumerable.Range(0, _filterContainer.Count)) {
                if (_filterContainer.ElementAt(i).Key == filterId) idx=i; 
            }
            return idx;
        }
    }
}
