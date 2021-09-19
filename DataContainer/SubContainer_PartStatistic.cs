using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public partial class SubContainer {

        private List<byte> _site_PartContainer;
        private List<UInt32?> _testTime_PartContainer;
        private List<UInt16> _hardBin_PartContainer;
        private List<UInt16> _softBin_PartContainer;
        private List<string> _partId_PartContainer;
        private List<short?> _xCord_PartContainer;
        private List<short?> _yCord_PartContainer;
        private List<DeviceType> _chipType_PartContainer;
        private List<ResultType> _resultType_PartContainer;

        private PartStatistic _partStatistic;
        private bool _ifCordValid;

        private void Initialize_PartStatistic() {
            _site_PartContainer = new List<byte>(200000);
            _testTime_PartContainer = new List<uint?>(200000);
            _hardBin_PartContainer = new List<ushort>(200000);
            _softBin_PartContainer = new List<ushort>(200000);
            _partId_PartContainer = new List<string>(200000);
            _xCord_PartContainer = new List<short?>(200000);
            _yCord_PartContainer = new List<short?>(200000);
            _chipType_PartContainer = new List<DeviceType>(200000);
            _resultType_PartContainer = new List<ResultType>(200000);

            _ifCordValid = true;
        }

        private void AnalyseParts() {
            _site_PartContainer.TrimExcess();
            _testTime_PartContainer.TrimExcess();
            _hardBin_PartContainer.TrimExcess();
            _softBin_PartContainer.TrimExcess();
            _partId_PartContainer.TrimExcess();
            _xCord_PartContainer.TrimExcess();
            _yCord_PartContainer.TrimExcess();
            _chipType_PartContainer.TrimExcess();
            _resultType_PartContainer.TrimExcess();

            _partStatistic = new PartStatistic(_siteContainer.Keys);

            if (_site_PartContainer.Count - 1 != _partIdx) throw new Exception("Error");

            for(int i=0; i <= _partIdx; i++) {
                var sn = _site_PartContainer[i];
                _partStatistic.SiteCnt[sn] += 1;
                
                if (_partStatistic.HardBinBySite[sn].ContainsKey(_hardBin_PartContainer[i])) {
                    _partStatistic.HardBinBySite[sn][_hardBin_PartContainer[i]] += 1;
                }else {
                    _partStatistic.HardBinBySite[sn].Add(_hardBin_PartContainer[i], 0);
                }
                
                if (_partStatistic.SoftBinBySite[sn].ContainsKey(_softBin_PartContainer[i])) {
                    _partStatistic.SoftBinBySite[sn][_softBin_PartContainer[i]] += 1;
                } else {
                    _partStatistic.SoftBinBySite[sn].Add(_softBin_PartContainer[i], 0);
                }
                
                switch (_chipType_PartContainer[i]) {
                    case DeviceType.Fresh:
                        _partStatistic.FreshCntBySite[sn] += 1;
                        break;
                    case DeviceType.RT_ID:
                        _partStatistic.RtByIdCntBySite[sn] += 1;
                        break;
                    case DeviceType.RT_Cord:
                        _partStatistic.RtByCordCntBySite[sn] += 1;
                        break;
                }

                switch (_resultType_PartContainer[i]) {
                    case ResultType.Pass:
                        _partStatistic.PassCntBySite[sn] += 1;
                        break;
                    case ResultType.Fail:
                        _partStatistic.FailCntBySite[sn] += 1;
                        break;
                    case ResultType.Abort:
                        _partStatistic.AbortCntBySite[sn] += 1;
                        break;
                    case ResultType.Null:
                        _partStatistic.NullCntBySite[sn] += 1;
                        break;
                }
            }

            _partStatistic.TotalCnt = (from r in _partStatistic.SiteCnt select r.Value).Sum();
            _partStatistic.HardBin = new Dictionary<ushort, int>();
            _partStatistic.SoftBin = new Dictionary<ushort, int>();
            foreach(var sv in _partStatistic.HardBinBySite) {
                foreach(var v in sv.Value) {
                    if (_partStatistic.HardBin.ContainsKey(v.Key)) {
                        _partStatistic.HardBin[v.Key] += v.Value;
                    } else {
                        _partStatistic.HardBin.Add(v.Key, v.Value);
                    }
                }
            }
            foreach (var sv in _partStatistic.SoftBinBySite) {
                foreach (var v in sv.Value) {
                    if (_partStatistic.SoftBin.ContainsKey(v.Key)) {
                        _partStatistic.SoftBin[v.Key] += v.Value;
                    } else {
                        _partStatistic.SoftBin.Add(v.Key, v.Value);
                    }
                }
            }

            _partStatistic.PassCnt = (from r in _partStatistic.PassCntBySite select r.Value).Sum();
            _partStatistic.FailCnt = (from r in _partStatistic.FailCntBySite select r.Value).Sum();
            _partStatistic.AbortCnt = (from r in _partStatistic.AbortCntBySite select r.Value).Sum();
            _partStatistic.NullCnt = (from r in _partStatistic.NullCntBySite select r.Value).Sum();
            _partStatistic.FreshCnt = (from r in _partStatistic.FreshCntBySite select r.Value).Sum();
            _partStatistic.RtByIdCnt = (from r in _partStatistic.RtByIdCntBySite select r.Value).Sum();
            _partStatistic.RtByCordCnt = (from r in _partStatistic.RtByCordCntBySite select r.Value).Sum();

        }


        private void AnalyseParts_Filtered(Filter filter) {
            filter.FilterPartStatistic.SiteCnt = new Dictionary<byte, int>();
            filter.FilterPartStatistic.HardBinBySite = new Dictionary<byte, Dictionary<ushort, int>>(); ;
            filter.FilterPartStatistic.SoftBinBySite = new Dictionary<byte, Dictionary<ushort, int>>();
            filter.FilterPartStatistic.PassCntBySite = new Dictionary<byte, int>();
            filter.FilterPartStatistic.FailCntBySite = new Dictionary<byte, int>();
            filter.FilterPartStatistic.AbortCntBySite = new Dictionary<byte, int>();
            filter.FilterPartStatistic.NullCntBySite = new Dictionary<byte, int>();
            filter.FilterPartStatistic.FreshCntBySite = new Dictionary<byte, int>();
            filter.FilterPartStatistic.RtByIdCntBySite = new Dictionary<byte, int>();
            filter.FilterPartStatistic.RtByCordCntBySite = new Dictionary<byte, int>();

            foreach (var s in _siteContainer) {
                filter.FilterPartStatistic.SiteCnt.Add(s.Key, 0);
                filter.FilterPartStatistic.HardBinBySite.Add(s.Key, new Dictionary<ushort, int>());
                filter.FilterPartStatistic.SoftBinBySite.Add(s.Key, new Dictionary<ushort, int>());
                filter.FilterPartStatistic.PassCntBySite.Add(s.Key, 0);
                filter.FilterPartStatistic.FailCntBySite.Add(s.Key, 0);
                filter.FilterPartStatistic.AbortCntBySite.Add(s.Key, 0);
                filter.FilterPartStatistic.NullCntBySite.Add(s.Key, 0);
                filter.FilterPartStatistic.FreshCntBySite.Add(s.Key, 0);
                filter.FilterPartStatistic.RtByIdCntBySite.Add(s.Key, 0);
                filter.FilterPartStatistic.RtByCordCntBySite.Add(s.Key, 0);
            }

            for (int i = 0; i <= _partIdx; i++) {
                var sn = _site_PartContainer[i];
                filter.FilterPartStatistic.SiteCnt[sn] += 1;

                if (filter.FilterPartStatistic.HardBinBySite[sn].ContainsKey(_hardBin_PartContainer[i])) {
                    filter.FilterPartStatistic.HardBinBySite[sn][_hardBin_PartContainer[i]] += 1;
                } else {
                    filter.FilterPartStatistic.HardBinBySite[sn].Add(_hardBin_PartContainer[i], 0);
                }

                if (filter.FilterPartStatistic.SoftBinBySite[sn].ContainsKey(_softBin_PartContainer[i])) {
                    filter.FilterPartStatistic.SoftBinBySite[sn][_softBin_PartContainer[i]] += 1;
                } else {
                    filter.FilterPartStatistic.SoftBinBySite[sn].Add(_softBin_PartContainer[i], 0);
                }

                switch (_chipType_PartContainer[i]) {
                    case DeviceType.Fresh:
                        filter.FilterPartStatistic.FreshCntBySite[sn] += 1;
                        break;
                    case DeviceType.RT_ID:
                        filter.FilterPartStatistic.RtByIdCntBySite[sn] += 1;
                        break;
                    case DeviceType.RT_Cord:
                        filter.FilterPartStatistic.RtByCordCntBySite[sn] += 1;
                        break;
                }

                switch (_resultType_PartContainer[i]) {
                    case ResultType.Pass:
                        filter.FilterPartStatistic.PassCntBySite[sn] += 1;
                        break;
                    case ResultType.Fail:
                        filter.FilterPartStatistic.FailCntBySite[sn] += 1;
                        break;
                    case ResultType.Abort:
                        filter.FilterPartStatistic.AbortCntBySite[sn] += 1;
                        break;
                    case ResultType.Null:
                        filter.FilterPartStatistic.NullCntBySite[sn] += 1;
                        break;
                }
            }

            filter.FilterPartStatistic.TotalCnt = (from r in filter.FilterPartStatistic.SiteCnt select r.Value).Sum();
            filter.FilterPartStatistic.HardBin = new Dictionary<ushort, int>();
            filter.FilterPartStatistic.SoftBin = new Dictionary<ushort, int>();
            foreach (var sv in filter.FilterPartStatistic.HardBinBySite) {
                foreach (var v in sv.Value) {
                    if (filter.FilterPartStatistic.HardBin.ContainsKey(v.Key)) {
                        filter.FilterPartStatistic.HardBin[v.Key] += v.Value;
                    } else {
                        filter.FilterPartStatistic.HardBin.Add(v.Key, v.Value);
                    }
                }
            }
            foreach (var sv in filter.FilterPartStatistic.SoftBinBySite) {
                foreach (var v in sv.Value) {
                    if (filter.FilterPartStatistic.SoftBin.ContainsKey(v.Key)) {
                        filter.FilterPartStatistic.SoftBin[v.Key] += v.Value;
                    } else {
                        filter.FilterPartStatistic.SoftBin.Add(v.Key, v.Value);
                    }
                }
            }

            filter.FilterPartStatistic.PassCnt = (from r in filter.FilterPartStatistic.PassCntBySite select r.Value).Sum();
            filter.FilterPartStatistic.FailCnt = (from r in filter.FilterPartStatistic.FailCntBySite select r.Value).Sum();
            filter.FilterPartStatistic.AbortCnt = (from r in filter.FilterPartStatistic.AbortCntBySite select r.Value).Sum();
            filter.FilterPartStatistic.NullCnt = (from r in filter.FilterPartStatistic.NullCntBySite select r.Value).Sum();
            filter.FilterPartStatistic.FreshCnt = (from r in filter.FilterPartStatistic.FreshCntBySite select r.Value).Sum();
            filter.FilterPartStatistic.RtByIdCnt = (from r in filter.FilterPartStatistic.RtByIdCntBySite select r.Value).Sum();
            filter.FilterPartStatistic.RtByCordCnt = (from r in filter.FilterPartStatistic.RtByCordCntBySite select r.Value).Sum();

        }

    }
}
