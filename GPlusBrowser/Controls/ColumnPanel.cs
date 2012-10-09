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