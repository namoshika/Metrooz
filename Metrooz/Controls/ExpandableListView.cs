using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace Metrooz.Controls
{
    public class ExpandableListView : ItemsControl
    {
        public ExpandableListView()
        {
            _miniAreaItemGetter = isExpanded => ItemContainerGenerator.Items
                .Skip(isExpanded ? 0 : Math.Max(ItemContainerGenerator.Items.Count - 2, 0))
                .Select(item => ItemContainerGenerator.ContainerFromItem(item))
                .Cast<FrameworkElement>();
        }
        static ExpandableListView()
        { DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpandableListView), new FrameworkPropertyMetadata(typeof(ExpandableListView))); }
        readonly static TimeSpan _insertAnimeDuration = TimeSpan.FromMilliseconds(100);
        readonly static TimeSpan _expandAnimeDuration = TimeSpan.FromMilliseconds(300);
        readonly Func<bool, IEnumerable<FrameworkElement>> _miniAreaItemGetter;
        int _activeAnimeCount;
        //表示領域調整用のスクロール量
        ScrollViewer _scrollViewer;
        //_scrollViewer.ExtentHeightではタイミングなどで上手く欲しい値が取得できなかった。
        //それが再現性の低いバグを生じさせていた。そのため、FullHeightとして自前のを用意する。
        public double FullHeight
        {
            get { return (double)GetValue(FullHeightProperty); }
            set { SetValue(FullHeightProperty, value); }
        }
        public double MiniHeight
        {
            get { return (double)GetValue(MiniHeightProperty); }
            set { SetValue(MiniHeightProperty, value); }
        }
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public double ScrollOffset
        {
            get { return (double)GetValue(ScrollOffsetProperty); }
            set { SetValue(ScrollOffsetProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scrollViewer = (ScrollViewer)Template.FindName("scrollViewer", this);
        }
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (_activeAnimeCount == 0)
                BeginAnimation(ScrollOffsetProperty,
                    new DoubleAnimation(IsExpanded ? 0.0 : FullHeight - MiniHeight, TimeSpan.Zero, FillBehavior.HoldEnd),
                    HandoffBehavior.SnapshotAndReplace);
            return base.ArrangeOverride(new Size(
                arrangeBounds.Width, double.IsNaN(Height) ? (IsExpanded ? FullHeight : MiniHeight) : Height));
        }
        protected override Size MeasureOverride(Size constraint)
        {
            //縮小モード時も最後の要素2個を表示する
            var desireHeight = IsExpanded ? FullHeight : MiniHeight;
            if (_activeAnimeCount == 0)
                BeginAnimation(ScrollOffsetProperty,
                    new DoubleAnimation(IsExpanded ? 0.0 : FullHeight - MiniHeight, TimeSpan.Zero, FillBehavior.HoldEnd),
                    HandoffBehavior.SnapshotAndReplace);
            return base.MeasureOverride(new Size(constraint.Width, double.IsNaN(Height) ? desireHeight : Height));
        }
        protected async override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var oldMiniHeight = ActualHeight;
                    var newMiniHeight = _miniAreaItemGetter(false)
                        .Select(container =>
                            {
                                container.UpdateLayout();
                                return container.ActualHeight;
                            })
                        .Sum();
                    var newFullHeight = _miniAreaItemGetter(true)
                        .Select(container => container.ActualHeight).Sum();

                    //高さと表示領域調整用のスクロール量を調整
                    //IsExpandedがtrueの時は常にScrollOffsetはtrueなのでスルー
                    System.Threading.Interlocked.Increment(ref _activeAnimeCount);
                    var heightTL = new DoubleAnimationUsingKeyFrames() { FillBehavior = FillBehavior.HoldEnd, AccelerationRatio = 0.0, DecelerationRatio = 1.0, };
                    heightTL.KeyFrames.Add(new LinearDoubleKeyFrame(oldMiniHeight, TimeSpan.FromMilliseconds(0)));
                    heightTL.KeyFrames.Add(new LinearDoubleKeyFrame(IsExpanded ? newFullHeight : newMiniHeight, _insertAnimeDuration));
                    heightTL.KeyFrames.Add(new DiscreteDoubleKeyFrame(double.NaN, _insertAnimeDuration));
                    BeginAnimation(HeightProperty, heightTL, HandoffBehavior.SnapshotAndReplace);
                    if (IsExpanded == false)
                        BeginAnimation(ScrollOffsetProperty, new DoubleAnimation(
                            newFullHeight - newMiniHeight, _insertAnimeDuration) { FillBehavior = FillBehavior.HoldEnd, AccelerationRatio = 0.0, DecelerationRatio = 1.0, },
                            HandoffBehavior.SnapshotAndReplace);
                    //アニメ終了後
                    await Task.Delay(_insertAnimeDuration);
                    System.Threading.Interlocked.Decrement(ref _activeAnimeCount);
                    break;
            }
            InvalidateMeasure();
            InvalidateArrange();
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(ExpandableListView), new UIPropertyMetadata(false, async (sender, e) =>
                {
                    var self = (ExpandableListView)sender;
                    System.Threading.Interlocked.Increment(ref self._activeAnimeCount);
                    var oldMiniHeight = self.ActualHeight;
                    var newMiniHeight = (bool)e.NewValue ? self.FullHeight : self.MiniHeight;
                    var heightTL = new DoubleAnimationUsingKeyFrames() { FillBehavior = FillBehavior.HoldEnd, AccelerationRatio = 0.0, DecelerationRatio = 1.0, };
                    heightTL.KeyFrames.Add(new LinearDoubleKeyFrame(oldMiniHeight, TimeSpan.FromMilliseconds(0)));
                    heightTL.KeyFrames.Add(new LinearDoubleKeyFrame(newMiniHeight, _expandAnimeDuration));
                    heightTL.KeyFrames.Add(new DiscreteDoubleKeyFrame(double.NaN, _expandAnimeDuration));
                    self.BeginAnimation(HeightProperty, heightTL, HandoffBehavior.SnapshotAndReplace);
                    self.BeginAnimation(ScrollOffsetProperty, new DoubleAnimation(
                        (bool)e.NewValue ? 0.0 : self.FullHeight - self.MiniHeight, _expandAnimeDuration) { FillBehavior = FillBehavior.HoldEnd, AccelerationRatio = 0.0, DecelerationRatio = 1.0, },
                        HandoffBehavior.SnapshotAndReplace);
                    //アニメ終了後
                    await Task.Delay(_expandAnimeDuration);
                    System.Threading.Interlocked.Decrement(ref self._activeAnimeCount);
                    self.InvalidateMeasure();
                    self.InvalidateArrange();
                }));
        public static readonly DependencyProperty MiniHeightProperty = DependencyProperty.Register(
            "MiniHeight", typeof(double), typeof(ExpandableListView), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty FullHeightProperty = DependencyProperty.Register(
            "FullHeight", typeof(double), typeof(ExpandableListView), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register(
            "ScrollOffset", typeof(double), typeof(ExpandableListView), new FrameworkPropertyMetadata(0.0, (sender, e) =>
                {
                    var self = (ExpandableListView)sender;
                    if (self._scrollViewer == null)
                        return;
                    self._scrollViewer.ScrollToVerticalOffset(Math.Max((double)e.NewValue, 0));
                }));
    }
    public class StackPanelEx : StackPanel
    {
        public StackPanelEx()
        {
            _miniAreaItemGetter = isExpanded =>
            {
                var startIdx = isExpanded ? 0 : Math.Max(InternalChildren.Count - 2, 0);
                return Enumerable
                    .Range(startIdx, InternalChildren.Count - startIdx)
                    .Select(idx => InternalChildren[idx])
                    .Cast<FrameworkElement>();
            };
            SizeChanged += StackPanelEx_SizeChanged;
        }
        readonly Func<bool, IEnumerable<FrameworkElement>> _miniAreaItemGetter;
        /// <summary>
        /// Childrenの最後の2件のみを表示した際の高さ
        /// </summary>
        public double MiniHeight
        {
            get { return (double)GetValue(MiniHeightProperty); }
            set { SetValue(MiniHeightProperty, value); }
        }
        /// <summary>
        /// Childrenの全てを表示した際の高さ
        /// </summary>
        public double FullHeight
        {
            get { return (double)GetValue(FullHeightProperty); }
            set { SetValue(FullHeightProperty, value); }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var res = base.ArrangeOverride(arrangeSize);
            MiniHeight = _miniAreaItemGetter(false).Select(ele => ele.ActualHeight).Sum();
            return res;
        }
        protected override Size MeasureOverride(Size constraint)
        {
            var res = base.MeasureOverride(constraint);
            MiniHeight = _miniAreaItemGetter(false).Select(ele => ele.DesiredSize.Height).Sum();
            return res;
        }
        void StackPanelEx_SizeChanged(object sender, SizeChangedEventArgs e)
        { FullHeight = ActualHeight; }

        public static readonly DependencyProperty MiniHeightProperty = DependencyProperty.Register(
            "MiniHeight", typeof(double), typeof(StackPanelEx), new FrameworkPropertyMetadata(0.0));
        public static readonly DependencyProperty FullHeightProperty = DependencyProperty.Register(
            "FullHeight", typeof(double), typeof(StackPanelEx), new FrameworkPropertyMetadata(0.0));
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
}
