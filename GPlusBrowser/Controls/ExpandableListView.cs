using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPlusBrowser.Controls
{
    /// <summary>
    /// ExpandableListView.xaml の相互作用ロジック
    /// </summary>
    public partial class ExpandableListView : HeaderedItemsControl
    {
        public ExpandableListView()
        {
            _expandAnimationDuration = _insertAnimationDuration = (Duration)TimeSpan.FromMilliseconds(100);
            _displayMaxCount = 2;
            ShiftTranslater = new TranslateTransform();
            Loaded += ExpandableListView_Loaded;
        }
        static ExpandableListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExpandableListView), new FrameworkPropertyMetadata(typeof(ExpandableListView)));
        }
        FrameworkElement _itemContainer;
        StackPanel _itemsPanel;
        Duration _expandAnimationDuration;
        Duration _insertAnimationDuration;
        int _displayMaxCount;

        public bool IsExpand
        {
            get { return (bool)GetValue(IsExpandProperty); }
            set { SetValue(IsExpandProperty, value); }
        }
        public bool Expandable
        {
            get { return (bool)GetValue(ExpandableProperty); }
            set { SetValue(ExpandableProperty, value); }
        }
        public double ScrollOffset
        {
            get { return (double)GetValue(ScrollOffsetProperty); }
            set { SetValue(ScrollOffsetProperty, value); }
        }
        public TranslateTransform ShiftTranslater
        {
            get { return (TranslateTransform)GetValue(ShiftTranslaterProperty); }
            set { SetValue(ShiftTranslaterProperty, value); }
        }
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            Expandable = Items.Count > 2;

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Refresh(true);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Refresh(false);
                    break;
            }
        }
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var container = (FrameworkElement)element;
            container.SizeChanged += itemPresenter_SizeChanged;
        }
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
            var container = (FrameworkElement)element;
            container.SizeChanged -= itemPresenter_SizeChanged;
        }
        void ExpandableListView_Loaded(object sender, RoutedEventArgs e)
        {
            _itemContainer = (FrameworkElement)Template.FindName("PART_scroller_itemsPresenter", this);
            _itemsPanel = (StackPanel)
                VisualTreeHelper.GetChild(
                VisualTreeHelper.GetChild(_itemContainer, 0), 0);
            Refresh(false);
        }
        void itemPresenter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_itemContainer == null)
                return;
            Refresh(false);
        }
        void Refresh(bool isEnableAnimation)
        {
            if (_itemsPanel == null)
                return;

            var newAreaHeight = 0.0;
            for (var i = IsExpand ? 0 : Math.Max(Items.Count - _displayMaxCount, 0); i < Items.Count; i++)
            {
                var container = (FrameworkElement)ItemContainerGenerator.ContainerFromIndex(i);
                if (container.IsMeasureValid == false)
                    container.UpdateLayout();
                newAreaHeight += container.ActualHeight;
            }
            if (isEnableAnimation)
            {
                //スクロール
                var scrollAnime = new DoubleAnimation(newAreaHeight - _itemsPanel.ActualHeight, _insertAnimationDuration);
                var heightAnime = new DoubleAnimation(newAreaHeight, _insertAnimationDuration);
                BeginAnimation(ScrollOffsetProperty, scrollAnime, HandoffBehavior.SnapshotAndReplace);
                _itemContainer.BeginAnimation(HeightProperty, heightAnime, HandoffBehavior.SnapshotAndReplace);
            }
            else
            {
                var scrollAnime = new DoubleAnimation(newAreaHeight - _itemsPanel.ActualHeight, TimeSpan.Zero);
                var heightAnime = new DoubleAnimation(newAreaHeight, TimeSpan.Zero);
                BeginAnimation(ScrollOffsetProperty, scrollAnime, HandoffBehavior.SnapshotAndReplace);
                _itemContainer.BeginAnimation(HeightProperty, heightAnime, HandoffBehavior.SnapshotAndReplace);
            }
        }

        public static readonly DependencyProperty IsExpandProperty = DependencyProperty.Register(
            "IsExpand", typeof(bool), typeof(ExpandableListView), new FrameworkPropertyMetadata(false, Changed_IsExpand));
        public static readonly DependencyProperty ExpandableProperty = DependencyProperty.Register(
            "Expandable", typeof(bool), typeof(ExpandableListView), new PropertyMetadata(true));
        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register(
            "ScrollOffset", typeof(double), typeof(ExpandableListView), new PropertyMetadata(0.0, Changed_ScrollOffset));
        public static readonly DependencyProperty ShiftTranslaterProperty = DependencyProperty.Register(
            "ShiftTranslater", typeof(TranslateTransform), typeof(ExpandableListView));

        static void Changed_IsExpand(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var target = (ExpandableListView)sender;
            var duration = (Duration)TimeSpan.FromMilliseconds(500);

            if ((bool)e.NewValue)
            {
                var heightAnime = new DoubleAnimation(target._itemsPanel.ActualHeight, duration) { DecelerationRatio = 1.0 };
                var scrollAnime = new DoubleAnimation(0.0, duration) { DecelerationRatio = 1.0 };
                target._itemContainer.BeginAnimation(HeightProperty, heightAnime, HandoffBehavior.SnapshotAndReplace);
                target.BeginAnimation(ScrollOffsetProperty, scrollAnime, HandoffBehavior.SnapshotAndReplace);
            }
            else
            {
                var newAreaHeight = 0.0;
                for (var i = target.IsExpand ? 0 : Math.Max(target.Items.Count - target._displayMaxCount, 0); i < target.Items.Count; i++)
                {
                    var container = (FrameworkElement)target.ItemContainerGenerator.ContainerFromIndex(i);
                    newAreaHeight += container.ActualHeight;
                }
                var heightAnime = new DoubleAnimation(newAreaHeight, duration) { DecelerationRatio = 1.0 };
                var scrollAnime = new DoubleAnimation(newAreaHeight - target._itemsPanel.ActualHeight, duration) { DecelerationRatio = 1.0 };
                target._itemContainer.BeginAnimation(HeightProperty, heightAnime, HandoffBehavior.SnapshotAndReplace);
                target.BeginAnimation(ScrollOffsetProperty, scrollAnime, HandoffBehavior.SnapshotAndReplace);
            }
        }
        static void Changed_ScrollOffset(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var mediator = (ExpandableListView)sender;
            mediator.ShiftTranslater.Y = (double)e.NewValue;
        }
    }

    public class IsExpandCommentCountToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;
            return ((bool)values[0]) ? "コメントを非表示" : string.Format("{0}件のコメント", values[1]);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}