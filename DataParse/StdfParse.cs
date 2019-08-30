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
            //...
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
                    _rawData.Set(_testItems.GetIndex(testID), InternalID[siteIdx], ((Ptr)r).Result);

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

                    TestID testID = TestID.NewSubTestID(testItemID);

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

                        _rawData.Set(_testItems.GetIndex(testID), InternalID[siteIdx], ((Mpr)r).Results[i]);
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

                    _testChips.AddChip(new ChipInfo(((Prr)r).SiteNumber,
                        ((Prr)r).TestTime,
                        ((Prr)r).HardBin,
                        ((Prr)r).SoftBin,
                        ((Prr)r).PartId,
                        new CordType(((Prr)r).XCoordinate, ((Prr)r).YCoordinate),
                        ((((Prr)r).PartFlag & 0x18) == 0),
                        (((((Prr)r).PartFlag >> 0) & 0x01) != ((((Prr)r).PartFlag >> 1) & 0x01)),
                        InternalID[siteIdx]));
                } else if (r.RecordType == StdfFile.MRR) {
                    BasicInfo.AddMrr((Mrr)r);
                }

            }
            ParseDone = true;

            _stdfFile = null;
            rs = null;

            GC.Collect();


        }

        ////property get the file default infomation
        public List<byte> GetSites() { throw new NotImplementedException(); }
        public Dictionary<byte, int> GetSitesChipCount() { throw new NotImplementedException(); }
        public List<ushort> GetSoftBins() { throw new NotImplementedException(); }
        public Dictionary<ushort, int> GetSoftBinsCount() { throw new NotImplementedException(); }
        public List<ushort> GetHardBins() { throw new NotImplementedException(); }
        public Dictionary<ushort, int> GetHardBinsCount() { throw new NotImplementedException(); }
        public List<TestID> GetTestIDs() {
            return _testItems.GetTestIDs();
        }
        public ItemInfo GetItemInfo(TestID testID) {
            return _testItems.GetItemInfo(testID);
        }
        public List<int> GetChipsIndexes() {
            return _testChips.GetChipsIndexes();
        }
        public List<int> GetChipsIndexes(List<byte> sites) {
            return _testChips.GetChipsIndexes(sites);
        }
        public int ChipsCount {
            get {
                return _testChips.ChipsCount;
            }
        }

        ////this info is filtered by filter
        public List<byte> GetFilteredSites() { throw new NotImplementedException(); }
        public Dictionary<byte, int> GetFilteredSitesChipCount() { throw new NotImplementedException(); }
        public List<ushort> GetFilteredSoftBins() { throw new NotImplementedException(); }
        public Dictionary<ushort, int> GetFilteredSoftBinsCount() { throw new NotImplementedException(); }
        public List<ushort> GetFilteredHardBins() { throw new NotImplementedException(); }
        public Dictionary<ushort, int> GetFilteredHardBinsCount() { throw new NotImplementedException(); }
        public List<TestID> GetFilteredTestIDs() {
            return _testItems.GetFilteredTestIDs();
        }
        public List<int> GetFilteredChipsIndexes() {
            return _testChips.GetFilteredChipsIndexes();
        }
        public List<int> GetFilteredChipsIndexes(List<byte> sites) {
            return _testChips.GetFilteredChipsIndexes(sites);
        }
        public List<ChipInfo> GetFilteredChipsInfo() {
            return _testChips.GetFilteredChipsInfo();
        }
        public List<ChipInfo> GetFilteredChipsInfo(List<byte> sites) {
            return _testChips.GetFilteredChipsInfo(sites);
        }
        /// <summary>
        /// return an array of the selected item data with the filter, 
        /// it will be null if the correspond partdon't have result there, 
        /// the filtered part won't take place in the array
        /// </summary>
        /// <param name="testID"></param>
        /// <returns></returns>
        public List<float?> GetFilteredItemData(TestID testID) {
            return _rawData.GetItemData(_testItems.GetIndex(testID), _testChips.GetChipFilter());
        }

        public List<float?> GetFilteredItemData(TestID testID, List<byte> sites) {
            return _rawData.GetItemData(_testItems.GetIndex(testID), _testChips.GetChipFilter(sites));
        }

        ///// <summary>
        ///// Get the raw data with the applied filter
        ///// </summary>
        ///// <param name="startIndex">Start index is 0</param>
        ///// <param name="count">Will only return the actually availiable chips' data if the count greater than the actually selected chips' count</param>
        ///// <returns></returns>
        public DataTable GetFilteredData(int startIndex, int count) { throw new NotImplementedException(); }

        public DataTable GetFilteredData(int startIndex, int count, List<byte> sites) { throw new NotImplementedException(); }

        ///// <summary>
        ///// To get selected chips' data, will return null if all of the chips are filtered or all test items are filtered
        ///// It's not recommended to get the whole data by this method, please use [DataTable GetFilteredData(int startIndex, int count);] instead
        ///// </summary>
        ///// <param name="chipsId"></param>
        ///// <returns></returns>
        public DataTable GetFilteredData(List<int> chipsId) { throw new NotImplementedException(); }

        public DataTable GetFilteredData(List<int> chipsId, List<byte> sites) { throw new NotImplementedException(); }



    }
}
