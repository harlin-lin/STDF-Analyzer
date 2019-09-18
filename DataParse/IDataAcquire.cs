using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse {
    interface IDataAcquire {

        #region property get the file default infomation
        /// <summary>
        /// Get all of the sites number in the stdf file
        /// </summary>
        /// <returns>return a list of site number</returns>
        List<byte> GetSites();

        /// <summary>
        /// Get all of the sites and the corresponding test devices
        /// </summary>
        /// <returns>return a dictionary of the sites and count, key is site number, value is device count</returns>
        Dictionary<byte, int> GetSitesChipCount();

        /// <summary>
        /// Get all of the soft bins in the stdf file
        /// </summary>
        /// <returns>return a list of the soft bins</returns>
        List<ushort> GetSoftBins();

        /// <summary>
        /// Get all of the soft bins and the corresponding devices count
        /// </summary>
        /// <returns>return a dictionary of the bins and the count, key is bin, value is device count</returns>
        Dictionary<ushort, int> GetSoftBinsCount();

        /// <summary>
        /// Get all of the hard bins in the stdf file
        /// </summary>
        /// <returns>return a list of the hard bins</returns>
        List<ushort> GetHardBins();

        /// <summary>
        /// Get all of the hard bins and the corresponding devices count
        /// </summary>
        /// <returns>return a dictionary of the bins and the count, key is bin, value is device count</returns>
        Dictionary<ushort, int> GetHardBinsCount();

        /// <summary>
        /// Get all of the test items ID(Id consist of the TN and the sub number)in the stdf file
        /// </summary>
        /// <returns>return a list of the test items ID</returns>
        List<TestID> GetTestIDs();

        /// <summary>
        /// Get all of the test ids and the corresponding item info
        /// </summary>
        /// <returns>return a dictionary of the id and th item info</returns>
        Dictionary<TestID, ItemInfo> GetTestIDs_Info();

        /// <summary>
        /// Get all of the chip indexes in the stdf file
        /// </summary>
        /// <returns>return a list of all of the indexes</returns>
        List<int> GetChipsIndexes();

        int ChipsCount { get; }
        string FilePath { get; }
        string FileName { get; }
        FileBasicInfo BasicInfo { get; }
        int ParsePercent { get; }
        #endregion


        #region this info is filtered by filter
        List<byte> GetFilteredSites();
        Dictionary<byte, int> GetFilteredSitesChipCount();
        List<ushort> GetFilteredSoftBins();
        Dictionary<ushort, int> GetFilteredSoftBinsCount();
        List<ushort> GetFilteredHardBins();
        Dictionary<ushort, int> GetFilteredHardBinsCount();
        List<TestID> GetFilteredTestIDs();
        List<int> GetFilteredChipsIndexes();
        List<ChipInfo> GetFilteredChipsInfo();
        /// <summary>
        /// return an array of the selected item data with the filter, 
        /// it will be null if the correspond partdon't have result there, 
        /// the filtered part won't take place in the array
        /// </summary>
        /// <param name="testID"></param>
        /// <returns></returns>
        List<float?> GetFilteredItemData(TestID testID);

        List<float?> GetFilteredItemData(TestID testID, int startIndex, int count);

        Dictionary<byte, ChipSummary> GetFilteredChipSummaryBySite();
        ChipSummary GetFilteredChipSummary();

        #endregion


        #region filter
        void SetFilter(Filter filter, int filterId);
        int CreateFilter();
        int CreateFilter(Filter filter);
        void RemoveFilter(int id);
        #endregion


    }
}
