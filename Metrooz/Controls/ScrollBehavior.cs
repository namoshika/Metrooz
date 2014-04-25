using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interactivity;
//using Microsoft.Expression.Interactivity.Core;

namespace Metrooz.Controls
{
    public class ScrollBehavior : Behavior<VirtualizingStackPanel>
    {
        public ScrollBehavior()
        {
            _offsetCheckTimer = new System.Windows.Threading.DispatcherTimer(
                TimeSpan.FromMilliseconds(500), System.Windows.Threading.DispatcherPriority.DataBind,
                (sender, e) => SyncOffset(AssociatedObject.VerticalOffset, AssociatedObject.VerticalOffset), Dispatcher);
        }
        System.Windows.Threading.DispatcherTimer _offsetCheckTimer;
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            _offsetCheckTimer.Start();
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            _offsetCheckTimer.Stop();
        }
        void SyncOffset(double newVerticalValue, double oldVerticalValue)
        {
            if (newVerticalValue != oldVerticalValue)
                AssociatedObject.SetVerticalOffset(newVerticalValue);
            if (VerticalOffset != AssociatedObject.VerticalOffset)
                VerticalOffset = AssociatedObject.VerticalOffset;
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            "VerticalOffset", typeof(double), typeof(ScrollBehavior), new UIPropertyMetadata(0.0, OnChangedBinding));
        static void OnChangedBinding(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (ScrollBehavior)sender;
            if (behavior.AssociatedObject == null)
                return;
            behavior.SyncOffset((double)e.NewValue, (double)e.OldValue);
        }
    }
}