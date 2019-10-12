using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
        public RelayCommand<DragEventArgs> DragEnterCommand { get; private set; }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel() {
            DragEnterCommand = new RelayCommand<DragEventArgs>((e) => {
                var paths = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
                foreach (string path in paths) {
                    var ext = System.IO.Path.GetExtension(path).ToLower();
                    if (ext == ".stdf" || ext == ".std") {
                        //_stdFiles.AddFile(path);
                    } else {
                        //log message not supported
                    }
                }
                //extract the files
                //await Task.Run(new Action(() => _stdFiles.ExtractFiles(new List<string>(paths.OfType<string>()))));
            });

        }


    }
}