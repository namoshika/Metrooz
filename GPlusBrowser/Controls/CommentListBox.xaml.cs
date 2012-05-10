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
    /// CommentListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class CommentListBox : UserControl
    {
        public CommentListBox()
        {
            ExpandAnimationDuration = new Duration(TimeSpan.FromMilliseconds(200));
            InitializeComponent();
            itemContainer.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            itemContainer.ChangedStatus += itemContainer_ChangedStatus;
        }

        public bool IsExpand
        {
            get { return (bool)GetValue(IsExpandProperty); }
            set { SetValue(IsExpandProperty, value); }
        }
        public CommentListBoxMode Mode
        {
            get { return (CommentListBoxMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }
        public int CommentCount
        {
            get { return (int)GetValue(CommentCountProperty); }
            set { SetValue(CommentCountProperty, value); }
        }
        public System.Collections.IEnumerable Comments
        {
            get { return (System.Collections.IEnumerable)GetValue(CommentsProperty); }
            set { SetValue(CommentsProperty, value); }
        }
        public string PostCommentText
        {
            get { return (string)GetValue(PostCommentTextProperty); }
            set { SetValue(PostCommentTextProperty, value); }
        }
        public ICommand PostCommentCommand
        {
            get { return (ICommand)GetValue(PostCommentCommandProperty); }
            set { SetValue(PostCommentCommandProperty, value); }
        }

        public Duration ExpandAnimationDuration { get; set; }

        void ExpandButton_Click(object sender, RoutedEventArgs e) { IsExpand = !IsExpand; }
        void itemContainer_ChangedStatus(object sender, EventArgs e) { itemContainer.StartExpandAnimation(IsExpand); }
        void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            CommentCount = itemContainer.Items.Count;
        }
        void TxtBxCommentArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mode == CommentListBoxMode.View)
                Mode = CommentListBoxMode.Write;
        }
        void BtnCommentCancel_Click(object sender, RoutedEventArgs e)
        {
            switch (Mode)
            {
                case CommentListBoxMode.Write:
                case CommentListBoxMode.Sending:
                    Mode = CommentListBoxMode.View;
                    break;
            }
        }

        public static readonly DependencyProperty IsExpandProperty = DependencyProperty.Register(
            "IsExpand", typeof(bool), typeof(CommentListBox), new UIPropertyMetadata(false, Changed_IsExpand));
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            "Mode", typeof(CommentListBoxMode), typeof(CommentListBox), new UIPropertyMetadata(CommentListBoxMode.View, Changed_Mode));
        public static readonly DependencyProperty CommentsProperty = DependencyProperty.Register(
            "Comments", typeof(System.Collections.IEnumerable), typeof(CommentListBox), new UIPropertyMetadata(null, Changed_Comments));
        public static readonly DependencyProperty CommentCountProperty = DependencyProperty.Register(
            "CommentCount", typeof(int), typeof(CommentListBox), new UIPropertyMetadata(0));
        public static readonly DependencyProperty PostCommentTextProperty = DependencyProperty.Register(
            "PostCommentText", typeof(string), typeof(CommentListBox), new UIPropertyMetadata(null));
        public static readonly DependencyProperty PostCommentCommandProperty = DependencyProperty.Register(
            "PostCommentCommand", typeof(ICommand), typeof(CommentListBox), new UIPropertyMetadata(null));

        static void Changed_IsExpand(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (CommentListBox)sender;
            element.itemContainer.StartExpandAnimation((bool)e.NewValue);
        }
        static void Changed_Comments(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (CommentListBox)sender;
            element.itemContainer.ItemsSource = element.Comments;
        }
        static void Changed_Mode(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (CommentListBox)sender;
            switch (element.Mode)
            {
                case CommentListBoxMode.View:
                    element.PostCommentText = null;
                    break;
            }
        }
    }
    public enum CommentListBoxMode { View, Write, Sending }

    public class ExListBox : ItemsControl
    {
        public ExListBox() { ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged; }
        bool _measureExtendHeightFlg = true;
        public bool Expandable
        {
            get { return (bool)GetValue(ExpandableProperty); }
            set { SetValue(ExpandableProperty, value); }
        }
        public double ExtendHeight
        {
            get { return (double)GetValue(ExtendHeightProperty); }
            set { SetValue(ExtendHeightProperty, value); }
        }
        public double ViewportHeight
        {
            get { return (double)GetValue(ViewportHeightProperty); }
            set { SetValue(ViewportHeightProperty, value); }
        }

        public void StartExpandAnimation(bool isExpand)
        {
            if (isExpand)
            {
                BeginAnimation(
                    CommentListBox.HeightProperty,
                    new DoubleAnimation(ActualHeight, ExtendHeight, new Duration(TimeSpan.FromMilliseconds(250)))
                    {
                        AccelerationRatio = 0.1,
                        DecelerationRatio = 0.9,
                    }, HandoffBehavior.SnapshotAndReplace);
            }
            else
            {
                BeginAnimation(
                    CommentListBox.HeightProperty,
                    new DoubleAnimation(ActualHeight, ViewportHeight, new Duration(TimeSpan.FromMilliseconds(250)))
                    {
                        AccelerationRatio = 0.1,
                        DecelerationRatio = 0.9,
                    }, HandoffBehavior.SnapshotAndReplace);
            }
        }
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var size = base.ArrangeOverride(arrangeBounds);

            if (_measureExtendHeightFlg)
            {
                var viewportHeight = 0.0;
                var extendHeight = 0.0;
                FrameworkElement child;
                for (var i = 0; i < Items.Count; i++)
                {
                    child = (FrameworkElement)ItemContainerGenerator.ContainerFromIndex(i);
                    if (child == null)
                        //childがnullだったら今回は諦めて次回以降に計算する
                        return size;

                    extendHeight += child.DesiredSize.Height;
                    if (i >= Items.Count - 2 || Items.Count <= 3)
                        viewportHeight += child.DesiredSize.Height;
                }

                var flg = ExtendHeight != extendHeight || ViewportHeight != viewportHeight;
                ExtendHeight = extendHeight;
                ViewportHeight = viewportHeight;
                Expandable = extendHeight != viewportHeight;
                _measureExtendHeightFlg = false;
                if (flg)
                    OnChangedStatus(new EventArgs());
            }

            return size;
        }
        protected override DependencyObject GetContainerForItemOverride()
        {
            var element = new ContentPresenter();
            element.RenderTransform = new TranslateTransform();
            element.SizeChanged += element_SizeChanged;
            element.Loaded += element_Loaded;
            return element;
        }
        void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            _measureExtendHeightFlg = true;
            InvalidateArrange();
        }
        void element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _measureExtendHeightFlg = true;
            InvalidateArrange();
        }
        void element_Loaded(object sender, RoutedEventArgs e)
        {
            var element = (ContentPresenter)sender;
            var duration = new Duration(TimeSpan.FromMilliseconds(250));
            var storyboard = new Storyboard();

            FrameworkElement child;
                for (var i = 0; i < Items.Count; i++)
                {
                    child = (ContentPresenter)ItemContainerGenerator.ContainerFromIndex(i);
                    if (child == null)
                        break;
                    var childAnime = new DoubleAnimation(element.ActualHeight, 0, duration);
                    childAnime.BeginTime = new TimeSpan();
                    childAnime.AccelerationRatio = 0.1;
                    childAnime.DecelerationRatio = 0.9;
                    Storyboard.SetTarget(childAnime, child);
                    Storyboard.SetTargetProperty(childAnime, new PropertyPath("RenderTransform.Y"));
                    storyboard.Children.Add(childAnime);
                }
            BeginStoryboard(storyboard, HandoffBehavior.SnapshotAndReplace, true);
        }

        public event EventHandler ChangedStatus;
        protected virtual void OnChangedStatus(EventArgs e)
        {
            if (ChangedStatus != null)
                ChangedStatus(this, e);
        }

        public static readonly DependencyProperty ExpandableProperty = DependencyProperty.Register(
            "Expandable", typeof(bool), typeof(ExListBox), new UIPropertyMetadata(false));
        public static readonly DependencyProperty ExtendHeightProperty = DependencyProperty.Register(
            "ExtendHeight", typeof(double), typeof(ExListBox), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty ViewportHeightProperty = DependencyProperty.Register(
            "ViewportHeight", typeof(double), typeof(ExListBox), new UIPropertyMetadata(0.0));
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