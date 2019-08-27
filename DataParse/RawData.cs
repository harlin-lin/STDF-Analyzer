using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public class RawData {
        const int DefaultItemsCapacity = 300;

        private class ItemData
        {
            const int DefaultFixedDataBlockLength = 4096;  //means 2^12
            const int DefaultFixedDataBits = 12;  //means 2^12


            class ItemDataBlock {
                public float?[] itemDataBlock;

                public ItemDataBlock() {
                    itemDataBlock = new float?[DefaultFixedDataBlockLength];
                }
            }

            private List<ItemDataBlock> _itemData;
            private int _size;

            public ItemData() {
                _itemData = new List<ItemDataBlock>();
                this._itemData.Add(new ItemDataBlock());
                _size = DefaultFixedDataBlockLength;
            }

            public float? this[int index] {
                get {
                    if (index < 0 || index >= this._size)
                        throw new ArgumentOutOfRangeException("index", "索引超出范围");
                    return this._itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)];
                }
                set {
                    if (index < 0)
                        throw new ArgumentOutOfRangeException("index", "索引超出范围");
                    else if (index >= this._size) {
                        do {
                            this._itemData.Add(new ItemDataBlock());
                            _size += DefaultFixedDataBlockLength;
                        } while (index >= this._size);
                    }
                    this._itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)] = value;
                }
            }

            [Obsolete]
            public void Set(int index, float? value) {
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index", "索引超出范围");
                else if (index >= this._size) {
                    do {
                        this._itemData.Add(new ItemDataBlock());
                        _size += DefaultFixedDataBlockLength;
                    } while (index >= this._size);
                }
                this._itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)] = value;
            }

            public float?[] GetItem(int count) {
                if (count > this._size)
                    throw new ArgumentOutOfRangeException("count", "count larger than the size");
                else if (count <= DefaultFixedDataBlockLength)
                    return this._itemData[0].itemDataBlock.Take(count).ToArray();
                else {
                    float?[] rt = new float?[count];
                    int i = 0;
                    while ((count -= DefaultFixedDataBlockLength) >= 0) {
                        this._itemData[i].itemDataBlock.CopyTo(rt, i * DefaultFixedDataBlockLength);
                        i++;
                    }
                    this._itemData[i].itemDataBlock.Take(count).ToArray().CopyTo(rt, (count & (DefaultFixedDataBlockLength - 1)));

                    return rt;
                }
            }

            public List<float?> GetItem(bool[] filter) {
                int len = filter.Length;
                if (len > this._size)
                    throw new ArgumentOutOfRangeException("count", "count larger than the size");
                else {
                    List<float?> rt = new List<float?>(len);

                    for(int i=0; i<len; i++) {
                        if (filter[i])
                            rt.Add(_itemData[i >> DefaultFixedDataBits].itemDataBlock[i & (DefaultFixedDataBlockLength - 1)]);
                    }

                    return rt;
                }
            }

        }

        private  List<ItemData> _data; 
        public int ChipCount { get; private set; }


        public RawData() {
            _data = new List<ItemData>();
            ChipCount = 0;
        }

        public float?[] GetItemData(int itemIndex) {
            return _data[itemIndex].GetItem(ChipCount);
        }
        
        


        public RawData AddItem() {
            _data.Add(new ItemData());
            return this;
        }

        public int ItemsCount {
            get {
                return _data.Count;
            }
        }
    
        public RawData AddChip(){
            ChipCount++;
            return this;
        }

        public void Set(int itemIndex, int chipIndex, float? value) {
            _data[itemIndex][chipIndex]=value;
        }

    }
}
