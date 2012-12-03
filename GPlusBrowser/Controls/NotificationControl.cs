using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Specialized;

namespace GPlusBrowser.Controls
{
    public class NotificationControl
    {
        public NotificationControl()
        {
            _isNotification = true;
            _hideWindow = new Window();
            _hideWindow.WindowStyle = WindowStyle.ToolWindow;
            _hideWindow.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            _hideWindow.Width = 0;
            _hideWindow.Height = 0;
            _hideWindow.ShowInTaskbar = false;
            _hideWindow.Show();
            _hideWindow.Visibility = Visibility.Collapsed;
            _stocks = new Queue<Window>();
            _items = new List<Notification>();
            NotificationDisplaySpan = TimeSpan.FromSeconds(5);
            Children = new ObservableCollection<UIElement>();
            Children.CollectionChanged += Children_CollectionChanged;
        }
        bool _isNotification;
        Window _hideWindow;
        Queue<Window> _stocks;
        List<Notification> _items;

        public int MaxHeight { get; private set; }
        public TimeSpan NotificationDisplaySpan { get; set; }
        public ObservableCollection<UIElement> Children { get; private set; }

        void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (UIElement item in e.NewItems)
                            {
                                var wnd = new NotificationWindow();
                                item.Measure(new Size(250, 500));
                                wnd.Content = item;
                                wnd.Owner = _hideWindow;
                                wnd.Width = 350;
                                wnd.Height = item.DesiredSize.Height;
                                wnd.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height
                                    - _items.Sum(pair => pair.Window.Height) - wnd.Height;
                                wnd.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - wnd.Width;
                                wnd.Opacity = 0.0;
                                wnd.MouseEnter += wnd_MouseEnter;
                                wnd.MouseLeave += wnd_MouseLeave;
                                wnd.Show();
                                wnd.BeginAnimation(
                                    Window.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(
                                        0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(500))));
                                var notification = new Notification(item, wnd, wnd.Top, NotificationDisplaySpan);
                                notification.LifeFinish += notification_LifeFinish;
                                if (_isNotification)
                                    notification.Begin();
                                _items.Add(notification);
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (UIElement item in e.OldItems)
                            {
                                var val = _items.First(pair => pair.Element == item);
                                var height = val.Window.RenderSize.Height;
                                val.LifeFinish -= notification_LifeFinish;
                                val.Window.MouseEnter -= wnd_MouseEnter;
                                val.Window.MouseLeave -= wnd_MouseLeave;
                                var animationSpan = TimeSpan.FromMilliseconds(500);
                                val.Window.BeginAnimation(Window.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0.0, (Duration)animationSpan));
                                Task.Delay(animationSpan).ContinueWith(tsk => App.Current.Dispatcher.InvokeAsync(val.Window.Close));
                                foreach (var bottomWnd in _items.Where(pair => val.Window.Top > pair.Window.Top))
                                {
                                    bottomWnd.Window.BeginAnimation(
                                        Window.TopProperty, new System.Windows.Media.Animation.DoubleAnimation(
                                            bottomWnd.VerticalOffset, bottomWnd.VerticalOffset + height,
                                            new Duration(TimeSpan.FromMilliseconds(500))) { DecelerationRatio = 1.0, });
                                    bottomWnd.VerticalOffset += height;
                                }
                                _items.Remove(val);
                            }
                            break;
                    }
                });
        }
        void notification_LifeFinish(object sender, EventArgs e)
        {
            var notification = (Notification)sender;
            lock (_items)
                Children.Remove(notification.Element);
        }
        void wnd_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_items.Any(item => item.Window.IsMouseOver) == false)
                return;
            Console.WriteLine("Enter");
            _isNotification = false;
            foreach (var item in _items)
                item.Pause();
        }
        void wnd_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_items.Any(item => item.Window.IsMouseOver))
                return;
            Console.WriteLine("Leave");
            _isNotification = true;
            lock (_items)
                foreach (var item in _items)
                    item.Begin();
        }

        class Notification
        {
            public Notification(UIElement item1, Window item2, double item3, TimeSpan lifeTime)
            {
                Element = item1;
                Window = item2;
                VerticalOffset = item3;
                LifeTime = lifeTime;
            }
            TimeSpan _pauseSpan;
            DateTime _pauseTime;
            DateTime _startTime;
            public NotificationStatusType Status { get; private set; }
            public TimeSpan LifeTime { get; private set; }
            public UIElement Element { get; private set; }
            public Window Window { get; private set; }
            public double VerticalOffset { get; set; }

            public void Begin()
            {
                switch (Status)
                {
                    case NotificationStatusType.Finished:
                    case NotificationStatusType.Started:
                        return;
                    case NotificationStatusType.NotStart:
                        _startTime = DateTime.UtcNow;
                        break;
                }

                _pauseSpan = _pauseSpan.Add(Status == NotificationStatusType.Paused ? DateTime.UtcNow - _pauseTime : TimeSpan.Zero);
                var span = LifeTime - (DateTime.UtcNow - _startTime) + _pauseSpan;
                Status = NotificationStatusType.Started;

                Task.Delay(span > TimeSpan.Zero ? span : TimeSpan.Zero)
                    .ContinueWith(tsk =>
                    {
                        span = LifeTime - (DateTime.UtcNow - _startTime) + _pauseSpan;
                        if (Status == NotificationStatusType.Started && span <= TimeSpan.FromMilliseconds(50))
                        {
                            Status = NotificationStatusType.Finished;
                            OnLifeFinish(new EventArgs());
                        }
                    });
            }
            public void Pause()
            {
                if (Status != NotificationStatusType.Started)
                    return;
                _pauseTime = DateTime.UtcNow;
                Status = NotificationStatusType.Paused;
            }
            public event EventHandler LifeFinish;
            protected virtual void OnLifeFinish(EventArgs e)
            {
                if (LifeFinish != null)
                    LifeFinish(this, e);
            }
        }
        enum NotificationStatusType
        { NotStart, Started, Paused, Finished }
    }
}
