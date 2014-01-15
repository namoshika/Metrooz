using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// CommentListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class ExpandableListView : ItemsControl
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
        protected override DependencyObject GetContainerForItemOverride()
        {
            var container = (FrameworkElement)base.GetContainerForItemOverride();
            container.Loaded += itemPresenter_Loaded;
            return container;
        }
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
            ((FrameworkElement)element).SizeChanged -= itemPresenter_SizeChanged;
            ((FrameworkElement)element).Loaded -= itemPresenter_Loaded;

            var newAreaHeight = 0.0;
            for (var i = IsExpand ? 0 : Math.Max(Items.Count - _displayMaxCount, 0); i < Items.Count; i++)
            {
                var container = (FrameworkElement)ItemContainerGenerator.ContainerFromIndex(i);
                newAreaHeight += container.ActualHeight;
            }
            _itemContainer.BeginAnimation(HeightProperty,
                new DoubleAnimation(newAreaHeight, TimeSpan.Zero));
            BeginAnimation(ScrollOffsetProperty,
                new DoubleAnimation(_itemsPanel.ActualHeight - ((FrameworkElement)element).ActualHeight - newAreaHeight,
                    TimeSpan.Zero));
        }
        void ExpandableListView_Loaded(object sender, RoutedEventArgs e)
        {
            _itemContainer = (FrameworkElement)Template.FindName("PART_scroller_itemsPresenter", this);
            _itemsPanel = (StackPanel)
                VisualTreeHelper.GetChild(
                VisualTreeHelper.GetChild(_itemContainer, 0), 0);
        }
        void itemPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            var target = (FrameworkElement)sender;
            target.Loaded -= itemPresenter_Loaded;
            target.SizeChanged += itemPresenter_SizeChanged;

            var newAreaHeight = 0.0;
            for (var i = IsExpand ? 0 : Math.Max(Items.Count - _displayMaxCount, 0); i < Items.Count; i++)
            {
                var container = (FrameworkElement)ItemContainerGenerator.ContainerFromIndex(i);
                newAreaHeight += container.ActualHeight;
            }

            //スクロール
            var scrollAnime = new DoubleAnimation(newAreaHeight - _itemsPanel.ActualHeight, _insertAnimationDuration);
            var heightAnime = new DoubleAnimation(newAreaHeight, _insertAnimationDuration);
            BeginAnimation(ScrollOffsetProperty, scrollAnime, HandoffBehavior.SnapshotAndReplace);
            _itemContainer.BeginAnimation(HeightProperty, heightAnime, HandoffBehavior.SnapshotAndReplace);
        }
        void itemPresenter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var newAreaHeight = 0.0;
            for (var i = IsExpand ? 0 : Math.Max(Items.Count - _displayMaxCount, 0); i < Items.Count; i++)
            {
                var container = (FrameworkElement)ItemContainerGenerator.ContainerFromIndex(i);
                newAreaHeight += container.ActualHeight;
            }
            _itemContainer.BeginAnimation(HeightProperty,
                new DoubleAnimation(newAreaHeight, TimeSpan.Zero));
            BeginAnimation(ScrollOffsetProperty,
                new DoubleAnimation(newAreaHeight - _itemsPanel.ActualHeight, TimeSpan.Zero));
        }

        public static readonly DependencyProperty IsExpandProperty = DependencyProperty.Register(
            "IsExpand", typeof(bool), typeof(ExpandableListView),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure, Changed_IsExpand));
        [System.ComponentModel.Browsable(false)]
        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register(
            "ScrollOffset",typeof(double),typeof(ExpandableListView), new PropertyMetadata(0.0, Changed_ScrollOffset));
        public static readonly DependencyProperty ShiftTranslaterProperty = DependencyProperty.Register(
            "ShiftTranslater", typeof(TranslateTransform), typeof(ExpandableListView));



        static void Changed_IsExpand(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var target = (ExpandableListView)sender;
            var duration = (Duration)TimeSpan.FromMilliseconds(500);

            if ((bool)e.NewValue)
            {
                var heightAnime = new DoubleAnimation(target._itemsPanel.ActualHeight, duration);
                var scrollAnime = new DoubleAnimation(0.0, duration);
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
                var heightAnime = new DoubleAnimation(newAreaHeight, duration);
                var scrollAnime = new DoubleAnimation(newAreaHeight - target._itemsPanel.ActualHeight, duration);
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
    public class CollapsibleStackPanel : VirtualizingPanel
    {
        public double CollapsibleHeight
        {
            get { return (double)GetValue(CollapsibleHeightProperty); }
            set { SetValue(CollapsibleHeightPropertyKey, value); }
        }
        public int DisplayableCount
        {
            get { return (int)GetValue(DisplayableCountProperty); }
            set { SetValue(DisplayableCountProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            //非表示要素を削除する
            for (var i = 0; i < InternalChildren.Count; i++)
                if (ItemContainerGenerator.GetItemContainerGeneratorForPanel(this).IndexFromContainer(InternalChildren[i]) < 0)
                {
                    RemoveInternalChildRange(i, 1);
                    i--;
                }

            var displayableCount = DisplayableCount;
            var generator = ItemContainerGenerator.GetItemContainerGeneratorForPanel(this);
            var firstVisibleItemIndex = Math.Max(generator.Items.Count - displayableCount, 0);
            var firstVisibleItemPos = ItemContainerGenerator.GeneratorPositionFromIndex(firstVisibleItemIndex);
            var childElementManageIdx = firstVisibleItemPos.Offset == 0 ? firstVisibleItemPos.Index : firstVisibleItemPos.Index + 1;
            using (ItemContainerGenerator.StartAt(firstVisibleItemPos, System.Windows.Controls.Primitives.GeneratorDirection.Forward, true))
            {
                var height = 0.0;
                var collHeight = 0.0;
                var idx = 0;
                if (InternalChildren.Count > 0)
                    RemoveInternalChildRange(0, Math.Min(Math.Max(firstVisibleItemIndex - generator.IndexFromContainer(InternalChildren[0]), 0), InternalChildren.Count));
                for (var i = firstVisibleItemIndex; i < generator.Items.Count; i++)
                {
                    bool isNewEle;
                    var item = (UIElement)ItemContainerGenerator.GenerateNext(out isNewEle);
                    if (InternalChildren.Contains(item) == false)
                    {
                        ItemContainerGenerator.PrepareItemContainer(item);
                        InsertInternalChild(idx, item);
                    }
                    item.Measure(availableSize);
                    height += item.DesiredSize.Height;
                    if (i >= generator.Items.Count - DisplayableCount)
                        collHeight += item.DesiredSize.Height;
                    idx++;
                }
                CollapsibleHeight = collHeight;

                var w = double.IsNaN(Width) ? availableSize.Width : Width;
                var h = double.IsNaN(Height) ? Math.Min(height, availableSize.Height) : Height;
                return new Size(w, h);
            }
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            var xOffset = finalSize.Height;
            var hSum = 0.0;
            for (var i = InternalChildren.Count - 1; i >= 0; i--)
            {
                UIElement item = InternalChildren[i];
                xOffset -= item.DesiredSize.Height;
                hSum += item.DesiredSize.Height;
                item.Arrange(new Rect(0, xOffset, item.DesiredSize.Width, item.DesiredSize.Height));
            }
            var w = double.IsNaN(Width) ? finalSize.Width : Width;
            var h = double.IsNaN(Height) ? Math.Min(hSum, finalSize.Height) : Height;
            return new Size(w, h);
        }

        public static readonly DependencyProperty DisplayableCountProperty = DependencyProperty.Register(
            "DisplayableCount", typeof(int), typeof(CollapsibleStackPanel),
            new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.AffectsMeasure));
        static readonly DependencyPropertyKey CollapsibleHeightPropertyKey = DependencyProperty.RegisterReadOnly(
            "CollapsibleHeight", typeof(double), typeof(CollapsibleStackPanel), new PropertyMetadata(0.0));
        public static readonly DependencyProperty CollapsibleHeightProperty = CollapsibleHeightPropertyKey.DependencyProperty;
    }

    public class IsExpandExpandableToVisibility : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;
            return (bool)values[0] && (bool)values[1] ? Visibility.Visible : Visibility.Collapsed;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
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
    public static class PlaceHolderBehavior
    {
        // プレースホルダーとして表示するテキスト
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text", typeof(string), typeof(PlaceHolderBehavior), new PropertyMetadata(null, OnPlaceHolderChanged));
        public static readonly DependencyProperty VerticalAlignmentProperty = DependencyProperty.RegisterAttached(
            "VerticalAlignment", typeof(VerticalAlignment), typeof(PlaceHolderBehavior), new PropertyMetadata(VerticalAlignment.Top, OnPlaceHolderChanged));

        static void OnPlaceHolderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            var placeHolder = e.NewValue as string;
            var handler = CreateEventHandler(placeHolder);
            if (string.IsNullOrEmpty(placeHolder))
                textBox.TextChanged -= handler;
            else
            {
                textBox.TextChanged -= handler;
                textBox.TextChanged += handler;
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Background = CreateVisualBrush(textBox, placeHolder);
                }
            }
        }
        static TextChangedEventHandler CreateEventHandler(string placeHolder)
        {
            // TextChanged イベントをハンドルし、TextBox が未入力のときだけ
            // プレースホルダーを表示するようにする。
            return (sender, e) =>
            {
                // 背景に TextBlock を描画する VisualBrush を使って
                // プレースホルダーを実現
                var textBox = (TextBox)sender;
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Background = CreateVisualBrush(textBox, placeHolder);
                }
                else
                {
                    textBox.Background = new SolidColorBrush(Colors.White);
                }
            };
        }
        static VisualBrush CreateVisualBrush(TextBox target, string placeHolder)
        {
            var align = GetVerticalAlignment(target);
            var visual = new Label()
            {
                Content = placeHolder,
                Padding = new Thickness(5, 1, 1, 1),
                Foreground = new SolidColorBrush(Colors.LightGray),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = align,
                VerticalContentAlignment = align,
            };
            visual.SetBinding(Label.BackgroundProperty, new Binding() { Source = target, Path = new PropertyPath(TextBlock.BackgroundProperty) });
            visual.SetBinding(Label.WidthProperty, new Binding() { Source = target, Path = new PropertyPath(TextBlock.ActualWidthProperty) });
            visual.SetBinding(Label.HeightProperty, new Binding() { Source = target, Path = new PropertyPath(TextBlock.ActualHeightProperty) });
            visual.Background = Brushes.White;
            return new VisualBrush(visual)
            {
                Stretch = Stretch.None,
                TileMode = TileMode.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Center,
            };
        }

        public static void SetText(TextBox textBox, string placeHolder)
        {
            textBox.SetValue(TextProperty, placeHolder);
        }
        public static string GetText(TextBox textBox)
        {
            return textBox.GetValue(TextProperty) as string;
        }
        public static void SetVerticalAlignment(TextBox target, VerticalAlignment value)
        {
            target.SetValue(VerticalAlignmentProperty, value);
        }
        public static VerticalAlignment GetVerticalAlignment(TextBox target)
        {
            return (VerticalAlignment)target.GetValue(VerticalAlignmentProperty);
        }
    }
}