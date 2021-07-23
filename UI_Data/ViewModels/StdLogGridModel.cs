using DataInterface;
using FastWpfGrid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UI_Data.ViewModels {
    public class StdLogGridModel : FastGridModelBase {

        Dictionary<TestID, IItemStatistic> _ststistic;
        Dictionary<TestID, IItemInfo> _itemInfo;
        List<IChipInfo> _chipInfo;
        float?[][] _rst;
        int _colCount;
        int _rowCount;
        int _filterId;
        IDataAcquire _dataAcquire;

        const int colFixedLength = 11;

        public StdLogGridModel(IDataAcquire dataAcquire, int filterId) {
            _dataAcquire = dataAcquire;
            _filterId = filterId;

            Update();

        }

        public void Update() {
            _ststistic = _dataAcquire.GetFilteredStatistic(_filterId);
            _itemInfo = _dataAcquire.GetFilteredTestIDs_Info(_filterId);

            _chipInfo = _dataAcquire.GetFilteredChipsInfo(_filterId);

            _rst = new float?[_itemInfo.Count][];
            for (int i = 0; i < _itemInfo.Count; i++) {
                _rst[i] = _dataAcquire.GetFilteredItemData(_itemInfo.ElementAt(i).Key, _filterId);
            }

            _colCount = _dataAcquire.GetFilteredChipSummary(_filterId).TotalCount;
            _rowCount = _itemInfo.Count;

            NotifyRefresh();
        }

        public override int ColumnCount { get { return _colCount; } }

        public override int RowCount { get { return _rowCount; } }

        public override string GetCellText(int row, int column) {
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
                            return v.HasValue ? v.Value.ToString("F4") : "";
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
                return _rst[row][column - colFixedLength].ToString();
            }
        }

        public override IFastGridCell GetCell(IFastGridView view, int row, int column) {
            _requestedRow = row;
            _requestedColumn = column;
            return this;
        }

        public override string GetRowHeaderText(int row) {
            return (row+1).ToString();
        }

        public override string GetColumnHeaderText(int column) {
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
                return $"ID:{v.PartId}\n{v.WaferCord}\nHB:{v.HardBin}\nSB:{v.SoftBin}\nPF:{v.Result}\nS:{v.Site}\nT:{v.TestTime}ms";
            }
        }

        public override Color? FontColor {
            get {
                if (_requestedRow.HasValue && _requestedColumn.HasValue)
                    return GetCellFontColor(_requestedRow.Value, _requestedColumn.Value);
                else
                    return null;
            }
        }
        public Color? GetCellFontColor(int row, int column) {
            //if (column < colFixedLength) {
            //    return null;
            //} else {
            //    var v = _rst[row][column - colFixedLength];
            //    if (v.HasValue && !v.IsPass) {
            //        if (v.IsLessLL) return Colors.Blue;
            //        if (v.IsGreaterHL) return Colors.Red;
            //        return null;
            //    } else
            return null;
            //}
        }
    }
}
