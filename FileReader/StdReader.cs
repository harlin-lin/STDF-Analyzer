using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DataInterface;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Data;

namespace FileReader {
    public enum StdFileType {
        STD,
        STD_GZ
    }
    /// <summary>
    /// Used to indicate endian-ness
    /// </summary>
    public enum Endian {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Big endian
        /// </summary>
		Big,
        /// <summary>
        /// Little Endian
        /// </summary>
		Little,
    }

    public class StdReader : IDataAcquire{
        [Serializable]
        private class FilterData {
            public FilterSetup Filter { get; private set; }
            //public bool[] ChipFilter;
            public List<int> ChipFilter;

            public bool[] ItemFilter;
            public Dictionary<byte, IChipSummary> SitesSummary;
            public IChipSummary Summary;
            public Dictionary<TestID, IItemStatistic> StatisticList;

            public FilterData(FilterSetup filterSetup, int chipsCount, int itemsCount) {
                Filter = filterSetup;
                //ChipFilter = new bool[chipsCount];
                ChipFilter = new List<int>(chipsCount);
                ItemFilter = new bool[itemsCount];
                SitesSummary = new Dictionary<byte, IChipSummary>();
                Summary = null;
                //StatisticList = new Dictionary<TestID, IItemStatistic>();
            }


            public static T DeepCopy<T>(T obj) {
                using (var ms = new MemoryStream()) {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, obj);
                    ms.Position = 0;

                    return (T)formatter.Deserialize(ms);
                }
            }

        }

        public event ExtractDoneEventHandler ExtractDone;
        //public event ExtractDoneEventHandler FilterGenerated;
        public event PropertyChangedEventHandler PropertyChanged;


        public string FilePath { get; private set; }
        public string FileName { get; private set; }

        public bool _parseDone;
        public bool ParseDone {
            get { return _parseDone; }
            private set {
                _parseDone = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ParseDone"));
            }
        }

        //private bool _filterDone;
        //public bool FilterDone {
        //    get { return _filterDone; }
        //    private set {
        //        _filterDone = value;
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilterDone"));
        //    }
        //}

        //basic file information
        public IFileBasicInfo BasicInfo { get { return _rawData; }  }

        private Dictionary<int, FilterData> _filterList;
        private Dictionary<byte, IChipSummary> _defaultSitesSummary;
        private IChipSummary _defaultSummary;
        private Dictionary<TestID, IItemStatistic> _defaultStatistic;


        public override int GetHashCode() {
            return FilePath.GetHashCode();
        }

        public void ExtractStdf() {
            var s = new System.Diagnostics.Stopwatch();
            s.Start();
            _rawData = _v4Reader.ReadRaw();
            s.Stop();
            Console.WriteLine("Read Raw:" +s.ElapsedMilliseconds);

            s.Restart();
            _filterList = new Dictionary<int, FilterData>();
            _defaultSitesSummary = new Dictionary<byte, IChipSummary>();
            _defaultSummary = null;
            _defaultStatistic = new Dictionary<TestID, IItemStatistic>();

            _rawData._testChips.UpdateSummary(ref _defaultSitesSummary);
            _defaultSummary = ChipSummary.Combine(_defaultSitesSummary);
            
            foreach (var t in _rawData._testItems.GetTestIDs_Info()) {
                _defaultStatistic.Add(t.Key, new ItemStatistic(_rawData._rawData[t.Key], t.Value.LoLimit, t.Value.HiLimit));
            }
            s.Stop();
            Console.WriteLine("Summary Update:" + s.ElapsedMilliseconds);

            s.Restart();
            ParseDone = true;
            ExtractDone?.Invoke(this);
            s.Stop();
            Console.WriteLine("Extract Done Invoke:" + s.ElapsedMilliseconds);

            s.Restart();
            GC.Collect();
            s.Stop();
            Console.WriteLine("GC:" + s.ElapsedMilliseconds);

            //s.Restart();
            //CreateDefaultFilters();
            //FilterDone = true;
            //s.Stop();
            //Console.WriteLine("Create Filters:" + s.ElapsedMilliseconds);

            //s.Restart();
            //FilterGenerated?.Invoke(this);
            //s.Stop();
            //Console.WriteLine("Filter Done Invoke:" + s.ElapsedMilliseconds);

        }

        ////property get the file default infomation
        public List<byte> GetSites() {
            if (!ParseDone) return null;
            return new List<byte>(_rawData._sites.Keys);
        }
        public Dictionary<byte, int> GetSitesChipCount() {
            if (!ParseDone) return null;
            Dictionary<byte, int> rst = new Dictionary<byte, int>();
            foreach (var v in _defaultSitesSummary) {
                rst.Add(v.Key, v.Value.TotalCount);
            }

            return rst;
        }
        public List<ushort> GetSoftBins() {
            return new List<ushort>(_defaultSummary.GetSoftBins().Keys);
        }
        public Dictionary<ushort, int> GetSoftBinsCount() {
            return new Dictionary<ushort, int>(_defaultSummary.GetSoftBins());
        }
        public List<ushort> GetHardBins() {
            return new List<ushort>(_defaultSummary.GetHardBins().Keys);
        }
        public Dictionary<ushort, int> GetHardBinsCount() {
            return new Dictionary<ushort, int>(_defaultSummary.GetHardBins());
        }
        public List<TestID> GetTestIDs() {
            return _rawData._testItems.GetTestIDsDefault();
        }
        public Dictionary<TestID, IItemInfo> GetTestIDs_Info() {
            return _rawData._testItems.GetTestIDs_Info();
        }
        public List<int> GetChipsIndexes() {
            return _rawData._testChips.GetChipsIndexes();
        }
        public int ChipsCount {
            get {
                return _rawData._testChips.ChipsCount;
            }
        }
        public List<IChipInfo> GetChipsInfo() {
            return _rawData._testChips.GetChipsInfo();
        }

        public Dictionary<byte, IChipSummary> GetChipSummaryBySite() {
            return _defaultSitesSummary;
        }
        public IChipSummary GetChipSummary() {
            return _defaultSummary;
        }

        public Dictionary<ushort, Tuple<string, string>> GetSBinInfo() {
            return _rawData._softBinNames;
        }
        public Dictionary<ushort, Tuple<string, string>> GetHBinInfo() {
            return _rawData._hardBinNames;
        }

        public Dictionary<TestID, IItemStatistic> GetStatistic() {
            return _defaultStatistic;
        }

        //////this info is filtered by filter
        public List<byte> GetFilteredSites(int filterId) {
            return new List<byte>(_filterList[filterId].SitesSummary.Keys);
        }
        public Dictionary<byte, int> GetFilteredSitesChipCount(int filterId) {
            Dictionary<byte, int> rst = new Dictionary<byte, int>();
            foreach (var v in _filterList[filterId].SitesSummary) {
                rst.Add(v.Key, v.Value.TotalCount);
            }

            return rst;
        }
        public List<ushort> GetFilteredSoftBins(int filterId) {
            return new List<ushort>(_filterList[filterId].Summary.GetSoftBins().Keys);
        }
        public Dictionary<ushort, int> GetFilteredSoftBinsCount(int filterId) {
            return new Dictionary<ushort, int>(_filterList[filterId].Summary.GetSoftBins());
        }
        public List<ushort> GetFilteredHardBins(int filterId) {
            return new List<ushort>(_filterList[filterId].Summary.GetHardBins().Keys);
        }
        public Dictionary<ushort, int> GetFilteredHardBinsCount(int filterId) {
            return new Dictionary<ushort, int>(_filterList[filterId].Summary.GetHardBins());
        }
        public List<TestID> GetFilteredTestIDs(int filterId) {
            return _rawData._testItems.GetTestIDsFiltered(_filterList[filterId].ItemFilter);
        }
        public Dictionary<TestID, IItemInfo> GetFilteredTestIDs_Info(int filterId) {
            return _rawData._testItems.GetTestIDs_InfoFiltered(_filterList[filterId].ItemFilter);
        }

        //public List<int> GetFilteredChipsIndexes(int filterId) {
        //    return _rawData._testChips.GetFilteredChipsIndexes(_filterList[filterId].ChipFilter);
        //}
        public List<IChipInfo> GetFilteredChipsInfo(int filterId) {
            return _rawData._testChips.GetFilteredChipsInfo(_filterList[filterId].ChipFilter);
        }
        public List<IChipInfo> GetFilteredChipsInfo(int startIndex, int count, int filterId) {
            return _rawData._testChips.GetFilteredChipsInfo(_filterList[filterId].ChipFilter).GetRange(startIndex, count);
        }

        /// <summary>
        /// return an array of the selected item data with the filter, 
        /// it will be null if the correspond partdon't have result there, 
        /// the filtered part won't take place in the array
        /// </summary>
        /// <param name="testID"></param>
        /// <returns></returns>
        public float?[] GetFilteredItemData(TestID testID, int filterId) {
            return _rawData.GetItemDataFiltered(testID, _filterList[filterId].ChipFilter);
        }

        public float?[] GetFilteredItemData(TestID testID, int startIndex, int count, int filterId) {
            return _rawData.GetItemDataFiltered(testID, startIndex, count, _filterList[filterId].ChipFilter);
        }

        public DataTable GetFilteredItemData(int startIndex, int count, int filterId, bool enableRowHeader) {
            DataTable table = new DataTable();

            if (enableRowHeader) {
                table.Columns.Add("TestNumber");
                table.Columns.Add("TestText");
                table.Columns.Add("LoLimit");
                table.Columns.Add("HiLimit");
                table.Columns.Add("Unit");
                table.Columns.Add("Min");
                table.Columns.Add("Max");
                table.Columns.Add("Mean");
                table.Columns.Add("Sigma");
                table.Columns.Add("CPK");
                table.Columns.Add("PassCount");
            }
            var chips = GetFilteredChipsInfo(filterId);
            if (count > chips.Count)
                count = chips.Count;

            table.Columns.AddRange(Enumerable.Range(startIndex, count).Select((x, DataColumn) => new DataColumn(x.ToString())).ToArray());

            //add chip info
            //new DataColumn($"{chips[x].PartId}<br><i>{chips[x].WaferCord}</i>{chips[x].HardBin}/n{chips[x].SoftBin}/n{chips[x].Result}/n{chips[x].Site}/n{chips[x].TestTime}ms")
            List<object> li = new List<object>(120);
            if (enableRowHeader) {
                li.Add("PartId");
                for (int i = 0; i < 10; i++)
                    li.Add(null);
            }
            for (int i = startIndex; i < startIndex + count; i++) {
                li.Add(chips[i].PartId);
            }
            table.Rows.Add(li.ToArray());

            li.Clear();
            if (enableRowHeader) {
                li.Add("WaferCord");
                for (int i = 0; i < 10; i++)
                    li.Add(null);
            }
            for (int i = startIndex; i < startIndex + count; i++) {
                li.Add(chips[i].WaferCord);
            }
            table.Rows.Add(li.ToArray());

            li.Clear();
            if (enableRowHeader) {
                li.Add("HardBin");
                for (int i = 0; i < 10; i++)
                    li.Add(null);
            }
            for (int i = startIndex; i < startIndex + count; i++) {
                li.Add(chips[i].HardBin);
            }
            table.Rows.Add(li.ToArray());

            li.Clear();
            if (enableRowHeader) {
                li.Add("SoftBin");
                for (int i = 0; i < 10; i++)
                    li.Add(null);
            }
            for (int i = startIndex; i < startIndex + count; i++) {
                li.Add(chips[i].SoftBin);
            }
            table.Rows.Add(li.ToArray());

            li.Clear();
            if (enableRowHeader) {
                li.Add("Result");
                for (int i = 0; i < 10; i++)
                    li.Add(null);
            }
            for (int i = startIndex; i < startIndex + count; i++) {
                li.Add(chips[i].Result);
            }
            table.Rows.Add(li.ToArray());

            li.Clear();
            if (enableRowHeader) {
                li.Add("Site");
                for (int i = 0; i < 10; i++)
                    li.Add(null);
            }
            for (int i = startIndex; i < startIndex + count; i++) {
                li.Add(chips[i].Site);
            }
            table.Rows.Add(li.ToArray());

            li.Clear();
            if (enableRowHeader) {
                li.Add("TestTime");
                for (int i = 0; i < 10; i++)
                    li.Add(null);
            }
            for (int i = startIndex; i < startIndex + count; i++) {
                li.Add(chips[i].TestTime);
            }
            table.Rows.Add(li.ToArray());


            foreach (var idInfo in GetFilteredTestIDs_Info(filterId)) {
                List<object> list = new List<object>(120);
                var statistic = GetFilteredStatistic(filterId);
                if (enableRowHeader) {
                    list.Add(idInfo.Key.GetGeneralTestNumber());
                    list.Add(idInfo.Value.TestText);
                    list.Add(idInfo.Value.LoLimit);
                    list.Add(idInfo.Value.HiLimit);
                    list.Add(idInfo.Value.Unit);
                    list.Add(statistic[idInfo.Key].MinValue);
                    list.Add(statistic[idInfo.Key].MaxValue);
                    list.Add(statistic[idInfo.Key].MeanValue);
                    list.Add(statistic[idInfo.Key].Sigma);
                    list.Add(statistic[idInfo.Key].Cpk);
                    list.Add(statistic[idInfo.Key].PassCount);
                }
                foreach (var v in _rawData.GetItemDataFiltered(idInfo.Key, startIndex, count, _filterList[filterId].ChipFilter)) {
                    list.Add(v);
                }
                table.Rows.Add(list.ToArray());
            }


            return table;
        }


        public Dictionary<byte, IChipSummary> GetFilteredChipSummaryBySite(int filterId) {
            return _filterList[filterId].SitesSummary;
        }
        public IChipSummary GetFilteredChipSummary(int filterId) {
            return _filterList[filterId].Summary;
        }

        public Dictionary<TestID, IItemStatistic> GetFilteredStatistic(int filterId) {
            return _filterList[filterId].StatisticList;
        }



        public void UpdateFilter(int filterId, FilterSetup newFilter) {
            _rawData._testChips.UpdateChipFilter(newFilter, ref _filterList[filterId].ChipFilter);
            _rawData._testItems.UpdateItemFilter(newFilter, ref _filterList[filterId].ItemFilter);

            _rawData._testChips.UpdateSummaryFiltered(_filterList[filterId].ChipFilter, ref _filterList[filterId].SitesSummary);
            _filterList[filterId].Summary = ChipSummary.Combine(_filterList[filterId].SitesSummary);

            foreach (var t in GetFilteredTestIDs_Info(filterId)) {
                //if(_filterList[filterId].StatisticList.ContainsKey(t.Key))
                _filterList[filterId].StatisticList[t.Key] = new ItemStatistic(GetFilteredItemData(t.Key, filterId), t.Value.LoLimit, t.Value.HiLimit);
                //else
                //    _filterList[filterId].StatisticList.Add(t.Key, new ItemStatistic(GetFilteredItemData(t.Key, filterId), t.Value.LoLimit, t.Value.HiLimit));
            }

        }

        public int CreateFilter() {
            int key = System.DateTime.UtcNow.Ticks.GetHashCode();
            while (_filterList.ContainsKey(key)) key++;
            _filterList.Add(key, new FilterData(new FilterSetup("all site"), _rawData._testChips.ChipsCount, _rawData._testItems.ItemsCount));

            //init the chip filter
            _filterList[key].ChipFilter.AddRange(Enumerable.Range(0, _rawData._testChips.ChipsCount));
            //item filter doesn't need init

            _rawData._testChips.UpdateSummaryFiltered(_filterList[key].ChipFilter, ref _filterList[key].SitesSummary);
            _filterList[key].Summary = ChipSummary.Combine(_filterList[key].SitesSummary);
            _filterList[key].StatisticList = new Dictionary<TestID, IItemStatistic>(_defaultStatistic);
            return key;
        }
        public int CreateFilter(FilterSetup filter) {
            int key = System.DateTime.UtcNow.Ticks.GetHashCode();
            while (_filterList.ContainsKey(key)) key++;
            _filterList.Add(key, new FilterData(filter, _rawData._testChips.ChipsCount, _rawData._testItems.ItemsCount));

            _rawData._testChips.UpdateChipFilter(filter, ref _filterList[key].ChipFilter);
            _rawData._testItems.UpdateItemFilter(filter, ref _filterList[key].ItemFilter);

            _rawData._testChips.UpdateSummaryFiltered(_filterList[key].ChipFilter, ref _filterList[key].SitesSummary);
            _filterList[key].Summary = ChipSummary.Combine(_filterList[key].SitesSummary);

            _filterList[key].StatisticList = new Dictionary<TestID, IItemStatistic>();
            foreach (var t in GetFilteredTestIDs_Info(key)) {
                _filterList[key].StatisticList.Add(t.Key, new ItemStatistic(GetFilteredItemData(t.Key, key), t.Value.LoLimit, t.Value.HiLimit));
            }

            return key;
        }

        public int CreateFilterCopy(int filterId) {
            int key = System.DateTime.UtcNow.Ticks.GetHashCode();
            while (_filterList.ContainsKey(key)) key++;

            if (_filterList.ContainsKey(filterId))
                _filterList.Add(key, FilterData.DeepCopy<FilterData>(_filterList[filterId]));
            else
                throw new Exception("no filter");
            return key;
        }

        public Dictionary<int, FilterSetup> GetAllFilter() {
            var rst = (from r in _filterList
                       select new KeyValuePair<int, FilterSetup>(r.Key, r.Value.Filter)).ToDictionary(k => k.Key, k => k.Value);

            return rst;
        }

        //public int GetFilterID(byte? site) {
        //    //if(site is null) {
        //    //    CreateFilter("Raw Data");
        //    //    return _filterList.ElementAt(0).Key;
        //    //} else {
        //    //    FilterSetup f = new FilterSetup("Site:" + site);
        //    //    f.EnableSingleSite(_rawData._sites.Keys.ToList(), site.Value);
        //    //    CreateFilter(f);

        //    //    return _filterList.ElementAt(_rawData._sites[site.Value] + 1).Key;
        //    //}
        //}

        public FilterSetup GetFilterSetup(int filterId) {
            if (!_filterList.ContainsKey(filterId)) return null;

            return _filterList[filterId].Filter;
        }

        public void RemoveFilter(int filterId) {
            _filterList.Remove(filterId);
        }

        public int GetFilterIndex(int filterId) {
            for (int i = 0; i < _filterList.Count; i++) {
                if (filterId == _filterList.ElementAt(i).Key)
                    return i;
            }
            return -1;
        }

        private void CreateDefaultFilters() {
            CreateFilter();

            foreach (var v in _rawData._sites) {
                FilterSetup f = new FilterSetup("Site:" + v.Key);
                f.EnableSingleSite(_rawData._sites.Keys.ToList(), v.Key);
                CreateFilter(f);
            }
        }

        public void CleanUp() {
            _rawData = null;

            FilePath = null;
            FileName = null;

            //basic file information
            ExtractDone = null;

            _filterList = null;

            _defaultSitesSummary = null;
            _defaultSummary = null;

            GC.Collect();
        }

        public void Dispose() {
            CleanUp();
        }

        private StdV4Reader _v4Reader;
        private RawData _rawData;

        public StdReader(string path, StdFileType stdFileType) {
            FilePath = path;
            FileName = Path.GetFileName(path);

            switch (stdFileType) {
                case StdFileType.STD:
                    _v4Reader = new StdV4Reader(path);
                    break;
                case StdFileType.STD_GZ:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }       
        }


    }
}
