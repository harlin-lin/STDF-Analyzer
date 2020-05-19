using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DataInterface {
    public class StdLogTable {
        Dictionary<TestID, IItemStatistic> _ststistic;
        Dictionary<TestID, IItemInfo> _itemInfo;
        List<IChipInfo> _chipInfo;
        //List<List<Rst>> _rst;
        Rst[][] _rst;
        int _colCount;
        int _rowCount;
        int _filterId;
        IDataAcquire _dataAcquire;

        const int colFixedLength = 11;

        public StdLogTable(IDataAcquire dataAcquire, int startIndex, int count, int filterId) {
            _ststistic = dataAcquire.GetFilteredStatistic(filterId);
            _itemInfo = dataAcquire.GetFilteredTestIDs_Info(filterId);

            _chipInfo = dataAcquire.GetFilteredChipsInfo(startIndex, count, filterId);

            _rst = new Rst[_itemInfo.Count][];
            for(int i=0; i<_itemInfo.Count; i++) {
                _rst[i] = dataAcquire.GetFilteredItemDataArr(_itemInfo.ElementAt(i).Key, startIndex, count, filterId);
                //_rst.Add(dataAcquire.GetFilteredItemData(_itemInfo.ElementAt(i).Key, filterId));
            }

            _dataAcquire = dataAcquire;
            _filterId = filterId;
            _colCount = count;
            _rowCount = _itemInfo.Count;
        }

        public void ChangePage(int startIndex, int count) {
            _ststistic = _dataAcquire.GetFilteredStatistic(_filterId);
            _itemInfo = _dataAcquire.GetFilteredTestIDs_Info(_filterId);

            _chipInfo = _dataAcquire.GetFilteredChipsInfo(startIndex, count, _filterId);

            _rst = new Rst[_itemInfo.Count][];
            for (int i = 0; i < _itemInfo.Count; i++) {
                _rst[i] = _dataAcquire.GetFilteredItemDataArr(_itemInfo.ElementAt(i).Key, startIndex, count, _filterId);
                //_rst.Add(dataAcquire.GetFilteredItemData(_itemInfo.ElementAt(i).Key, filterId));
            }

        }

        public int ColumnCount { get { return _colCount + colFixedLength; } }

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
                    case 5: {
                            var v = _ststistic[_itemInfo.ElementAt(row).Key].MinValue;
                            return v.HasValue? v.Value.ToString("F4"):"";
                        }
                    case 6: {
                            var v = _ststistic[_itemInfo.ElementAt(row).Key].MaxValue;
                            return v.HasValue ? v.Value.ToString("F4") : "";
                        }
                    case 7: {
                            var v = _ststistic[_itemInfo.ElementAt(row).Key].MeanValue;
                            return v.HasValue ? v.Value.ToString("F4") : "";
                        }
                    case 8: {
                            var v = _ststistic[_itemInfo.ElementAt(row).Key].Sigma;
                            return v.HasValue ? v.Value.ToString("F4") : "";
                        }
                    case 9: {
                            var v = _ststistic[_itemInfo.ElementAt(row).Key].Cpk;
                            return v.HasValue ? v.Value.ToString("F4") : "";
                        }
                    case 10:
                        return _ststistic[_itemInfo.ElementAt(row).Key].PassCount.ToString();
                    default:
                        throw new Exception("Out of Range");
                }
            } else {
                return _rst[row][column- colFixedLength].ToString();
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
                    case 5:
                        return "Min";
                    case 6:
                        return "Max";
                    case 7:
                        return "Mean";
                    case 8:
                        return "Sigma";
                    case 9:
                        return "CPK";
                    case 10:
                        return "Pass Cnt";
                    default:
                        throw new Exception("Out of Range");
                }
            } else {
                var v = _chipInfo[column - colFixedLength];
                return $"{v.PartId}\n{v.WaferCord}\n{v.HardBin}\n{v.SoftBin}\n{v.Result}\n{v.Site}\n{v.TestTime}ms";
            }
        }

        public Color? GetCellFontColor(int row, int column) {
            if (column < colFixedLength) {
                return null;
            } else {
                var v = _rst[row][column - colFixedLength];
                if (v.HasValue && !v.IsPass) {
                    if (v.IsLessLL) return Colors.Blue;
                    if (v.IsGreaterHL) return Colors.Red;
                    return null;
                } else
                    return null;
            }
        }
    }
}
