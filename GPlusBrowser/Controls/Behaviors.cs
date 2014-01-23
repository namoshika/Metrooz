using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.Controls
{
    public static class InlineBehavior
    {
        public static System.Windows.Documents.Inline GetInline(DependencyObject obj)
        { return (System.Windows.Documents.Inline)obj.GetValue(InlineProperty); }
        public static void SetInline(DependencyObject obj, System.Windows.Documents.Inline value)
        { obj.SetValue(InlineProperty, value); }
        public static System.Windows.Documents.Inline PrivateConvertInlines(SunokoLibrary.Web.GooglePlus.ContentElement tree)
        {
            if (tree == null)
                return null;
            System.Windows.Documents.Inline inline = null;
            switch (tree.Type)
            {
                case ElementType.Style:
                    var styleEle = ((StyleElement)tree);
                    switch (styleEle.Style)
                    {
                        case StyleType.Bold:
                            inline = new System.Windows.Documents.Bold();
                            ((System.Windows.Documents.Bold)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                        case StyleType.Italic:
                            inline = new System.Windows.Documents.Italic();
                            ((System.Windows.Documents.Italic)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                        case StyleType.Middle:
                            inline = new System.Windows.Documents.Span();
                            inline.TextDecorations.Add(System.Windows.TextDecorations.Strikethrough);
                            ((System.Windows.Documents.Span)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                        default:
                            inline = new System.Windows.Documents.Span();
                            ((System.Windows.Documents.Span)inline).Inlines.AddRange(
                                ((StyleElement)tree).Children.Select(ele => PrivateConvertInlines(ele)));
                            break;
                    }
                    break;
                case ElementType.Hyperlink:
                    var hyperEle = (HyperlinkElement)tree;
                    var target = hyperEle.Target;
                    var hyperLink = new System.Windows.Documents.Hyperlink(new System.Windows.Documents.Run(hyperEle.Text)) { Focusable = false };
                    hyperLink.Click += (sender, e) => { System.Diagnostics.Process.Start(target.AbsoluteUri); };
                    inline = hyperLink;
                    break;
                case ElementType.Mension:
                    var spanInline = new System.Windows.Documents.Span();
                    spanInline.Inlines.AddRange(
                        new System.Windows.Documents.Inline[]
                        {
                            new System.Windows.Documents.Run("+"),
                            new System.Windows.Documents.Hyperlink(
                                new System.Windows.Documents.Run(((MensionElement)tree).Text.Substring(1)))
                                { TextDecorations = null, Focusable = false }
                        });
                    inline = spanInline;
                    break;
                case ElementType.Text:
                    inline = new System.Windows.Documents.Run(((TextElement)tree).Text);
                    break;
                case ElementType.Break:
                    inline = new System.Windows.Documents.LineBreak();
                    break;
                default:
                    throw new Exception();
            }
            return inline;
        }

        public static readonly DependencyProperty InlineProperty = DependencyProperty.RegisterAttached(
            "Inline", typeof(SunokoLibrary.Web.GooglePlus.ContentElement), typeof(InlineBehavior),
            new UIPropertyMetadata(null,
                (sender, e) =>
                {
                    var textBlock = sender as TextBlock;
                    if (e.NewValue != null)
                    {
                        textBlock.Inlines.Clear();
                        textBlock.Inlines.Add(PrivateConvertInlines((SunokoLibrary.Web.GooglePlus.ContentElement)e.NewValue));
                    }
                    else
                        textBlock.Inlines.Clear();
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
            "PlaceHolderText", typeof(string), typeof(PlaceHolderBehavior), new PropertyMetadata(null, OnPlaceHolderChanged));
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

        public static void SetPlaceHolderText(TextBox textBox, string placeHolder)
        {
            textBox.SetValue(PlaceHolderTextProperty, placeHolder);
        }
        public static string GetPlaceHolderText(TextBox textBox)
        {
            return textBox.GetValue(PlaceHolderTextProperty) as string;
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
