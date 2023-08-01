using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public partial class SubContainer {
        //private struct DataBlock_Float{
        //    public const int BLOCK_SIZE = (1<<12);
        //    public int _offset;
        //    public float[] _dataBlock;

        //    public DataBlock_Float(int offset) {
        //        _offset = offset;
        //        _dataBlock = new float[BLOCK_SIZE];
        //        for(int i=0; i<BLOCK_SIZE; i++) {
        //            _dataBlock[i] = float.NaN;
        //        }
        //    }

        //    public void SetVal(int idx, float rst) {
        //        _dataBlock[idx] = rst;
        //    }
        //}

        private ConcurrentDictionary<string, float[]> _dataBase_Result;
        private ConcurrentDictionary<string, ItemInfo> _itemContainer;
        private int _preIdx;
        private int _partIdx;
        //mark the current site corresponding part index
        private ConcurrentDictionary<byte, int> _siteContainer;

        private ConcurrentDictionary<string, string> _basicInfo;

        private ConcurrentDictionary<ushort, Tuple<string, string>> _softBinNames;
        private ConcurrentDictionary<ushort, Tuple<string, string>> _hardBinNames;


        private void Initialize_RawData() {
            _dataBase_Result = new ConcurrentDictionary<string, float[]>();
            _itemContainer = new ConcurrentDictionary<string, ItemInfo>();
            _preIdx = -1;
            _partIdx = -1;
            _siteContainer = new ConcurrentDictionary<byte, int>();
            _basicInfo = new ConcurrentDictionary<string, string>();

            _softBinNames = new ConcurrentDictionary<ushort, Tuple<string, string>>();
            _hardBinNames = new ConcurrentDictionary<ushort, Tuple<string, string>>();
        }
        
        private bool CheckItemContainer(string uid) {
            if (!_itemContainer.ContainsKey(uid)) {
                _itemContainer.TryAdd(uid, null);
                _dataBase_Result.TryAdd(uid, Enumerable.Repeat(float.NaN, _preIdx + 1).ToArray());

                return false;
            }
            return true;
        }


        private void SetData(string uid, int idx, float rst) {
            float[] vv;
            while (!_dataBase_Result.TryGetValue(uid, out vv)) ;
            vv[idx] = rst;
            //_dataBase_Result[uid][idx] = rst;
        }

        private IEnumerable<float> GetItemVal(string uid) {
            List<float> rst = new List<float>(_partIdx + 1);

            rst.AddRange(_dataBase_Result[uid]);

            return rst;
        }

        private IEnumerable<float> GetItemVal(string uid, int offset, int length) {
            if (offset > _partIdx || (offset + length) > _partIdx) throw new Exception("Required Out Of DataBase Range");
            List<float> rst = new List<float>(length);

            rst.AddRange(_dataBase_Result[uid].Skip(offset).Take(length));

            return rst;
        }

        private float GetItemVal(string uid, int partIdx) {
            //check idx?
            return _dataBase_Result[uid][partIdx];
        }

        private IEnumerable<float> GetItemVal(string uid, Filter filter) {
            if (filter.IfNoChanged) {
                return GetItemVal(uid);
            } else {
                return from i in Enumerable.Range(0, _partIdx + 1)
                        where !filter.FilterIdxFlag[i]
                        select GetItemVal(uid, i);
            }
        }

        private IEnumerable<float> GetItemVal(string uid, int offset, int length, Filter filter) {
            if (offset > _partIdx || (offset + length) > _partIdx) throw new Exception("Required Out Of DataBase Range");
            if (filter.IfNoChanged) {
                return GetItemVal(uid, offset, length);
            } else {
                int c = 0;
                return from i in Enumerable.Range(offset, _partIdx - offset + 1)
                        where !filter.FilterIdxFlag[i] && (c++ < length)
                        select GetItemVal(uid, i);
            }
        }

        private IEnumerable<float> GetItemValBySite(string uid, Filter filter, byte site) {
            return from i in Enumerable.Range(0, _partIdx + 1)
                    where !filter.FilterIdxFlag[i] && _site_PartContainer[i]==site
                    select GetItemVal(uid, i);
        }

    }
}
