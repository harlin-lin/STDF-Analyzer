using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContainer {
    public partial class SubContainer {
        public void SetBasicInfo(string name, string val) {
            CurrentLoadingPhase = LoadingPhase.Reading;
            _basicInfo.Add(name, val);
        }

        public void AddSiteNum(byte siteNum) {
            _siteContainer.Add(siteNum, 0);
        }

        public void AddPir(byte siteNum) {
            _preIdx +=1 ;
            _siteContainer[siteNum] = _preIdx;
            AdjustDataBaseCapcity();
        }

        public void AddSbr(ushort binNO, Tuple<string,string> binNmaeInfo) {
            if (!_softBinNames.ContainsKey(binNO)) {
                _softBinNames.Add(binNO, binNmaeInfo);
            }
        }

        public void AddHbr(ushort binNO, Tuple<string, string> binNmaeInfo) {
            if (!_hardBinNames.ContainsKey(binNO)) {
                _hardBinNames.Add(binNO, binNmaeInfo);
            }
        }

        public void UpdateItemInfo(string uid, ItemInfo itemInfo) {
            _itemContainer[uid] = itemInfo;
        }
        public void AddTestData(byte siteNum, string uid, float rst) {
            SetData(uid, _siteContainer[siteNum], rst);
        }

        public void AddPrr(byte siteNum, uint? testTime, ushort hardBin, 
            ushort softBin, string partId, short xCord, short yCord, 
            DeviceType deviceType, ResultType result) {
            _site_PartContainer.Add(siteNum);
            _testTime_PartContainer.Add(testTime);
            _hardBin_PartContainer.Add(hardBin);
            _softBin_PartContainer.Add(softBin);
            _partId_PartContainer.Add(partId);
            _xCord_PartContainer.Add(xCord);
            _yCord_PartContainer.Add(yCord);
            if(_ifCordValid && (xCord == short.MinValue || yCord == short.MinValue)) {
                _ifCordValid = false;
            }
            _chipType_PartContainer.Add(deviceType);
            _resultType_PartContainer.Add(result);
            if (_partIdx < _preIdx) {
                _partIdx += 1;
                _allIndex.Add(_partIdx);
            }
        }

        public ItemInfo IfContainItemInfo(string uid) {
            if (CheckItemContainer(uid)) {
                return _itemContainer[uid];
            }
            return null;
        } 

        public void SetReadingPercent(int percent) {
            if (percent < 0) {
                CurrentLoadingProgress = 0;
            } else if(percent > 100) {
                CurrentLoadingProgress = 100;
            } else { 
                CurrentLoadingProgress = percent;
            }
            OnPropertyChanged("CurrentLoadingProgress");

            //Console.WriteLine("Percent:" + percent);
        }

        public void AnalyseData() {
            CurrentLoadingPhase = LoadingPhase.Analysing;
            OnPropertyChanged("CurrentLoadingPhase");

            CurrentLoadingProgress = 0;
            OnPropertyChanged("CurrentLoadingProgress");

            //Initialize_ItemStatistic();
            Task[] asyncTask= new Task[2];
            asyncTask[0] = Task.Run(()=> {
                AnalyseParts();
                CurrentLoadingProgress += 50;
                OnPropertyChanged("CurrentLoadingProgress");

                //Console.WriteLine("Parts Done");
            });
            asyncTask[1] = Task.Run(() => {
                AnalyseItems();
                CurrentLoadingProgress += 50;
                OnPropertyChanged("CurrentLoadingProgress");

                //Console.WriteLine("Items Done");
            });

            Task.WaitAll(asyncTask);

            CurrentLoadingPhase = LoadingPhase.Done;
            OnPropertyChanged("CurrentLoadingPhase");

            LoadingDone = true;
            OnPropertyChanged("LoadingDone");
        }

        public bool MergeData(SubContainer da) {
            if (!da.LoadingDone) return false;

            SetReadingPercent(0);

            //merge the misc
            foreach(var s in da._siteContainer) {
                if (!_siteContainer.ContainsKey(s.Key)) {
                    _siteContainer.Add(s.Key, 0);
                }
            }
            if(_basicInfo is null)
                _basicInfo = new Dictionary<string, string>(da._basicInfo);

            foreach (var s in da._softBinNames) {
                if (!_softBinNames.ContainsKey(s.Key)) {
                    _softBinNames.Add(s.Key, s.Value);
                }
            }
            foreach (var s in da._hardBinNames) {
                if (!_hardBinNames.ContainsKey(s.Key)) {
                    _hardBinNames.Add(s.Key, s.Value);
                }
            }

            //add item info
            foreach (var item in da._itemContainer) {
                if (!CheckItemContainer(item.Key)) {
                    _itemContainer[item.Key] = new ItemInfo(item.Value);
                }
            }
            SetReadingPercent(2);

            int start = _partIdx + 1;
            _preIdx += da._preIdx+1;
            _partIdx += da._partIdx + 1;
            AdjustDataBaseCapcity();
            SetReadingPercent(5);
            double p = 1.0;
            foreach (var uid in da._dataBase_Result.Keys) {
                int i = start;
                foreach(var v in da.GetItemVal(uid)) {
                    SetData(uid, i++, v);
                }
                SetReadingPercent((int)((p / (double)(da._dataBase_Result.Keys.Count()))*90));
            }

            for (int i = 0; i<= da._partIdx; i++) {
                _site_PartContainer.Add(da._site_PartContainer[i]);
                _testTime_PartContainer.Add(da._testTime_PartContainer[i]);
                _hardBin_PartContainer.Add(da._hardBin_PartContainer[i]);
                _softBin_PartContainer.Add(da._softBin_PartContainer[i]);
                _partId_PartContainer.Add(da._partId_PartContainer[i]);
                _xCord_PartContainer.Add(da._xCord_PartContainer[i]);
                _yCord_PartContainer.Add(da._yCord_PartContainer[i]);
                if (_ifCordValid && (da._xCord_PartContainer[i]==short.MinValue || da._yCord_PartContainer[i] == short.MinValue)) {
                    _ifCordValid = false;
                }
                _chipType_PartContainer.Add(da._chipType_PartContainer[i]);
                _resultType_PartContainer.Add(da._resultType_PartContainer[i]);
            }

            _allIndex = (from i in Enumerable.Range(0, _partIdx + 1)
                         select i).ToList();

            SetReadingPercent(100);
            return true;
        }

        public bool MergeSubData(SubContainer da, int filterId) {
            if (!da.LoadingDone) return false;

            SetReadingPercent(0);

            if (!da._filterContainer.ContainsKey(filterId)) {
                throw new Exception("merge filter not exsist");
            }

            var filter = da._filterContainer[filterId];

            //merge the misc
            foreach (var s in filter.FilterPartStatistic.SiteCnt) {
                if (s.Value>0 && !_siteContainer.ContainsKey(s.Key)) {
                    _siteContainer.Add(s.Key, 0);
                }
            }
            _basicInfo = new Dictionary<string, string>(da._basicInfo);

            foreach (var s in da._softBinNames) {
                if (!_softBinNames.ContainsKey(s.Key)) {
                    _softBinNames.Add(s.Key, s.Value);
                }
            }
            foreach (var s in da._hardBinNames) {
                if (!_hardBinNames.ContainsKey(s.Key)) {
                    _hardBinNames.Add(s.Key, s.Value);
                }
            }

            var tgtItems = da._itemContainer.Keys; // filter.FilterItemStatistics.Keys;

            //add item info
            foreach (var item in tgtItems) {
                if (!CheckItemContainer(item)) {
                    _itemContainer[item] = new ItemInfo(da._itemContainer[item]);
                }
            }
            SetReadingPercent(2);

            int start = _partIdx + 1;
            _preIdx += filter.FilterPartStatistic.TotalCnt;
            _partIdx += filter.FilterPartStatistic.TotalCnt;
            AdjustDataBaseCapcity();
            SetReadingPercent(5);
            double p = 1.0;
            foreach (var uid in tgtItems) {
                int i = start;
                foreach (var v in da.GetFilteredItemData(uid, filterId)) {
                    SetData(uid, i++, v);
                }
                SetReadingPercent((int)((p / (double)(tgtItems.Count())) * 90));
            }

            foreach(var i in filter.FilteredPartIdx) {
                _site_PartContainer.Add(da._site_PartContainer[i]);
                _testTime_PartContainer.Add(da._testTime_PartContainer[i]);
                _hardBin_PartContainer.Add(da._hardBin_PartContainer[i]);
                _softBin_PartContainer.Add(da._softBin_PartContainer[i]);
                _partId_PartContainer.Add(da._partId_PartContainer[i]);
                _xCord_PartContainer.Add(da._xCord_PartContainer[i]);
                _yCord_PartContainer.Add(da._yCord_PartContainer[i]);
                if (_ifCordValid && (da._xCord_PartContainer[i] == short.MinValue || da._yCord_PartContainer[i] == short.MinValue)) {
                    _ifCordValid = false;
                }
                _chipType_PartContainer.Add(da._chipType_PartContainer[i]);
                _resultType_PartContainer.Add(da._resultType_PartContainer[i]);
            }

            _allIndex = (from i in Enumerable.Range(0, _partIdx + 1)
                         select i).ToList();

            SetReadingPercent(100);
            return true;
        }

    }
}
