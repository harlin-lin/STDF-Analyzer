using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    interface IDataAcquire {

        //property get the file default infomation
        List<byte> Sites { get; }
        Dictionary<byte, int> SitesChipCount { get; }
        List<ushort> SoftBins { get; }
        Dictionary<ushort, int> SoftBinsCount { get; }
        List<ushort> HardBins { get; }
        Dictionary<ushort, int> HardBinsCount { get; }
        List<TestNumber> TestNumbers { get; }
        Dictionary<TestNumber, ItemInfo> Items { get; }
        List<int> ChipsId { get; }
        int ChipsCount { get; }
        //List<ChipInfo> ChipsInfo { get; }

        //this info is filtered by filter
        List<byte> GetFilteredSites();
        Dictionary<byte, int> GetFilteredSitesChipCount();
        List<ushort> GetFilteredSoftBins();
        Dictionary<ushort, int> GetFilteredSoftBinsCount();
        List<ushort> GetFilteredHardBins();
        Dictionary<ushort, int> GetFilteredHardBinsCount();
        List<TestNumber> GetFilteredTestNumbers();
        List<TestNumber> GetFilteredTestNumbers(List<byte> sites);
        Dictionary<TestNumber, ItemInfo> GetFilteredItems();
        Dictionary<TestNumber, ItemInfo> GetFilteredItems(List<byte> sites);
        ItemInfo GetFilteredItemInfo(TestNumber testNumber);
        ItemInfo GetFilteredItemInfo(TestNumber testNumber, List<byte> sites);
        List<int> GetFilteredChipsId();
        List<int> GetFilteredChipsId(List<byte> sites);
        List<ChipInfo> GetFilteredChipsInfo();
        List<ChipInfo> GetFilteredChipsInfo(List<byte> sites);
        /// <summary>
        /// return an array of the selected item data with the filter, 
        /// it will be null if the correspond partdon't have result there, 
        /// the filtered part won't take place in the array
        /// </summary>
        /// <param name="testNumber"></param>
        /// <returns></returns>
        float?[] GetFilteredItemData(TestNumber testNumber);

        float?[] GetFilteredItemData(TestNumber testNumber, List<byte> sites);

        /// <summary>
        /// Get the raw data with the applied filter
        /// </summary>
        /// <param name="startIndex">Start index is 0</param>
        /// <param name="count">Will only return the actually availiable chips' data if the count greater than the actually selected chips' count</param>
        /// <returns></returns>
        DataTable GetFilteredData(int startIndex, int count);

        DataTable GetFilteredData(int startIndex, int count, List<byte> sites);

        /// <summary>
        /// To get selected chips' data, will return null if all of the chips are filtered or all test items are filtered
        /// It's not recommended to get the whole data by this method, please use [DataTable GetFilteredData(int startIndex, int count);] instead
        /// </summary>
        /// <param name="chipsId"></param>
        /// <returns></returns>
        DataTable GetFilteredData(List<int> chipsId);

        DataTable GetFilteredData(List<int> chipsId, List<byte> sites);

        //basic file information
        string FilePath { get; }
        string FileName { get; }
        FileBasicInfo basicInfo{ get; }


        int ParsePercent { get; }

    }
}
