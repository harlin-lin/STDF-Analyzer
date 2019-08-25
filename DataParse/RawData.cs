using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    public class RawData {
        const int DefaultFixedDataBlockLength = 3000;
        const int DefaultItemsCapacity = 300;


        class ItemDataBlock {
            public float?[] DataBlock { get; set; }

            public ItemDataBlock() {
                DataBlock = new float?[DefaultFixedDataBlockLength];
            }
        }

        class ItemsBlock {
            List<ItemDataBlock> Block { get; set; }

            public ItemsBlock() {
                Block = new List<ItemDataBlock>(DefaultItemsCapacity);
            }
        }

        List<ItemsBlock> _data;
        public int ChipCount { get; set; }
        //public List<int> ChipsId { get; set; }




    }
}
