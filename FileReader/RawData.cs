using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader {
    public class RawData :IFileBasicInfo{
        const int DefaultTestCount = 500;
        int ChipCount = 50000;

        public Dictionary<TestID, float?[]> _rawData;
        public uint _recordCnt;
        public Dictionary<byte, int> _sites;
        public Dictionary<byte, int> _bySitePartIdx;
        public int _partCnt;
        public Dictionary<byte, uint> _curBySiteRecordIdx;
        public Dictionary<byte, uint> _lastBySiteTN;
        public Dictionary<byte, uint> _curBySiteSubNumber;
        public TestItems _testItems;
        public TestChips _testChips;

        public Dictionary<ushort, Tuple<string, string>> _softBinNames;
        public Dictionary<ushort, Tuple<string, string>> _hardBinNames;
        List<PinMapRecord> listPinMaps;
        List<PinGroupRecord> listPinGroups;

        #region prop
        public DateTime? SetupTime { get; private set; }
        public DateTime? StartTime { get; private set; }
        public DateTime? FinishTime { get; private set; }
        public byte? StationNumber { get; private set; }
        public string TestModeCode { get; private set; }
        public string ReTestCode { get; private set; }
        public string ProtectionCode { get; private set; }
        public ushort? BurnInTime { get; private set; }
        public string CommandModeCode { get; private set; }
        public string LotId { get; private set; }
        public string PartType { get; private set; }
        public string NodeName { get; private set; }
        public string TesterType { get; private set; }
        public string JobName { get; private set; }
        public string JobRevision { get; private set; }
        public string SublotId { get; private set; }
        public string Operator { get; private set; }
        public string ExecType { get; private set; }
        public string ExecVersion { get; private set; }
        public string TestCode { get; private set; }
        public string TestTemperature { get; private set; }
        public string UserText { get; private set; }
        public string AuxiliaryDataFile { get; private set; }
        public string PackageType { get; private set; }
        public string FamilyId { get; private set; }
        public string DateCode { get; private set; }
        public string FacilityId { get; private set; }
        public string FloorID { get; private set; }
        public string ProcessID { get; private set; }
        public string OperationFreq { get; private set; }
        public string SpecificationName { get; private set; }
        public string SpecificationVersion { get; private set; }
        public string FlowID { get; private set; }
        public string SetupID { get; private set; }
        public string DesignRevision { get; private set; }
        public string EngineeringID { get; private set; }
        public string RomID { get; private set; }
        public string SerialNumber { get; private set; }
        public string SupervisorID { get; private set; }
        public string LotDispositionCode { get; private set; }
        public string LotUserDecription { get; private set; }
        public string LotExecDecription { get; private set; }

        #endregion

        public RawData(int partCnt) {
            ChipCount = partCnt;

            _rawData = new Dictionary<TestID, float?[]>(DefaultTestCount);
            _recordCnt = 0;
            _sites = new Dictionary<byte, int>(256);
            _bySitePartIdx = new Dictionary<byte, int>(256);
            _curBySiteRecordIdx = new Dictionary<byte, uint>(256);
            _lastBySiteTN = new Dictionary<byte, uint>(256);
            _curBySiteSubNumber = new Dictionary<byte, uint>(256);
            _testItems = new TestItems(DefaultTestCount);
            _testChips = new TestChips(ChipCount);

            _softBinNames = new Dictionary<ushort, Tuple<string, string>>();
            _hardBinNames = new Dictionary<ushort, Tuple<string, string>>();
            listPinMaps = new List<PinMapRecord>();
            listPinGroups = new List<PinGroupRecord>();
        }

        public float?[] GetItemDataFiltered(TestID id, List<int> filter) {
            
            return (from i in filter let r=_rawData[id][i] select r).ToArray();
        }

        public float?[] GetItemDataFiltered(TestID id, int chipIndexFrom, int count, List<int> filter) {
            int idxEnd = chipIndexFrom + count-1;
            if (chipIndexFrom > (filter.Count + 1)) return null;
            if((chipIndexFrom+count-1) > filter.Count) {
                idxEnd = filter.Count - chipIndexFrom-1;
            }

            float?[] rst = new float?[idxEnd - chipIndexFrom + 1];
            for (int i = chipIndexFrom, x=0; i <= idxEnd; i++, x++) {
                rst[x] = _rawData[id][filter[i]];
            }

            return rst;
        }

        #region record
        private void AddFar(byte[] record, ushort len) {
            //ignore
        }
        private void AddAtr(byte[] record, ushort len) {
            //ignore
        }
        private void AddMir(byte[] record, ushort len) {
            ushort i = 0;
            SetupTime = rdDateTime(record, i, len); i += 4;
            StartTime = rdDateTime(record, i, len); i += 4;
            StationNumber = rdU1(record, i, len); i += 1;
            TestModeCode = rdCx(record, i, len, 1); i += 1;
            ReTestCode = rdCx(record, i, len, 1); i += 1;
            ProtectionCode = rdCx(record, i, len, 1); i += 1;
            BurnInTime = rdU2(record, i, len); i += 2;
            CommandModeCode = rdCx(record, i, len, 1); i += 1;
            LotId = rdCn(record, i, len); i += (ushort)(1 + LotId.Length);
            PartType = rdCn(record, i, len); i += (ushort)(1 + PartType.Length);
            NodeName = rdCn(record, i, len); i += (ushort)(1 + NodeName.Length);
            TesterType = rdCn(record, i, len); i += (ushort)(1 + TesterType.Length);
            JobName = rdCn(record, i, len); i += (ushort)(1 + JobName.Length);


            JobRevision = rdCn(record, i, len); i += (ushort)(1 + JobRevision.Length); if (i > len) return;
            SublotId = rdCn(record, i, len); i += (ushort)(1 + SublotId.Length); if (i > len) return;
            Operator = rdCn(record, i, len); i += (ushort)(1 + Operator.Length); if (i > len) return;
            ExecType = rdCn(record, i, len); i += (ushort)(1 + ExecType.Length); if (i > len) return;
            ExecVersion = rdCn(record, i, len); i += (ushort)(1 + ExecVersion.Length); if (i > len) return;
            TestCode = rdCn(record, i, len); i += (ushort)(1 + TestCode.Length); if (i > len) return;
            TestTemperature = rdCn(record, i, len); i += (ushort)(1 + TestTemperature.Length); if (i > len) return;
            UserText = rdCn(record, i, len); i += (ushort)(1 + UserText.Length); if (i > len) return;
            AuxiliaryDataFile = rdCn(record, i, len); i += (ushort)(1 + AuxiliaryDataFile.Length); if (i > len) return;
            PackageType = rdCn(record, i, len); i += (ushort)(1 + PackageType.Length); if (i > len) return;
            FamilyId = rdCn(record, i, len); i += (ushort)(1 + FamilyId.Length); if (i > len) return;
            DateCode = rdCn(record, i, len); i += (ushort)(1 + DateCode.Length); if (i > len) return;
            FacilityId = rdCn(record, i, len); i += (ushort)(1 + FacilityId.Length); if (i > len) return;
            FloorID = rdCn(record, i, len); i += (ushort)(1 + FloorID.Length); if (i > len) return;
            ProcessID = rdCn(record, i, len); i += (ushort)(1 + ProcessID.Length); if (i > len) return;
            OperationFreq = rdCn(record, i, len); i += (ushort)(1 + OperationFreq.Length); if (i > len) return;
            SpecificationName = rdCn(record, i, len); i += (ushort)(1 + SpecificationName.Length); if (i > len) return;
            SpecificationVersion = rdCn(record, i, len); i += (ushort)(1 + SpecificationVersion.Length); if (i > len) return;
            FlowID = rdCn(record, i, len); i += (ushort)(1 + FlowID.Length); if (i > len) return;
            SetupID = rdCn(record, i, len); i += (ushort)(1 + SetupID.Length); if (i > len) return;
            DesignRevision = rdCn(record, i, len); i += (ushort)(1 + DesignRevision.Length); if (i > len) return;
            EngineeringID = rdCn(record, i, len); i += (ushort)(1 + EngineeringID.Length); if (i > len) return;
            RomID = rdCn(record, i, len); i += (ushort)(1 + RomID.Length); if (i > len) return;
            SerialNumber = rdCn(record, i, len); i += (ushort)(1 + SerialNumber.Length); if (i > len) return;
            SupervisorID = rdCn(record, i, len); i += (ushort)(1 + SupervisorID.Length); if (i > len) return;
        }
        private void AddMrr(byte[] record, ushort len) {
            ushort i = 0;
            FinishTime = rdDateTime(record, i, len); i += 4; if (i > len) return;
            LotDispositionCode = rdCx(record, i, len, 1); i += 1; if (i > len) return;
            LotUserDecription = rdCn(record, i, len); i += (ushort)(1 + LotUserDecription.Length);
            LotExecDecription = rdCn(record, i, len); i += (ushort)(1 + LotExecDecription.Length);
        }
        private void AddPcr(byte[] record, ushort len) {
            //ignore
        }
        private void AddHbr(byte[] record, ushort len) {
            ushort i = 0;
            var hn = rdU1(record, i, len); i += 1;
            var sn = rdU1(record, i, len); i += 1;
            var binNO = rdU2(record, i, len); i += 2;
            i += 4; //skip bin count
            var PorF = rdCx(record, i, len, 1); i += 1;
            var binName = rdCn(record, i, len); 
            if (!_hardBinNames.ContainsKey(binNO)) {
                _hardBinNames.Add(binNO, new Tuple<string, string>(binName, PorF));
            }
        }
        private void AddSbr(byte[] record, ushort len) {
            ushort i = 0;
            var hn = rdU1(record, i, len); i += 1;
            var sn = rdU1(record, i, len); i += 1;
            var binNO = rdU2(record, i, len); i += 2;
            i += 4; //skip bin count
            var PorF = rdCx(record, i, len, 1); i += 1;
            var binName = rdCn(record, i, len);
            if (!_softBinNames.ContainsKey(binNO)) {
                _softBinNames.Add(binNO, new Tuple<string, string>(binName, PorF));
            }
        }
        private void AddPmr(byte[] record, ushort len) {
            ushort i = 0;
            var pinIdx = rdU2(record, i, len); i += 2;
            var chanType = rdU2(record, i, len); i += 2;
            var chanName = rdCn(record, i, len); i += (ushort)(1 + chanName.Length);
            var phyName = rdCn(record, i, len); i += (ushort)(1 + phyName.Length);
            var logicName = rdCn(record, i, len); i += (ushort)(1 + logicName.Length);
            var hn = rdU1(record, i, len); i += 1;
            var sn = rdU1(record, i, len); i += 1;

            listPinMaps.Add(new PinMapRecord(pinIdx, chanName, phyName, logicName));
        }
        private void AddPgr(byte[] record, ushort len) {
            ushort i = 0;
            var grpIdx = rdU2(record, i, len); i += 2;
            var grpName = rdCn(record, i, len); i += (ushort)(1 + grpName.Length);
            var cn = rdU2(record, i, len); i += 2; 
            if (cn == 0) return;
            var idxes = rdKxU2(record, i, len, cn);

            listPinGroups.Add(new PinGroupRecord(grpIdx, grpName, idxes, listPinMaps));
        }
        private void AddPlr(byte[] record, ushort len) {
            //ignore
        }
        private void AddRdr(byte[] record, ushort len) {
            //ignore
        }
        private void AddSdr(byte[] record, ushort len) {
            ushort i = 0;
            if (len < 4) throw new Exception("PTR record error");
            var hn = rdU1(record, i, len); i += 1;
            var sg = rdU1(record, i, len); i += 1;
            //if (sg > 0) throw new Exception("multi site group! not support");
            var sc = rdU1(record, i, len); i += 1;
            var sn = rdKxU1(record, i, len, sc); i += sc;
              for(int j=0; j<sn.Length; j++) {
                _sites.Add(sn[j], j);
                _bySitePartIdx.Add(sn[j], 0);
                _curBySiteRecordIdx.Add(sn[j], 0);
                _lastBySiteTN.Add(sn[j], 0);
                _curBySiteSubNumber.Add(sn[j], 0);
            }
            //ignore other optional data
        }
        private void AddWir(byte[] record, ushort len) {
            //ignore
        }
        private void AddWrr(byte[] record, ushort len) {
            //ignore
        }
        private void AddWcr(byte[] record, ushort len) {
            //ignore
        }
        private void AddPir(byte[] record, ushort len) {
            ushort i = 0;
            var hn = rdU1(record, i, len); i += 1;
            var sn = rdU1(record, i, len);

            _partCnt++;
            _bySitePartIdx[sn]= _partCnt-1;
            _curBySiteRecordIdx[sn] = 0;
            _lastBySiteTN[sn] = 0;
        }
        private void AddPrr(byte[] record, ushort len) {
            ushort i = 0;
            var hn = rdU1(record, i, len); i += 1;
            var sn = rdU1(record, i, len); i += 1;
            var partflag = rdB1(record, i, len); i += 1;
            var numtest = rdU2(record, i, len); i += 2;
            var hardbin = rdU2(record, i, len); i += 2;
            ushort softbin = rdU2(record, i, len); i += 2;
            var xcord = rdI2(record, i, len); i += 2;
            var ycord = rdI2(record, i, len); i += 2;
            uint? testtime = rdU4(record, i, len); i += 4;
            var partid = rdCn(record, i, len);
            //ignore the left part text and part fix

            _testChips.AddChip(new ChipInfo(sn,
                testtime == 0 ? null : testtime,
                hardbin,
                softbin,
                partid,
                new CordType(xcord, ycord),
                Bit(partflag,0)?DeviceType.RT_ID: (Bit(partflag, 1) ? DeviceType.RT_Cord:DeviceType.Fresh),
                Bit(partflag, 2)?ResultType.Abort:(Bit(partflag, 4)?ResultType.Null:(Bit(partflag, 3)?ResultType.Fail:ResultType.Pass)),
                _partCnt));
        }
        private void AddTsr(byte[] record, ushort len) {
            //ignore
        }
        private void AddPtr(byte[] record, ushort len) {
            ushort i=0;
            //if(len<28) throw new Exception("PTR record error");
            var tn = rdU4(record,i,len); i+=4;
            var hn = rdU1(record,i,len); i+=1;
            var sn = rdU1(record,i,len); i+=1;
            _curBySiteRecordIdx[sn]++;
            if (_lastBySiteTN[sn] == tn) {
                _curBySiteSubNumber[sn]++;
            } else {
                _curBySiteSubNumber[sn] = 0;
            }
            _lastBySiteTN[sn] = tn;
            var tf = rdB1(record,i,len); i+=1;
            var pf = rdB1(record,i,len); i+=1;

            if (Bit(tf, 1)) return;

            var result = rdR4(record, i, len); i += 4;

            TestID id;
            id = new TestID(tn, _curBySiteSubNumber[sn]);
            if (!_rawData.ContainsKey(id)) {
                _rawData.Add(id, new float?[ChipCount]);
            }
            _rawData[id][_bySitePartIdx[sn]] = result;

            if(_curBySiteSubNumber[sn]>0 && _testItems.ExistTestItem(id) == false) {
                _testItems.AddTestItem(id, _testItems.GetItemInfo(new TestID(tn, _curBySiteSubNumber[sn]-1)));
                return;
            }

            if (i < len && (!_testItems.ExistTestItem(id))) {
                var txt = rdCn(record, i, len); i += (ushort)(1 + txt.Length);

                var alarm = rdCn(record, i, len); i += (ushort)(1 + alarm.Length);

                if (i >= len) return;
                var oFg = rdB1(record, i, len); i += 1;

                sbyte? resScal = null, llScal = null, hlScal=null;
                float? ll = null, hl = null;
                string unit="";

                if (!Bit(oFg, 0) && i<len) {
                    resScal = rdI1(record, i, len); 
                }
                i++;
                if ((!Bit(oFg, 4)) && (!Bit(oFg, 6)) && i < len) {
                    llScal = rdI1(record, i, len); 
                }
                i++;
                if ((!Bit(oFg, 5)) && (!Bit(oFg, 7)) && i < len) {
                    hlScal = rdI1(record, i, len); 
                }
                i++;
                if ((!Bit(oFg, 4)) && (!Bit(oFg, 6)) && i < len) {
                    ll = rdR4(record, i, len); 
                }
                i+=4;
                if ((!Bit(oFg, 5)) && (!Bit(oFg, 7)) && i < len) {
                    hl = rdR4(record, i, len); 
                }
                i+=4;
                if (i < len) {
                    unit = rdCn(record, i, len);
                }
                i += (ushort)(1 + unit.Length);
                _testItems.AddTestItem(id, new ItemInfo(txt, ll, hl, unit, llScal, hlScal, resScal));
            }

        }
        private void AddMpr(byte[] record, ushort len) {
            ushort i = 0;
            var tn = rdU4(record, i, len); i += 4;
            var hn = rdU1(record, i, len); i += 1;
            var sn = rdU1(record, i, len); i += 1;
            _curBySiteRecordIdx[sn]++;
            if (_lastBySiteTN[sn] == tn) {
                _curBySiteSubNumber[sn]++;
            } else {
                _curBySiteSubNumber[sn] = 0;
            }
            _lastBySiteTN[sn] = tn;
            var tf = rdB1(record, i, len); i += 1;
            var pf = rdB1(record, i, len); i += 1;

            if (Bit(tf, 2)) return;

            try {
                var rtnCnt = rdU2(record, i, len);  i += 2;
                var rstCnt = rdU2(record, i, len); i += 2;

                if (rtnCnt != rstCnt) throw new Exception("MPR pin count mismatch");

                var stat = rdKxN1(record, i, len, rtnCnt); i += (ushort)(rtnCnt / 2 + rtnCnt % 2);
                var rsts = rdKxR4(record, i, len, rstCnt); i += (ushort)(rstCnt*4);

                TestID id;
                for(uint j=0; j< rtnCnt; j++) {
                    id = new TestID(tn, _curBySiteSubNumber[sn] + j);
                    if (!_rawData.ContainsKey(id)) {
                        _rawData.Add(id, new float?[ChipCount]);
                    }
                    _rawData[id][_bySitePartIdx[sn]] = rsts[j];
                }

                if (_curBySiteSubNumber[sn] > 0) {
                    for (uint j = 0; j < rtnCnt; j++) {
                        id = new TestID(tn, _curBySiteSubNumber[sn] + j);
                        if(_testItems.ExistTestItem(id) == false) {
                            var info = _testItems.GetItemInfo(new TestID(tn, _curBySiteSubNumber[sn] - 1));
                            _testItems.AddTestItem(id, info);
                        } else {
                            break;
                        }
                    }
                    return;
                }

                if (i < len && (!_testItems.ExistTestItem(new TestID(tn, _curBySiteSubNumber[sn])))) {
                    var txt = rdCn(record, i, len); i += (ushort)(1 + txt.Length); 
                    if (i >= len) return;

                    var alarm = rdCn(record, i, len); i += (ushort)(1 + alarm.Length); 
                    if (i >= len) return;

                    var oFg = rdB1(record, i, len); i += 1; 
                    if (i >= len) return;

                    sbyte? resScal = null, llScal = null, hlScal = null;
                    float? ll = null, hl = null;
                    string unit = "";

                    if (!Bit(oFg, 0) && i < len) {
                        resScal = rdI1(record, i, len);
                    }
                    i++;
                    if ((!Bit(oFg, 4)) && (!Bit(oFg, 6)) && i < len) {
                        llScal = rdI1(record, i, len);
                    }
                    i++;
                    if ((!Bit(oFg, 5)) && (!Bit(oFg, 7)) && i < len) {
                        hlScal = rdI1(record, i, len);
                    }
                    i++;
                    if ((!Bit(oFg, 4)) && (!Bit(oFg, 6)) && i < len) {
                        ll = rdR4(record, i, len);
                    }
                    i += 4;
                    if ((!Bit(oFg, 5)) && (!Bit(oFg, 7)) && i < len) {
                        hl = rdR4(record, i, len);
                    }
                    i += 4;

                    i += 8; //skip start in and incr in
                    if (i >= len) return;
                    var idxs = rdKxU2(record, i, len, rtnCnt); i += (ushort)(rtnCnt * 2);

                    if (i < len) {
                        unit = rdCn(record, i, len);
                    }
                    i += (ushort)(1 + unit.Length);

                    for (uint j = 0; j < rtnCnt; j++) {
                        id = new TestID(tn, _curBySiteSubNumber[sn] + j);
                        var pin = listPinMaps.Find(x => x.PinIndex == (j + 1)).ChanName;
                        _testItems.AddTestItem(id, new ItemInfo(txt + "_" + pin, ll, hl, unit, llScal, hlScal, resScal));
                    }
                } 

            } catch {
                throw;
            } 



        }
        private void AddFtr(byte[] record, ushort len) {
            ushort i = 0;
            var tn = rdU4(record, i, len); i += 4;
            var hn = rdU1(record, i, len); i += 1;
            var sn = rdU1(record, i, len); i += 1;
            _curBySiteRecordIdx[sn]++;

            //FTR no sub test, same test number means fail cyc record, do not need proceed
            _curBySiteSubNumber[sn] = 0;
            if (_lastBySiteTN[sn] == tn) return;

            _lastBySiteTN[sn] = tn;
            var tf = rdB1(record, i, len); i += 1;
            if (Bit(tf, 2)) return;

            TestID id;
            int result = (Bit(tf,6)||Bit(tf,7)) == false ? 1: 0;
            //proceed the rst
            id = new TestID(tn, _curBySiteSubNumber[sn]);
            if (!_rawData.ContainsKey(id)) {
                _rawData.Add(id, new float?[ChipCount]);
            }
            _rawData[id][_bySitePartIdx[sn]] = result;

            //add teest item info
            string txt = "";
            if (!_testItems.ExistTestItem(id)) {
                _testItems.AddTestItem(id, new ItemInfo(txt, (float)0.5, (float)1.5, "", 0, 0, 0));

                try {
                    i += 25; //option flag

                    var rtncnt = rdU2(record, i, len); i += 2;
                    var pgmcnt = rdU2(record, i, len); i += 2;

                    i += (ushort)(rtncnt * 2);
                    i += (ushort)(rtncnt / 2 + rtncnt % 2);
                    i += (ushort)(pgmcnt * 2);
                    i += (ushort)(pgmcnt / 2 + pgmcnt % 2);

                    var failpin = rdDn(record, i, len); i += (ushort)(2+ failpin.Length);

                    i += (ushort)(1 + record[i]);
                    i += (ushort)(1 + record[i]);

                    txt = rdCn(record, i, len); i += (ushort)(1 + txt.Length);
                } catch {
                    return;
                }
                _testItems.UpdateTestText(id, txt);

            } else {
                return;
            }

            return;
        }
        private void AddBps(byte[] record, ushort len) {

        }
        private void AddEps(byte[] record, ushort len) {

        }
        private void AddGdr(byte[] record, ushort len) {

        }
        private void AddDtr(byte[] record, ushort len) {

        }
        #endregion

        #region parse Byte
        private bool Bit(byte flag, byte bit){
            return ((flag>>bit) & 0x1) == 0x1;
        }
        private string rdCx(byte[] record, ushort i, ushort len, ushort charCnt) {
            if ((i+charCnt-1) > len) throw new Exception("wrong record index");
            //return BitConverter.ToString(record, i, charCnt);
            return Encoding.ASCII.GetString(record, i, charCnt);
        }
        private string rdCn(byte[] record, ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            byte charCnt = record[i];
            if (charCnt == 0) return "";
            if ((i+charCnt) > len) throw new Exception("wrong record index");
            //return BitConverter.ToString(record, i+1, charCnt);
            return Encoding.ASCII.GetString(record, i+1, charCnt);
        }
        private string rdCf(byte[] record, ushort i, ushort len, ushort charCnt) {
            if ((i + charCnt) > len) throw new Exception("wrong record index");
            //return BitConverter.ToString(record, i, charCnt);
            return Encoding.ASCII.GetString(record, i, charCnt);
        }
        private byte rdU1(byte[] record, ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            return record[i];
        }
        private ushort rdU2(byte[] record, ushort i, ushort len) {
            if ((i+1) > len) throw new Exception("wrong record index");
            return BitConverter.ToUInt16(record,i);
        }
        private uint rdU4(byte[] record, ushort i, ushort len) {
            if ((i + 3) > len) throw new Exception("wrong record index");
            return BitConverter.ToUInt32(record, i);
        }
        /// <summary>
        /// Reads an STDF datetime (4-byte integer seconds since the epoch)
        /// </summary>
        private DateTime rdDateTime(byte[] record, ushort i, ushort len) {
            var seconds = rdU4(record, i, len);
            return new DateTime(1970, 1, 1) + TimeSpan.FromSeconds((double)seconds);
        }

        private sbyte rdI1(byte[] record, ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            return (sbyte)(record[i]);
        }
        private short rdI2(byte[] record, ushort i, ushort len) {
            if ((i + 1) > len) throw new Exception("wrong record index");
            return BitConverter.ToInt16(record, i);
        }
        private int rdI4(byte[] record, ushort i, ushort len) {
            if ((i + 3) > len) throw new Exception("wrong record index");
            return BitConverter.ToInt32(record, i);
        }
        private float rdR4(byte[] record, ushort i, ushort len) {
            if ((i + 3) > len) throw new Exception("wrong record index");
            return BitConverter.ToSingle(record, i);
        }
        private double rdR8(byte[] record, ushort i, ushort len) {
            if ((i + 7) > len) throw new Exception("wrong record index");
            return BitConverter.ToDouble(record, i);
        }
        private byte rdB1(byte[] record, ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            return record[i];
        }
        private byte[] rdBx(byte[] record, ushort i, ushort len, ushort byteCnt) {
            if ((i + byteCnt - 1) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[byteCnt];
            for(byte j=0;j< byteCnt; j++) {
                rst[j] = record[i + j];
            }
            return rst;
        }
        private byte[] rdBn(byte[] record, ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            byte byteCnt = record[i];
            if ((i + byteCnt) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[byteCnt];
            for (byte j = 0; j < byteCnt; j++) {
                rst[j] = record[i + j];
            }
            return rst;
        }
        private byte[] rdVn(byte[] record, ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            byte byteCnt = record[i];
            if ((i + byteCnt) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[byteCnt];
            for (byte j = 0; j < byteCnt; j++) {
                rst[j] = record[i + j];
            }
            return rst;
        }
        private byte[] rdDn(byte[] record, ushort i, ushort len) {
            if ((i+1) > len) throw new Exception("wrong record index");
            ushort byteCnt = BitConverter.ToUInt16(record, i);
            if ((i + byteCnt) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[byteCnt];
            for (byte j = 0; j < byteCnt; j++) {
                rst[j] = record[i + j];
            }
            return rst;
        }
        private byte[] rdNx(byte[] record, ushort i, ushort len, ushort nibbleCnt) {
            if ((i + nibbleCnt - 1) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[nibbleCnt];
            for (byte j = 0; j < nibbleCnt; j++) {
                if (j % 2 == 0) {
                    rst[j] = (byte)(record[i + j/2] & 0xf);
                } else {
                    rst[j] = (byte)((record[i + j / 2] & 0xf0)>>4);
                }
            }
            return rst;
        }

        private ushort[] rdKxU2(byte[] record, ushort i, ushort len, ushort cnt) {
            if ((i + cnt*2 - 1) > len) throw new Exception("wrong record index");
            ushort[] rst = new ushort[cnt];
            for (int j = 0; j < cnt; j++) {
                rst[j] = BitConverter.ToUInt16(record, (i+j*2));
            }
            return rst;
        }
        private byte[] rdKxU1(byte[] record, ushort i, ushort len, ushort cnt) {
            if ((i + cnt - 1) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[cnt];
            for (int j = 0; j < cnt; j++) {
                rst[j] = record[i + j];
            }
            return rst;
        }
        private string[] rdKxCn(byte[] record, ushort i, ushort len, ushort cnt) {
            string[] rst = new string[cnt];
            for (int j = 0; j < cnt; j++) {
                if (i > len) throw new Exception("wrong record index");
                byte charCnt = record[i];
                if (charCnt == 0) {
                    rst[j] = "";
                    continue;
                }
                if ((i + charCnt) > len) throw new Exception("wrong record index");
                //rst[j] = BitConverter.ToString(record, i + 1, charCnt);
                rst[j] = Encoding.ASCII.GetString(record, i, charCnt);
                i += (byte)(charCnt+1);
            }
            return rst;
        }
        private byte[] rdKxN1(byte[] record, ushort i, ushort len, ushort cnt) {
            if ((i + cnt - 1) > len) throw new Exception("wrong record index");
            byte[] rst = new byte[cnt];
            for (int j = 0; j < cnt; j++) {
                rst[j] = (byte)(record[i + j] & 0xf);
            }
            return rst;
        }
        private float[] rdKxR4(byte[] record, ushort i, ushort len, ushort cnt) {
            if ((i + cnt * 4 - 1) > len) throw new Exception("wrong record index");
            float[] rst = new float[cnt];
            for (int j = 0; j < cnt; j++) {
                rst[j] = BitConverter.ToSingle(record, (i + j * 4));
            }
            return rst;
        }
        #endregion

        #region static
        public static RecordType FAR = new RecordType(0, 10);
        public static RecordType ATR = new RecordType(0, 20);
        public static RecordType MIR = new RecordType(1, 10);
        public static RecordType MRR = new RecordType(1, 20);
        public static RecordType PCR = new RecordType(1, 30);
        public static RecordType HBR = new RecordType(1, 40);
        public static RecordType SBR = new RecordType(1, 50);
        public static RecordType PMR = new RecordType(1, 60);
        public static RecordType PGR = new RecordType(1, 62);
        public static RecordType PLR = new RecordType(1, 63);
        public static RecordType RDR = new RecordType(1, 70);
        public static RecordType SDR = new RecordType(1, 80);
        public static RecordType WIR = new RecordType(2, 10);
        public static RecordType WRR = new RecordType(2, 20);
        public static RecordType WCR = new RecordType(2, 30);
        public static RecordType PIR = new RecordType(5, 10);
        public static RecordType PRR = new RecordType(5, 20);
        public static RecordType TSR = new RecordType(10, 30);
        public static RecordType PTR = new RecordType(15, 10);
        public static RecordType MPR = new RecordType(15, 15);
        public static RecordType FTR = new RecordType(15, 20);
        public static RecordType BPS = new RecordType(20, 10);
        public static RecordType EPS = new RecordType(20, 20);
        public static RecordType GDR = new RecordType(50, 10);
        public static RecordType DTR = new RecordType(50, 30);

        public static void AddRecord(ref RawData rawData, RecordType recordType, byte[] recordData, ushort len) {
            if (rawData is null) throw new Exception("RawData not init");
            if (recordType == PTR) rawData.AddPtr(recordData, len);
            else if (recordType == MPR) rawData.AddMpr(recordData, len);
            else if (recordType == FTR) rawData.AddFtr(recordData, len);
            else if (recordType == FAR) rawData.AddFar(recordData, len);
            else if (recordType == ATR) rawData.AddAtr(recordData, len);
            else if (recordType == MIR) rawData.AddMir(recordData, len);
            else if (recordType == MRR) rawData.AddMrr(recordData, len);
            else if (recordType == PCR) rawData.AddPcr(recordData, len);
            else if (recordType == HBR) rawData.AddHbr(recordData, len);
            else if (recordType == SBR) rawData.AddSbr(recordData, len);
            else if (recordType == PMR) rawData.AddPmr(recordData, len);
            else if (recordType == PGR) rawData.AddPgr(recordData, len);
            else if (recordType == PLR) rawData.AddPlr(recordData, len);
            else if (recordType == RDR) rawData.AddRdr(recordData, len);
            else if (recordType == SDR) rawData.AddSdr(recordData, len);
            else if (recordType == WIR) rawData.AddWir(recordData, len);
            else if (recordType == WRR) rawData.AddWrr(recordData, len);
            else if (recordType == WCR) rawData.AddWcr(recordData, len);
            else if (recordType == PIR) rawData.AddPir(recordData, len);
            else if (recordType == PRR) rawData.AddPrr(recordData, len);
            else if (recordType == TSR) rawData.AddTsr(recordData, len);
            else if (recordType == BPS) rawData.AddBps(recordData, len);
            else if (recordType == EPS) rawData.AddEps(recordData, len);
            else if (recordType == GDR) rawData.AddGdr(recordData, len);
            else if (recordType == DTR) rawData.AddDtr(recordData, len);
            //else throw new Exception("No matched record");

            rawData._recordCnt++;
        }
        #endregion 
    }
}
