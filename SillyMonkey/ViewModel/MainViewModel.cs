using DataInterface;
using FileHelper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SillyMonkey.View;

namespace SillyMonkey.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase {
        private StdFileHelper _fileHelper;

        public RelayCommand<DragEventArgs> DropCommand { get; private set; }

        public FileManagementModel Files { get; private set; }

        public ObservableCollection<TabItem> DataTabItems { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel() {
            DropCommand = new RelayCommand<DragEventArgs>(async (e) => {
                var paths = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
                foreach (string path in paths) {
                    var ext = System.IO.Path.GetExtension(path).ToLower();
                    if (ext == ".stdf" || ext == ".std") {
                        AddFile(path);
                    } else {
                        //log message not supported
                    }
                }
                //extract the files
                await Task.Run(new Action(() => ExtractFiles(new List<string>(paths.OfType<string>()))));
            });

            DataTabItems = new ObservableCollection<TabItem>();

            _fileHelper = new StdFileHelper();
            Files = new FileManagementModel(_fileHelper);
            Files.OpenDetailEvent += Files_OpenDetailEvent;
            Files.RemoveTabEvent += Files_RemoveTabEvent;
            Files.RemoveFileEvent += RemoveFile;
        }

        private void Files_RemoveTabEvent(int tag) {
            foreach(var t in DataTabItems) {
                if ((int)t.Tag == tag) {
                    DataTabItems.Remove(t);
                    break;
                }
            }
        }

        private void Files_OpenDetailEvent(int fileHash, byte? site, int tag) {
            var id =_fileHelper.CreateFilterDataHandler(fileHash, site);
            if (!id.HasValue) return;

            DataGridTabModel dataGridTabModel = new DataGridTabModel(_fileHelper, fileHash, id.Value);

            TabItem tabItem = new TabItem();
            var grid = new DataGridTab();
            tabItem.Content = grid;
            tabItem.DataContext = dataGridTabModel;
            tabItem.Header = dataGridTabModel.TabTitle;
            tabItem.ToolTip = dataGridTabModel.FilePath;
            tabItem.Tag = tag;
            DataTabItems.Add(tabItem);
            tabItem.IsSelected = true;
        }

        private void AddFile(string path) {
            var val = _fileHelper.AddFile(path);
        }
        private void RemoveFile(string path) {
            _fileHelper.RemoveFile(path);
        }

        private void ExtractFiles(List<string> paths) {
            _fileHelper.ExtractFiles(paths);
        }

    }
}