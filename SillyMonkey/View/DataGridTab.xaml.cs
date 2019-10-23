using C1.WPF.FlexGrid;
using SillyMonkey.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SillyMonkey.View {
    /// <summary>
    /// DataGridTab.xaml 的交互逻辑
    /// </summary>
    public partial class DataGridTab : UserControl {
        public DataGridTab() {
            InitializeComponent();
        }

        //public DataGridTabModel ItemsSource {
        //    get { return (DataGridTabModel)GetValue(ItemsSourceProperty); }
        //    set { SetValue(ItemsSourceProperty, value); }
        //}

        //public static readonly DependencyProperty ItemsSourceProperty =
        //    DependencyProperty.Register("ItemsSource", typeof(DataGridTabModel), typeof(DataGridTab), new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

        //private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
        //    var control = sender as DataGridTab;
        //    if (control != null)
        //        control.OnItemsSourceChanged((DataGridTabModel)e.OldValue, (DataGridTabModel)e.NewValue);
        //}



        //private void OnItemsSourceChanged(DataGridTabModel oldValue, DataGridTabModel newValue) {
        //    // Remove handler for oldValue.CollectionChanged
        //    var oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;

        //    if (null != oldValueINotifyCollectionChanged) {
        //        oldValueINotifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
        //    }
        //    // Add handler for newValue.CollectionChanged (if possible)
        //    var newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
        //    if (null != newValueINotifyCollectionChanged) {
        //        newValueINotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
        //    }

        //}

        //void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        //    //Do your stuff here.
        //}


        public DataGridTabModel ItemsSource {
            get => (DataGridTabModel)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(DataGridTabModel), typeof(DataGridTab), new PropertyMetadata(null, (s, e) => {
                if (s is DataGridTab uc) {
                    if (e.OldValue is INotifyCollectionChanged oldValueINotifyCollectionChanged) {
                        oldValueINotifyCollectionChanged.CollectionChanged -= uc.ItemsSource_CollectionChanged;
                    }

                    if (e.NewValue is INotifyCollectionChanged newValueINotifyCollectionChanged) {
                        newValueINotifyCollectionChanged.CollectionChanged += uc.ItemsSource_CollectionChanged;
                    }
                }
            }));

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            // Logic Here
        }

        // Do Not Forget To Remove Event On UserControl Unloaded
        private void DataGridTab_Unloaded(object sender, RoutedEventArgs e) {
            if (ItemsSource is INotifyCollectionChanged incc) {
                incc.CollectionChanged -= ItemsSource_CollectionChanged;
            }
        }


    }
}
