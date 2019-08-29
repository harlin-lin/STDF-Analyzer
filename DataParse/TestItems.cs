using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse
{
    public class TestItems
    {
        private Dictionary<TestID, ItemInfo> _testItems;
        private Dictionary<TestID, int> _itemIndexes;
        private List<bool> _ItemFilter;

        //public float MeanValue { get; private set; }
        //public float MinValue { get; private set; }
        //public float MaxValue { get; private set; }
        //public double Cp { get; private set; }
        //public double Cpk { get; private set; }
        //public double? Sigma { get; private set; }
        //public int PassCount { get; private set; }
        //public int FailCount { get; private set; }


        public TestItems(int capacity) {
            _testItems = new Dictionary<TestID, ItemInfo>(capacity);
            _itemIndexes = new Dictionary<TestID, int>(capacity);
            _ItemFilter = new List<bool>(capacity);
        }

        public bool ExistTestItem(TestID testID) {
            if (_testItems.ContainsKey(testID))
                return true;
            else
                return false;
        }
        
        public int GetIndex(TestID testID) {
            int index;
            if (_itemIndexes.TryGetValue(testID, out index))
                return index;
            else
                throw new Exception("Do Check the testID if exist first!");
        }

        public bool AddTestItem(TestID testID, ItemInfo itemInfo) {
            //if (_testItems.ContainsKey(testID))
            //    return false;
            //else {
            _testItems.Add(testID, itemInfo);
            _itemIndexes.Add(testID, _testItems.Count - 1);
            _ItemFilter.Add(true);
            //}
            return true;
        }

        public List<TestID> GetTestIDs() {
            return _testItems.Keys.ToList();
        }
        public ItemInfo GetItemInfo(TestID testID) {
            ItemInfo info;
            if (_testItems.TryGetValue(testID, out info))
                return info;
            else
                return null;
        }
        public List<TestID> GetFilteredTestIDs() {
            List<TestID> testIDs = new List<TestID>(_testItems.Count);
            for(int i=0; i< _ItemFilter.Count; i++) {
                if (_ItemFilter[i])
                    testIDs.Add(_testItems.ElementAt(i).Key);
            }
            return testIDs;
        }

    }
}
