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
            public List<float?> GetItem(int count) {
                if (count <= DefaultFixedDataBlockLength)
                    return this._itemData[0].itemDataBlock.Take(count).ToList();
                else {
                    List<float?> rt = new List<float?>(count);
                    int i = 0;
                    while ((count -= DefaultFixedDataBlockLength) >= 0 && i < _itemData.Count) {
                        rt.AddRange(_itemData[i].itemDataBlock);
                        i++;
                    }
                    if(i < _itemData.Count)
                        rt.AddRange(_itemData[i].itemDataBlock.Take(count));

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
            public List<float?> GetItem(int indexFrom, int count, List<bool> filter) {
                List<float?> rt = new List<float?>(count);

                int index = indexFrom;
                for (int i=0; i< count; i++) {
                    index += i;
                    if (index > _capacity)  break;

                    if (filter[i]) 
                        rt.Add(_itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)]);
                }

                return rt;
            }

            public List<float?> GetItem(int indexFrom, int count) {
                List<float?> rt = new List<float?>(count);

                int index = indexFrom;
                for (int i = 0; i < count; i++) {
                    index += i;
                    if (index > _capacity) break;

                    rt.Add(_itemData[index >> DefaultFixedDataBits].itemDataBlock[index & (DefaultFixedDataBlockLength - 1)]);
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
    
        public int ItemsCount {
            get {
                return _data.Count;
            }
        }


        public List<float?> GetItemData(int itemIndex) {
            return _data[itemIndex].GetItem(0, ChipCount);
        }

        public List<float?> GetItemData(int itemIndex, int chipIndexFrom, int count) {
            return _data[itemIndex].GetItem(chipIndexFrom, count);
        }

        public List<float?> GetItemData(int itemIndex, List<bool> filter) {
            return _data[itemIndex].GetItem(0, ChipCount, filter);
        }

        public List<float?> GetItemData(int itemIndex, int chipIndexFrom, int count, List<bool> filter) {
            return _data[itemIndex].GetItem(chipIndexFrom, count, filter);
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

        //#region  FilterMethod
        //public void SetChipFilter(List<bool> filter) {
        //    if (filter.Count != ChipCount)
        //        throw new ArgumentException("list count not match the chip count", "filter");
        //    else
        //        ChipFilter = filter;
        //}

        //public void SetChipFilter(int index, bool filter) {
        //    if (index > (ChipCount - 1))
        //        throw new ArgumentOutOfRangeException("index", "index out of chip range");
        //    else
        //        ChipFilter[index] = filter;
        //}

        //public void MaskChips(int index) {
        //    if (index > (ChipCount - 1))
        //        throw new ArgumentOutOfRangeException("index", "index out of chip range");
        //    else
        //        ChipFilter[index] = false;
        //}

        //public void MaskChips(List<int> indexes) {
        //    foreach(int i in indexes) {
        //        if (i > (ChipCount - 1))
        //            throw new ArgumentOutOfRangeException("index", "index out of chip range");
        //        else
        //            ChipFilter[i] = false;
        //    }
        //}

        //public void MaskChips(int indexFrom, int indexTo) {
        //    if (indexTo > (ChipCount - 1))
        //        throw new ArgumentOutOfRangeException("indexTo", "index out of chip range");
        //    else {
        //        for(int i=indexFrom; i<=indexTo; i++)
        //            ChipFilter[i] = false;
        //    }
        //}

        //public void EnableChips(int index) {
        //    if (index > (ChipCount - 1))
        //        throw new ArgumentOutOfRangeException("index", "index out of chip range");
        //    else
        //        ChipFilter[index] = true;
        //}

        //public void EnableChips(List<int> indexes) {
        //    foreach (int i in indexes) {
        //        if (i > (ChipCount - 1))
        //            throw new ArgumentOutOfRangeException("index", "index out of chip range");
        //        else
        //            ChipFilter[i] = true;
        //    }
        //}

        //public void EnableChips(int indexFrom, int indexTo) {
        //    if (indexTo > (ChipCount - 1))
        //        throw new ArgumentOutOfRangeException("indexTo", "index out of chip range");
        //    else {
        //        for (int i = indexFrom; i <= indexTo; i++)
        //            ChipFilter[i] = true;
        //    }
        //}

        //public void ClearChipFilter() {
        //    for (int i = 0; i < ChipFilter.Count; i++)
        //        ChipFilter[i] = true;
        //}



        ////public void SetItemFilter(List<bool> filter) {
        ////    if (filter.Count != _data.Count)
        ////        throw new ArgumentException("list count not match the item count", "filter");
        ////    else
        ////        ItemFilter = filter;
        ////}

        ////public void SetItemFilter(int index, bool filter) {
        ////    if (index > (_data.Count - 1))
        ////        throw new ArgumentOutOfRangeException("index", "index out of item range");
        ////    else
        ////        ItemFilter[index] = filter;
        ////}

        ////public void MaskItems(int index) {
        ////    if (index > (_data.Count - 1))
        ////        throw new ArgumentOutOfRangeException("index", "index out of item range");
        ////    else
        ////        ItemFilter[index] = false;
        ////}

        ////public void MaskItems(List<int> indexes) {
        ////    foreach (int i in indexes) {
        ////        if (i > (_data.Count - 1))
        ////            throw new ArgumentOutOfRangeException("index", "index out of item range");
        ////        else
        ////            ItemFilter[i] = false;
        ////    }
        ////}

        ////public void MaskItems(int indexFrom, int indexTo) {
        ////    if (indexTo > (_data.Count - 1))
        ////        throw new ArgumentOutOfRangeException("indexTo", "index out of item range");
        ////    else {
        ////        for (int i = indexFrom; i <= indexTo; i++)
        ////            ItemFilter[i] = false;
        ////    }
        ////}

        ////public void EnableItems(int index) {
        ////    if (index > (_data.Count - 1))
        ////        throw new ArgumentOutOfRangeException("index", "index out of item range");
        ////    else
        ////        ItemFilter[index] = true;
        ////}

        ////public void EnableItems(List<int> indexes) {
        ////    foreach (int i in indexes) {
        ////        if (i > (_data.Count - 1))
        ////            throw new ArgumentOutOfRangeException("index", "index out of item range");
        ////        else
        ////            ItemFilter[i] = true;
        ////    }
        ////}

        ////public void EnableItems(int indexFrom, int indexTo) {
        ////    if (indexTo > (_data.Count - 1))
        ////        throw new ArgumentOutOfRangeException("indexTo", "index out of item range");
        ////    else {
        ////        for (int i = indexFrom; i <= indexTo; i++)
        ////            ItemFilter[i] = true;
        ////    }
        ////}

        ////public void ClearItemFilter() {
        ////    for (int i = 0; i < ItemFilter.Count; i++)
        ////        ItemFilter[i] = true;
        ////}


        ////public void ClearFilters() {
        ////    ClearChipFilter();
        ////    ClearItemFilter();
        ////}
        //#endregion


        }
    }
