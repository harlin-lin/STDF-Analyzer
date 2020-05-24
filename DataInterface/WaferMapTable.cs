using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DataInterface {
    public class WaferMapTable {
        List<IChipInfo> _chipInfo;
        Dictionary<CordType, IChipInfo> _waferMap;
        int _colCount;
        int _rowCount;
        bool valid;
        int _filterId;
        IDataAcquire _dataAcquire;

        public WaferMapTable(IDataAcquire dataAcquire, int filterId) {
            _dataAcquire = dataAcquire;
            _filterId = filterId;
            _waferMap = new Dictionary<CordType, IChipInfo>();

            foreach (var v in dataAcquire.GetChipsInfo()) {
                var c = new CordType(v.WaferCord.CordX, v.WaferCord.CordY);
                if (_colCount <= c.CordX) _colCount = c.CordX + 1;
                if (_rowCount <= c.CordY) _rowCount = c.CordY + 1;
            }

            Update();
        }

        public void Update() {

            _chipInfo = _dataAcquire.GetFilteredChipsInfo(_filterId);
            valid = true;
            _waferMap.Clear();

            //check if can be plot into wafer map
            int i = 0;
            foreach (var v in _chipInfo) {
                if (v.WaferCord.CordX != 0 && v.WaferCord.CordY != 0) break;
                else i++;

                if (i >= 3) {
                    _colCount = 0;
                    _rowCount = 0;
                    valid = false;
                }
            }

            //get the cord dict
            foreach (var v in _chipInfo) {
                var c = new CordType(v.WaferCord.CordX, v.WaferCord.CordY);
                if (_waferMap.ContainsKey(c)){
                    _waferMap[c] = v;
                } else {
                    _waferMap.Add(c, v);
                }
                if (_colCount <= c.CordX) _colCount = c.CordX + 1;
                if (_rowCount <= c.CordY) _rowCount = c.CordY + 1;
            }
        }

        public int ColumnCount { get { return _colCount; } }

        public int RowCount { get { return _rowCount; } }

        public string GetCellText(int row, int column) {
            if (!valid) return "";

            CordType c = new CordType((short)(column - 1), (short)(row - 1));

            if (!_waferMap.ContainsKey(c)) return "";

            if (_waferMap[c].Result == ResultType.Null) {
                return "";
            } else {
                return _waferMap[c].SoftBin.ToString();
            }
        }

        public string GetColHeader(int column) {
            if (!valid) return "";

            return $"{column}";
        }

        public string GetRowHeader(int row) {
            if (!valid) return "";

            return $"{row}";
        }

        public Color? GetCellColor(int row, int column) {
            if (!valid) return null;

            CordType c = new CordType((short)(column - 1), (short)(row - 1));

            if (!_waferMap.ContainsKey(c)) return null;

            switch (_waferMap[c].Result) {
                case ResultType.Pass: return Colors.Green;
                case ResultType.Fail: return Colors.Red;
                case ResultType.Abort: return Colors.Yellow;
                default: return null;
            }

        }
    }
}
