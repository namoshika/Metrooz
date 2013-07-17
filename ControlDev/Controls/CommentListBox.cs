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
            ExpandAnimationDuration = new Duration(TimeSpan.FromMilliseconds(200));
            ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            Loaded += CommentListBox_Loaded;
        }
        static ExpandableListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExpandableListView), new FrameworkPropertyMetadata(typeof(ExpandableListView)));
        }
        ItemsPresenter _itemContainer;
        CollapsibleStackPanel _itemPanel;

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
        public int DisplayableCount
        {
            get { return (int)GetValue(DisplayableCountProperty); }
            set { SetValue(DisplayableCountProperty, value); }
        }
        public int CommentCount
        {
            get { return (int)GetValue(CommentCountProperty); }
            set { SetValue(CommentCountProperty, value); }
        }
        public Duration ExpandAnimationDuration { get; set; }
        void CommentListBox_Loaded(object sender, RoutedEventArgs e)
        {
            _itemContainer = (ItemsPresenter)Template.FindName("itemContainer", this);
            _itemPanel = (CollapsibleStackPanel)VisualTreeHelper.GetChild(_itemContainer, 0);
            _itemPanel.RenderTransform = new TranslateTransform();
        }
        void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            var generator = ((System.Windows.Controls.Primitives.IItemContainerGenerator)ItemContainerGenerator);
            var startPos = e.Position;
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:

                    //新しい高さと上方向へのスライド量を計算
                    //新しく以下される項目数と現状維持項目、表示領域外へ押し出される項目数を計算
                    int i;
                    var height = 0.0;
                    var shiftLen = 0.0;
                    using (generator.StartAt(startPos, System.Windows.Controls.Primitives.GeneratorDirection.Forward))
                        for (i = Math.Max(e.ItemCount - DisplayableCount, 0); i < e.ItemCount; i++)
                        {
                            var ctrl = (FrameworkElement)generator.GenerateNext();
                            generator.PrepareItemContainer(ctrl);
                            ctrl.Measure(new Size(_itemPanel.ActualWidth, double.PositiveInfinity));
                            ctrl.InvalidateMeasure();
                            height += ctrl.DesiredSize.Height;
                        }
                    var newSlotLen = i;
                    int kepSlotLen = Math.Max(Math.Min(Items.Count, DisplayableCount) - newSlotLen, 0);
                    var delSlotLen = Math.Min(Items.Count - e.ItemCount, DisplayableCount) - kepSlotLen;
                    if (kepSlotLen > 0)
                        for (i = Items.Count - newSlotLen - kepSlotLen; i < Items.Count - newSlotLen; i++)
                            height += ((FrameworkElement)ItemContainerGenerator.ContainerFromIndex(i)).ActualHeight;
                    if (delSlotLen > 0)
                        for (i = Items.Count - newSlotLen - kepSlotLen - delSlotLen; i < Items.Count - newSlotLen - kepSlotLen; i++)
                            shiftLen += ((FrameworkElement)ItemContainerGenerator.ContainerFromIndex(i)).ActualHeight;


                    var board = new Storyboard();
                    var bgnTime = KeyTime.FromPercent(0.0);
                    var endTime = KeyTime.FromPercent(1.0);
                    var nowHeight = _itemContainer.ActualHeight;
                    var duration = (Duration)TimeSpan.FromMilliseconds(500);
                    board.FillBehavior = FillBehavior.Stop;

                    var aaaAnime = new Int32AnimationUsingKeyFrames();
                    aaaAnime.Duration = duration;
                    board.Children.Add(aaaAnime);
                    Storyboard.SetTarget(aaaAnime, _itemPanel);
                    Storyboard.SetTargetProperty(aaaAnime, new PropertyPath("DisplayableCount"));
                    aaaAnime.KeyFrames.Add(new DiscreteInt32KeyFrame(Math.Max(newSlotLen + kepSlotLen + delSlotLen, 3), bgnTime));
                    aaaAnime.KeyFrames.Add(new DiscreteInt32KeyFrame(Math.Max(newSlotLen + kepSlotLen, 2), endTime));

                    var translateAnime = new DoubleAnimationUsingKeyFrames();
                    board.Children.Add(translateAnime);
                    Storyboard.SetTarget(translateAnime, _itemPanel);
                    Storyboard.SetTargetProperty(translateAnime, new PropertyPath("RenderTransform.Y"));
                    translateAnime.Duration = duration;
                    translateAnime.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, bgnTime));
                    translateAnime.KeyFrames.Add(new LinearDoubleKeyFrame(-shiftLen, endTime));
                    translateAnime.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, endTime));

                    var heightAnime = new DoubleAnimationUsingKeyFrames();
                    heightAnime.Duration = duration;
                    board.Children.Add(heightAnime);
                    Storyboard.SetTarget(heightAnime, _itemContainer);
                    Storyboard.SetTargetProperty(heightAnime, new PropertyPath("Height"));
                    heightAnime.KeyFrames.Add(new LinearDoubleKeyFrame(nowHeight, bgnTime));
                    heightAnime.KeyFrames.Add(new LinearDoubleKeyFrame(height, endTime));
                    heightAnime.KeyFrames.Add(new DiscreteDoubleKeyFrame(double.NaN, endTime));

                    BeginStoryboard(board, HandoffBehavior.SnapshotAndReplace);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:

                    break;
            }
        }

        public static readonly DependencyProperty IsExpandProperty = DependencyProperty.Register(
            "IsExpand", typeof(bool), typeof(ExpandableListView),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure, Changed_IsExpand));
        public static readonly DependencyProperty ExpandableProperty = DependencyProperty.Register(
            "Expandable", typeof(bool), typeof(ExpandableListView), new PropertyMetadata(false));
        public static readonly DependencyProperty DisplayableCountProperty = DependencyProperty.Register(
            "DisplayableCount", typeof(int), typeof(ExpandableListView), new PropertyMetadata(2));
        public static readonly DependencyProperty CommentCountProperty = DependencyProperty.Register(
            "CommentCount", typeof(int), typeof(ExpandableListView), new UIPropertyMetadata(0));

        static void Changed_IsExpand(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (ExpandableListView)sender;
            var board = new Storyboard();
            var duration = (Duration)TimeSpan.FromMilliseconds(500);
            var bgnKeyTime = KeyTime.FromPercent(0.0);
            var endKeyTime = KeyTime.FromPercent(1.0);
            board.FillBehavior = FillBehavior.Stop;

            var commeLstBxAnime = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(commeLstBxAnime, element._itemContainer);
            Storyboard.SetTargetProperty(commeLstBxAnime, new PropertyPath(FrameworkElement.HeightProperty));
            board.Children.Add(commeLstBxAnime);
            commeLstBxAnime.Duration = duration;
            if ((bool)e.NewValue)
            {
                //開く時
                var beforeHeight = element._itemContainer.ActualHeight;
                element._itemPanel.DisplayableCount = int.MaxValue;
                element._itemPanel.Measure(new Size(element._itemPanel.ActualHeight, double.PositiveInfinity));
                var affterHeight = element._itemPanel.DesiredSize.Height;
                commeLstBxAnime.KeyFrames.Add(new LinearDoubleKeyFrame(beforeHeight, bgnKeyTime));
                commeLstBxAnime.KeyFrames.Add(new LinearDoubleKeyFrame(affterHeight, endKeyTime));
                commeLstBxAnime.KeyFrames.Add(new DiscreteDoubleKeyFrame(double.NaN, endKeyTime));

                var yOffset = element._itemContainer.ActualHeight - element._itemPanel.DesiredSize.Height;
                var offsetAnime = new DoubleAnimationUsingKeyFrames();
                offsetAnime.Duration = duration;
                Storyboard.SetTarget(offsetAnime, element._itemPanel);
                Storyboard.SetTargetProperty(offsetAnime, new PropertyPath("RenderTransform.Y"));
                board.Children.Add(offsetAnime);
                offsetAnime.KeyFrames.Add(new LinearDoubleKeyFrame(yOffset, bgnKeyTime));
                offsetAnime.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, endKeyTime));
            }
            else
            {
                var beforeHeight = element._itemPanel.ActualHeight;
                element._itemPanel.DisplayableCount = element.DisplayableCount;
                element._itemPanel.Measure(new Size(element._itemPanel.ActualHeight, double.PositiveInfinity));
                var affterHeight = element._itemPanel.DesiredSize.Height;
                element._itemPanel.DisplayableCount = int.MaxValue;
                commeLstBxAnime.KeyFrames.Add(new LinearDoubleKeyFrame(beforeHeight, bgnKeyTime));
                commeLstBxAnime.KeyFrames.Add(new LinearDoubleKeyFrame(affterHeight, endKeyTime));
                commeLstBxAnime.KeyFrames.Add(new DiscreteDoubleKeyFrame(double.NaN, endKeyTime));
                commeLstBxAnime.Completed += (a, b) => element._itemPanel.DisplayableCount = element.DisplayableCount;

                var yOffset = affterHeight - beforeHeight;
                var offsetAnime = new DoubleAnimationUsingKeyFrames();
                offsetAnime.Duration = duration;
                Storyboard.SetTarget(offsetAnime, element._itemPanel);
                Storyboard.SetTargetProperty(offsetAnime, new PropertyPath("RenderTransform.Y"));
                board.Children.Add(offsetAnime);
                offsetAnime.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, bgnKeyTime));
                offsetAnime.KeyFrames.Add(new LinearDoubleKeyFrame(yOffset, endKeyTime));
                offsetAnime.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, endKeyTime));
            }
            element._itemContainer.BeginStoryboard(board, HandoffBehavior.SnapshotAndReplace);
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
            var xOffset = 0.0;
            foreach (UIElement item in InternalChildren)
            {
                item.Arrange(new Rect(0, xOffset, item.DesiredSize.Width, item.DesiredSize.Height));
                xOffset += item.DesiredSize.Height;
            }
            var w = double.IsNaN(Width) ? finalSize.Width : Width;
            var h = double.IsNaN(Height) ? Math.Min(xOffset, finalSize.Height) : Height;
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