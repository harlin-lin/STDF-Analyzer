using System.Windows;
using System.Windows.Controls;

namespace UI_DataList.Views {
    public class TreeViewEx : TreeView {
        public static readonly RoutedEvent MouseDoubleClickExEvent =
                    EventManager.RegisterRoutedEvent("MouseDoubleClickEx", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeViewEx));

        public event RoutedEventHandler MouseDoubleClickEx {
            add {
                AddHandler(MouseDoubleClickExEvent, value);
            }

            remove {
                RemoveHandler(MouseDoubleClickExEvent, value);
            }
        }

        protected override void OnPreviewMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e) {
            e.Handled = true;
            //base.OnPreviewMouseDoubleClick(e);  
            RoutedEventArgs args = new RoutedEventArgs(MouseDoubleClickExEvent, this);
            RaiseEvent(args);
        }
    }

    /// <summary>
    /// Interaction logic for DataManagement
    /// </summary>
    public partial class DataManagement : UserControl {

        public DataManagement() {
            InitializeComponent();
        }
    }
}
