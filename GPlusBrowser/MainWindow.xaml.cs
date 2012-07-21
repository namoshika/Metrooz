using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
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
            _sizeChangedTrigger = new System.Reactive.Subjects.Subject<EventPattern<EventArgs>>();
            
            Loaded += MainWindow_Loaded;
            Observable.FromEventPattern(this, "SizeChanged")
                .Merge(_sizeChangedTrigger)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(Dispatcher)
                .Subscribe(MainWindow_SizeChanged);
        }
        System.Reactive.Subjects.Subject<EventPattern<EventArgs>> _sizeChangedTrigger;
        Model.SettingModelManager _settingManager;
        Model.AccountManager _accountManager;
        ViewModel.PageSwitcherViewModel _accountSwitcherVM;
        public bool NowResizeAnimation
        {
            get { return (bool)GetValue(NowResizeAnimationProperty); }
            set { SetValue(NowResizeAnimationProperty, value); }
        }

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
            _accountSwitcherVM = new ViewModel.PageSwitcherViewModel(_accountManager, Dispatcher);
            DataContext = _accountSwitcherVM;
            _accountManager.Initialize();
        }
        void MainWindow_SizeChanged(EventPattern<EventArgs> e)
        {
            //_accountManager.Accounts[0].Initialize();
            //foreach (var item in _accountManager.Accounts[0].Stream.DisplayStreams)
            //    item.Refresh();
            var args = (SizeChangedEventArgs)e.EventArgs;
            if (args == null || args.WidthChanged)
            {
                if (NowResizeAnimation)
                    return;
                mainPane.Width = ActualWidth
                    - SystemParameters.ResizeFrameHorizontalBorderHeight * 2
                    - (((ViewModel.PageSwitcherViewModel)DataContext).IsShowSidePanel ? sidePane.ActualWidth : 0.0);
            }
        }

        public static readonly DependencyProperty NowResizeAnimationProperty = DependencyProperty.Register(
            "NowResizeAnimation", typeof(bool), typeof(MainWindow),
            new UIPropertyMetadata(false, (sender, e) =>
                ((MainWindow)sender)._sizeChangedTrigger.OnNext(new EventPattern<EventArgs>(sender, null))));
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
    public static class PasswordHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password",
            typeof(string), typeof(PasswordHelper),
            new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",
            typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, Attach));
        private static readonly DependencyProperty IsUpdatingProperty =
           DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
           typeof(PasswordHelper));

        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }
        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(PasswordProperty);
        }
        public static void SetPassword(DependencyObject dp, string value)
        {
            dp.SetValue(PasswordProperty, value);
        }
        static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }
        static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }
        static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            passwordBox.PasswordChanged -= PasswordChanged;

            if (!(bool)GetIsUpdating(passwordBox))
            {
                passwordBox.Password = (string)e.NewValue;
            }
            passwordBox.PasswordChanged += PasswordChanged;
        }
        static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;

            if (passwordBox == null)
                return;

            if ((bool)e.OldValue)
            {
                passwordBox.PasswordChanged -= PasswordChanged;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }
        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            SetIsUpdating(passwordBox, true);
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }
    }

    //コピペ元
    //http://d.hatena.ne.jp/griefworker/20100929/textbox_placeholder
    public static class PlaceHolderBehavior
    {
        // プレースホルダーとして表示するテキスト
        public static readonly DependencyProperty PlaceHolderTextProperty = DependencyProperty.RegisterAttached(
            "PlaceHolderText",　typeof(string),　typeof(PlaceHolderBehavior),
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

    //コピペ元
    //http://stackoverflow.com/questions/1083224/pushing-read-only-gui-properties-back-into-viewmodel
    public static class DataPiping
    {
        public static void SetDataPipes(DependencyObject o, DataPipeCollection value)
        {
            o.SetValue(DataPipesProperty, value);
        }
        public static DataPipeCollection GetDataPipes(DependencyObject o)
        {
            return (DataPipeCollection)o.GetValue(DataPipesProperty);
        }
        public static readonly DependencyProperty DataPipesProperty = DependencyProperty.RegisterAttached(
            "DataPipes", typeof(DataPipeCollection), typeof(DataPiping), new UIPropertyMetadata(null));
    }
    public class DataPipeCollection : FreezableCollection<DataPipe> { }
    public class DataPipe : Freezable
    {
        public object Target
        {
            get { return (object)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }
        public object Source
        {
            get { return (object)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            Target = e.NewValue;
        }

        static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        { ((DataPipe)d).OnSourceChanged(e); }
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            "Target", typeof(object), typeof(DataPipe), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(object), typeof(DataPipe),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSourceChanged)));

        protected override Freezable CreateInstanceCore() { return new DataPipe(); }
    }
}
