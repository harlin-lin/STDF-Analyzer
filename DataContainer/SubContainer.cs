using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public partial class SubContainer : IDataCollect, IDataAcquire{
        const int DEFAULT_ITEMS_COUNT = 400;

        public event PropertyChangedEventHandler PropertyChanged;
        public string FilePath { get; }
        public LoadingPhase CurrentLoadingPhase { get; private set; }
        public int CurrentLoadingProgress { get; private set; }
        public bool LoadingDone { get;private set; }

        public SubContainer(string filePath) {
            FilePath = filePath;
            CurrentLoadingPhase = LoadingPhase.NotStart;
            CurrentLoadingProgress = 0;
            LoadingDone = false;

            Initialize_RawData();
            //Initialize_PartStatistic();
            Initialize_Filter();
        }

        public void OnPropertyChanged(String propertyName) {
            var hander = PropertyChanged;
            if (hander != null) {
                hander(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Dispose() {
            _dataBase_Result = null;
            _itemContainer = null;
            _siteContainer = null;
            _basicInfo = null;
            _itemStatistics = null;
            _site_PartContainer = null;
            _testTime_PartContainer = null;
            _hardBin_PartContainer = null;
            _softBin_PartContainer = null;
            _partId_PartContainer = null;
            _xCord_PartContainer = null;
            _yCord_PartContainer = null;
            _chipType_PartContainer = null;
            _resultType_PartContainer = null;

        }
    }
}
