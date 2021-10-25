using System;
using System.Collections.Concurrent;
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

        public IEnumerable<int> GetAllIndex() {
            return _allIndex;
        }

        public IEnumerable<int> GetSiteIndex(byte site) {
            return from i in _allIndex
                   where _site_PartContainer[i] == site
                   select i;
        }

        public Dictionary<byte, int> GetSitesChipCount() {
            return _partStatistic.SiteCnt;
        }

        public ushort[] GetSoftBins() {
            return _softBinNames.Keys.ToArray();
        }

        public Dictionary<ushort, int> GetSoftBinsCount() {
            return _partStatistic.SoftBin;
        }

        public ushort[] GetHardBins() {
            return _hardBinNames.Keys.ToArray();
        }

        public Dictionary<ushort, int> GetHardBinsCount() {
            return _partStatistic.HardBin;
        }

        public IEnumerable<string> GetTestIDs() {
            return _itemContainer.Keys;
        }

        public ItemInfo GetTestInfo(string id) {
            if (!_itemContainer.ContainsKey(id)) throw new Exception("No required Test Id");
            return _itemContainer[id];
        }

        public Dictionary<string, ItemInfo> GetTestIDs_Info() {
            return _itemContainer;
        }

        public Dictionary<ushort, Tuple<string, string>> GetSBinInfo() {
            return _softBinNames;
        }

        public Dictionary<ushort, Tuple<string, string>> GetHBinInfo() {
            return _hardBinNames;
        }
        //
        public ConcurrentDictionary<string, ItemStatistic> GetStatistic() {
            return _itemStatistics;
        }

        public PartStatistic GetPartStatistic() {
            return _partStatistic;
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
        public string GetBasicInfo(string key) {
            if (_basicInfo.ContainsKey(key)) {
                return _basicInfo[key];
            } else {
                return "";
            }
        }

        public string GetPartId(int partIndex) {
            return _partId_PartContainer[partIndex];
        }

        public string GetWaferCord(int partIndex) {
            return $"{_xCord_PartContainer[partIndex]}_{_yCord_PartContainer[partIndex]}";
        }

        public ushort GetHardBin(int partIndex) {
            return _hardBin_PartContainer[partIndex];
        }

        public ushort GetSoftBin(int partIndex) {
            return _softBin_PartContainer[partIndex];
        }

        public byte GetSite(int partIndex) {
            return _site_PartContainer[partIndex];
        }

        public IEnumerable<float> GetFilteredItemData(string testID, int filterId) {
            return GetItemVal(testID, _filterContainer[filterId]);
        }
        public IEnumerable<float> GetFilteredItemData(string testID, int startIndex, int count, int filterId) {
            return GetItemVal(testID, startIndex, count, _filterContainer[filterId]);
        }

        public IEnumerable<Item> GetFilteredItems(int filterId) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");

            //return from r in _filterContainer[filterId].FilterItemStatistics
            //        let item = new Item(r.Key, _itemContainer[r.Key], r.Value)
            //        select item;
            return from r in _filterContainer[filterId].FilteredUid
                   let item = new Item(r, _itemContainer[r], _filterContainer[filterId].FilterItemStatistics[r])
                   select item;

        }

        public IEnumerable<int> GetFilteredPartIndex(int filterId) {
            return _filterContainer[filterId].FilteredPartIdx;
        }

        public int GetFilteredChipsCount(int filterId) {
            return _filterContainer[filterId].FilterPartStatistic.TotalCnt;
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

        public ConcurrentDictionary<string, ItemStatistic> GetFilteredStatistic(int filterId) {
            return _filterContainer[filterId].FilterItemStatistics;
        }
        public ItemStatistic GetFilteredStatistic(int filterId, string uid) {
            return _filterContainer[filterId].FilterItemStatistics[uid];
        }

        public PartStatistic GetFilteredPartStatistic(int filterId) {
            if (!_filterContainer.ContainsKey(filterId)) throw new Exception("No Such Filter Id");
            return _filterContainer[filterId].FilterPartStatistic;
        }



    }
}
