using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StdfReader;
using System.IO;
using StdfReader.Records.V4;

namespace DataParse{

    public class StdfParse: IDataAcquire{
        private class FilterData {
            public string Comment;
            public bool[] ChipFilter;
            public bool[] ItemFilter;
            public Dictionary<byte, ChipSummary> SitesSummary;
            public ChipSummary Summary;
            public Dictionary<TestID, ItemStatistic> StatisticList;

            public FilterData(int chipsCount, int itemsCount, string comment) {
                Comment = comment;
                ChipFilter = new bool[chipsCount];
                ItemFilter = new bool[itemsCount];
                SitesSummary = new Dictionary<byte, ChipSummary>();
                Summary = null;
                StatisticList = new Dictionary<TestID, ItemStatistic>();
            }
        }

        private StdfFile _stdfFile;
        private RawData _rawData;
        private TestChips _testChips;
        private TestItems _testItems;
        private Dictionary<byte, int> _sites;

        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public bool ParseDone { get; private set; }
        //basic file information
        public FileBasicInfo BasicInfo { get; private set; }
        public int ParsePercent {
            get { throw new NotImplementedException(); }
            private set { }
        }

        private Dictionary<int, FilterData> _filterList;

        private Dictionary<byte, ChipSummary> _defaultSitesSummary;
        private ChipSummary _defaultSummary;

        public StdfParse(String filePath) {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            ParseDone = false;
            _stdfFile = null;
            _sites = new Dictionary<byte, int>();

            _rawData = new RawData();
            _testChips = new TestChips(RawData.DefaultFixedDataBlockLength);
            _testItems = new TestItems(RawData.DefaultItemsCapacity);

            BasicInfo = null;
            ParsePercent = 0;

            _filterList = new Dictionary<int, FilterData>();
            _defaultSitesSummary = new Dictionary<byte, ChipSummary>();
            _defaultSummary = null;
        }

        public override int GetHashCode() {
            return FilePath.GetHashCode();
        }


        public void ExtractStdf() {
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
                    if (ptrLastTN[siteIdx].CompareTestNumber(((Ptr)r).TestNumber)) {//it's a sub test
                        testID = TestID.NewSubTestID(ptrLastTN[siteIdx]);
                        if (!_testItems.ExistTestItem(testID)) {
                            ItemInfo info = _testItems.GetItemInfo(ptrLastTN[siteIdx]);
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
                    _rawData.Set(_testItems.GetIndex(testID), InternalID[siteIdx], _testItems.GetItemInfo(testID).GetScaledRst(((Ptr)r).Result));

                    ptrLastTN[siteIdx] = testID;                    
                } else if (r.RecordType == StdfFile.FTR) {
                    if (!_sites.TryGetValue(((Ftr)r).SiteNumber, out siteIdx)) throw new Exception("No Site");

                    if (!catchedPirFlag[siteIdx])
                        throw new Exception("PIR Data Error");

                    TestID testID;
                    //compare with the previous test name to decide the testNO
                    if (ptrLastTN[siteIdx].CompareTestNumber(((Ftr)r).TestNumber)) {//it's a sub test
                        testID = TestID.NewSubTestID(ptrLastTN[siteIdx]);
                    } else {
                        testID = new TestID(((Ftr)r).TestNumber);
                    }

                    if (!_testItems.ExistTestItem(testID)) {
                        _testItems.AddTestItem(testID, new ItemInfo(((Ftr)r).TestText, (float)0.5, (float)1.5, "", 0, 0, 0));
                        _rawData.AddItem();
                    }

                    _rawData.Set(_testItems.GetIndex(testID), InternalID[siteIdx], ((Ftr)r).Results);

                    ptrLastTN[siteIdx] = testID;
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

                        _rawData.Set(_testItems.GetIndex(testID), InternalID[siteIdx], _testItems.GetItemInfo(testID).GetScaledRst(((Mpr)r).Results[i]));
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

                } else if (r.RecordType == StdfFile.BPS) {
                    //do nothing
                } else if (r.RecordType == StdfFile.EPS) {
                    //do nothing
                } else if (r.RecordType == StdfFile.PRR) {
                    if (!_sites.TryGetValue(((Prr)r).SiteNumber, out siteIdx)) throw new Exception("No Site");

                    if (!catchedPirFlag[siteIdx])
                        throw new Exception("PIR Data Error");
                    else
                        catchedPirFlag[siteIdx] = false;

                    _testChips.AddChip(new ChipInfo((Prr)r, InternalID[siteIdx]));
                }else if (r.RecordType == StdfFile.TSR) {


                    _testItems.UpdateTestText(new TestID(((Tsr)r).TestNumber), ((Tsr)r).TestLabel);
                }else if (r.RecordType == StdfFile.MRR) {
                    BasicInfo.AddMrr((Mrr)r);
                }

            }
            ParseDone = true;

            _testChips.UpdateSummary(ref _defaultSitesSummary);
            _defaultSummary = ChipSummary.Combine(_defaultSitesSummary);

            _stdfFile = null;
            rs = null;

            GC.Collect();

            CreateDefaultFilters();
        }

        ////property get the file default infomation
        public List<byte> GetSites() {
            return new List<byte>(_sites.Keys);
        }
        public Dictionary<byte, int> GetSitesChipCount() {
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
        public Dictionary<TestID, ItemInfo> GetTestIDs_Info() {
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
        public Dictionary<byte, ChipSummary> GetChipSummaryBySite() {
            return _defaultSitesSummary;
        }
        public ChipSummary GetChipSummary() {
            return _defaultSummary;
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
        public Dictionary<TestID, ItemInfo> GetFilteredTestIDs_Info(int filterId) {
            return _testItems.GetTestIDs_InfoFiltered(_filterList[filterId].ItemFilter);
        }

        public List<int> GetFilteredChipsIndexes(int filterId) {
            return _testChips.GetFilteredChipsIndexes(_filterList[filterId].ChipFilter);
        }
        public List<ChipInfo> GetFilteredChipsInfo(int filterId) {
            return _testChips.GetFilteredChipsInfo(_filterList[filterId].ChipFilter);
        }
        /// <summary>
        /// return an array of the selected item data with the filter, 
        /// it will be null if the correspond partdon't have result there, 
        /// the filtered part won't take place in the array
        /// </summary>
        /// <param name="testID"></param>
        /// <returns></returns>
        public List<float?> GetFilteredItemData(TestID testID, int filterId) {
            return _rawData.GetItemDataFiltered(_testItems.GetIndex(testID), _filterList[filterId].ChipFilter);
        }

        public List<float?> GetFilteredItemData(TestID testID, int startIndex, int count, int filterId) {
            return _rawData.GetItemDataFiltered(_testItems.GetIndex(testID), startIndex, count, _filterList[filterId].ChipFilter);
        }

        public Dictionary<byte, ChipSummary> GetFilteredChipSummaryBySite(int filterId) {
            return _filterList[filterId].SitesSummary;
        }
        public ChipSummary GetFilteredChipSummary(int filterId) {
            return _filterList[filterId].Summary;
        }

        public Dictionary<TestID, ItemStatistic> GetFilteredStatistic(int filterId) {
            return _filterList[filterId].StatisticList;
        }



        public void SetFilter(int filterId, FilterSetup filter) {
            _testChips.UpdateChipFilter(filter, ref _filterList[filterId].ChipFilter);
            _testItems.UpdateItemFilter(filter, ref _filterList[filterId].ItemFilter);

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
            _filterList.Add(key, new FilterData(_testChips.ChipsCount, _testItems.ItemsCount, comment));

            return key;
        }
        public int CreateFilter(FilterSetup filter, string comment) {
            int key = CreateFilter(comment);

            _testChips.UpdateChipFilter(filter, ref _filterList[key].ChipFilter);
            _testItems.UpdateItemFilter(filter, ref _filterList[key].ChipFilter);

            _testChips.UpdateSummaryFiltered(_filterList[key].ChipFilter, ref _filterList[key].SitesSummary);
            _filterList[key].Summary = ChipSummary.Combine(_filterList[key].SitesSummary);
            foreach (var t in GetFilteredTestIDs_Info(key)) {
                _filterList[key].StatisticList.Add(t.Key, new ItemStatistic(GetFilteredItemData(t.Key, key), t.Value.LoLimit, t.Value.HiLimit));
            }

            return key;
        }

        public Dictionary<int, string> GetAllFilter() {
            var rst = (from r in _filterList
                       select new KeyValuePair<int, string>(r.Key, r.Value.Comment)).ToDictionary(k => k.Key, k => k.Value);

            return rst;
        }
        public void RemoveFilter(int filterId) {
            _filterList.Remove(filterId);
        }

        private void CreateDefaultFilters() {
            int dKey = CreateFilter("Full File");
            _testChips.UpdateSummaryFiltered(_filterList[dKey].ChipFilter, ref _filterList[dKey].SitesSummary);
            _filterList[dKey].Summary = ChipSummary.Combine(_filterList[dKey].SitesSummary);
            foreach(var t in GetFilteredTestIDs_Info(dKey)) {
                _filterList[dKey].StatisticList.Add(t.Key, new ItemStatistic(GetFilteredItemData(t.Key, dKey), t.Value.LoLimit, t.Value.HiLimit));
            }

            foreach (var v in _sites) {
                int key = CreateFilter("Site:" + v.Key);
                _testChips.UpdateChipFilter(v.Key, ref _filterList[key].ChipFilter);
                _testChips.UpdateSummaryFiltered(_filterList[key].ChipFilter, ref _filterList[key].SitesSummary);
                _filterList[key].Summary = ChipSummary.Combine(_filterList[key].SitesSummary);
                foreach (var t in GetFilteredTestIDs_Info(key)) {
                    _filterList[key].StatisticList.Add(t.Key, new ItemStatistic(GetFilteredItemData(t.Key, key), t.Value.LoLimit, t.Value.HiLimit));
                }

            }
        }
    }
}
