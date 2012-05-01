﻿using System;
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
            ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            //new System.Windows.Threading.DispatcherTimer(
            //    TimeSpan.FromMilliseconds(1000), System.Windows.Threading.DispatcherPriority.Normal,
            //    (sender, e) => ArrangeOverride(DesiredSize), App.Current.Dispatcher).Start();
        }
        static ItemSelecter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemSelecter), new FrameworkPropertyMetadata(typeof(ItemSelecter)));
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            switch (ItemContainerGenerator.Status)
            {
                case System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated:
                    for (var i = 0; ; i++)
                    {
                        var content = (ContentPresenter)ItemContainerGenerator.ContainerFromIndex(i);
                        if (content == null)
                            break;
                        content.Visibility = i == SelectedIndex ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
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
                    oldContent.Visibility = Visibility.Collapsed;
            }
            if ((int)e.NewValue >= 0)
            {
                var newContent = (ContentPresenter)paperBoard.ItemContainerGenerator.ContainerFromIndex((int)e.NewValue);
                if (newContent != null)
                    newContent.Visibility = Visibility.Visible;
            }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ItemSelecter),
            new UIPropertyMetadata(-1, SelectedIndexProperty_Changed));
    }
}