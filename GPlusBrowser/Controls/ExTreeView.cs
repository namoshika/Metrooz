using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GPlusBrowser.Controls
{
    public class ExTreeView : TreeView
    {
        public ExTreeView()
        {
            _itemHeights = new List<double>();
            _itemPairDict = new Dictionary<object, TreeViewItem>();
            _translateTransformer = new TranslateTransform();
            SizeChanged += ExTreeView_SizeChanged;
            Loaded += ExTreeView_Loaded;
        }
        TranslateTransform _translateTransformer;
        ScrollViewer _scrollviewer;
        List<double> _itemHeights;
        Dictionary<object, TreeViewItem> _itemPairDict;

        public double ViewportHeight
        {
            get { return (double)GetValue(ViewportHeightProperty); }
            set { SetValue(ViewportHeightProperty, value); }
        }
        public double ScrollOffset
        {
            get { return (double)GetValue(ScrollOffsetProperty); }
            set { SetValue(ScrollOffsetProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scrollviewer = (ScrollViewer)((Border)VisualTreeHelper.GetChild(this, 0)).Child;
            Observable.FromEventPattern(_scrollviewer, "ScrollChanged")
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(Dispatcher)
                .Subscribe(_scrollviewer_ScrollChanged);
        }
        protected override DependencyObject GetContainerForItemOverride()
        {
            var element = (TreeViewItem)base.GetContainerForItemOverride();
            element.SizeChanged += element_SizeChanged;
            element.Loaded += element_Loaded;
            element.RenderTransform = _translateTransformer;
            return element;
        }
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    var value = _itemHeights[e.NewStartingIndex];
                    _itemHeights[e.NewStartingIndex] = _itemHeights[e.OldStartingIndex];
                    _itemHeights[e.OldStartingIndex] = value;

                    var offset = 0.0;
                    var top = _scrollviewer.VerticalOffset;
                    var bottom = _scrollviewer.VerticalOffset + _scrollviewer.ViewportHeight;
                    TreeViewItem item;
                    for (var i = 0; (item = (TreeViewItem)ItemContainerGenerator.ContainerFromIndex(i)) != null; i++)
                    {
                        SetIsEnableAnimation(item, offset < bottom && offset + item.ActualHeight > top);
                        offset += item.ActualHeight;
                    }
                    ScrollOffset = _scrollviewer.VerticalOffset;

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    var dataContext = e.OldItems[0];
                    var height = _itemHeights[e.OldStartingIndex];
                    _itemPairDict[dataContext].SizeChanged -= element_SizeChanged;
                    _itemPairDict.Remove(dataContext);

                    if (_scrollviewer.VerticalOffset > _itemHeights.Take(e.OldStartingIndex).Sum())
                        _scrollviewer.ScrollToVerticalOffset(_scrollviewer.VerticalOffset - height);
                    _itemHeights.RemoveAt(e.OldStartingIndex);
                    break;
            }
        }

        void ExTreeView_Loaded(object sender, RoutedEventArgs e)
        { ViewportHeight = _scrollviewer.ViewportHeight; }
        void ExTreeView_SizeChanged(object sender, SizeChangedEventArgs e)
        { ViewportHeight = _scrollviewer.ViewportHeight; }
        void element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e != null && !e.HeightChanged)
                return;

            var element = (TreeViewItem)sender;
            var idx = ItemContainerGenerator.IndexFromContainer(element);
            var upActivityTopOffset = 0.0;
            var upActivityBottomOffset = 0.0;
            for (var i = 0; i <= idx; i++)
            {
                var child = (TreeViewItem)ItemContainerGenerator.ContainerFromIndex(i);
                upActivityTopOffset = upActivityBottomOffset;
                upActivityBottomOffset += child.ActualHeight;
            }

            // *note:
            //サイズ変更があった要素の下部の座標が垂直オフセット値より上の状態で
            //そのまま変更すると今表示されている下の要素の座標が変わってしまう。
            //これへの対策として垂直オフセット値を調整する事で座標が変わってない
            //ように見せる。この時、垂直オフセット値よりサイズ変更要素の下部座標
            //が150px程はみ出ていても表示されていない上部要素と同様に扱う。
            // *note:
            //_itemHeights.Count == Items.Countがtrueにならない状態では正しく計算
            //できないので飛ばす。このメソッドはelement_Loaded()時に手動で呼び出さ
            //れているため、飛ばしても誤作動はしない
            if (_scrollviewer.VerticalOffset > 150
                && upActivityBottomOffset - 150.0 < _scrollviewer.VerticalOffset
                && _itemHeights.Count == Items.Count)
            {
                var diff = upActivityBottomOffset - _itemHeights.Take(idx + 1).Sum();
                _scrollviewer.ScrollToVerticalOffset(_scrollviewer.VerticalOffset + diff);
            }
            if (_itemHeights.Count == Items.Count)
                _itemHeights[idx] = element.ActualHeight;
        }
        void element_Loaded(object sender, RoutedEventArgs e)
        {
            var element = (TreeViewItem)sender;
            var idx = ItemContainerGenerator.IndexFromContainer(element);
            _itemHeights.Insert(Math.Min(_itemHeights.Count, idx), 0);
            _itemPairDict.Add(ItemContainerGenerator.ItemFromContainer(element), element);
            element_SizeChanged(element, null);
            element.Loaded -= element_Loaded;

            var elementOffset = _itemHeights.Take(idx).Sum();
            if (_scrollviewer.VerticalOffset <= 150)
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
        void _scrollviewer_ScrollChanged(EventPattern<EventArgs> e)
        {
            var args = (ScrollChangedEventArgs)e.EventArgs;
            var offset = 0.0;
            var top = args.VerticalOffset;
            var bottom = args.VerticalOffset + args.ViewportHeight;
            TreeViewItem item;
            for (var i = 0; (item = (TreeViewItem)ItemContainerGenerator.ContainerFromIndex(i)) != null; i++)
            {
                SetIsEnableAnimation(item, offset < bottom && offset + item.ActualHeight > top);
                offset += item.ActualHeight;
            }
            ScrollOffset = _scrollviewer.VerticalOffset;
        }

        public static bool GetIsEnableAnimation(DependencyObject obj)
        { return (bool)obj.GetValue(IsEnableAnimationProperty); }
        public static void SetIsEnableAnimation(DependencyObject obj, bool value)
        { obj.SetValue(IsEnableAnimationProperty, value); }

        public static readonly DependencyProperty IsEnableAnimationProperty = DependencyProperty.RegisterAttached(
            "IsEnableAnimation", typeof(bool), typeof(ExTreeView), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty ViewportHeightProperty = DependencyProperty.Register(
            "ViewportHeight", typeof(double), typeof(ExTreeView), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register(
            "ScrollOffset", typeof(double), typeof(ExTreeView), new UIPropertyMetadata(0.0));
    }
}
