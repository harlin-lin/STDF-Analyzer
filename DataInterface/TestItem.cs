using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterface {
    public struct TestItem {
        public TestID ID { get; private set; }
        public IItemInfo ItemInfo { get; private set; }
        public IItemStatistic ItemStatistic{ get; private set; }
        public float?[] ItemData { get; private set; }


    }
}
