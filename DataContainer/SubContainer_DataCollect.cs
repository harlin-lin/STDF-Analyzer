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
            _partIdx +=1 ;
            _siteContainer[siteNum] = _partIdx;
            _allIndex.Add(_partIdx);
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
            ushort softBin, string partId, short? xCord, short? yCord, 
            DeviceType deviceType, ResultType result) {
            _site_PartContainer.Add(siteNum);
            _testTime_PartContainer.Add(testTime);
            _hardBin_PartContainer.Add(hardBin);
            _softBin_PartContainer.Add(softBin);
            _partId_PartContainer.Add(partId);
            _xCord_PartContainer.Add(xCord);
            _yCord_PartContainer.Add(yCord);
            if(_ifCordValid && (!xCord.HasValue || !yCord.HasValue)) {
                _ifCordValid = false;
            }
            _chipType_PartContainer.Add(deviceType);
            _resultType_PartContainer.Add(result);
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

            Console.WriteLine("Percent:" + percent);
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

                Console.WriteLine("Parts Done");
            });
            asyncTask[1] = Task.Run(() => {
                AnalyseItems();
                CurrentLoadingProgress += 50;
                OnPropertyChanged("CurrentLoadingProgress");

                Console.WriteLine("Items Done");
            });

            Task.WaitAll(asyncTask);

            CurrentLoadingPhase = LoadingPhase.Done;
            OnPropertyChanged("CurrentLoadingPhase");

            LoadingDone = true;
            OnPropertyChanged("LoadingDone");
        }
    }
}
