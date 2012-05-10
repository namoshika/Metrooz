using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GPlusBrowser.Controls
{
    public class ActivityTreeView : TreeView
    {
        public ActivityTreeView()
        {
            _itemHeights = new List<double>();
            _items = new List<TreeViewItem>();
            Loaded += ActivityTreeView_Loaded;
        }
        ScrollViewer _scrollviewer;
        List<double> _itemHeights;
        List<TreeViewItem> _items;

        protected override DependencyObject GetContainerForItemOverride()
        {
            var element = (TreeViewItem)base.GetContainerForItemOverride();
            element.SizeChanged += element_SizeChanged;
            element.Loaded += element_Loaded;
            element.Unloaded += element_Unloaded;
            return element;
        }
        void ActivityTreeView_Loaded(object sender, RoutedEventArgs e)
        {
            _scrollviewer = (ScrollViewer)((Border)VisualTreeHelper.GetChild(this, 0)).Child;
        }
        void element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e != null && !e.HeightChanged)
                return;
            var element = (TreeViewItem)sender;
            lock (_itemHeights)
                if (!_items.Contains(element))
                    return;
            var idx = ItemContainerGenerator.IndexFromContainer(element);
            var upActivityOffsetAndHeight = 0.0;
            var upActivityOffset = 0.0;
            for(var i = 0; i <= idx; i++)
            {
                var child = (TreeViewItem)ItemContainerGenerator.ContainerFromIndex(i);
                upActivityOffset = upActivityOffsetAndHeight;
                upActivityOffsetAndHeight += child.ActualHeight;
            }
            //150px程表示されている要素があっても非表示要素扱いとしてスクロール量調整
            //が必要な上部要素が存在すると扱う。しかし、上が見えている要素だった場合は
            //そうしない
            if (_scrollviewer.VerticalOffset > 100
                && upActivityOffset < _scrollviewer.VerticalOffset
                && upActivityOffsetAndHeight - 150.0 < _scrollviewer.VerticalOffset)
            {
                var diff = upActivityOffsetAndHeight - _itemHeights.Take(idx + 1).Sum();
                _scrollviewer.ScrollToVerticalOffset(_scrollviewer.VerticalOffset + diff);
            }

            _itemHeights[Math.Min(_itemHeights.Count - 1, idx)] = element.ActualHeight;
        }
        void element_Loaded(object sender, RoutedEventArgs e)
        {
            var element = (TreeViewItem)sender;
            var idx = ItemContainerGenerator.IndexFromContainer(element);
            lock (_itemHeights)
            {
                _itemHeights.Insert(Math.Min(_itemHeights.Count, idx), 0);
                _items.Insert(Math.Min(_items.Count, idx), element);
            }
            element_SizeChanged(element, null);

            element.BeginAnimation(
                TreeViewItem.HeightProperty,
                new System.Windows.Media.Animation.DoubleAnimation(
                    0, element.ActualHeight, new Duration(TimeSpan.FromMilliseconds(300))));
            var timeline = new ObjectAnimationUsingKeyFrames();
            timeline.KeyFrames.Add(new DiscreteObjectKeyFrame(double.NaN));
            element.BeginAnimation(
                TreeViewItem.HeightProperty,
                timeline, HandoffBehavior.Compose);
        }
        void element_Unloaded(object sender, RoutedEventArgs e)
        {
            var element = (TreeViewItem)sender;
            var idx = _items.IndexOf(element);
            var height = _itemHeights[idx];
            element.SizeChanged -= element_SizeChanged;
            element.Loaded -= element_Loaded;

            if (_scrollviewer.VerticalOffset > _itemHeights.Take(idx).Sum())
                _scrollviewer.ScrollToVerticalOffset(_scrollviewer.VerticalOffset - height);
            _itemHeights.RemoveAt(idx);
            _items.RemoveAt(idx);
        }
    }
}
