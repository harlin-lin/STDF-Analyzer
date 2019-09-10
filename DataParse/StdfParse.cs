﻿using System;
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

                    _testChips.AddChip(new ChipInfo((Prr)r, InternalID[siteIdx]));
                }else if (r.RecordType == StdfFile.TSR) {
                    _testItems.UpdateTestText(new TestID(((Tsr)r).TestNumber), ((Tsr)r).TestLabel);
                }else if (r.RecordType == StdfFile.MRR) {
                    BasicInfo.AddMrr((Mrr)r);
                }

            }
            ParseDone = true;

            _stdfFile = null;
            rs = null;

            GC.Collect();


        }

        ////property get the file default infomation
        public List<byte> GetSites() {
            return new List<byte>(_sites.Keys);
        }
        public Dictionary<byte, int> GetSitesChipCount() {
            var vv = _testChips.GetChipSummaryBySiteDefault();
            Dictionary<byte, int> rst = new Dictionary<byte, int>();
            foreach(var v in vv) {
                rst.Add(v.Key, v.Value.TotalCount);
            }

            return rst;
        }
        public List<ushort> GetSoftBins() {
            return new List<ushort>(_testChips.GetChipSummaryDefault().GetSoftBins().Keys);
        }
        public Dictionary<ushort, int> GetSoftBinsCount() {
            return new Dictionary<ushort, int>(_testChips.GetChipSummaryDefault().GetSoftBins());
        }
        public List<ushort> GetHardBins() {
            return new List<ushort>(_testChips.GetChipSummaryDefault().GetHardBins().Keys);
        }
        public Dictionary<ushort, int> GetHardBinsCount() {
            return new Dictionary<ushort, int>(_testChips.GetChipSummaryDefault().GetHardBins());
        }
        public List<TestID> GetTestIDs() {
            return _testItems.GetTestIDsDefault();
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
        public List<byte> GetFilteredSites() {
            return new List<byte>(_testChips.GetChipSummaryBySiteFiltered().Keys);
        }
        public Dictionary<byte, int> GetFilteredSitesChipCount() {
            var vv = _testChips.GetChipSummaryBySiteFiltered();
            Dictionary<byte, int> rst = new Dictionary<byte, int>();
            foreach (var v in vv) {
                rst.Add(v.Key, v.Value.TotalCount);
            }

            return rst;
        }
        public List<ushort> GetFilteredSoftBins() {
            return new List<ushort>(_testChips.GetChipSummaryFiltered().GetSoftBins().Keys);
        }
        public Dictionary<ushort, int> GetFilteredSoftBinsCount() {
            return new Dictionary<ushort, int>(_testChips.GetChipSummaryFiltered().GetSoftBins());
        }
        public List<ushort> GetFilteredHardBins() {
            return new List<ushort>(_testChips.GetChipSummaryFiltered().GetHardBins().Keys);
        }
        public Dictionary<ushort, int> GetFilteredHardBinsCount() {
            return new Dictionary<ushort, int>(_testChips.GetChipSummaryFiltered().GetHardBins());
        }
        public List<TestID> GetFilteredTestIDs() {
            return _testItems.GetTestIDsFiltered();
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
            return _rawData.GetItemDataFiltered(_testItems.GetIndex(testID), _testChips.GetChipFilter());
        }

        public List<float?> GetFilteredItemData(TestID testID, List<byte> sites) {
            return _rawData.GetItemDataFiltered(_testItems.GetIndex(testID), _testChips.GetChipFilter(sites));
        }

        public DataTable GetFilteredData(int startIndex, int count, List<byte> sites) { throw new NotImplementedException(); }


        public Dictionary<byte, ChipSummary> GetChipSummaryBySite() {
            return _testChips.GetChipSummaryBySiteFiltered();
        }
        public Dictionary<byte, ChipSummary> GetChipSummaryBySite(List<byte> sites) {
            return _testChips.GetChipSummaryBySiteFiltered(sites);
        }
        public ChipSummary GetChipSummary() {
            return _testChips.GetChipSummaryFiltered();
        }
        public ChipSummary GetChipSummary(List<byte> sites) {
            return _testChips.GetChipSummaryFiltered(sites);
        }

        public void SetFilter(Filter filter) {
            _testChips.UpdateChipFilter(filter);
            _testItems.UpdateItemFilter(filter);
        }


    }
}
