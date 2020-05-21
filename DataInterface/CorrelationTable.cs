using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DataInterface {
    public class CorrelationTable {
        IEnumerable<KeyValuePair<TestID, IItemInfo>> _itemInfo;
        CorrVal[][] _corr;
        int _colCount;
        int _rowCount;
        List<Tuple<IDataAcquire, int>> _dataFilterTuple;
        bool _multiCompare;

        const int colFixedLength = 5;

        public CorrelationTable(List<Tuple<IDataAcquire, int>> dataFilterTuple) {
            if (dataFilterTuple.Count < 2)
                throw new Exception("At least two compare items");
            else if (dataFilterTuple.Count == 2)
                _multiCompare = false;
            else
                _multiCompare = true;

            _dataFilterTuple = dataFilterTuple;

            Update();
        }

        public void Update() {
            if (!_multiCompare) {
                _colCount = 11 + colFixedLength;
                var i1 = _dataFilterTuple[0].Item1.GetFilteredTestIDs_Info(_dataFilterTuple[0].Item2);
                var i2 = _dataFilterTuple[1].Item1.GetFilteredTestIDs_Info(_dataFilterTuple[1].Item2);
                _itemInfo = (from r in i1
                            where i2.ContainsKey(r.Key)
                            select r);

                Dictionary<TestID, IItemStatistic> ststistic1 = _dataFilterTuple[0].Item1.GetFilteredStatistic(_dataFilterTuple[0].Item2);
                Dictionary<TestID, IItemStatistic> ststistic2 = _dataFilterTuple[1].Item1.GetFilteredStatistic(_dataFilterTuple[1].Item2);

                _corr = new CorrVal[_itemInfo.Count()][];
                for (int i = 0; i < _corr.GetLength(0); i++) {
                    var id = _itemInfo.ElementAt(i).Key;
                    var ll = _itemInfo.ElementAt(i).Value.LoLimit;
                    var hl = _itemInfo.ElementAt(i).Value.HiLimit;
                    float? bw=null;
                    if (ll.HasValue && hl.HasValue) bw = hl - ll;

                    var type = Corr(ststistic1[id].MinValue, ststistic2[id].MinValue, bw);
                    CorrVal min1 = new CorrVal(ststistic1[id].MinValue, type);
                    CorrVal min2 = new CorrVal(ststistic2[id].MinValue, type);

                    type = Corr(ststistic1[id].MaxValue, ststistic2[id].MaxValue, bw);
                    CorrVal max1 = new CorrVal(ststistic1[id].MaxValue, type);
                    CorrVal max2 = new CorrVal(ststistic2[id].MaxValue, type);

                    type = Corr(ststistic1[id].MeanValue, ststistic2[id].MeanValue, bw);
                    CorrVal mean1 = new CorrVal(ststistic1[id].MeanValue, type);
                    CorrVal mean2 = new CorrVal(ststistic2[id].MeanValue, type);
                    CorrVal meanDelta;
                    if (mean1.HasValue && mean2.HasValue)
                        meanDelta = new CorrVal(Math.Abs(mean1.Value - mean2.Value), type);
                    else
                        meanDelta = new CorrVal(null, type);

                    type = Corr(ststistic1[id].Sigma, ststistic2[id].Sigma);
                    CorrVal sigma1 = new CorrVal(ststistic1[id].Sigma, type);
                    CorrVal sigma2 = new CorrVal(ststistic2[id].Sigma, type);

                    type = Corr(ststistic1[id].Cpk, ststistic2[id].Cpk);
                    CorrVal cpk1 = new CorrVal(ststistic1[id].Cpk, type);
                    CorrVal cpk2 = new CorrVal(ststistic2[id].Cpk, type);

                    _corr[i] = new CorrVal[] { mean1, mean2, meanDelta, min1, min2, max1, max2, sigma1, sigma2, cpk1, cpk2 };

                }

            } else {
                _colCount = _dataFilterTuple.Count + 1 + colFixedLength;
                List<Dictionary<TestID, IItemStatistic>> ststistic = new List<Dictionary<TestID, IItemStatistic>>();

                _itemInfo = _dataFilterTuple[0].Item1.GetFilteredTestIDs_Info(_dataFilterTuple[0].Item2);
                foreach (var v in _dataFilterTuple) {
                    var i2 = v.Item1.GetFilteredTestIDs_Info(v.Item2);
                    _itemInfo = (from r in _itemInfo
                                 where i2.ContainsKey(r.Key)
                                 select r);

                    ststistic.Add(v.Item1.GetFilteredStatistic(v.Item2));
                }

                _corr = new CorrVal[_itemInfo.Count()][];
                for (int i = 0; i < _corr.GetLength(0); i++) {
                    var id = _itemInfo.ElementAt(i).Key;
                    var ll = _itemInfo.ElementAt(i).Value.LoLimit;
                    var hl = _itemInfo.ElementAt(i).Value.HiLimit;
                    float? bw = null;
                    if (ll.HasValue && hl.HasValue) bw = hl - ll;

                    List<CorrVal> meanList = new List<CorrVal>();
                    float[] ml= (from r in ststistic
                               where r[id].MeanValue.HasValue
                               let f = r[id].MeanValue.Value
                               select f).ToArray();
                    float average = ml.Average();
                    foreach (var s in ststistic) {
                        meanList.Add(new CorrVal(s[id].MeanValue, Corr(s[id].MeanValue, average, bw)));
                    }
                    meanList.Add(new CorrVal(average,  CorrValType.Pass));

                    _corr[i] = meanList.ToArray();

                }


            }

            _rowCount = _itemInfo.Count();

        }

        private CorrValType Corr(float? v1, float? v2, float? bw) {
            if(v1.HasValue && v2.HasValue && bw.HasValue) {
                if (bw.Value == 0) {
                    if (v1.Value == v2.Value) return CorrValType.Pass;
                    else return CorrValType.Error;
                }

                var p = Math.Abs((double)(100*(v1.Value - v2.Value)/ bw.Value));
                if (p <= 5) return CorrValType.Pass;
                else if (p <= 10) return CorrValType.Warn;
                else return CorrValType.Error;
            } else if(!(v1.HasValue || v2.HasValue)){
                return CorrValType.Error;
            } else {
                return CorrValType.Pass;
            }
        }

        private CorrValType Corr(float? v1, float? v2) {
            if (v1.HasValue && v2.HasValue) {
                var bw = (v1.Value + v2.Value) / 2;
                if (bw == 0 || float.IsNaN(bw) || float.IsInfinity(bw)) {
                    if (v1.Value == v2.Value || float.IsNaN(v1.Value)) return CorrValType.Pass;
                    else return CorrValType.Error;
                }

                var p = Math.Abs((double)(100 * (v1.Value - v2.Value) / bw));
                if (p <= 5) return CorrValType.Pass;
                else if (p <= 10) return CorrValType.Warn;
                else return CorrValType.Error;
            } else if (!(v1.HasValue || v2.HasValue)) {
                return CorrValType.Error;
            } else {
                return CorrValType.Pass;
            }
        }

        public int ColumnCount { get { return _colCount; } }

        public int RowCount { get { return _rowCount; } }

        public string GetCellText(int row, int column) {
            if (column < colFixedLength) {
                switch (column) {
                    case 0:
                        return _itemInfo.ElementAt(row).Key.ToString();
                    case 1:
                        return _itemInfo.ElementAt(row).Value.TestText;
                    case 2:
                        return _itemInfo.ElementAt(row).Value.LoLimit.ToString();
                    case 3:
                        return _itemInfo.ElementAt(row).Value.HiLimit.ToString();
                    case 4:
                        return _itemInfo.ElementAt(row).Value.Unit;
                    default:
                        throw new Exception("Out of Range");
                }
            } else {
                return _corr[row][column- colFixedLength].ToString();
            }
        }

        public string GetColHeader(int column) {
            if (column < colFixedLength) {
                switch (column) {
                    case 0:
                        return "Test Num";
                    case 1:
                        return "Test Text";
                    case 2:
                        return "Low Limit";
                    case 3:
                        return "Hi Limit";
                    case 4:
                        return "Unit";
                    default:
                        return "";
                        //throw new Exception("Out of Range");
                }
            } else {
                if (!_multiCompare) {
                    switch (column) {
                        case 5:
                            return "Mean 1";
                        case 6:
                            return "Mean 2";
                        case 7:
                            return "Mean Delta";
                        case 8:
                            return "Min 1";
                        case 9:
                            return "Min 2";
                        case 10:
                            return "Max 1";
                        case 11:
                            return "Max 2";
                        case 12:
                            return "Sigma 1";
                        case 13:
                            return "Sigma 2";
                        case 14:
                            return "Cpk 1";
                        case 15:
                            return "Cpk 2";
                        default:
                            throw new Exception("Out of Range");
                    }
                } else {
                    if(column == _colCount - 1) {
                        return "Average";
                    } else {
                        return "Mean " + (column - colFixedLength + 1);
                    }
                }
            }
        }

        public Color? GetCellFontColor(int row, int column) {
            if (column < colFixedLength) {
                return null;
            } else {
                var v = _corr[row][column - colFixedLength];
                if (v.HasValue && v.GetRstType() != CorrValType.Pass) {
                    if (v.GetRstType() == CorrValType.Warn) return Colors.Orange;
                    if (v.GetRstType() == CorrValType.Error) return Colors.Red;
                    return null;
                } else
                    return null;
            }
        }
    }
}
