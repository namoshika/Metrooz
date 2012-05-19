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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPlusBrowser
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

#if ENABLED_VMTEST_MODE
            DataContext = FindResource("testVm");
#else
            Loaded += MainWindow_Loaded;
#endif
        }
        Model.SettingModelManager _settingManager;
        Model.AccountManager _accountManager;
        ViewModel.AccountSwitcherViewModel _accountSwitcherVM;

        protected override void OnClosed(EventArgs e)
        {
            _accountSwitcherVM.Dispose();
            _accountManager.Dispose();
            base.OnClosed(e);
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _settingManager = new Model.SettingModelManager();
            _accountManager = new Model.AccountManager();
            _accountSwitcherVM = new ViewModel.AccountSwitcherViewModel(_accountManager, Dispatcher);
            DataContext = _accountSwitcherVM;
            _accountManager.Initialize();
        }
    }
    public static class InlineBehavior
    {
        public static System.Windows.Documents.Inline GetInline(DependencyObject obj)
        { return (System.Windows.Documents.Inline)obj.GetValue(InlineProperty); }
        public static void SetInline(DependencyObject obj, System.Windows.Documents.Inline value)
        { obj.SetValue(InlineProperty, value); }

        // Using a DependencyProperty as the backing store for Inline.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InlineProperty = DependencyProperty.RegisterAttached(
            "Inline", typeof(System.Windows.Documents.Inline), typeof(InlineBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender,
                (sender, e) =>
                {
                    var textBlock = sender as TextBlock;
                    if (textBlock == null)
                        return;
                    textBlock.Inlines.Clear();
                    textBlock.Inlines.Add((Inline)e.NewValue);
                }));


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
