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
    public class DataGridTabModel : ViewModelBase {
        const int MinPerPageCount = 30;
        const int DefaultPerPageCount = 100;

        private StdFileHelper _fileHelper;
        private int _fileHash;
        private int _filterId { get; set; }
        private IDataAcquire _dataAcquire;
        private int _countPerPage;

        public string TabTitle {get; private set;}
        public string FilePath { get; private set; }

        public DataTable Data { get; private set; }
        public int CountPerPage {
            get { return _countPerPage; }
            set {
                if (value > MinPerPageCount)
                    _countPerPage = value;
                else
                    _countPerPage = MinPerPageCount;
            }
        }
        public int TotalCount { get; private set; }

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

            CountPerPage = DefaultPerPageCount;
            TotalCount = _dataAcquire.GetFilteredChipSummary(_filterId).TotalCount;


            if (TotalCount > CountPerPage)
                Data = _dataAcquire.GetFilteredItemData(0, CountPerPage, _filterId, true);
            else
                Data = _dataAcquire.GetFilteredItemData(0, TotalCount, _filterId, true);
        }
    }
}
