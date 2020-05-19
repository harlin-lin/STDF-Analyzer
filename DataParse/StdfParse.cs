using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StdfReader;
using System.IO;
using StdfReader.Records.V4;
using DataInterface;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.ComponentModel;

namespace DataParse{

    public class StdfParse : IDataAcquire {
        [Serializable]
        private class FilterData {
            public FilterSetup Filter { get; private set; }
            public bool[] ChipFilter;
            public bool[] ItemFilter;
            public Dictionary<byte, IChipSummary> SitesSummary;
            public IChipSummary Summary;
            public Dictionary<TestID, IItemStatistic> StatisticList;

            public FilterData(FilterSetup filterSetup, int chipsCount, int itemsCount) {
                Filter = filterSetup;
                ChipFilter = new bool[chipsCount];
                ItemFilter = new bool[itemsCount];
                SitesSummary = new Dictionary<byte, IChipSummary>();
                Summary = null;
                StatisticList = new Dictionary<TestID, IItemStatistic>();
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

        private StdfFile _stdfFile;
        private RawData _rawData;
        private TestChips _testChips;
        private TestItems _testItems;
        private Dictionary<byte, int> _sites;

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

        private bool _filterDone;
        public bool FilterDone {
            get { return _filterDone; }
            private set {
                _filterDone = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilterDone"));
            }
        }

        //basic file information
        public IFileBasicInfo BasicInfo { get; private set; }

        private Dictionary<int, FilterData> _filterList;

        private Dictionary<byte, IChipSummary> _defaultSitesSummary;
        private IChipSummary _defaultSummary;

        private Dictionary<ushort, Tuple<string, string>> _softBinNames;
        private Dictionary<ushort, Tuple<string, string>> _hardBinNames;

        public event ExtractDoneEventHandler ExtractDone;
        public event ExtractDoneEventHandler FilterGenerated;
        public event PropertyChangedEventHandler PropertyChanged;

        public StdfParse(String filePath) {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            ParseDone = false;
            FilterDone = false;
            _stdfFile = null;
            _sites = new Dictionary<byte, int>();

            _rawData = new RawData();
            _testChips = new TestChips(RawData.DefaultFixedDataBlockLength);
            _testItems = new TestItems(RawData.DefaultItemsCapacity);

            BasicInfo = null;

            _filterList = new Dictionary<int, FilterData>();
            _defaultSitesSummary = new Dictionary<byte, IChipSummary>();
            _defaultSummary = null;

            _softBinNames = new Dictionary<ushort, Tuple<string, string>>();
            _hardBinNames = new Dictionary<ushort, Tuple<string, string>>();
        }

        public override int GetHashCode() {
            return FilePath.GetHashCode();
        }


        public void ExtractStdf() {
            if (ParseDone) return;
            //private data
            bool[] catchedPirFlag = null;
            TestID[] ptrLastTN = null;
            int[] InternalID = null;

            _stdfFile = new StdfFile(FilePath);


            List<PinMapRecord>  listPinMaps = new List<PinMapRecord>();
            List<PinGroupRecord> listPinGroups = new List<PinGroupRecord>();

            var rs = from r in _stdfFile.GetRecords()
                     select r;
            int siteIdx;

            foreach (var r in rs) {
                if (r.RecordType == StdfFile.PTR) {
                    if (!_sites.TryGetValue(((Ptr)r).SiteNumber, out siteIdx)) throw new Exception("No Site");
                    if (!catchedPirFlag[siteIdx])
                        throw new Exception("PIR Data Error");

                    TestID testID;
                    //compare with the previous test name to decide the testNO
                    if (ptrLastTN[siteIdx].CompareTestNumber(((Ptr)r).TestNumber) && _testItems.ItemsCount>0) {//it's a sub test
                        testID = TestID.NewSubTestID(ptrLastTN[siteIdx]);
                        if (!_testItems.ExistTestItem(testID)) {
                            IItemInfo info = _testItems.GetItemInfo(ptrLastTN[siteIdx]);
                            info.SetTestText(((Ptr)r).TestText);
                            _testItems.AddTestItem(testID, info);
                            _rawData.AddItem();
                        }
                    } else {
                        testID = new TestID(((Ptr)r).TestNumber);
                        if (!_testItems.ExistTestItem(testID)) {
                            _testItems.AddTestItem(testID, new ItemInfo(((Ptr)r).TestText, ((Ptr)r).LowLimit, ((Ptr)r).HighLimit, 
                                ((Ptr)r).Units, ((Ptr)r).LowLimitScalingExponent, ((Ptr)r).HighLimitScalingExponent, ((Ptr)r).ResultScalingExponent));
                            _rawData.AddItem();
                        }
                    }
                    Rst rst = new Rst(_testItems.GetItemInfo(testID).GetScaledRst(((Ptr)r).Result), _testItems.GetItemInfo(testID).LoLimit, _testItems.GetItemInfo(testID).HiLimit);
                    _rawData.Set(_testItems.GetIndex(testID), InternalID[siteIdx], rst);

                    ptrLastTN[siteIdx] = testID;                    
                } else if (r.RecordType == StdfFile.FTR) {
                    if (!_sites.TryGetValue(((Ftr)r).SiteNumber, out siteIdx)) throw new Exception("No Site");

                    if (!catchedPirFlag[siteIdx])
                        throw new Exception("PIR Data Error");

                    TestID testID;
                    //compare with the previous test name to decide the testNO
                    if (!ptrLastTN[siteIdx].CompareTestNumber(((Ftr)r).TestNumber)) {//it's a unused test, ftr do not set sub test
                        testID = new TestID(((Ftr)r).TestNumber);

                        if (!_testItems.ExistTestItem(testID)) {
                            _testItems.AddTestItem(testID, new ItemInfo(((Ftr)r).TestText, (float)0.5, (float)1.5, "", 0, 0, 0));
                            _rawData.AddItem();
                        }
                        Rst rst = new Rst(((Ftr)r).Results, (float)0.5, (float)1.5);
                        _rawData.Set(_testItems.GetIndex(testID), InternalID[siteIdx], rst);

                        ptrLastTN[siteIdx] = testID;
                    }
                } else if (r.RecordType == StdfFile.MPR) {
                    if (!_sites.TryGetValue(((Mpr)r).SiteNumber, out siteIdx)) throw new Exception("No Site");

                    if (!catchedPirFlag[siteIdx])
                        throw new Exception("PIR Data Error");

                    TestID testItemID;
                    if (ptrLastTN[siteIdx].CompareTestNumber(((Mpr)r).TestNumber)) {//it's a sub test
                        testItemID = TestID.NewSubTestID(ptrLastTN[siteIdx]);
                    } else {
                        testItemID = new TestID(((Mpr)r).TestNumber);
                    }

                    TestID testID = testItemID;

                    for (uint i = 0; i < ((Mpr)r).Results.Count(); i++) {
                        PinMapRecord pin = new PinMapRecord();
                        if (siteIdx == 0 && ((Mpr)r).PinIndexes != null)
                            pin = listPinMaps.Find(x => x.PinIndex == ((Mpr)r).PinIndexes[i]);
                        //else
                        //    throw new Exception("MPR Pin doesn't exist");

                        if(i > 0)
                            testID = TestID.NewSubTestID(testID);

                        if (!_testItems.ExistTestItem(testID)) {
                            _testItems.AddTestItem(testID, new ItemInfo(((Mpr)r).TestText + "<>" + pin.LogicalName, ((Mpr)r).LowLimit, ((Mpr)r).HighLimit,
                                ((Mpr)r).Units, ((Mpr)r).LowLimitScalingExponent, ((Mpr)r).HighLimitScalingExponent, ((Mpr)r).ResultScalingExponent));
                            _rawData.AddItem();
                        }

                        Rst rst = new Rst(_testItems.GetItemInfo(testID).GetScaledRst(((Mpr)r).Results[i]), _testItems.GetItemInfo(testID).LoLimit, _testItems.GetItemInfo(testID).HiLimit);
                        _rawData.Set(_testItems.GetIndex(testID), InternalID[siteIdx], rst);
                    }

                    ptrLastTN[siteIdx] = testItemID;
                } else if (r.RecordType == StdfFile.MIR) {
                    BasicInfo = new FileBasicInfo((Mir)r);
                    continue;
                } else if (r.RecordType == StdfFile.SDR) {
                    for (int i = 0; i < ((Sdr)r).SiteNumbers.Length; i++)
                        _sites.Add(((Sdr)r).SiteNumbers[i], i);

                    catchedPirFlag = new bool[((Sdr)r).SiteNumbers.Count()];
                    InternalID = new int[((Sdr)r).SiteNumbers.Count()];

                    ptrLastTN = new TestID[((Sdr)r).SiteNumbers.Count()];
                } else if (r.RecordType == StdfFile.PMR) {
                    listPinMaps.Add(new PinMapRecord((Pmr)r));
                } else if (r.RecordType == StdfFile.PGR) {
                    listPinGroups.Add(new PinGroupRecord((Pgr)r, listPinMaps));
                } else if (r.RecordType == StdfFile.PIR) {
                    if (!_sites.TryGetValue(((Pir)r).SiteNumber, out siteIdx)) throw new Exception("No Site");

                    if (!catchedPirFlag[siteIdx])
                        catchedPirFlag[siteIdx] = true;
                    else
                        throw new Exception("PIR Data Error");

                    ptrLastTN[siteIdx] = new TestID();

                    InternalID[siteIdx] = _rawData.AddChip();

                } else if (r.RecordType == StdfFile.SBR) {
                    if (!_softBinNames.ContainsKey(((Sbr)r).BinNumber)) {
                        _softBinNames.Add(((Sbr)r).BinNumber, new Tuple<string,string>(((Sbr)r).BinName, ((Sbr)r).BinPassFail));
                    }
                } else if (r.RecordType == StdfFile.HBR) {
                    if (!_hardBinNames.ContainsKey(((Hbr)r).BinNumber)) {
                        _hardBinNames.Add(((Hbr)r).BinNumber, new Tuple<string, string>(((Hbr)r).BinName, ((Hbr)r).BinPassFail));
                    }
                }
                else if (r.RecordType == StdfFile.PRR) {
                    if (!_sites.TryGetValue(((Prr)r).SiteNumber, out siteIdx)) throw new Exception("No Site");

                    if (!catchedPirFlag[siteIdx])
                        throw new Exception("PIR Data Error");
                    else
                        catchedPirFlag[siteIdx] = false;

                    _testChips.AddChip(new ChipInfo((Prr)r, InternalID[siteIdx]));
                }else if (r.RecordType == StdfFile.TSR) {

                    var v= ((Tsr)r).TestLabel;

                    if ( v!= null && v != "")
                        _testItems.UpdateTestText(new TestID(((Tsr)r).TestNumber), v);
                }else if (r.RecordType == StdfFile.MRR) {
                    ((FileBasicInfo)BasicInfo).AddMrr((Mrr)r);
                }

            }

            _testChips.UpdateSummary(ref _defaultSitesSummary);
            _defaultSummary = ChipSummary.Combine(_defaultSitesSummary);
            ParseDone = true;
            ExtractDone?.Invoke(this);

            _stdfFile = null;
            rs = null;

            GC.Collect();

            CreateDefaultFilters();
            FilterDone = true;
            FilterGenerated?.Invoke(this);
        }

        ////property get the file default infomation
        public List<byte> GetSites() {
            if (!ParseDone) return null;
            return new List<byte>(_sites.Keys);
        }
        public Dictionary<byte, int> GetSitesChipCount() {
            if (!ParseDone) return null;
            Dictionary<byte, int> rst = new Dictionary<byte, int>();
            foreach(var v in _defaultSitesSummary) {
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
            return _testItems.GetTestIDsDefault();
        }
        public Dictionary<TestID, IItemInfo> GetTestIDs_Info() {
            return _testItems.GetTestIDs_Info();
        }
        public List<int> GetChipsIndexes() {
            return _testChips.GetChipsIndexes();
        }
        public int ChipsCount {
            get {
                return _testChips.ChipsCount;
            }
        }
        public Dictionary<byte, IChipSummary> GetChipSummaryBySite() {
            return _defaultSitesSummary;
        }
        public IChipSummary GetChipSummary() {
            return _defaultSummary;
        }

        public Dictionary<ushort, Tuple<string, string>> GetSBinInfo() {
            return _softBinNames;
        }
        public Dictionary<ushort, Tuple<string, string>> GetHBinInfo() {
            return _hardBinNames;
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
            return _testItems.GetTestIDsFiltered(_filterList[filterId].ItemFilter);
        }
        public Dictionary<TestID, IItemInfo> GetFilteredTestIDs_Info(int filterId) {
            return _testItems.GetTestIDs_InfoFiltered(_filterList[filterId].ItemFilter);
        }

        public List<int> GetFilteredChipsIndexes(int filterId) {
            return _testChips.GetFilteredChipsIndexes(_filterList[filterId].ChipFilter);
        }
        public List<IChipInfo> GetFilteredChipsInfo(int filterId) {
            return _testChips.GetFilteredChipsInfo(_filterList[filterId].ChipFilter);
        }
        public List<IChipInfo> GetFilteredChipsInfo(int startIndex, int count, int filterId) {
            return _testChips.GetFilteredChipsInfo(_filterList[filterId].ChipFilter).GetRange(startIndex, count);
        }

        /// <summary>
        /// return an array of the selected item data with the filter, 
        /// it will be null if the correspond partdon't have result there, 
        /// the filtered part won't take place in the array
        /// </summary>
        /// <param name="testID"></param>
        /// <returns></returns>
        public List<Rst> GetFilteredItemData(TestID testID, int filterId) {
            return _rawData.GetItemDataFiltered(_testItems.GetIndex(testID), _filterList[filterId].ChipFilter);
        }

        public List<Rst> GetFilteredItemData(TestID testID, int startIndex, int count, int filterId) {
            return _rawData.GetItemDataFiltered(_testItems.GetIndex(testID), startIndex, count, _filterList[filterId].ChipFilter);
        }
        public Rst[] GetFilteredItemDataArr(TestID testID, int startIndex, int count, int filterId) {
            return _rawData.GetItemDataFilteredArr(_testItems.GetIndex(testID), startIndex, count, _filterList[filterId].ChipFilter);
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

            table.Columns.AddRange(Enumerable.Range(startIndex, count).Select((x, DataColumn) => new DataColumn( x.ToString())).ToArray());

            //add chip info
            //new DataColumn($"{chips[x].PartId}<br><i>{chips[x].WaferCord}</i>{chips[x].HardBin}/n{chips[x].SoftBin}/n{chips[x].Result}/n{chips[x].Site}/n{chips[x].TestTime}ms")
            List<object> li = new List<object>(120);
            if (enableRowHeader) {
                li.Add("PartId");
                for (int i=0; i<10; i++)
                    li.Add(null);
            }
            for(int i=startIndex; i< startIndex + count; i++) {
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
                foreach(var v in _rawData.GetItemDataFiltered(_testItems.GetIndex(idInfo.Key), startIndex, count, _filterList[filterId].ChipFilter)) {
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
            _testChips.UpdateChipFilter(newFilter, ref _filterList[filterId].ChipFilter);
            _testItems.UpdateItemFilter(newFilter, ref _filterList[filterId].ItemFilter);

            _testChips.UpdateSummaryFiltered(_filterList[filterId].ChipFilter, ref _filterList[filterId].SitesSummary);
            _filterList[filterId].Summary = ChipSummary.Combine(_filterList[filterId].SitesSummary);

            foreach (var t in GetFilteredTestIDs_Info(filterId)) {
                //if(_filterList[filterId].StatisticList.ContainsKey(t.Key))
                    _filterList[filterId].StatisticList[t.Key] = new ItemStatistic(GetFilteredItemData(t.Key, filterId), t.Value.LoLimit, t.Value.HiLimit);
                //else
                //    _filterList[filterId].StatisticList.Add(t.Key, new ItemStatistic(GetFilteredItemData(t.Key, filterId), t.Value.LoLimit, t.Value.HiLimit));
            }

        }

        private int CreateFilter(string comment) {
            int key = System.DateTime.UtcNow.Ticks.GetHashCode();
            while (_filterList.ContainsKey(key)) key++;
            _filterList.Add(key, new FilterData(new FilterSetup(comment), _testChips.ChipsCount, _testItems.ItemsCount));

            _testChips.UpdateSummaryFiltered(_filterList[key].ChipFilter, ref _filterList[key].SitesSummary);
            _filterList[key].Summary = ChipSummary.Combine(_filterList[key].SitesSummary);
            foreach (var t in GetFilteredTestIDs_Info(key)) {
                _filterList[key].StatisticList.Add(t.Key, new ItemStatistic(GetFilteredItemData(t.Key, key), t.Value.LoLimit, t.Value.HiLimit));
            }

            return key;
        }
        public int CreateFilter(FilterSetup filter) {
            int key = System.DateTime.UtcNow.Ticks.GetHashCode();
            while (_filterList.ContainsKey(key)) key++;
            _filterList.Add(key, new FilterData(filter, _testChips.ChipsCount, _testItems.ItemsCount));


            _testChips.UpdateChipFilter(filter, ref _filterList[key].ChipFilter);
            _testItems.UpdateItemFilter(filter, ref _filterList[key].ChipFilter);

            _testChips.UpdateSummaryFiltered(_filterList[key].ChipFilter, ref _filterList[key].SitesSummary);
            _filterList[key].Summary = ChipSummary.Combine(_filterList[key].SitesSummary);
            foreach (var t in GetFilteredTestIDs_Info(key)) {
                _filterList[key].StatisticList.Add(t.Key, new ItemStatistic(GetFilteredItemData(t.Key, key), t.Value.LoLimit, t.Value.HiLimit));
            }

            return key;
        }

        public int CreateFilterCopy(int filterId) {
            int key = System.DateTime.UtcNow.Ticks.GetHashCode();
            while(_filterList.ContainsKey(key)) key++;

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

        public int GetFilterID(byte? site) {
            return _filterList.ElementAt(site.HasValue ? (_sites[site.Value] + 1) : 0).Key;
        }

        public FilterSetup GetFilterSetup(int filterId) {
            if (!_filterList.ContainsKey(filterId)) return null;

            return _filterList[filterId].Filter;
        }

        public void RemoveFilter(int filterId) {
            _filterList.Remove(filterId);
        }

        public int GetFilterIndex(int filterId) {
            for(int i=0; i< _filterList.Count; i++) {
                if (filterId == _filterList.ElementAt(i).Key)
                    return i;
            }
            return -1;
        }

        private void CreateDefaultFilters() {
            CreateFilter("Full File");

            foreach (var v in _sites) {
                FilterSetup f= new FilterSetup("Site:" + v.Key);
                f.EnableSingleSite(_sites.Keys.ToList(), v.Key);
                CreateFilter(f);
            }
        }

        public void CleanUp() {
            _stdfFile=null;
            _rawData=null;
            _testChips = null;
            _testItems = null;
            _sites = null;

            FilePath = null;
            FileName = null;

            //basic file information
            BasicInfo = null;
            ExtractDone = null;

            _filterList = null;

            _defaultSitesSummary = null;
            _defaultSummary = null;

            GC.Collect();
        }
    }
}
