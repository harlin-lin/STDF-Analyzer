using DataInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SillyMonkey.ViewModel {
    public class FileInfo : INotifyPropertyChanged {
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public bool FileStatus { get; private set; }
        public int FileDeviceCount { get; private set; }
        public Dictionary<byte, KeyValuePair<int, string>> Sites { get; private set; }

        public FileInfo(IDataAcquire stdfParse) {
            FileName = stdfParse.FileName;
            FilePath = stdfParse.FilePath;
            FileStatus = stdfParse.ParseDone;
            FileDeviceCount = stdfParse.ChipsCount;
            //Sites = new List<byte>();
            //SitesCount = new List<int>();
            Sites = new Dictionary<byte, KeyValuePair<int, string>>();
        }

        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public void UpdateFileInfo(IDataAcquire stdfParse) {
            FileName = stdfParse.FileName;
            FilePath = stdfParse.FilePath;
            FileStatus = stdfParse.ParseDone;
            FileDeviceCount = stdfParse.ChipsCount;
            _dispatcher.Invoke(new Action(() => {
                //Sites = stdfParse.GetSites();
                //SitesCount=stdfParse.GetSitesChipCount().Values.ToList();
                Sites = (from f in stdfParse.GetSitesChipCount()
                         let x = new KeyValuePair<byte, KeyValuePair<int, string>>(f.Key, new KeyValuePair<int, string>(f.Value, FilePath))
                         select x).ToDictionary(x => x.Key, x => x.Value);
            }));

            OnPropertyChanged("FileStatus");
            OnPropertyChanged("FileDeviceCount");
            OnPropertyChanged("Sites");
            //OnPropertyChanged("SitesCount");

        }

        public event PropertyChangedEventHandler PropertyChanged;
        //OnPropertyChanged event handler to update property value in binding
        private void OnPropertyChanged(string info) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }
}
