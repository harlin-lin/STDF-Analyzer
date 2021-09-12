using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer{
    public delegate void ExtractDoneEventHandler(IDataAcquire data);

    public interface IDataAcquire : IDisposable {

        #region property get the file default infomation
        /// <summary>
        /// Get all of the sites number in the stdf file
        /// </summary>
        /// <returns>return a list of site number</returns>
        byte[] GetSites();

        /// <summary>
        /// Get all of the sites and the corresponding test devices
        /// </summary>
        /// <returns>return a dictionary of the sites and count, key is site number, value is device count</returns>
        Dictionary<byte, int> GetSitesChipCount();

        /// <summary>
        /// Get all of the soft bins in the stdf file
        /// </summary>
        /// <returns>return a list of the soft bins</returns>
        ushort[] GetSoftBins();

        /// <summary>
        /// Get all of the soft bins and the corresponding devices count
        /// </summary>
        /// <returns>return a dictionary of the bins and the count, key is bin, value is device count</returns>
        Dictionary<ushort, int> GetSoftBinsCount();

        /// <summary>
        /// Get all of the hard bins in the stdf file
        /// </summary>
        /// <returns>return a list of the hard bins</returns>
        ushort[] GetHardBins();

        /// <summary>
        /// Get all of the hard bins and the corresponding devices count
        /// </summary>
        /// <returns>return a dictionary of the bins and the count, key is bin, value is device count</returns>
        Dictionary<ushort, int> GetHardBinsCount();

        /// <summary>
        /// Get all of the test items ID(Id consist of the TN and the sub number)in the stdf file
        /// </summary>
        /// <returns>return a list of the test items ID</returns>
        string[] GetTestIDs();

        /// <summary>
        /// Get the test items Info by ID
        /// </summary>
        /// <returns>return test info</returns>
        ItemInfo GetTestInfo(string id);

        ///// <summary>
        ///// Get the test items Info by ID
        ///// </summary>
        ///// <returns>return test info</returns>
        //TestItem GetTestItem(TestID id);

        /// <summary>
        /// Get all of the test ids and the corresponding item info
        /// </summary>
        /// <returns>return a dictionary of the id and th item info</returns>
        Dictionary<string, ItemInfo> GetTestIDs_Info();

        ///// <summary>
        ///// Get all of the chip indexes in the stdf file
        ///// </summary>
        ///// <returns>return a list of all of the indexes</returns>
        //IEnumerable<int> GetChipsIndexes();

        //IEnumerable<IChipInfo> GetChipsInfo();


        ///// <summary>
        ///// get the full file summary by site
        ///// </summary>
        ///// <returns>key is site</returns>
        //Dictionary<byte, IChipSummary> GetChipSummaryBySite();

        ///// <summary>
        ///// get the full file summary info
        ///// </summary>
        ///// <returns></returns>
        //IChipSummary GetChipSummary();

        //string GetSummary();

        /// <summary>
        /// get the soft bin names defined in the file, item1 is Pass/fail, item2 is bin name
        /// </summary>
        /// <returns></returns>
        Dictionary<ushort, Tuple<string, string>> GetSBinInfo();

        /// <summary>
        /// get the hard bin names defined in the file, item1 is Pass/fail, item2 is bin name
        /// </summary>
        /// <returns></returns>
        Dictionary<ushort, Tuple<string, string>> GetHBinInfo();


        Dictionary<string, ItemStatistic> GetStatistic();


        int ChipsCount { get; }
        string FilePath { get; }
        string FileName { get; }
        LoadingPhase CurrentLoadingPhase { get; }
        bool LoadingDone { get; }
        int CurrentLoadingProgress { get; }
        #endregion


        #region this info is filtered by filter
        Filter GetFilter(int filterId);

        float?[] GetFilteredItemData(string testID, int filterId);
        float?[] GetFilteredItemData(string testID, int startIndex, int count, int filterId);

        #endregion


        #region filter
        ///// <summary>
        ///// set the corresponding filter
        ///// </summary>
        ///// <param name="filterId">filter ID</param>
        ///// <param name="filter">filter setup</param>
        //void UpdateFilter(int filterId, FilterSetup newFilter);

        ///// <summary>
        ///// create a new filter with the setup
        ///// </summary>
        ///// <param name="filter">filter setup</param>
        ///// <returns>the created filter's id</returns>
        //int CreateFilter(FilterSetup filter);
        int CreateFilter();

        void ResetFilter(int filterId);

        void UpdateFilter(int filterId, int[] maskIds, int[] maskItemIds);

        //int CreateFilterCopy(int filterId);

        ///// <summary>
        ///// get all of the created filters, include the default created 1 for basic file and n for n sites
        ///// </summary>
        ///// <returns>filters' id and the corresponding comment</returns>
        //Dictionary<int, FilterSetup> GetAllFilter();

        ///// <summary>
        ///// get corresponding site's filter
        ///// </summary>
        ///// <param name="site">if is null means get the full sites one</param>
        ///// <returns></returns>  
        //int GetFilterID(byte? site);

        /// <summary>
        /// find the index of the correspoding FilterId, return -1 if cannot find
        /// </summary>
        /// <param name="filterId"></param>
        /// <returns></returns>
        int GetFilterIndex(int filterId);

        //FilterSetup GetFilterSetup(int filterId);

        /// <summary>
        /// remove the corresponding filter
        /// </summary>
        /// <param name="filterId">the filter's id which need to be removed</param>
        void RemoveFilter(int filterId);
        #endregion

    }
}
