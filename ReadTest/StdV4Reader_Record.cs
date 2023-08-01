using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using DataContainer;

namespace ReadTest {
    public partial class StdV4Reader : IDisposable {
        //#region record
        //private void AddFar(byte[] record, ushort len) {
        //    //ignore
        //}
        //private void AddAtr(byte[] record, ushort len) {
        //    //ignore
        //}
        //private void AddMir(byte[] record, ushort len) {
        //    ushort i = 0;
        //    var SetupTime = rdDateTime(record, i, len); i += 4;
        //    _dc.SetBasicInfo("SetupTime", SetupTime.ToString());
        //    var StartTime = rdDateTime(record, i, len); i += 4;
        //    _dc.SetBasicInfo("StartTime", StartTime.ToString());
        //    var StationNumber = rdU1(record, i, len); i += 1;
        //    _dc.SetBasicInfo("StationNumber", StationNumber.ToString());
        //    var TestModeCode = rdCx(record, i, len, 1); i += 1;
        //    _dc.SetBasicInfo("TestModeCode", TestModeCode);
        //    var ReTestCode = rdCx(record, i, len, 1); i += 1;
        //    _dc.SetBasicInfo("ReTestCode", ReTestCode);
        //    var ProtectionCode = rdCx(record, i, len, 1); i += 1;
        //    _dc.SetBasicInfo("ProtectionCode", ProtectionCode);
        //    var BurnInTime = rdU2(record, i, len); i += 2;
        //    _dc.SetBasicInfo("BurnInTime", BurnInTime.ToString());
        //    var CommandModeCode = rdCx(record, i, len, 1); i += 1;
        //    _dc.SetBasicInfo("CommandModeCode", CommandModeCode);
        //    var LotId = rdCn(record, i, len); i += (ushort)(1 + LotId.Length);
        //    _dc.SetBasicInfo("LotId", LotId);
        //    var PartType = rdCn(record, i, len); i += (ushort)(1 + PartType.Length);
        //    _dc.SetBasicInfo("PartType", PartType);
        //    var NodeName = rdCn(record, i, len); i += (ushort)(1 + NodeName.Length);
        //    _dc.SetBasicInfo("NodeName", NodeName);
        //    var TesterType = rdCn(record, i, len); i += (ushort)(1 + TesterType.Length);
        //    _dc.SetBasicInfo("TesterType", TesterType);
        //    var JobName = rdCn(record, i, len); i += (ushort)(1 + JobName.Length);
        //    _dc.SetBasicInfo("JobName", JobName);

        //    var JobRevision = rdCn(record, i, len); i += (ushort)(1 + JobRevision.Length); if (i > len) return;
        //    _dc.SetBasicInfo("JobRevision", JobRevision);
        //    var SublotId = rdCn(record, i, len); i += (ushort)(1 + SublotId.Length); if (i > len) return;
        //    _dc.SetBasicInfo("SublotId", SublotId);
        //    var Operator = rdCn(record, i, len); i += (ushort)(1 + Operator.Length); if (i > len) return;
        //    _dc.SetBasicInfo("Operator", Operator);
        //    var ExecType = rdCn(record, i, len); i += (ushort)(1 + ExecType.Length); if (i > len) return;
        //    _dc.SetBasicInfo("ExecType", ExecType);
        //    var ExecVersion = rdCn(record, i, len); i += (ushort)(1 + ExecVersion.Length); if (i > len) return;
        //    _dc.SetBasicInfo("ExecVersion", ExecVersion);
        //    var TestCode = rdCn(record, i, len); i += (ushort)(1 + TestCode.Length); if (i > len) return;
        //    _dc.SetBasicInfo("TestCode", TestCode);
        //    var TestTemperature = rdCn(record, i, len); i += (ushort)(1 + TestTemperature.Length); if (i > len) return;
        //    _dc.SetBasicInfo("TestTemperature", TestTemperature);
        //    var UserText = rdCn(record, i, len); i += (ushort)(1 + UserText.Length); if (i > len) return;
        //    _dc.SetBasicInfo("UserText", UserText);
        //    var AuxiliaryDataFile = rdCn(record, i, len); i += (ushort)(1 + AuxiliaryDataFile.Length); if (i > len) return;
        //    _dc.SetBasicInfo("AuxiliaryDataFile", AuxiliaryDataFile);
        //    var PackageType = rdCn(record, i, len); i += (ushort)(1 + PackageType.Length); if (i > len) return;
        //    _dc.SetBasicInfo("PackageType", PackageType);
        //    var FamilyId = rdCn(record, i, len); i += (ushort)(1 + FamilyId.Length); if (i > len) return;
        //    _dc.SetBasicInfo("FamilyId", FamilyId);
        //    var DateCode = rdCn(record, i, len); i += (ushort)(1 + DateCode.Length); if (i > len) return;
        //    _dc.SetBasicInfo("DateCode", DateCode);
        //    var FacilityId = rdCn(record, i, len); i += (ushort)(1 + FacilityId.Length); if (i > len) return;
        //    _dc.SetBasicInfo("FacilityId", FacilityId);
        //    var FloorID = rdCn(record, i, len); i += (ushort)(1 + FloorID.Length); if (i > len) return;
        //    _dc.SetBasicInfo("FloorID", FloorID);
        //    var ProcessID = rdCn(record, i, len); i += (ushort)(1 + ProcessID.Length); if (i > len) return;
        //    _dc.SetBasicInfo("ProcessID", ProcessID);
        //    var OperationFreq = rdCn(record, i, len); i += (ushort)(1 + OperationFreq.Length); if (i > len) return;
        //    _dc.SetBasicInfo("OperationFreq", OperationFreq);
        //    var SpecificationName = rdCn(record, i, len); i += (ushort)(1 + SpecificationName.Length); if (i > len) return;
        //    _dc.SetBasicInfo("SpecificationName", SpecificationName);
        //    var SpecificationVersion = rdCn(record, i, len); i += (ushort)(1 + SpecificationVersion.Length); if (i > len) return;
        //    _dc.SetBasicInfo("SpecificationVersion", SpecificationVersion);
        //    var FlowID = rdCn(record, i, len); i += (ushort)(1 + FlowID.Length); if (i > len) return;
        //    _dc.SetBasicInfo("FlowID", FlowID);
        //    var SetupID = rdCn(record, i, len); i += (ushort)(1 + SetupID.Length); if (i > len) return;
        //    _dc.SetBasicInfo("SetupID", SetupID);
        //    var DesignRevision = rdCn(record, i, len); i += (ushort)(1 + DesignRevision.Length); if (i > len) return;
        //    _dc.SetBasicInfo("DesignRevision", DesignRevision);
        //    var EngineeringID = rdCn(record, i, len); i += (ushort)(1 + EngineeringID.Length); if (i > len) return;
        //    _dc.SetBasicInfo("EngineeringID", EngineeringID);
        //    var RomID = rdCn(record, i, len); i += (ushort)(1 + RomID.Length); if (i > len) return;
        //    _dc.SetBasicInfo("RomID", RomID);
        //    var SerialNumber = rdCn(record, i, len); i += (ushort)(1 + SerialNumber.Length); if (i > len) return;
        //    _dc.SetBasicInfo("SerialNumber", SerialNumber);
        //    var SupervisorID = rdCn(record, i, len); i += (ushort)(1 + SupervisorID.Length); if (i > len) return;
        //    _dc.SetBasicInfo("SupervisorID", SupervisorID);
        //}
        //private void AddMrr(byte[] record, ushort len) {
        //    ushort i = 0;
        //    var FinishTime = rdDateTime(record, i, len); i += 4; if (i > len) return;
        //    _dc.SetBasicInfo("FinishTime", FinishTime.ToString());
        //    var LotDispositionCode = rdCx(record, i, len, 1); i += 1; if (i > len) return;
        //    _dc.SetBasicInfo("LotDispositionCode", LotDispositionCode);
        //    var LotUserDecription = rdCn(record, i, len); i += (ushort)(1 + LotUserDecription.Length);
        //    _dc.SetBasicInfo("LotUserDecription", LotUserDecription);
        //    var LotExecDecription = rdCn(record, i, len); i += (ushort)(1 + LotExecDecription.Length);
        //    _dc.SetBasicInfo("LotExecDecription", LotExecDecription);
        //}
        //private void AddPcr(byte[] record, ushort len) {
        //    //ignore
        //}
        //private void AddHbr(byte[] record, ushort len) {
        //    ushort i = 0;
        //    var hn = rdU1(record, i, len); i += 1;
        //    var sn = rdU1(record, i, len); i += 1;
        //    var binNO = rdU2(record, i, len); i += 2;
        //    i += 4; //skip bin count
        //    var PorF = "";
        //    var binName = "";
        //    if (i < len) {
        //        PorF = rdCx(record, i, len, 1); i += 1;
        //    }
        //    if (i < len) {
        //        binName = rdCn(record, i, len);
        //    }

        //    _dc.AddHbr(binNO, new Tuple<string, string>(binName, PorF));

        //}
        //private void AddSbr(byte[] record, ushort len) {
        //    ushort i = 0;
        //    var hn = rdU1(record, i, len); i += 1;
        //    var sn = rdU1(record, i, len); i += 1;
        //    var binNO = rdU2(record, i, len); i += 2;
        //    i += 4; //skip bin count
        //    var PorF = "";
        //    var binName = "";
        //    if (i < len) {
        //        PorF = rdCx(record, i, len, 1); i += 1;
        //    }
        //    if (i < len) {
        //        binName = rdCn(record, i, len);
        //    }
        //    _dc.AddSbr(binNO, new Tuple<string, string>(binName, PorF));
        //}
        //private void AddPmr(byte[] record, ushort len) {
        //    ushort i = 0;
        //    var pinIdx = rdU2(record, i, len); i += 2;
        //    var chanType = rdU2(record, i, len); i += 2;
        //    var chanName = rdCn(record, i, len); i += (ushort)(1 + chanName.Length);
        //    //var phyName = rdCn(record, i, len); i += (ushort)(1 + phyName.Length);
        //    //var logicName = rdCn(record, i, len); i += (ushort)(1 + logicName.Length);
        //    //var hn = rdU1(record, i, len); i += 1;
        //    //var sn = rdU1(record, i, len); i += 1;

        //    listPinMaps.Add(new PinMapRecord(pinIdx, chanName));
        //}
        //private void AddPgr(byte[] record, ushort len) {
        //    //2022/03/09 comment PGR record
        //    //ushort i = 0;
        //    //var grpIdx = rdU2(record, i, len); i += 2;
        //    //var grpName = rdCn(record, i, len); i += (ushort)(1 + grpName.Length);
        //    //var cn = rdU2(record, i, len); i += 2;
        //    //if (cn == 0) return;
        //    //var idxes = rdKxU2(record, i, len, cn);

        //    //listPinGroups.Add(new PinGroupRecord(grpIdx, grpName, idxes, listPinMaps));
        //}
        //private void AddPlr(byte[] record, ushort len) {
        //    //ignore
        //}
        //private void AddRdr(byte[] record, ushort len) {
        //    //ignore
        //}
        //private void AddSdr(byte[] record, ushort len) {
        //    ushort i = 0;
        //    if (len < 4) throw new Exception("PTR record error");
        //    var hn = rdU1(record, i, len); i += 1;
        //    var sg = rdU1(record, i, len); i += 1;
        //    //if (sg > 0) throw new Exception("multi site group! not support");
        //    var sc = rdU1(record, i, len); i += 1;
        //    var sn = rdKxU1(record, i, len, sc); i += sc;
        //    for (int j = 0; j < sn.Length; j++) {
        //        _dc.AddSiteNum(sn[j]);
        //        _lastUidBySite.Add(sn[j], new TestID());
        //    }
        //    //ignore other optional data
        //}
        //private void AddWir(byte[] record, ushort len) {
        //    //ignore
        //}
        //private void AddWrr(byte[] record, ushort len) {
        //    //ignore
        //}
        //private void AddWcr(byte[] record, ushort len) {
        //    //ignore
        //}
        //private void AddPir(byte[] record, ushort len) {
        //    ushort i = 0;
        //    var hn = rdU1(record, i, len); i += 1;
        //    var sn = rdU1(record, i, len);
        //    //_dc.AddPir(sn);
        //    //_lastUidBySite[sn] = new TestID();
        //}
        //private void AddPrr(byte[] record, ushort len) {
        //    ushort i = 0;
        //    var hn = rdU1(record, i, len); i += 1;
        //    var sn = rdU1(record, i, len); i += 1;
        //    var partflag = rdB1(record, i, len); i += 1;
        //    var numtest = rdU2(record, i, len); i += 2;
        //    var hardbin = rdU2(record, i, len); i += 2;
        //    ushort softbin = rdU2(record, i, len); i += 2;
        //    var xcord = rdI2(record, i, len); i += 2;
        //    var ycord = rdI2(record, i, len); i += 2;
        //    uint? testtime = rdU4(record, i, len); i += 4;
        //    var partid = rdCn(record, i, len);
        //    //ignore the left part text and part fix

        //    //_dc.AddPrr(sn,
        //    //    testtime == 0 ? null : testtime,
        //    //    hardbin,
        //    //    softbin,
        //    //    partid,
        //    //    xcord, ycord,
        //    //    Bit(partflag, 0) ? DeviceType.RT_ID : (Bit(partflag, 1) ? DeviceType.RT_Cord : DeviceType.Fresh),
        //    //    Bit(partflag, 2) ? ResultType.Abort : (Bit(partflag, 4) ? ResultType.Null : (Bit(partflag, 3) ? ResultType.Fail : ResultType.Pass)));


        //}
        //private void AddTsr(byte[] record, ushort len) {
        //    //ignore
        //}
        //private void AddPtr(byte[] record, ushort len) {
        //    ushort i = 0;
        //    //if(len<28) throw new Exception("PTR record error");
        //    var tn = rdU4(record, i, len); i += 4;
        //    var hn = rdU1(record, i, len); i += 1;
        //    var sn = rdU1(record, i, len); i += 1;
        //    var tf = rdB1(record, i, len); i += 1;
        //    var pf = rdB1(record, i, len); i += 1;

        //    if (Bit(tf, 1))
        //        return;

        //    var result = rdR4(record, i, len); i += 4;
        //    string txt = "";
        //    if (i < len) {
        //        txt = rdCn(record, i, len); i += (ushort)(1 + txt.Length);
        //    }

        //    TestID id;
        //    //if (!_lastUidBySite[sn].IfSubTest(tn, txt)) {
        //    id = new TestID(tn, txt);
        //    //} else {
        //    //    id = new TestID(_lastUidBySite[sn]);
        //    //}

        //    //var info = _dc.IfContainItemInfo(id.GetUID());
        //    ItemInfo info = null;

        //    if (info == null && i < len) {
        //        var alarm = rdCn(record, i, len); i += (ushort)(1 + alarm.Length);
        //        var oFg = rdB1(record, i, len); i += 1;

        //        sbyte? resScal = null, llScal = null, hlScal = null;
        //        float? ll = null, hl = null;
        //        string unit = "";

        //        if (!Bit(oFg, 0) && i < len) {
        //            resScal = rdI1(record, i, len);
        //        }
        //        i++;
        //        if ((!Bit(oFg, 4)) && (!Bit(oFg, 6)) && i < len) {
        //            llScal = rdI1(record, i, len);
        //        }
        //        i++;
        //        if ((!Bit(oFg, 5)) && (!Bit(oFg, 7)) && i < len) {
        //            hlScal = rdI1(record, i, len);
        //        }
        //        i++;
        //        if ((!Bit(oFg, 4)) && (!Bit(oFg, 6)) && i < len) {
        //            ll = rdR4(record, i, len);
        //        }
        //        i += 4;
        //        if ((!Bit(oFg, 5)) && (!Bit(oFg, 7)) && i < len) {
        //            hl = rdR4(record, i, len);
        //        }
        //        i += 4;
        //        if (i < len) {
        //            unit = rdCn(record, i, len);
        //        }
        //        i += (ushort)(1 + unit.Length);
        //        info = new ItemInfo(txt, ll, hl, unit, llScal, hlScal, resScal);
        //        //_dc.UpdateItemInfo(id.GetUID(), info);
        //    }

        //    //info = _dc.GetTestInfo(id.GetUID());

        //    //means use last test limt
        //    //if (info == null) {
        //    //    if (_lastUidBySite[sn].IfSubTest(tn, txt)) {
        //    //        var tmpInfo = new ItemInfo(_dc.GetTestInfo(_lastUidBySite[sn].GetUID()));
        //    //        tmpInfo.TestText = txt;
        //    //        _dc.UpdateItemInfo(id.GetUID(), tmpInfo);
        //    //        result = tmpInfo.GetScaledRst(result);
        //    //    } else {
        //    //        var tmpInfo = new ItemInfo(txt, null, null, "", null, null, null);
        //    //        _dc.UpdateItemInfo(id.GetUID(), tmpInfo);
        //    //        result = tmpInfo.GetScaledRst(result);
        //    //    }
        //    //} else {
        //    //    result = info.GetScaledRst(result);
        //    //}


        //    //_dc.AddTestData(sn, id.GetUID(), result);
        //    //_lastUidBySite[sn] = id;
        //}
        //private void AddMpr(byte[] record, ushort len) {
        //    return;

        //    ushort i = 0;
        //    var tn = rdU4(record, i, len); i += 4;
        //    var hn = rdU1(record, i, len); i += 1;
        //    var sn = rdU1(record, i, len); i += 1;
        //    var tf = rdB1(record, i, len); i += 1;
        //    var pf = rdB1(record, i, len); i += 1;

        //    if (Bit(tf, 2)) return;

        //    try {
        //        var rtnCnt = rdU2(record, i, len); i += 2;
        //        var rstCnt = rdU2(record, i, len); i += 2;

        //        if (rtnCnt != rstCnt) throw new Exception("MPR pin count mismatch");

        //        var stat = rdKxN1(record, i, len, rtnCnt); i += (ushort)(rtnCnt / 2 + rtnCnt % 2);
        //        var rsts = rdKxR4(record, i, len, rstCnt); i += (ushort)(rstCnt * 4);
        //        string txt = "";
        //        if (i < len) {
        //            txt = rdCn(record, i, len); i += (ushort)(1 + txt.Length);
        //        }

        //        TestID id;
        //        if (!_lastUidBySite[sn].IfSubTest(tn, txt)) {
        //            id = new TestID(tn, txt);
        //        } else {
        //            id = new TestID(_lastUidBySite[sn]);
        //        }


        //        TestID[] uids = new TestID[rsts.Length];

        //        uids[0] = id;
        //        //var info = _dc.IfContainItemInfo(uids[0].GetUID());
        //        ItemInfo info = null;

        //        for (uint j = 1; j < rtnCnt; j++) {
        //            uids[j] = new TestID(uids[j - 1]);
        //            //if (_dc.IfContainItemInfo(uids[j].GetUID()) == null && info != null) _dc.UpdateItemInfo(uids[j].GetUID(), info);
        //        }

        //        if (info == null && i < len) {

        //            var alarm = rdCn(record, i, len); i += (ushort)(1 + alarm.Length);

        //            byte oFg = 0;
        //            if (i < len) {
        //                oFg = rdB1(record, i, len); i += 1;
        //            }

        //            sbyte? resScal = null, llScal = null, hlScal = null;
        //            float? ll = null, hl = null;
        //            string unit = "";

        //            if (!Bit(oFg, 0) && i < len) {
        //                resScal = rdI1(record, i, len);
        //            }
        //            i++;
        //            if ((!Bit(oFg, 4)) && (!Bit(oFg, 6)) && i < len) {
        //                llScal = rdI1(record, i, len);
        //            }
        //            i++;
        //            if ((!Bit(oFg, 5)) && (!Bit(oFg, 7)) && i < len) {
        //                hlScal = rdI1(record, i, len);
        //            }
        //            i++;
        //            if ((!Bit(oFg, 4)) && (!Bit(oFg, 6)) && i < len) {
        //                ll = rdR4(record, i, len);
        //            }
        //            i += 4;
        //            if ((!Bit(oFg, 5)) && (!Bit(oFg, 7)) && i < len) {
        //                hl = rdR4(record, i, len);
        //            }
        //            i += 4;

        //            i += 8; //skip start in and incr in
        //            if (i < len) {
        //                var idxs = rdKxU2(record, i, len, rtnCnt); i += (ushort)(rtnCnt * 2);

        //                if (i < len) {
        //                    unit = rdCn(record, i, len);
        //                }
        //                i += (ushort)(1 + unit.Length);

        //                for (uint j = 0; j < idxs.Length; j++) {
        //                    var pin = listPinMaps.Find(x => x.PinIndex == idxs[j]).ChanName;
        //                    var tmpInfo = new ItemInfo(txt + "_" + pin, ll, hl, unit, llScal, hlScal, resScal);
        //                    //_dc.UpdateItemInfo(uids[j].GetUID(), tmpInfo);
        //                }
        //            }

        //        }

        //        //means use last test limt
        //        //if (_dc.GetTestInfo(uids[0].GetUID()) == null) {
        //        //    var refInfo = _dc.GetTestInfo(_lastUidBySite[sn].GetUID());
        //        //    for (uint j = 0; j < rtnCnt; j++) {
        //        //        var tmpInfo = new ItemInfo(refInfo);
        //        //        tmpInfo.TestText = txt;
        //        //        _dc.UpdateItemInfo(uids[j].GetUID(), tmpInfo);
        //        //    }

        //        //}

        //        //info = _dc.GetTestInfo(uids[0].GetUID());
        //        //for (uint j = 0; j < rtnCnt; j++) {
        //        //    rsts[j] = info.GetScaledRst(rsts[j]);
        //        //    _dc.AddTestData(sn, uids[j].GetUID(), rsts[j]);
        //        //    _lastUidBySite[sn] = uids[j];
        //        //}
        //    } catch {
        //        throw;
        //    }



        //}
        //private void AddFtr(byte[] record, ushort len) {
        //    return;

        //    ushort i = 0;
        //    var tn = rdU4(record, i, len); i += 4;
        //    var hn = rdU1(record, i, len); i += 1;
        //    var sn = rdU1(record, i, len); i += 1;

        //    //FTR no sub test, same test number means fail cyc record, do not need proceed
        //    var tf = rdB1(record, i, len); i += 1;
        //    if (Bit(tf, 2)) return;
        //    int result = (Bit(tf, 6) || Bit(tf, 7)) == false ? 1 : 0;

        //    if (i >= len) return;
        //    try {
        //        //var of = rdB1(record, i, len); i += 1; //option flag
        //        //if (Bit(of, 0)) i += 4;
        //        //if (Bit(of, 1)) i += 4;
        //        //if (Bit(of, 2)) i += 4;
        //        //if (Bit(of, 3)) i += 4;
        //        //if (Bit(of, 4)) i += 8;
        //        //if (Bit(of, 5)) i += 2;
        //        i += 27;

        //        var rtncnt = rdU2(record, i, len); i += 2;
        //        var pgmcnt = rdU2(record, i, len); i += 2;

        //        i += (ushort)(rtncnt * 2);
        //        i += (ushort)(rtncnt / 2 + rtncnt % 2);
        //        i += (ushort)(pgmcnt * 2);
        //        i += (ushort)(pgmcnt / 2 + pgmcnt % 2);

        //        if (i >= len) return;
        //        //var failpin = rdDn(record, i, len); i += (ushort)(2 + failpin.Length);
        //        i += skipDn(record, i, len);

        //        i += (ushort)(1 + record[i]);
        //        i += (ushort)(1 + record[i]);
        //        i += (ushort)(1 + record[i]);

        //        if (i >= len) return;
        //        var txt = rdCn(record, i, len); i += (ushort)(1 + txt.Length);

        //        TestID id;
        //        if (!_lastUidBySite[sn].IfSubTest(tn, txt)) {
        //            id = new TestID(tn, txt);
        //            _lastUidBySite[sn] = id;
        //        } else {
        //            id = _lastUidBySite[sn];
        //            //id = new TestID(_lastUidBySite[sn]);
        //            //_lastUidBySite[sn] = id;
        //        }

        //        //if (_dc.IfContainItemInfo(id.GetUID()) is null) {
        //        //    _dc.UpdateItemInfo(id.GetUID(), new ItemInfo(txt, 0.5f, 1.5f, "", 0, 0, 0));
        //        //}
        //        ////proceed the rst
        //        //_dc.AddTestData(sn, id.GetUID(), result);
        //    } catch {
        //        throw new Exception("FTR cannot get item info");
        //    }
        //}
        //private void AddBps(byte[] record, ushort len) {

        //}
        //private void AddEps(byte[] record, ushort len) {

        //}
        //private void AddGdr(byte[] record, ushort len) {

        //}
        //private void AddDtr(byte[] record, ushort len) {

        //}
        //#endregion


        #region record
        private void AddFar(byte[] record, ushort len) {
            //ignore
        }

        private void AddAtr(byte[] record, ushort len) {
            //ignore
        }

        private void AddMir(byte[] record, ushort len) {
            ushort i = 0;
            try {
                var SetupTime = rdDateTime(record, ref i, len);
                _dc.SetBasicInfo("SetupTime", SetupTime.ToString());
                var StartTime = rdDateTime(record, ref i, len);
                _dc.SetBasicInfo("StartTime", StartTime.ToString());
                var StationNumber = rdU1(record, ref i, len);
                _dc.SetBasicInfo("StationNumber", StationNumber.ToString());
                var TestModeCode = rdCx(record, ref i, len, 1);
                _dc.SetBasicInfo("TestModeCode", TestModeCode);
                var ReTestCode = rdCx(record, ref i, len, 1);
                _dc.SetBasicInfo("ReTestCode", ReTestCode);
                var ProtectionCode = rdCx(record, ref i, len, 1);
                _dc.SetBasicInfo("ProtectionCode", ProtectionCode);
                var BurnInTime = rdU2(record, ref i, len);
                _dc.SetBasicInfo("BurnInTime", BurnInTime.ToString());
                var CommandModeCode = rdCx(record, ref i, len, 1);
                _dc.SetBasicInfo("CommandModeCode", CommandModeCode);
                var LotId = rdCn(record, ref i, len);
                _dc.SetBasicInfo("LotId", LotId);
                var PartType = rdCn(record, ref i, len);
                _dc.SetBasicInfo("PartType", PartType);
                var NodeName = rdCn(record, ref i, len);
                _dc.SetBasicInfo("NodeName", NodeName);
                var TesterType = rdCn(record, ref i, len);
                _dc.SetBasicInfo("TesterType", TesterType);
                var JobName = rdCn(record, ref i, len);
                _dc.SetBasicInfo("JobName", JobName);

                var JobRevision = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("JobRevision", JobRevision); if (i >= len) return;
                var SublotId = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("SublotId", SublotId); if (i >= len) return;
                var Operator = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("Operator", Operator); if (i >= len) return;
                var ExecType = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("ExecType", ExecType); if (i >= len) return;
                var ExecVersion = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("ExecVersion", ExecVersion); if (i >= len) return;
                var TestCode = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("TestCode", TestCode); if (i >= len) return;
                var TestTemperature = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("TestTemperature", TestTemperature); if (i >= len) return;
                var UserText = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("UserText", UserText); if (i >= len) return;
                var AuxiliaryDataFile = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("AuxiliaryDataFile", AuxiliaryDataFile); if (i >= len) return;
                var PackageType = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("PackageType", PackageType); if (i >= len) return;
                var FamilyId = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("FamilyId", FamilyId); if (i >= len) return;
                var DateCode = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("DateCode", DateCode); if (i >= len) return;
                var FacilityId = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("FacilityId", FacilityId); if (i >= len) return;
                var FloorID = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("FloorID", FloorID); if (i >= len) return;
                var ProcessID = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("ProcessID", ProcessID); if (i >= len) return;
                var OperationFreq = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("OperationFreq", OperationFreq); if (i >= len) return;
                var SpecificationName = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("SpecificationName", SpecificationName); if (i >= len) return;
                var SpecificationVersion = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("SpecificationVersion", SpecificationVersion); if (i >= len) return;
                var FlowID = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("FlowID", FlowID); if (i >= len) return;
                var SetupID = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("SetupID", SetupID); if (i >= len) return;
                var DesignRevision = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("DesignRevision", DesignRevision); if (i >= len) return;
                var EngineeringID = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("EngineeringID", EngineeringID); if (i >= len) return;
                var RomID = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("RomID", RomID); if (i >= len) return;
                var SerialNumber = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("SerialNumber", SerialNumber); if (i >= len) return;
                var SupervisorID = rdCn(record, ref i, len); 
                _dc.SetBasicInfo("SupervisorID", SupervisorID); if (i >= len) return;
            } catch {
                //msg
            }
        }

        private void AddMrr(byte[] record, ushort len) {
            ushort i = 0;
            try {
                var FinishTime = rdDateTime(record, ref i, len); 
                _dc.SetBasicInfo("FinishTime", FinishTime.ToString()); if (i >= len) return;
                var LotDispositionCode = rdCx(record, ref i, len, 1); 
                _dc.SetBasicInfo("LotDispositionCode", LotDispositionCode); if (i >= len) return;
                var LotUserDecription = rdCn(record, ref i, len);
                _dc.SetBasicInfo("LotUserDecription", LotUserDecription);
                var LotExecDecription = rdCn(record, ref i, len);
                _dc.SetBasicInfo("LotExecDecription", LotExecDecription);
            } catch {
                //msg
            }
        }

        private void AddPcr(byte[] record, ushort len) {
            //ignore
        }

        private void AddHbr(byte[] record, ushort len) {
            ushort i = 0;
            try {
                var hn = rdU1(record, ref i, len);
                var sn = rdU1(record, ref i, len);
                var binNO = rdU2(record, ref i, len);
                i += 4; //skip bin count
                var PorF = "";
                var binName = "";
                if (i < len) {
                    PorF = rdCx(record, ref i, len, 1);
                }
                if (i < len) {
                    binName = rdCn(record, ref i, len);
                }

                _dc.AddHbr(binNO, new Tuple<string, string>(binName, PorF));
            } catch {
                //msg
            }

        }

        private void AddSbr(byte[] record, ushort len) {
            ushort i = 0;
            try {
                var hn = rdU1(record, ref i, len);
                var sn = rdU1(record, ref i, len);
                var binNO = rdU2(record, ref i, len);
                i += 4; //skip bin count
                var PorF = "";
                var binName = "";
                if (i < len) {
                    PorF = rdCx(record, ref i, len, 1);
                }
                if (i < len) {
                    binName = rdCn(record, ref i, len);
                }
                _dc.AddSbr(binNO, new Tuple<string, string>(binName, PorF));
            } catch {
                //msg
            }
        }

        private void AddPmr(byte[] record, ushort len) {
            ushort i = 0;
            try {
                var pinIdx = rdU2(record, ref i, len);
                var chanType = rdU2(record, ref i, len);
                var chanName = rdCn(record, ref i, len);
                //var phyName = rdCn(record, ref i, len); 
                //var logicName = rdCn(record, ref i, len); 
                //var hn = rdU1(record, ref i, len); 
                //var sn = rdU1(record, ref i, len); 

                listPinMaps.Add(new PinMapRecord(pinIdx, chanName));
            } catch {
                //msg
            }
        }

        private void AddPgr(byte[] record, ushort len) {
            //2022/03/09 comment PGR record
            //ushort i = 0;
            //var grpIdx = rdU2(record, ref i, len); 
            //var grpName = rdCn(record, ref i, len); 
            //var cn = rdU2(record, ref i, len); 
            //if (cn == 0) return;
            //var idxes = rdKxU2(record, ref i, len, cn);

            //listPinGroups.Add(new PinGroupRecord(grpIdx, grpName, idxes, listPinMaps));
        }

        private void AddPlr(byte[] record, ushort len) {
            //ignore
        }

        private void AddRdr(byte[] record, ushort len) {
            //ignore
        }

        private void AddSdr(byte[] record, ushort len) {
            ushort i = 0;
            try {
                var hn = rdU1(record, ref i, len);
                var sg = rdU1(record, ref i, len);
                //if (sg > 0) throw new Exception("multi site group! not support");
                var sc = rdU1(record, ref i, len);
                var sn = rdKxU1(record, ref i, len, sc);
                for (int j = 0; j < sn.Length; j++) {
                    _dc.AddSiteNum(sn[j]);
                }
                var HAND_TYP = rdCn(record, ref i, len);
                _dc.SetBasicInfo("HAND_TYP", HAND_TYP); if (i >= len) return;

                var HAND_ID = rdCn(record, ref i, len);
                _dc.SetBasicInfo("HAND_ID", HAND_ID); if (i >= len) return;

                var CARD_TYP = rdCn(record, ref i, len);
                _dc.SetBasicInfo("CARD_TYP", CARD_TYP); if (i >= len) return;

                var CARD_ID = rdCn(record, ref i, len);
                _dc.SetBasicInfo("CARD_ID", CARD_ID); if (i >= len) return;

                var LOAD_TYP = rdCn(record, ref i, len);
                _dc.SetBasicInfo("LOAD_TYP", LOAD_TYP); if (i >= len) return;

                var LOAD_ID = rdCn(record, ref i, len);
                _dc.SetBasicInfo("LOAD_ID", LOAD_ID); if (i >= len) return;

                var DIB_TYP = rdCn(record, ref i, len);
                _dc.SetBasicInfo("DIB_TYP", DIB_TYP); if (i >= len) return;

                var DIB_ID = rdCn(record, ref i, len);
                _dc.SetBasicInfo("DIB_ID", DIB_ID); if (i >= len) return;

                var CABL_TYP = rdCn(record, ref i, len);
                _dc.SetBasicInfo("CABL_TYP", CABL_TYP); if (i >= len) return;

                var CABL_ID = rdCn(record, ref i, len);
                _dc.SetBasicInfo("CABL_ID", CABL_ID); if (i >= len) return;

                var CONT_TYP = rdCn(record, ref i, len);
                _dc.SetBasicInfo("CONT_TYP", CONT_TYP); if (i >= len) return;

                var CONT_ID = rdCn(record, ref i, len);
                _dc.SetBasicInfo("CONT_ID", CONT_ID); if (i >= len) return;

                var LASR_TYP = rdCn(record, ref i, len);
                _dc.SetBasicInfo("LASR_TYP", LASR_TYP); if (i >= len) return;

                var LASR_ID = rdCn(record, ref i, len);
                _dc.SetBasicInfo("LASR_ID", LASR_ID); if (i >= len) return;

                var EXTR_TYP = rdCn(record, ref i, len);
                _dc.SetBasicInfo("EXTR_TYP", EXTR_TYP); if (i >= len) return;

                var EXTR_ID = rdCn(record, ref i, len);
                _dc.SetBasicInfo("EXTR_ID", EXTR_ID); if (i >= len) return;

            } catch {
                //msg
            }
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

        private void AddPir(byte[] record, ushort len, ref PirType[] pirBuf, int pIdx) {
            ushort i = 0;
            try {
                var hn = rdU1(record, ref i, len);
                var sn = rdU1(record, ref i, len);
                //_dc.AddPir(sn);
                pirBuf[sn].PartIdx = pIdx;
                pirBuf[sn].LastTestUid = new TestID();
            } catch {
                //msg
            }
        }

        private void AddPrr(byte[] record, ushort len) {
            ushort i = 0;
            try {
                var hn = rdU1(record, ref i, len);
                var sn = rdU1(record, ref i, len);
                var partflag = rdB1(record, ref i, len);
                var numtest = rdU2(record, ref i, len);
                var hardbin = rdU2(record, ref i, len);
                ushort softbin = rdU2(record, ref i, len);
                var xcord = rdI2(record, ref i, len);
                var ycord = rdI2(record, ref i, len);
                uint testtime = rdU4(record, ref i, len);
                var partid = rdCn(record, ref i, len);
                //ignore the left part text and part fix

                //_dc.AddPrr(sn,
                //    testtime == 0 ? null : testtime,
                //    hardbin,
                //    softbin,
                //    partid,
                //    xcord, ycord,
                //    Bit(partflag, 0) ? DeviceType.RT_ID : (Bit(partflag, 1) ? DeviceType.RT_Cord : DeviceType.Fresh),
                //    Bit(partflag, 2) ? ResultType.Abort : (Bit(partflag, 4) ? ResultType.Null : (Bit(partflag, 3) ? ResultType.Fail : ResultType.Pass)));
            } catch {

            }

        }

        private void AddTsr(byte[] record, ushort len) {
            //ignore
        }

        private void AddPtr(byte[] record, ushort len, PirType[] pirBuf) {
            ushort i = 0;
            //try {
                var tn = rdU4(record, ref i, len);
                var hn = rdU1(record, ref i, len);
                var sn = rdU1(record, ref i, len);
                var tf = rdB1(record, ref i, len);
                var pf = rdB1(record, ref i, len);

                if (Bit(tf, 1)) return;

                var result = rdR4(record, ref i, len);
                string txt = "";
                if (i < len) {
                    txt = rdCn(record, ref i, len);
                } else {
                    return;
                }

                TestID id;
                if (!pirBuf[sn].LastTestUid.IfSubTest(tn, txt)) {
                    id = new TestID(tn, txt);
                } else {
                    id = new TestID(pirBuf[sn].LastTestUid);
                }

                var info = _dc.IfContainItemInfo(id.GetUID());

                if (info == null && i < len) {
                    var alarm = rdCn(record, ref i, len);
                    var oFg = rdB1(record, ref i, len);

                    sbyte? resScal = null, llScal = null, hlScal = null;
                    float? ll = null, hl = null;
                    string unit = "";
                    
                    if(i < len) {
                        var v = rdI1(record, ref i, len);
                        if (!Bit(oFg, 0)) {
                            resScal = v;
                        }
                    }
                    if (i < len) {
                        var v = rdI1(record, ref i, len);
                        if ((!Bit(oFg, 4)) && (!Bit(oFg, 6))) {
                            llScal = v;
                        }
                    }

                    if (i < len) {
                        var v = rdI1(record, ref i, len);
                        if ((!Bit(oFg, 5)) && (!Bit(oFg, 7))) {
                            hlScal = v;
                        }
                    }

                    if (i < len) {
                        var v = rdI4(record, ref i, len);
                        if ((!Bit(oFg, 4)) && (!Bit(oFg, 6))) {
                            ll = v;
                        }
                    }

                    if (i < len) {
                        var v = rdI4(record, ref i, len);
                        if ((!Bit(oFg, 5)) && (!Bit(oFg, 7))) {
                            hl = v;
                        }
                    }
                    if (i < len) {
                        unit = rdCn(record, ref i, len);
                    }
                    info = new ItemInfo(txt, ll, hl, unit, llScal, hlScal, resScal);
                    _dc.UpdateItemInfo(id.GetUID(), info);
                }

                info = _dc.GetTestInfo(id.GetUID());

                //means use last test limt
                if (info == null) {
                    if (pirBuf[sn].LastTestUid.IfSubTest(tn, txt)) {
                        var tmpInfo = new ItemInfo(_dc.GetTestInfo(pirBuf[sn].LastTestUid.GetUID()));
                        tmpInfo.TestText = txt;
                        _dc.UpdateItemInfo(id.GetUID(), tmpInfo);
                        result = tmpInfo.GetScaledRst(result);
                    } else {
                        var tmpInfo = new ItemInfo(txt, null, null, "", null, null, null);
                        _dc.UpdateItemInfo(id.GetUID(), tmpInfo);
                        result = tmpInfo.GetScaledRst(result);
                    }
                } else {
                    result = info.GetScaledRst(result);
                }


                _dc.AddTestData(id.GetUID(), pirBuf[sn].PartIdx, result);
                pirBuf[sn].LastTestUid = id;
            //} catch {
            //    //msg
            //}
        }

        private void AddMpr(byte[] record, ushort len, PirType[] pirBuf) {
            return;

            ushort i = 0;
            var tn = rdU4(record, ref i, len);
            var hn = rdU1(record, ref i, len);
            var sn = rdU1(record, ref i, len);
            var tf = rdB1(record, ref i, len);
            var pf = rdB1(record, ref i, len);

            if (Bit(tf, 2)) return;

            try {
                var rtnCnt = rdU2(record, ref i, len);
                var rstCnt = rdU2(record, ref i, len);

                if (rtnCnt != rstCnt) throw new Exception("MPR pin count mismatch");

                var stat = rdKxN1(record, ref i, len, rtnCnt);
                var rsts = rdKxR4(record, ref i, len, rstCnt);
                string txt = "";
                if (i < len) {
                    txt = rdCn(record, ref i, len);
                }

                TestID id;
                if (!pirBuf[sn].LastTestUid.IfSubTest(tn, txt)) {
                    id = new TestID(tn, txt);
                } else {
                    id = new TestID(pirBuf[sn].LastTestUid);
                }


                TestID[] uids = new TestID[rsts.Length];

                uids[0] = id;
                //var info = _dc.IfContainItemInfo(uids[0].GetUID());
                ItemInfo info = null;

                for (uint j = 1; j < rtnCnt; j++) {
                    uids[j] = new TestID(uids[j - 1]);
                    //if(_dc.IfContainItemInfo(uids[j].GetUID()) == null && info != null) _dc.UpdateItemInfo(uids[j].GetUID(), info);
                }

                if (info == null && i < len) {

                    var alarm = rdCn(record, ref i, len);

                    byte oFg = 0;
                    if (i < len) {
                        oFg = rdB1(record, ref i, len);
                    }

                    sbyte? resScal = null, llScal = null, hlScal = null;
                    float? ll = null, hl = null;
                    string unit = "";

                    if (!Bit(oFg, 0) && i < len) {
                        resScal = rdI1(record, ref i, len);
                    }
                    if ((!Bit(oFg, 4)) && (!Bit(oFg, 6)) && i < len) {
                        llScal = rdI1(record, ref i, len);
                    }
                    if ((!Bit(oFg, 5)) && (!Bit(oFg, 7)) && i < len) {
                        hlScal = rdI1(record, ref i, len);
                    }
                    if ((!Bit(oFg, 4)) && (!Bit(oFg, 6)) && i < len) {
                        ll = rdR4(record, ref i, len);
                    }
                    if ((!Bit(oFg, 5)) && (!Bit(oFg, 7)) && i < len) {
                        hl = rdR4(record, ref i, len);
                    }

                    i += 8; //skip start in and incr in
                    if (i < len) {
                        var idxs = rdKxU2(record, ref i, len, rtnCnt);

                        if (i < len) {
                            unit = rdCn(record, ref i, len);
                        }

                        for (uint j = 0; j < idxs.Length; j++) {
                            var pin = listPinMaps.Find(x => x.PinIndex == idxs[j]).ChanName;
                            var tmpInfo = new ItemInfo(txt + "_" + pin, ll, hl, unit, llScal, hlScal, resScal);
                            //_dc.UpdateItemInfo(uids[j].GetUID(), tmpInfo);
                        }
                    }

                }

                //means use last test limt
                //if(_dc.GetTestInfo(uids[0].GetUID()) == null) {
                //    var refInfo = _dc.GetTestInfo(pirBuf[sn].LastTestUid.GetUID());
                //    for (uint j = 0; j < rtnCnt; j++) {
                //        var tmpInfo = new ItemInfo(refInfo);
                //        tmpInfo.TestText = txt;
                //        _dc.UpdateItemInfo(uids[j].GetUID(), tmpInfo);
                //    }

                //}

                //info = _dc.GetTestInfo(uids[0].GetUID());
                //for (uint j = 0; j < rtnCnt; j++) {
                //    rsts[j] = info.GetScaledRst(rsts[j]);
                //    _dc.AddTestData(sn, uids[j].GetUID(), rsts[j]);
                //    pirBuf[sn].LastTestUid = uids[j];
                //}
            } catch {
                throw;
            }



        }

        private void AddFtr(byte[] record, ushort len, PirType[] pirBuf) {
            return;

            ushort i = 0;
            var tn = rdU4(record, ref i, len);
            var hn = rdU1(record, ref i, len);
            var sn = rdU1(record, ref i, len);

            //FTR no sub test, same test number means fail cyc record, do not need proceed
            var tf = rdB1(record, ref i, len);
            if (Bit(tf, 2)) return;
            int result = (Bit(tf, 6) || Bit(tf, 7)) == false ? 1 : 0;

            if (i >= len) return;
            try {
                //var of = rdB1(record, ref i, len);  //option flag
                //if (Bit(of, 0)) 
                //if (Bit(of, 1)) 
                //if (Bit(of, 2)) 
                //if (Bit(of, 3)) 
                //if (Bit(of, 4)) 
                //if (Bit(of, 5)) 
                i += 27;

                var rtncnt = rdU2(record, ref i, len);
                var pgmcnt = rdU2(record, ref i, len);

                i += (ushort)(rtncnt * 2);
                i += (ushort)(rtncnt / 2 + rtncnt % 2);
                i += (ushort)(pgmcnt * 2);
                i += (ushort)(pgmcnt / 2 + pgmcnt % 2);

                if (i >= len) return;
                //var failpin = rdDn(record, ref i, len); 
                skipDn(record, ref i, len);

                i += (ushort)(1 + record[i]);
                i += (ushort)(1 + record[i]);
                i += (ushort)(1 + record[i]);

                if (i >= len) return;
                var txt = rdCn(record, ref i, len);

                TestID id;
                if (!pirBuf[sn].LastTestUid.IfSubTest(tn, txt)) {
                    id = new TestID(tn, txt);
                    pirBuf[sn].LastTestUid = id;
                } else {
                    id = pirBuf[sn].LastTestUid;
                    //id = new TestID(pirBuf[sn].LastTestUid);
                    //    pirBuf[sn].LastTestUid = id;
                }

                //if (_dc.IfContainItemInfo(id.GetUID()) is null) {
                //    _dc.UpdateItemInfo(id.GetUID(), new ItemInfo(txt, 0.5f, 1.5f, "", 0, 0, 0));
                //}
                ////proceed the rst
                //_dc.AddTestData(sn, id.GetUID(), result);
            } catch {
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


    }
}
