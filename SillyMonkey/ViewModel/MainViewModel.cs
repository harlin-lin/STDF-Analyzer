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


            _fileHelper = new StdFileHelper();
            Files = new FileManagementModel(_fileHelper);
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