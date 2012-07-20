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

        public bool IsError { get; private set; }
        public int MaxItemCount { get; set; }
        public int UnreadItemCount { get; private set; }
        public ReadOnlyCollection<NotificationInfo> Items { get; private set; }

        public void Initialize()
        {
            lock (_syncerInit)
            {
                if (_notificationModel != null)
                    _notificationModel.Updated -= _notificationModel_Updated;

                _notificationModel = _account.GooglePlusClient.Notification
                    .GetNotificationContainer(NotificationsFilter.All);
                _notificationModel.Updated += _notificationModel_Updated;
                Connect();
            }
            Update();
        }
        public async void Update()
        {
            IsError = false;
            OnChangedIsError(new EventArgs());
            try
            { await _notificationModel.UpdateAsync(false); }
            catch (FailToOperationException)
            {
                IsError = true;
                OnChangedIsError(new EventArgs());
            }
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
        public void Connect()
        {
            if (_notificationTrigger != null)
                _notificationTrigger.Dispose();

            _notificationTrigger = _account.GooglePlusClient.Activity.GetStream()
                .OfType<NotificationSignal>()
                .Throttle(TimeSpan.FromMilliseconds(3000))
                .Subscribe(signal => Update());
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

        public event EventHandler ChangedIsError;
        protected virtual void OnChangedIsError(EventArgs e)
        {
            if (ChangedIsError != null)
                ChangedIsError(this, e);
        }
        public event NotificationContainerEventHandler Updated;
        protected virtual void OnUpdated(NotificationContainerUpdatedEventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
        }
    }
}
