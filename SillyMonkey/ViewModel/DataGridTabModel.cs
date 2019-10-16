using C1.WPF;
using DataInterface;
using FileHelper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SillyMonkey.ViewModel {
    public class ShowData {
        public List<int> Testnumber { get; set; }
        public List<string> TestName { get; set; }

        public ShowData() {
            Testnumber = new List<int>();
            Testnumber.AddRange(Enumerable.Range(0, 10).ToList());

            TestName = new List<string>();
            TestName.AddRange(Enumerable.Range(0, 9).Select((x)=> $"{x}{x}"));
        }

    }
    public class DataGridTabModel : ViewModelBase {
        private StdFileHelper _fileHelper;
        private int _fileHash;
        private int _filterId { get; set; }
        private IDataAcquire _dataAcquire;

        public string TabTitle {get; private set;}
        public string FilePath { get; private set; }

        public List<ShowData> GridData { get; private set; }

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

            GridData = new List<ShowData>();
            GridData.Add(new ShowData());
            GridData.Add(new ShowData());
            GridData.Add(new ShowData());


            RaisePropertyChanged("TabTitle");
            RaisePropertyChanged("FilePath");
        }
    }
}
