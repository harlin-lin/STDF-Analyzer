using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public class RawData {
        public const int DefaultItemsCapacity = 1000;
        public const int DefaultFixedDataBlockLength = 4096;  //means 2^12
        private const int DefaultFixedDataBits = 12;  //means 2^12

        private class ItemData
        {


            class ItemDataBlock {
                public Rst[] itemDataBlock;

                public ItemDataBlock() {
                    itemDataBlock = new Rst[DefaultFixedDataBlockLength];
                }
            }

            private List<ItemDataBlock> _itemData;
            private int _capacity;

            public ItemData() {
                _itemData = new List<ItemDataBlock>();
                this._itemData.Add(new ItemDataBlock());
                _capacity = DefaultFixedDataBlockLength;
            }

            public Rst this[int index] {
                //deleted all of the parameter check codes
                get {
                    var bi = index >> DefaultFixedDataBits;
                    var di = index & (DefaultFixedDataBlockLength - 1);
                    if (bi < _itemData.Count)
                        return _itemData[bi].itemDataBlock[di];
                    else
                        return new Rst();
                    //return this._itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)];
                }
                set {
                    if (index >= this._capacity) {
                        do {
                            this._itemData.Add(new ItemDataBlock());
                            _capacity += DefaultFixedDataBlockLength;
                        } while (index >= this._capacity);
                    }
                    this._itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)] = value;
                }
            }

            [Obsolete]
            public void Set(int index, Rst value) {
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index", "索引超出范围");
                else if (index >= this._capacity) {
                    do {
                        this._itemData.Add(new ItemDataBlock());
                        _capacity += DefaultFixedDataBlockLength;
                    } while (index >= this._capacity);
                }
                this._itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)] = value;
            }

            [Obsolete]
            public Rst[] GetItem(int count) {
                if (count <= DefaultFixedDataBlockLength)
                    return this._itemData[0].itemDataBlock.Take(count).ToArray();
                else {
                    Rst[] rt = new Rst[count];
                    int i = 0;
                    while ((count -= DefaultFixedDataBlockLength) >= 0 && i < _itemData.Count) {
                        _itemData[i].itemDataBlock.CopyTo(rt, i* DefaultFixedDataBlockLength);
                        i++;
                    }
                    if(i < _itemData.Count)
                        _itemData[i].itemDataBlock.Take(count).ToArray().CopyTo(rt, i * DefaultFixedDataBlockLength);

                    return rt;
                }
            }

            /// <summary>
            /// get item data with filter
            /// </summary>
            /// <param name="indexFrom"></param>
            /// <param name="count"></param>
            /// <param name="filter"></param>
            /// <returns></returns>
            public List<Rst> GetItem(int indexFrom, int count, bool[] filter) {
                List<Rst> rt = new List<Rst>(count);

                int index = indexFrom;
                int c = 0;
                for (int i = 0; i < (filter.Length - indexFrom); i++) {
                    index = indexFrom + i;
                    if (c >= count) break;

                    if (!filter[index]) {
                        var bi = index >> DefaultFixedDataBits;
                        var di = index & (DefaultFixedDataBlockLength - 1);
                        if (bi < _itemData.Count)
                            rt.Add(_itemData[bi].itemDataBlock[di]);
                        else
                            rt.Add(new Rst());
                        c++;
                    }
                }

                return rt;
            }

            /// <summary>
            /// get item data with filter
            /// </summary>
            /// <param name="indexFrom"></param>
            /// <param name="count"></param>
            /// <param name="filter"></param>
            /// <returns></returns>
            public Rst[] GetItemArr(int indexFrom, int count, bool[] filter) {
                Rst[] rt = new Rst[count];
                
                int index = indexFrom;
                int c = 0;
                for (int i = 0; i < (filter.Length - indexFrom); i++) {
                    index = indexFrom + i;
                    if (c >= count) break;

                    if (!filter[index]) {
                        var bi = index >> DefaultFixedDataBits;
                        var di = index & (DefaultFixedDataBlockLength - 1);
                        if (bi < _itemData.Count)
                            rt[c] = _itemData[bi].itemDataBlock[di];
                        else
                            rt[c] = new Rst();
                        c++;
                    }
                }

                return rt;
            }

            [Obsolete]
            public List<Rst> GetItem(int indexFrom, int count) {
                List<Rst> rt = new List<Rst>(count);

                int index = indexFrom;
                for (int i = 0; i < count; i++) {
                    index = indexFrom + i;
                    if (index > _capacity) break;

                    var bi = index >> DefaultFixedDataBits;
                    var di = index & (DefaultFixedDataBlockLength - 1);
                    if (bi < _itemData.Count)
                        rt.Add(_itemData[bi].itemDataBlock[di]);
                    else
                        rt.Add(new Rst());
                }

                return rt;
            }

        }

        private  List<ItemData> _data; 
        public int ChipCount { get; private set; }

        public RawData() {
            _data = new List<ItemData>(DefaultItemsCapacity);
            ChipCount = 0;
        }

        /// <summary>
        /// AddItem
        /// </summary>
        /// <returns>index of the added item</returns>
        public int AddItem() {
            _data.Add(new ItemData());
            return _data.Count-1;
        }
        
        /// <summary>
        /// Get Total Test Items Count
        /// </summary>
        public int ItemsCount {
            get {
                return _data.Count;
            }
        }

        [Obsolete]
        public List<Rst> GetItemData(int itemIndex) {
            return _data[itemIndex].GetItem(0, ChipCount);
        }
        [Obsolete]
        public List<Rst> GetItemData(int itemIndex, int chipIndexFrom, int count) {
            return _data[itemIndex].GetItem(chipIndexFrom, count);
        }

        public List<Rst> GetItemDataFiltered(int itemIndex, bool[] filter) {
            return _data[itemIndex].GetItem(0, ChipCount, filter);
            
        }

        public List<Rst> GetItemDataFiltered(int itemIndex, int chipIndexFrom, int count, bool[] filter) {
            return _data[itemIndex].GetItem(chipIndexFrom, count, filter);
        }
        public Rst[] GetItemDataFilteredArr(int itemIndex, int chipIndexFrom, int count, bool[] filter) {
            return _data[itemIndex].GetItemArr(chipIndexFrom, count, filter);
        }

        /// <summary>
        /// AddChip
        /// </summary>
        /// <returns>index of the added chip</returns>
        public int AddChip(){
            ChipCount++;
            return ChipCount-1;
        }

        public void Set(int itemIndex, int chipIndex, Rst value) {
            //check index valid

            _data[itemIndex][chipIndex]=value;
        }


    }
}
