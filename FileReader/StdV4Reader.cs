using DataContainer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace FileReader {
    public struct RecordAddr {
        private int _offset;
        private int _length;
        private RecordType _recordType;

        public RecordAddr(byte[] headBuf, int offset) {
            _offset = offset;
            _length = BitConverter.ToUInt16(headBuf, 0);
            _recordType = new RecordType(headBuf[2], headBuf[3]);
        }

        public int Offset { get { return _offset; } }
        public int Length { get { return _length; } }
        public RecordType RecordType { get { return _recordType; } }

    }

    public class StdV4Reader : IDisposable {
        public static bool IfCmpTextInUid = false;
        const int BufferSize = 409600;
        public static int ExpectItemCounts = 500;

        private FileStream _stream;
        private int _recordCnt;
        private string _path;
        private IDataCollect _dc;

        public StdV4Reader(string path) {
            _path = path;
        }
        public enum ReadStatus {
            Done,
            FileInvalid,
            Error
        }
        public ReadStatus ReadRaw(IDataCollect dc) {
            InitBuffer();
            _dc = dc;

            using (_stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize)) {
                if (!ValidFile()) return ReadStatus.FileInvalid;
                long length = _stream.Length;

                System.Threading.Timer myTimer = new System.Threading.Timer((x)=> { _dc.SetReadingPercent((int)(_stream.Position * 100.0 / length)); }, null, 100, 100);

                byte[] buf = new byte[4];
                ushort len = 0;
                byte[] dataBuf = new byte[ushort.MaxValue];
                while (_stream.Position < length) {
                    if (!ReadRecordHeader(buf)) return ReadStatus.Error;
                    len = BitConverter.ToUInt16(buf, 0);
                    if (!ReadRecordData(dataBuf, len)) return ReadStatus.Error;
                    AddRecord(new RecordType(buf[2], buf[3]), dataBuf, len);
                    _recordCnt++;
                }
                myTimer.Dispose();
                //_sq.StopTransaction();
            }

            return ReadStatus.Done;
        }

        private bool ReadRecordHeader(byte[] buf) {
            if (_stream.Read(buf, 0, 4) == 4) {
                return true;
            } else {
                return false;
            }

        }
        private bool ReadRecordData(byte[] buf, ushort length) {
            if (_stream.Read(buf, 0, length) == length) {
                return true;
            } else {
                return false;
            }

        }


        private bool ValidFile() {
            bool valid = true;
            var far = new byte[6];
            if (_stream.Read(far, 0, 6) < 6) {
                valid = false;
            }
            var length = far[0];
            if (length != 2) {
                valid = false;
            }
            //validate record type
            if (far[2] != 0) {
                valid = false;
            }
            //validate record type
            if (far[3] != 10) {
                valid = false;
            }
            _stream.Position = 0;
            return valid;
        }

        public void Dispose() {
            ((IDisposable)_stream).Dispose();
            _path = null;
            _dc = null;
        }

        //int _partCnt;
        //Dictionary<byte, int> _sites;
        //Dictionary<TempID, ItemInfo> _idSet; //for data collecting phase use
        //Dictionary<TestID, ItemInfo> _itemUidSet; // for db sync use


        //public Dictionary<ushort, Tuple<string, string>> _softBinNames;
        //public Dictionary<ushort, Tuple<string, string>> _hardBinNames;
        List<PinMapRecord> listPinMaps;
        List<PinGroupRecord> listPinGroups;
        Dictionary<byte, TestID> _lastUidBySite;

        private void InitBuffer() {
            //_sites = new Dictionary<byte, int>();
            //_idSet = new Dictionary<TempID, ItemInfo>(ExpectItemCounts);
            //_itemUidSet = new Dictionary<TestID, ItemInfo>(ExpectItemCounts);

            //_softBinNames = new Dictionary<ushort, Tuple<string, string>>();
            //_hardBinNames = new Dictionary<ushort, Tuple<string, string>>();
            listPinMaps = new List<PinMapRecord>();
            listPinGroups = new List<PinGroupRecord>();

            _lastUidBySite = new Dictionary<byte, TestID>();
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
            var SetupTime = rdDateTime(record, i, len); i += 4;
            _dc.SetBasicInfo("SetupTime", SetupTime.ToString());
            var StartTime = rdDateTime(record, i, len); i += 4;
            _dc.SetBasicInfo("StartTime", StartTime.ToString());
            var StationNumber = rdU1(record, i, len); i += 1;
            _dc.SetBasicInfo("StationNumber", StationNumber.ToString());
            var TestModeCode = rdCx(record, i, len, 1); i += 1;
            _dc.SetBasicInfo("TestModeCode", TestModeCode);
            var ReTestCode = rdCx(record, i, len, 1); i += 1;
            _dc.SetBasicInfo("ReTestCode", ReTestCode);
            var ProtectionCode = rdCx(record, i, len, 1); i += 1;
            _dc.SetBasicInfo("ProtectionCode", ProtectionCode);
            var BurnInTime = rdU2(record, i, len); i += 2;
            _dc.SetBasicInfo("BurnInTime", BurnInTime.ToString());
            var CommandModeCode = rdCx(record, i, len, 1); i += 1;
            _dc.SetBasicInfo("CommandModeCode", CommandModeCode);
            var LotId = rdCn(record, i, len); i += (ushort)(1 + LotId.Length);
            _dc.SetBasicInfo("LotId", LotId);
            var PartType = rdCn(record, i, len); i += (ushort)(1 + PartType.Length);
            _dc.SetBasicInfo("PartType", PartType);
            var NodeName = rdCn(record, i, len); i += (ushort)(1 + NodeName.Length);
            _dc.SetBasicInfo("NodeName", NodeName);
            var TesterType = rdCn(record, i, len); i += (ushort)(1 + TesterType.Length);
            _dc.SetBasicInfo("TesterType", TesterType);
            var JobName = rdCn(record, i, len); i += (ushort)(1 + JobName.Length);
            _dc.SetBasicInfo("JobName", JobName);

            var JobRevision = rdCn(record, i, len); i += (ushort)(1 + JobRevision.Length); if (i > len) return;
            _dc.SetBasicInfo("JobRevision", JobRevision);
            var SublotId = rdCn(record, i, len); i += (ushort)(1 + SublotId.Length); if (i > len) return;
            _dc.SetBasicInfo("SublotId", SublotId);
            var Operator = rdCn(record, i, len); i += (ushort)(1 + Operator.Length); if (i > len) return;
            _dc.SetBasicInfo("Operator", Operator);
            var ExecType = rdCn(record, i, len); i += (ushort)(1 + ExecType.Length); if (i > len) return;
            _dc.SetBasicInfo("ExecType", ExecType);
            var ExecVersion = rdCn(record, i, len); i += (ushort)(1 + ExecVersion.Length); if (i > len) return;
            _dc.SetBasicInfo("ExecVersion", ExecVersion);
            var TestCode = rdCn(record, i, len); i += (ushort)(1 + TestCode.Length); if (i > len) return;
            _dc.SetBasicInfo("TestCode", TestCode);
            var TestTemperature = rdCn(record, i, len); i += (ushort)(1 + TestTemperature.Length); if (i > len) return;
            _dc.SetBasicInfo("TestTemperature", TestTemperature);
            var UserText = rdCn(record, i, len); i += (ushort)(1 + UserText.Length); if (i > len) return;
            _dc.SetBasicInfo("UserText", UserText);
            var AuxiliaryDataFile = rdCn(record, i, len); i += (ushort)(1 + AuxiliaryDataFile.Length); if (i > len) return;
            _dc.SetBasicInfo("AuxiliaryDataFile", AuxiliaryDataFile);
            var PackageType = rdCn(record, i, len); i += (ushort)(1 + PackageType.Length); if (i > len) return;
            _dc.SetBasicInfo("PackageType", PackageType);
            var FamilyId = rdCn(record, i, len); i += (ushort)(1 + FamilyId.Length); if (i > len) return;
            _dc.SetBasicInfo("FamilyId", FamilyId);
            var DateCode = rdCn(record, i, len); i += (ushort)(1 + DateCode.Length); if (i > len) return;
            _dc.SetBasicInfo("DateCode", DateCode);
            var FacilityId = rdCn(record, i, len); i += (ushort)(1 + FacilityId.Length); if (i > len) return;
            _dc.SetBasicInfo("FacilityId", FacilityId);
            var FloorID = rdCn(record, i, len); i += (ushort)(1 + FloorID.Length); if (i > len) return;
            _dc.SetBasicInfo("FloorID", FloorID);
            var ProcessID = rdCn(record, i, len); i += (ushort)(1 + ProcessID.Length); if (i > len) return;
            _dc.SetBasicInfo("ProcessID", ProcessID);
            var OperationFreq = rdCn(record, i, len); i += (ushort)(1 + OperationFreq.Length); if (i > len) return;
            _dc.SetBasicInfo("OperationFreq", OperationFreq);
            var SpecificationName = rdCn(record, i, len); i += (ushort)(1 + SpecificationName.Length); if (i > len) return;
            _dc.SetBasicInfo("SpecificationName", SpecificationName);
            var SpecificationVersion = rdCn(record, i, len); i += (ushort)(1 + SpecificationVersion.Length); if (i > len) return;
            _dc.SetBasicInfo("SpecificationVersion", SpecificationVersion);
            var FlowID = rdCn(record, i, len); i += (ushort)(1 + FlowID.Length); if (i > len) return;
            _dc.SetBasicInfo("FlowID", FlowID);
            var SetupID = rdCn(record, i, len); i += (ushort)(1 + SetupID.Length); if (i > len) return;
            _dc.SetBasicInfo("SetupID", SetupID);
            var DesignRevision = rdCn(record, i, len); i += (ushort)(1 + DesignRevision.Length); if (i > len) return;
            _dc.SetBasicInfo("DesignRevision", DesignRevision);
            var EngineeringID = rdCn(record, i, len); i += (ushort)(1 + EngineeringID.Length); if (i > len) return;
            _dc.SetBasicInfo("EngineeringID", EngineeringID);
            var RomID = rdCn(record, i, len); i += (ushort)(1 + RomID.Length); if (i > len) return;
            _dc.SetBasicInfo("RomID", RomID);
            var SerialNumber = rdCn(record, i, len); i += (ushort)(1 + SerialNumber.Length); if (i > len) return;
            _dc.SetBasicInfo("SerialNumber", SerialNumber);
            var SupervisorID = rdCn(record, i, len); i += (ushort)(1 + SupervisorID.Length); if (i > len) return;
            _dc.SetBasicInfo("SupervisorID", SupervisorID);
        }
        private void AddMrr(byte[] record, ushort len) {
            ushort i = 0;
            var FinishTime = rdDateTime(record, i, len); i += 4; if (i > len) return;
            _dc.SetBasicInfo("FinishTime", FinishTime.ToString());
            var LotDispositionCode = rdCx(record, i, len, 1); i += 1; if (i > len) return;
            _dc.SetBasicInfo("LotDispositionCode", LotDispositionCode);
            var LotUserDecription = rdCn(record, i, len); i += (ushort)(1 + LotUserDecription.Length);
            _dc.SetBasicInfo("LotUserDecription", LotUserDecription);
            var LotExecDecription = rdCn(record, i, len); i += (ushort)(1 + LotExecDecription.Length);
            _dc.SetBasicInfo("LotExecDecription", LotExecDecription);
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
            _dc.AddHbr(binNO, new Tuple<string, string>(binName, PorF));

        }
        private void AddSbr(byte[] record, ushort len) {
            ushort i = 0;
            var hn = rdU1(record, i, len); i += 1;
            var sn = rdU1(record, i, len); i += 1;
            var binNO = rdU2(record, i, len); i += 2;
            i += 4; //skip bin count
            var PorF = rdCx(record, i, len, 1); i += 1;
            var binName = rdCn(record, i, len);
            _dc.AddSbr(binNO, new Tuple<string, string>(binName, PorF));
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
            for (int j = 0; j < sn.Length; j++) {
                _dc.AddSiteNum(sn[j]);
                _lastUidBySite.Add(sn[j], new TestID());
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
            _dc.AddPir(sn);
            _lastUidBySite[sn] = new TestID();
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

            _dc.AddPrr(sn,
                testtime == 0 ? null : testtime,
                hardbin,
                softbin,
                partid,
                xcord, ycord,
                Bit(partflag, 0) ? DeviceType.RT_ID : (Bit(partflag, 1) ? DeviceType.RT_Cord : DeviceType.Fresh),
                Bit(partflag, 2) ? ResultType.Abort : (Bit(partflag, 4) ? ResultType.Null : (Bit(partflag, 3) ? ResultType.Fail : ResultType.Pass)));


        }
        private void AddTsr(byte[] record, ushort len) {
            //ignore
        }
        private void AddPtr(byte[] record, ushort len) {
            ushort i = 0;
            //if(len<28) throw new Exception("PTR record error");
            var tn = rdU4(record, i, len); i += 4;
            var hn = rdU1(record, i, len); i += 1;
            var sn = rdU1(record, i, len); i += 1;
            var tf = rdB1(record, i, len); i += 1;
            var pf = rdB1(record, i, len); i += 1;

            if (Bit(tf, 1)) return;

            var result = rdR4(record, i, len); i += 4;
            var txt = rdCn(record, i, len); i += (ushort)(1 + txt.Length);

            TestID id;
            if (!_lastUidBySite[sn].IfSubTest(tn, txt)) {
                id = new TestID(tn, txt);
                _lastUidBySite[sn] = id;
            } else {
                id = new TestID(_lastUidBySite[sn]);
                _lastUidBySite[sn] = id;
            }

            if ((!_dc.IfContainItemInfo(id.GetUID())) && i < len) {
                var alarm = rdCn(record, i, len); i += (ushort)(1 + alarm.Length);
                var oFg = rdB1(record, i, len); i += 1;

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
                if (i < len) {
                    unit = rdCn(record, i, len);
                }
                i += (ushort)(1 + unit.Length);
                _dc.UpdateItemInfo(id.GetUID(), new ItemInfo(txt, ll, hl, unit, llScal, hlScal, resScal));
            }

            _dc.AddTestData(sn, id.GetUID(), result);

        }
        private void AddMpr(byte[] record, ushort len) {
            ushort i = 0;
            var tn = rdU4(record, i, len); i += 4;
            var hn = rdU1(record, i, len); i += 1;
            var sn = rdU1(record, i, len); i += 1;
            var tf = rdB1(record, i, len); i += 1;
            var pf = rdB1(record, i, len); i += 1;

            if (Bit(tf, 2)) return;

            try {
                var rtnCnt = rdU2(record, i, len); i += 2;
                var rstCnt = rdU2(record, i, len); i += 2;

                if (rtnCnt != rstCnt) throw new Exception("MPR pin count mismatch");

                var stat = rdKxN1(record, i, len, rtnCnt); i += (ushort)(rtnCnt / 2 + rtnCnt % 2);
                var rsts = rdKxR4(record, i, len, rstCnt); i += (ushort)(rstCnt * 4);
                var txt = rdCn(record, i, len); i += (ushort)(1 + txt.Length);


                TestID id;
                if (!_lastUidBySite[sn].IfSubTest(tn, txt)) {
                    id = new TestID(tn, txt);
                    _lastUidBySite[sn] = id;
                } else {
                    id = new TestID(_lastUidBySite[sn]);
                    _lastUidBySite[sn] = id;
                }


                TestID[] uids = new TestID[rsts.Length];

                uids[0] = id;
                bool flg = _dc.IfContainItemInfo(uids[0].GetUID());
                _dc.AddTestData(sn, uids[0].GetUID(), rsts[0]);

                for (uint j = 1; j < rtnCnt; j++) {
                    uids[j] = new TestID(uids[j-1]);
                    _dc.IfContainItemInfo(uids[j].GetUID());
                    _dc.AddTestData(sn, uids[j].GetUID(), rsts[j]);
                }

                if (!flg && i < len) {

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

                    for (uint j = 0; j < idxs.Length; j++) {
                        var pin = listPinMaps.Find(x => x.PinIndex == idxs[j]).ChanName;
                        _dc.UpdateItemInfo(uids[j].GetUID(), new ItemInfo(txt + "_" + pin, ll, hl, unit, llScal, hlScal, resScal));
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

            //FTR no sub test, same test number means fail cyc record, do not need proceed
            var tf = rdB1(record, i, len); i += 1;
            if (Bit(tf, 2)) return;

            int result = (Bit(tf, 6) || Bit(tf, 7)) == false ? 1 : 0;

            try {
                i += 25; //option flag

                var rtncnt = rdU2(record, i, len); i += 2;
                var pgmcnt = rdU2(record, i, len); i += 2;

                i += (ushort)(rtncnt * 2);
                i += (ushort)(rtncnt / 2 + rtncnt % 2);
                i += (ushort)(pgmcnt * 2);
                i += (ushort)(pgmcnt / 2 + pgmcnt % 2);

                var failpin = rdDn(record, i, len); i += (ushort)(2 + failpin.Length);

                i += (ushort)(1 + record[i]);
                i += (ushort)(1 + record[i]);
                i += (ushort)(1 + record[i]);

                var txt = rdCn(record, i, len); i += (ushort)(1 + txt.Length);

                TestID id;
                if (!_lastUidBySite[sn].IfSubTest(tn, txt)) {
                    //id = new TestID(tn, txt);
                    //_lastUidBySite[sn] = id;
                    id = _lastUidBySite[sn];
                } else {
                    id = new TestID(_lastUidBySite[sn]);
                    _lastUidBySite[sn] = id;
                }

                if (!_dc.IfContainItemInfo(id.GetUID())) {
                    _dc.UpdateItemInfo(id.GetUID(), new ItemInfo(txt, 0.5f, 1.5f, "", 0, 0, 0));
                }
                //proceed the rst
                _dc.AddTestData(sn, id.GetUID(), result);
            }
            catch {
                throw new Exception("FTR cannot get item info");
            }
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

        #region readRecord
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

        public void AddRecord(RecordType recordType, byte[] recordData, ushort len) {
            if (recordType == PTR) AddPtr(recordData, len);
            else if (recordType == MPR) AddMpr(recordData, len);
            else if (recordType == FTR) AddFtr(recordData, len);
            else if (recordType == FAR) AddFar(recordData, len);
            else if (recordType == ATR) AddAtr(recordData, len);
            else if (recordType == MIR) AddMir(recordData, len);
            else if (recordType == MRR) AddMrr(recordData, len);
            else if (recordType == PCR) AddPcr(recordData, len);
            else if (recordType == HBR) AddHbr(recordData, len);
            else if (recordType == SBR) AddSbr(recordData, len);
            else if (recordType == PMR) AddPmr(recordData, len);
            else if (recordType == PGR) AddPgr(recordData, len);
            else if (recordType == PLR) AddPlr(recordData, len);
            else if (recordType == RDR) AddRdr(recordData, len);
            else if (recordType == SDR) AddSdr(recordData, len);
            else if (recordType == WIR) AddWir(recordData, len);
            else if (recordType == WRR) AddWrr(recordData, len);
            else if (recordType == WCR) AddWcr(recordData, len);
            else if (recordType == PIR) AddPir(recordData, len);
            else if (recordType == PRR) AddPrr(recordData, len);
            else if (recordType == TSR) AddTsr(recordData, len);
            else if (recordType == BPS) AddBps(recordData, len);
            else if (recordType == EPS) AddEps(recordData, len);
            else if (recordType == GDR) AddGdr(recordData, len);
            else if (recordType == DTR) AddDtr(recordData, len);
            //else throw new Exception("No matched record");

            _recordCnt++;
        }
        #endregion 

        #region parse Byte
        private bool Bit(byte flag, byte bit) {
            return ((flag >> bit) & 0x1) == 0x1;
        }
        private string rdCx(byte[] record, ushort i, ushort len, ushort charCnt) {
            if ((i + charCnt - 1) > len) throw new Exception("wrong record index");
            //return BitConverter.ToString(record, i, charCnt);
            return Encoding.ASCII.GetString(record, i, charCnt);
        }
        private string rdCn(byte[] record, ushort i, ushort len) {
            if (i > len) throw new Exception("wrong record index");
            byte charCnt = record[i];
            if (charCnt == 0) return "";
            if ((i + charCnt) > len) throw new Exception("wrong record index");
            //return BitConverter.ToString(record, i+1, charCnt);
            return Encoding.ASCII.GetString(record, i + 1, charCnt);
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
            if ((i + 1) > len) throw new Exception("wrong record index");
            return BitConverter.ToUInt16(record, i);
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
            for (byte j = 0; j < byteCnt; j++) {
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
            if ((i + 1) > len) throw new Exception("wrong record index");
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
                    rst[j] = (byte)(record[i + j / 2] & 0xf);
                } else {
                    rst[j] = (byte)((record[i + j / 2] & 0xf0) >> 4);
                }
            }
            return rst;
        }

        private ushort[] rdKxU2(byte[] record, ushort i, ushort len, ushort cnt) {
            if ((i + cnt * 2 - 1) > len) throw new Exception("wrong record index");
            ushort[] rst = new ushort[cnt];
            for (int j = 0; j < cnt; j++) {
                rst[j] = BitConverter.ToUInt16(record, (i + j * 2));
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
                i += (byte)(charCnt + 1);
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

    }
}
