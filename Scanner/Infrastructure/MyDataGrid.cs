using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Scanner.Infrastructure
{

    //AutoScrolling DataGrid
    public class MyDataGrid : DataGrid
    {
        private ScrollViewer _scrollViewer;

        public bool Scrollable
        {
            get { return (bool)GetValue(ScrollableProperty); }
            set { SetValue(ScrollableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollableProperty =
            DependencyProperty.Register("Scrollable", typeof(bool), typeof(MyDataGrid), new PropertyMetadata(false));


        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            var value = oldValue as INotifyCollectionChanged;
            if (value != null)
                value.CollectionChanged -= ItemsCollectionChanged;

            if (!(newValue is INotifyCollectionChanged)) return;

            ((INotifyCollectionChanged)newValue).CollectionChanged += ItemsCollectionChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scrollViewer = RecursiveVisualChildFinder<ScrollViewer>(this) as ScrollViewer;
        }

       private  void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_scrollViewer == null) return;

            UpdateLayout();
            if (Scrollable)
                _scrollViewer.ScrollToBottom();
        }

        private static DependencyObject RecursiveVisualChildFinder<T>(DependencyObject rootObject)
        {
            var child = VisualTreeHelper.GetChild(rootObject, 0);
            if (child == null) return null;

            return child.GetType() == typeof(T) ? child : RecursiveVisualChildFinder<T>(child);
        }
    }
}
