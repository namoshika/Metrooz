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
    public class ColumnPanel : Panel
    {
        static ColumnPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColumnPanel), new FrameworkPropertyMetadata(typeof(ColumnPanel)));
        }

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);
            var list = new List<UIElement>();
            {
                foreach (UIElement item in Children)
                    list.Add(item);
                list.Sort(new Comparison<UIElement>(
                    (eleA, eleB) => GetOrder(eleA) - GetOrder(eleB)));
            }
            var offset = 0.0;
            foreach (var item in list)
            {
                item.Measure(new Size(constraint.Width / list.Count, constraint.Height));
                offset += item.DesiredSize.Width;
            }
            return new Size(offset, constraint.Height);
        }
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var list = new List<UIElement>();
            {
                foreach (UIElement item in Children)
                    list.Add(item);
                list.Sort(new Comparison<UIElement>(
                    (eleA, eleB) => GetOrder(eleA) - GetOrder(eleB)));
            }
            var offset = 0.0;
            foreach (var item in list)
            {
                item.Arrange(new Rect(offset, 0, item.DesiredSize.Width, arrangeBounds.Height));
                offset += item.DesiredSize.Width;
            }
            return new Size(Math.Max(offset, arrangeBounds.Width), arrangeBounds.Height + 100);
        }

        public static int GetOrder(DependencyObject obj)
        { return (int)obj.GetValue(OrderProperty); }
        public static void SetOrder(DependencyObject obj, int value)
        { obj.SetValue(OrderProperty, value); }
        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.RegisterAttached("Order", typeof(int), typeof(ColumnPanel),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
    }
}
