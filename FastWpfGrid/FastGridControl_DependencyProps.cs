using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FastWpfGrid
{
    partial class FastGridControl
    {
        #region property Model

        public IFastGridModel Model
        {
            get { return (IFastGridModel) this.GetValue(ModelProperty); }
            set { this.SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            "Model", typeof (IFastGridModel), typeof (FastGridControl), new PropertyMetadata(null, OnModelPropertyChanged));

        private static void OnModelPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl) dependencyObject).OnModelPropertyChanged();
        }

        #endregion

        #region property UseClearType

        public bool UseClearType
        {
            get { return (bool)this.GetValue(UseClearTypeProperty); }
            set { this.SetValue(UseClearTypeProperty, value); }
        }

        public static readonly DependencyProperty UseClearTypeProperty = DependencyProperty.Register(
            "UseClearType", typeof(bool), typeof(FastGridControl), new PropertyMetadata(true, OnUseClearTypePropertyChanged));

        private static void OnUseClearTypePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnUseClearTypePropertyChanged();
        }

        #endregion

        #region property AllowFlexibleRows

        public bool AllowFlexibleRows
        {
            get { return (bool)this.GetValue(AllowFlexibleRowsProperty); }
            set { this.SetValue(AllowFlexibleRowsProperty, value); }
        }

        public static readonly DependencyProperty AllowFlexibleRowsProperty = DependencyProperty.Register(
            "AllowFlexibleRows", typeof(bool), typeof(FastGridControl), new PropertyMetadata(false, OnAllowFlexibleRowsPropertyChanged));

        private static void OnAllowFlexibleRowsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((FastGridControl)dependencyObject).OnAllowFlexibleRowsPropertyChanged();
        }

        #endregion

    }
}