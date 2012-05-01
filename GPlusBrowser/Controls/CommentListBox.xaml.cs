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
            ExpandAnimationDuration = new Duration(TimeSpan.FromMilliseconds(300));
            InitializeComponent();
            itemContainer.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            itemContainer.ChangedStatus += itemContainer_ChangedStatus;
        }

        public bool IsExpand
        {
            get { return (bool)GetValue(IsExpandProperty); }
            set { SetValue(IsExpandProperty, value); }
        }
        public bool IsCommentAddMode
        {
            get { return (bool)GetValue(IsCommentAddModeProperty); }
            set { SetValue(IsCommentAddModeProperty, value); }
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

        public Duration ExpandAnimationDuration { get; set; }

        void ExpandButton_Click(object sender, RoutedEventArgs e) { IsExpand = !IsExpand; }
        void itemContainer_ChangedStatus(object sender, EventArgs e) { StartExpandAnimation(this, ExpandAnimationDuration, IsExpand); }
        void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            CommentCount = itemContainer.Items.Count;
        }

        public static readonly DependencyProperty IsExpandProperty = DependencyProperty.Register(
            "IsExpand", typeof(bool), typeof(CommentListBox), new UIPropertyMetadata(false, Changed_IsExpand));
        public static readonly DependencyProperty IsCommentAddModeProperty = DependencyProperty.Register(
            "IsCommentAddMode", typeof(bool), typeof(CommentListBox), new UIPropertyMetadata(false));
        public static readonly DependencyProperty CommentsProperty = DependencyProperty.Register(
            "Comments", typeof(System.Collections.IEnumerable), typeof(CommentListBox), new UIPropertyMetadata(null, Changed_Comments));
        public static readonly DependencyProperty CommentCountProperty = DependencyProperty.Register(
            "CommentCount", typeof(int), typeof(CommentListBox), new UIPropertyMetadata(0));

        static void Changed_IsExpand(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (CommentListBox)sender;
            StartExpandAnimation(element, element.ExpandAnimationDuration, (bool)e.NewValue);
        }
        static void Changed_Comments(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (CommentListBox)sender;
            element.itemContainer.ItemsSource = element.Comments;
        }
        static void StartExpandAnimation(CommentListBox element, Duration duration, bool isExpand)
        {
            if (isExpand)
            {
                element.BeginAnimation(
                    CommentListBox.HeightProperty,
                    new DoubleAnimation()
                    {
                        From = element.ActualHeight,
                        By = element.itemContainer.ExtendHeight - element.itemContainer.ActualHeight,
                        Duration = duration,
                        AccelerationRatio = 0.5,
                        DecelerationRatio = 0.5,
                    },
                    HandoffBehavior.SnapshotAndReplace);
            }
            else
            {
                element.BeginAnimation(
                    CommentListBox.HeightProperty,
                    new DoubleAnimation()
                    {
                        From = element.ActualHeight,
                        By = element.itemContainer.ViewportHeight - element.itemContainer.ActualHeight,
                        Duration = duration,
                        AccelerationRatio = 0.5,
                        DecelerationRatio = 0.5,
                    },
                    HandoffBehavior.SnapshotAndReplace);
            }
        }
    }
    public class ExListBox : ItemsControl
    {
        public ExListBox()
        {
            ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
        }

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

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var viewportHeight = 0.0;
            var extendHeight = 0.0;
            FrameworkElement child;
            for (var i = 0; i < Items.Count; i++)
            {
                child = (FrameworkElement)ItemContainerGenerator.ContainerFromItem(Items[Items.Count - i - 1]);
                if (child == null)
                    return base.ArrangeOverride(arrangeBounds);
                extendHeight += child.DesiredSize.Height;
                if (i >= Items.Count - 2)
                    viewportHeight += child.DesiredSize.Height;
            }

            var flg = ExtendHeight != extendHeight || ViewportHeight != viewportHeight;
            ExtendHeight = extendHeight;
            ViewportHeight = viewportHeight;
            Expandable = extendHeight != viewportHeight;
            if (flg)
                OnChangedStatus(new EventArgs());

            return base.ArrangeOverride(arrangeBounds);
        }
        void ItemContainerGenerator_ItemsChanged(
            object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e) { InvalidateArrange(); }

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
    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;
            return (bool)values ? Visibility.Visible : Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolBoolToVisibility : IMultiValueConverter
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
    public class BoolIntToString : IMultiValueConverter
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
        public static readonly DependencyProperty PlaceHolderTextProperty = DependencyProperty.RegisterAttached(
            "PlaceHolderText",
            typeof(string),
            typeof(PlaceHolderBehavior),
            new PropertyMetadata(null, OnPlaceHolderChanged));

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
            var visual = new Label()
            {
                Content = placeHolder,
                Padding = new Thickness(5, 1, 1, 1),
                Foreground = new SolidColorBrush(Colors.LightGray),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
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

        public static void SetPlaceHolderText(TextBox textBox, string placeHolder)
        {
            textBox.SetValue(PlaceHolderTextProperty, placeHolder);
        }
        public static string GetPlaceHolderText(TextBox textBox)
        {
            return textBox.GetValue(PlaceHolderTextProperty) as string;
        }
    }
}
