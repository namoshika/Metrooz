using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.Model
{
    public class NotificationManager : IDisposable
    {
        public NotificationManager(Account account)
        {
            _account = account;
            _syncerInit = new object();
            MaxItemCount = 20;
        }
        int _isDisposed;
        object _syncerInit;
        Account _account;
        IDisposable _notificationTrigger;
        NotificationInfoContainer _notificationModel;

        public int MaxItemCount { get; set; }
        public int UnreadItemCount { get; private set; }
        public ReadOnlyCollection<NotificationInfo> Items
        { get { return _notificationModel.Notifications; } }

        public async void Initialize()
        {
            lock (_syncerInit)
            {
                _notificationModel = _account.GooglePlusClient.Notification
                    .GetNotificationContainer(NotificationsFilter.All);
                _notificationModel.Updated += _notificationModel_Updated;

                if (_notificationTrigger != null)
                {
                    _notificationModel.Updated -= _notificationModel_Updated;
                    _notificationTrigger.Dispose();
                }
                _notificationTrigger = _account.GooglePlusClient.Activity.GetStream()
                    .OfType<NotificationSignal>()
                    .Throttle(TimeSpan.FromMilliseconds(3000))
                    .Subscribe(signal => _notificationModel.UpdateAsync(false));
            }
            await _notificationModel.UpdateAsync(false);
        }
        public void MarkAllAsRead()
        {
            lock (Items)
            {
                if (Items.Count == 0)
                    return;
                _account.GooglePlusClient.Notification
                    .UpdateLastReadTimeAsync(Items.First().LatestNoticeDate);
            }
        }
        public void Dispose()
        {
            if (System.Threading.Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
                return;
            if (_notificationTrigger != null)
                _notificationTrigger.Dispose();
        }
        void _notificationModel_Updated(object sender, NotificationContainerUpdatedEventArgs e)
        {
            UnreadItemCount = _notificationModel.Notifications
                .Count(info => info.IsReaded == false);
            OnUpdated(e);
        }

        public event NotificationContainerEventHandler Updated;
        protected virtual void OnUpdated(NotificationContainerUpdatedEventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
        }
    }
}
