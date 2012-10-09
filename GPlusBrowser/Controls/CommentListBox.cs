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
        }
        static CommentListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CommentListBox), new FrameworkPropertyMetadata(typeof(CommentListBox)));
        }
        ExListBox itemContainer;

        public bool IsExpand
        {
            get { return (bool)GetValue(IsExpandProperty); }
            set { SetValue(IsExpandProperty, value); }
        }
        public bool IsWritable
        {
            get { return (bool)GetValue(IsWritableProperty); }
            set { SetValue(IsWritableProperty, value); }
        }
        public bool IsWriteMode
        {
            get { return (bool)GetValue(IsWriteModeProperty); }
            set { SetValue(IsWriteModeProperty, value); }
        }
        public bool IsEnableAnimation
        {
            get { return (bool)GetValue(IsEnableAnimationProperty); }
            set { SetValue(IsEnableAnimationProperty, value); }
        }
        public int CommentCount
        {
            get { return (int)GetValue(CommentCountProperty); }
            set { SetValue(CommentCountProperty, value); }
        }
        public System.Collections.IEnumerable ItemsSource
        {
            get { return (System.Collections.IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public string PostText
        {
            get { return (string)GetValue(PostTextProperty); }
            set { SetValue(PostTextProperty, value); }
        }
        public ICommand PostCommand
        {
            get { return (ICommand)GetValue(PostCommandProperty); }
            set { SetValue(PostCommandProperty, value); }
        }
        public Duration ExpandAnimationDuration { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            itemContainer = (ExListBox)Template.FindName("itemContainer", this);
            itemContainer.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            itemContainer.ChangedStatus += itemContainer_ChangedStatus;
        }
        void itemContainer_ChangedStatus(object sender, EventArgs e) { itemContainer.StartExpandAnimation(IsExpand); }
        void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            CommentCount = itemContainer.Items.Count;
        }

        public static readonly DependencyProperty IsExpandProperty = DependencyProperty.Register(
            "IsExpand", typeof(bool), typeof(CommentListBox), new UIPropertyMetadata(false, Changed_IsExpand));
        public static readonly DependencyProperty IsWritableProperty = DependencyProperty.Register(
            "IsWritable", typeof(bool), typeof(CommentListBox), new UIPropertyMetadata(false));
        public static readonly DependencyProperty IsWriteModeProperty = DependencyProperty.Register(
            "IsWriteMode", typeof(bool), typeof(CommentListBox), new UIPropertyMetadata(false, Changed_IsWriteMode));
        public static readonly DependencyProperty IsEnableAnimationProperty = DependencyProperty.Register(
            "IsEnableAnimation", typeof(bool), typeof(CommentListBox), new UIPropertyMetadata(false));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(System.Collections.IEnumerable), typeof(CommentListBox), new UIPropertyMetadata(null));
        public static readonly DependencyProperty CommentCountProperty = DependencyProperty.Register(
            "CommentCount", typeof(int), typeof(CommentListBox), new UIPropertyMetadata(0));
        public static readonly DependencyProperty PostTextProperty = DependencyProperty.Register(
            "PostText", typeof(string), typeof(CommentListBox), new UIPropertyMetadata(null));
        public static readonly DependencyProperty PostCommandProperty = DependencyProperty.Register(
            "PostCommand", typeof(ICommand), typeof(CommentListBox), new UIPropertyMetadata(null));

        static void Changed_IsExpand(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (CommentListBox)sender;
            element.itemContainer.StartExpandAnimation((bool)e.NewValue);
        }
        static void Changed_IsWriteMode(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (CommentListBox)sender;
            if ((bool)e.NewValue == false)
                element.PostText = null;
        }
    }
    public class ExListBox : ItemsControl
    {
        public ExListBox()
        {
            ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            _translateTransformer = new TranslateTransform();
        }
        bool _measureExtendHeightFlg = true;
        TranslateTransform _translateTransformer;
        public bool IsEnableAnimation
        {
            get { return (bool)GetValue(IsEnableAnimationProperty); }
            set { SetValue(IsEnableAnimationProperty, value); }
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

        public void StartExpandAnimation(bool isExpand)
        {
            if (isExpand)
            {
                if (IsEnableAnimation)
                    BeginAnimation(
                        ExListBox.HeightProperty,
                        new DoubleAnimation(ActualHeight, ExtendHeight, new Duration(TimeSpan.FromMilliseconds(250)))
                        {
                            AccelerationRatio = 0.0,
                            DecelerationRatio = 1.0,
                        }, HandoffBehavior.SnapshotAndReplace);
                else
                    Height = ExtendHeight;

            }
            else
            {
                if (IsEnableAnimation)
                    BeginAnimation(
                        CommentListBox.HeightProperty,
                        new DoubleAnimation(ActualHeight, ViewportHeight, new Duration(TimeSpan.FromMilliseconds(250)))
                        {
                            AccelerationRatio = 0.1,
                            DecelerationRatio = 0.9,
                        }, HandoffBehavior.SnapshotAndReplace);
                else
                    Height = ViewportHeight;
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
            element.RenderTransform = _translateTransformer;
            element.SizeChanged += element_SizeChanged;
            element.Loaded += element_Loaded;
            element.Unloaded += element_Unloaded;
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
            element.Loaded += element_Loaded;

            if (IsEnableAnimation)
            {
                _translateTransformer.BeginAnimation(
                    TranslateTransform.YProperty,
                    new DoubleAnimation(element.ActualHeight, 0, duration)
                    {
                        AccelerationRatio = 0.0,
                        DecelerationRatio = 1.0
                    }, HandoffBehavior.SnapshotAndReplace);
            }
        }
        void element_Unloaded(object sender, RoutedEventArgs e)
        {
            var element = (ContentPresenter)sender;
            element.SizeChanged += element_SizeChanged;
            element.Unloaded += element_Unloaded;
        }

        public event EventHandler ChangedStatus;
        protected virtual void OnChangedStatus(EventArgs e)
        {
            if (ChangedStatus != null)
                ChangedStatus(this, e);
        }

        public static readonly DependencyProperty IsEnableAnimationProperty = DependencyProperty.Register(
            "IsEnableAnimation", typeof(bool), typeof(ExListBox), new UIPropertyMetadata(false));
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