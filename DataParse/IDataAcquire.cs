using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    interface IDataAcquire {

        //property get the file default infomation
        List<byte> GetSites();
        Dictionary<byte, int> GetSitesChipCount();
        List<ushort> GetSoftBins();
        Dictionary<ushort, int> GetSoftBinsCount();
        List<ushort> GetHardBins();
        Dictionary<ushort, int> GetHardBinsCount();
        List<TestID> GetTestIDs();
        ItemInfo GetItemInfo(TestID testID);
        List<int> GetChipsIndexes();
        List<int> GetChipsIndexes(List<byte> sites);
        int ChipsCount { get; }
        //List<ChipInfo> GetChipsInfo();

        //this info is filtered by filter
        List<byte> GetFilteredSites();
        Dictionary<byte, int> GetFilteredSitesChipCount();
        List<ushort> GetFilteredSoftBins();
        Dictionary<ushort, int> GetFilteredSoftBinsCount();
        List<ushort> GetFilteredHardBins();
        Dictionary<ushort, int> GetFilteredHardBinsCount();
        List<TestID> GetFilteredTestIDs();
        List<int> GetFilteredChipsIndexes();
        List<int> GetFilteredChipsIndexes(List<byte> sites);
        List<ChipInfo> GetFilteredChipsInfo();
        List<ChipInfo> GetFilteredChipsInfo(List<byte> sites);
        /// <summary>
        /// return an array of the selected item data with the filter, 
        /// it will be null if the correspond partdon't have result there, 
        /// the filtered part won't take place in the array
        /// </summary>
        /// <param name="testID"></param>
        /// <returns></returns>
        List<float?> GetFilteredItemData(TestID testID);

        List<float?> GetFilteredItemData(TestID testID, List<byte> sites);

        ///// <summary>
        ///// To get selected chips' data, will return null if all of the chips are filtered or all test items are filtered
        ///// It's not recommended to get the whole data by this method, please use [DataTable GetFilteredData(int startIndex, int count);] instead
        ///// </summary>
        ///// <param name="chipsId"></param>
        ///// <returns></returns>
        //DataTable GetFilteredData(List<int> chipsId);

        //DataTable GetFilteredData(List<int> chipsId, List<byte> sites);


        Dictionary<byte, ChipSummary> GetChipSummaryBySite();
        Dictionary<byte, ChipSummary> GetChipSummaryBySite(List<byte> sites);
        ChipSummary GetChipSummary();
        ChipSummary GetChipSummary(List<byte> sites);

        //basic file information
        string FilePath { get; }
        string FileName { get; }
        FileBasicInfo BasicInfo{ get; }


        //filter
        void SetFilter(Filter filter);


        int ParsePercent { get; }

    }
}
