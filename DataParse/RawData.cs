using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public class RawData {
        public const int DefaultItemsCapacity = 300;
        public const int DefaultFixedDataBlockLength = 4096;  //means 2^12
        private const int DefaultFixedDataBits = 12;  //means 2^12
        private List<bool> _ChipFilter;


        private class ItemData
        {


            class ItemDataBlock {
                public float?[] itemDataBlock;

                public ItemDataBlock() {
                    itemDataBlock = new float?[DefaultFixedDataBlockLength];
                }
            }

            private List<ItemDataBlock> _itemData;
            private int _capacity;

            public ItemData() {
                _itemData = new List<ItemDataBlock>();
                this._itemData.Add(new ItemDataBlock());
                _capacity = DefaultFixedDataBlockLength;
            }

            public float? this[int index] {
                //deleted all of the parameter check codes
                get {
                    return this._itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)];
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
            public void Set(int index, float? value) {
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
            public float?[] GetItem(int count) {
                if (count <= DefaultFixedDataBlockLength)
                    return this._itemData[0].itemDataBlock.Take(count).ToArray();
                else {
                    float?[] rt = new float?[count];
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
            /// 
            /// </summary>
            /// <param name="indexFrom"></param>
            /// <param name="count"></param>
            /// <param name="filter"></param>
            /// <returns></returns>
            public float?[] GetItem(int indexFrom, int count, List<bool> filter) {
                float?[] rt = new float?[count];

                int index = indexFrom;
                for (int i=0; i< count; i++) {
                    index += i;
                    if (index > _capacity)  break;

                    if (filter[index]) 
                        rt[i] = (_itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)]);
                }

                return rt;
            }

            public float?[] GetItem(int indexFrom, int count) {
                float?[] rt = new float?[count];

                int index = indexFrom;
                for (int i = 0; i < count; i++) {
                    index += i;
                    if (index > _capacity) break;

                    rt[i]=(_itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)]);
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

        public void SetFilter(List<bool> filter) {
            _ChipFilter = filter;
        }

        /// <summary>
        /// AddItem
        /// </summary>
        /// <returns>index of the added item</returns>
        public int AddItem() {
            _data.Add(new ItemData());
            return _data.Count-1;
        }
    
        public int ItemsCount {
            get {
                return _data.Count;
            }
        }


        public float?[] GetItemData(int itemIndex) {
            return _data[itemIndex].GetItem(0, ChipCount);
        }

        public float?[] GetItemData(int itemIndex, int chipIndexFrom, int count) {
            return _data[itemIndex].GetItem(chipIndexFrom, count);
        }

        public float?[] GetItemDataFiltered(int itemIndex) {
            return _data[itemIndex].GetItem(0, ChipCount, _ChipFilter);
        }

        public float?[] GetItemDataFiltered(int itemIndex, int chipIndexFrom, int count) {
            return _data[itemIndex].GetItem(chipIndexFrom, count, _ChipFilter);
        }


        /// <summary>
        /// AddChip
        /// </summary>
        /// <returns>index of the added chip</returns>
        public int AddChip(){
            ChipCount++;
            return ChipCount-1;
        }

        public void Set(int itemIndex, int chipIndex, float? value) {
            //check index valid

            _data[itemIndex][chipIndex]=value;
        }


    }
}
