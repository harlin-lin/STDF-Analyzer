using C1.WPF;
using DataInterface;
using FileHelper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SillyMonkey.ViewModel {
    public class ShowData: DynamicObject {
        public string Testnumber { get; set; }
        public string TestText { get; set; }
        public float? LoLimit { get; set; }
        public float? HiLimit { get; set; }
        public string Unit { get; set; }

        public float? Min { get; set; }
        public float? Max { get; set; }
        public float? Mean { get; set; }
        public double? CPK { get; set; }
        public double? Sigma { get; set; }
        public int PassCount { get; set; }


        public ShowData(string TN, IItemInfo itemInfo, IItemStatistic itemStatistic, List<float?> data) {
            Testnumber = TN;
            TestText = itemInfo.TestText;
            LoLimit = itemInfo.LoLimit;
            HiLimit = itemInfo.HiLimit;
            Unit = itemInfo.Unit;

            Min = itemStatistic.MinValue;
            Max = itemStatistic.MaxValue;
            Mean = itemStatistic.MeanValue;
            CPK = itemStatistic.Cpk;
            Sigma = itemStatistic.Sigma;
            PassCount = itemStatistic.PassCount;

            for (int i = 1; i <= data.Count; i++)
                _properties.Add(i.ToString(),data[i-1]);
        }

        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        public override IEnumerable<string> GetDynamicMemberNames() {
            return _properties.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            if (_properties.ContainsKey(binder.Name)) {
                result = _properties[binder.Name];
                return true;
            } else {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            if (_properties.ContainsKey(binder.Name)) {
                _properties[binder.Name] = value;
                return true;
            } else {
                return false;
            }
        }
    }
    public class DataGridTabModel : ViewModelBase {
        private StdFileHelper _fileHelper;
        private int _fileHash;
        private int _filterId { get; set; }
        private IDataAcquire _dataAcquire;

        public string TabTitle {get; private set;}
        public string FilePath { get; private set; }

        public List<dynamic> GridData { get; private set; }

        public DataGridTabModel(StdFileHelper stdFileHelper, int fileHash, int filterId) {
            _fileHelper = stdFileHelper;
            _fileHash = fileHash;
            _filterId = filterId;

            _dataAcquire =stdFileHelper.GetFile(fileHash);

            if (_dataAcquire.FileName.Length > 15) 
                TabTitle = _dataAcquire.FileName.Substring(0, 15) + "...";
            else
                TabTitle = _dataAcquire.FileName;
            FilePath = _dataAcquire.FilePath;

            GridData = new List<dynamic>();
            var idInfos = _dataAcquire.GetFilteredTestIDs_Info(_filterId);
            var idStatistic = _dataAcquire.GetFilteredStatistic(_filterId);
            foreach(var ii in idInfos) {
                List<float?> rst = _dataAcquire.GetFilteredItemData(ii.Key, 0, 100, _filterId);
                ShowData sd = new ShowData(ii.Key.GetGeneralTestNumber(), ii.Value, idStatistic[ii.Key], rst);
                GridData.Add(sd);
            }

            RaisePropertyChanged("TabTitle");
            RaisePropertyChanged("FilePath");
        }
    }
}
