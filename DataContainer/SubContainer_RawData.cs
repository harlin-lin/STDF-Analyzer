using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public partial class SubContainer {
        private struct DataBlock_Float{
            public const int BLOCK_SIZE = (1<<12);
            public int _offset;
            public float[] _dataBlock;
            public ulong[] _dataFlg;

            public DataBlock_Float(int offset) {
                _offset = offset;
                _dataFlg = new ulong[BLOCK_SIZE/64];
                _dataBlock = new float[BLOCK_SIZE];
            }

            public bool CheckIdxNull(int idxInBlock) {
                return ((_dataFlg[idxInBlock >> 6] >> (idxInBlock & 63)) & 1) == 1;
            }

            public void SetVal(int idx, float? rst) {
                if (rst.HasValue) {
                    _dataBlock[idx] = rst.Value;
                    _dataFlg[idx >> 6] |= (0x1UL << (idx & 63));
                } 
            }
        }

        private Dictionary<string, List<DataBlock_Float>> _dataBase_Result;
        private Dictionary<string, ItemInfo> _itemContainer;
        private int _partIdx;
        //mark the current site corresponding part index
        private Dictionary<byte, int> _siteContainer;

        private Dictionary<string, string> _basicInfo;

        private Dictionary<ushort, Tuple<string, string>> _softBinNames;
        private Dictionary<ushort, Tuple<string, string>> _hardBinNames;


        private void Initialize_RawData() {
            _dataBase_Result = new Dictionary<string, List<DataBlock_Float>>(DEFAULT_ITEMS_COUNT);
            _itemContainer = new Dictionary<string, ItemInfo>(DEFAULT_ITEMS_COUNT);
            _partIdx = -1;
            _siteContainer = new Dictionary<byte, int>();
            _basicInfo = new Dictionary<string, string>(50);

            _softBinNames = new Dictionary<ushort, Tuple<string, string>>();
            _hardBinNames = new Dictionary<ushort, Tuple<string, string>>();
        }
        
        private bool CheckItemContainer(string uid) {
            if (!_itemContainer.ContainsKey(uid)) {
                _itemContainer.Add(uid, null);
                _dataBase_Result.Add(uid, new List<DataBlock_Float>(200));

                var requiredBlockCnt = (_partIdx >> 12) + 1;
                var currentBlockCnt = _dataBase_Result[uid].Count;
                for (int i = 0; i < (requiredBlockCnt - currentBlockCnt); i++) {
                    _dataBase_Result[uid].Add(new DataBlock_Float((currentBlockCnt + i) * DataBlock_Float.BLOCK_SIZE));
                }
                return false;
            }
            return true;
        }

        private void AdjustDataBaseCapcity() {
            foreach(var v in _dataBase_Result) {
                if (v.Value.Count <= (_partIdx >> 12)) {
                    var requiredBlockCnt = (_partIdx >> 12) + 1;
                    var currentBlockCnt = v.Value.Count;
                    for (int i = 0; i < (requiredBlockCnt - currentBlockCnt); i++) {
                        v.Value.Add(new DataBlock_Float((currentBlockCnt + i) * DataBlock_Float.BLOCK_SIZE));
                    }
                }

            }
        }

        private void SetData(string uid, int idx, float? rst) {

            _dataBase_Result[uid][idx >> 12].SetVal(idx & 4095, rst);

        }

        private List<float?> GetItemVal(string uid) {
            List<float?> rst = new List<float?>(_partIdx + 1);

            int bdBound = _partIdx >> 12;

            for (int i=0; i< bdBound; i++) {
                rst.AddRange(from r in _dataBase_Result[uid][i]._dataBlock
                             let l = new float?(r)
                             select l);

                for (int x = 0; x < _dataBase_Result[uid][i]._dataFlg.Length; x++) {
                    if (_dataBase_Result[uid][i]._dataFlg[x] == ulong.MaxValue) continue;
                    for (int j = 0; j < 64; j++) {
                        if (((_dataBase_Result[uid][i]._dataFlg[i] >> j) & 1) == 0) {
                            rst[(x << 6) + j + _dataBase_Result[uid][i]._offset] = null;
                        }
                    }
                }
            }

            int mod = (_partIdx & 4095) + 1;

            rst.AddRange( from r in _dataBase_Result[uid][bdBound]._dataBlock.Take(mod)
                          let l = new float?(r)
                          select l);


            int m = 0;
            for (int x = 0; x < _dataBase_Result[uid][bdBound]._dataFlg.Length; x++) {
                for (int j = 0; j < 64; j++) {
                    if (m >= mod) break;
                    if (((_dataBase_Result[uid][bdBound]._dataFlg[bdBound] >> j) & 1) == 0) {
                        rst[(x << 6) + j + _dataBase_Result[uid][bdBound]._offset] = null;
                    }
                    m++;
                }
                if (m >= mod) break;
            }

            return rst;
        }

        private List<float?> GetItemVal(string uid, int offset, int length) {
            if (offset > _partIdx || (offset + length) > _partIdx) throw new Exception("Required Out Of DataBase Range");
            List<float?> rst = new List<float?>(length);

            for(int i=offset; i<offset+length; i++) {
                rst.Add(GetItemVal(uid, i));
            }

            return rst;
        }

        private float? GetItemVal(string uid, int partIdx) {
            if(_dataBase_Result[uid][partIdx >> 12].CheckIdxNull(partIdx & 4095)) {
                return _dataBase_Result[uid][partIdx >> 12]._dataBlock[partIdx & 4095];
            } else {
                return null;
            }
        }

        private List<float?> GetItemVal(string uid, Filter filter) {
            if (filter.IfNoChanged) {
                return GetItemVal(uid);
            } else {
                return (from i in Enumerable.Range(0, _partIdx + 1)
                        where !filter.FilterIdxFlag[i]
                        select GetItemVal(uid, i)).ToList();
            }
        }

        private List<float?> GetItemVal(string uid, int offset, int length, Filter filter) {
            if (offset > _partIdx || (offset + length) > _partIdx) throw new Exception("Required Out Of DataBase Range");
            if (filter.IfNoChanged) {
                return GetItemVal(uid, offset, length);
            } else {
                int c = 0;
                return (from i in Enumerable.Range(offset, _partIdx-offset+1)
                        where !filter.FilterIdxFlag[i] && (c++<length)
                        select GetItemVal(uid, i)).ToList();
            }
        }
    }
}
