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
    public class DataGridTabModel : ViewModelBase {
        private StdFileHelper _fileHelper;
        private int _fileHash;
        private int _filterId { get; set; }



        public DataGridTabModel(StdFileHelper stdFileHelper, int fileHash, int filterId) {
            _fileHelper = stdFileHelper;
            _fileHash = fileHash;
            _filterId = filterId;
        }
    }
}
