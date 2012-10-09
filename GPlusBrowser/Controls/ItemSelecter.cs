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

namespace GPlusBrowser.Controls
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:GPlusBrowser.Controls"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:GPlusBrowser.Controls;assembly=GPlusBrowser.Controls"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:PaperBoard/>
    ///
    /// </summary>
    public class ItemSelecter : ItemsControl
    {
        public ItemSelecter()
        {
            ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(Grid)));
        }
        static ItemSelecter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ItemSelecter), new FrameworkPropertyMetadata(typeof(ItemSelecter)));
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    for (var i = 0; ; i++)
                    {
                        var content = (ContentPresenter)ItemContainerGenerator.ContainerFromIndex(i);
                        if (content == null)
                            break;
                        content.Visibility = i == SelectedIndex ? Visibility.Visible : Visibility.Hidden;
                    }
                    break;
            }
        }
        static void SelectedIndexProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var paperBoard = (ItemSelecter)sender;
            if ((int)e.OldValue >= 0)
            {
                var oldContent = (ContentPresenter)paperBoard.ItemContainerGenerator.ContainerFromIndex((int)e.OldValue);
                if (oldContent != null)
                    oldContent.Visibility = Visibility.Hidden;
            }
            if ((int)e.NewValue >= 0)
            {
                var newContent = (ContentPresenter)paperBoard.ItemContainerGenerator.ContainerFromIndex((int)e.NewValue);
                if (newContent != null)
                    newContent.Visibility = Visibility.Visible;
                paperBoard.SelectedItem = paperBoard.Items[(int)e.NewValue];
            }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ItemSelecter),
            new UIPropertyMetadata(-1, SelectedIndexProperty_Changed));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(object), typeof(ItemSelecter), new UIPropertyMetadata(null));
    }
}
