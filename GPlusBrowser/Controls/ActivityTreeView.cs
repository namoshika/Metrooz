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
            _translateTransformer = new TranslateTransform();
        }
        TranslateTransform _translateTransformer;
        ScrollViewer _scrollviewer;
        List<double> _itemHeights;
        List<TreeViewItem> _items;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scrollviewer = (ScrollViewer)((Border)VisualTreeHelper.GetChild(this, 0)).Child;
            _scrollviewer.ScrollChanged += _scrollviewer_ScrollChanged;
        }
        protected override DependencyObject GetContainerForItemOverride()
        {
            var element = (TreeViewItem)base.GetContainerForItemOverride();
            element.SizeChanged += element_SizeChanged;
            element.Loaded += element_Loaded;
            element.Unloaded += element_Unloaded;
            element.RenderTransform = _translateTransformer;
            return element;
        }
        void element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e != null && !e.HeightChanged)
                return;
            var element = (TreeViewItem)sender;
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
            _itemHeights.Insert(Math.Min(_itemHeights.Count, idx), 0);
            _items.Insert(Math.Min(_items.Count, idx), element);
            element_SizeChanged(element, null);
            element.Loaded -= element_Loaded;

            var elementOffset = _itemHeights.Take(idx).Sum();
            if (_scrollviewer.VerticalOffset + _scrollviewer.ActualHeight > elementOffset
                && _scrollviewer.VerticalOffset < elementOffset + element.ActualHeight)
            {
                _translateTransformer.BeginAnimation(
                    TranslateTransform.YProperty,
                    new System.Windows.Media.Animation.DoubleAnimation(
                        -element.ActualHeight, 0, new Duration(TimeSpan.FromMilliseconds(300)))
                        {
                            AccelerationRatio = 0.0,
                            DecelerationRatio = 1.0
                        },
                    HandoffBehavior.SnapshotAndReplace);
            }
        }
        void element_Unloaded(object sender, RoutedEventArgs e)
        {
            var element = (TreeViewItem)sender;
            var idx = _items.IndexOf(element);
            var height = _itemHeights[idx];
            element.SizeChanged -= element_SizeChanged;
            element.Unloaded -= element_Unloaded;

            if (_scrollviewer.VerticalOffset > _itemHeights.Take(idx).Sum())
                _scrollviewer.ScrollToVerticalOffset(_scrollviewer.VerticalOffset - height);
            _itemHeights.RemoveAt(idx);
            _items.RemoveAt(idx);
        }
        void _scrollviewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = 0.0;
            for (var i = 0; i < _itemHeights.Count; i++)
            {
                var item = _items[i];
                SetIsEnableAnimation(item, offset < e.VerticalOffset + e.ViewportHeight && offset + item.ActualHeight > e.VerticalOffset);
                offset += item.ActualHeight;
            }
        }

        public static bool GetIsEnableAnimation(DependencyObject obj)
        { return (bool)obj.GetValue(IsEnableAnimationProperty); }
        public static void SetIsEnableAnimation(DependencyObject obj, bool value)
        { obj.SetValue(IsEnableAnimationProperty, value); }
        public static readonly DependencyProperty IsEnableAnimationProperty = DependencyProperty.RegisterAttached(
            "IsEnableAnimation", typeof(bool), typeof(ActivityTreeView), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));
    }
}
