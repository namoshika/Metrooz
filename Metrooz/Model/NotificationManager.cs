using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Metrooz.Model
{
    public class NotificationManager : IDisposable
    {
        public NotificationManager(Account account) { _account = account; }
        readonly SemaphoreSlim _syncer = new SemaphoreSlim(1, 1);
        Account _account;
        IDisposable _timer;
        IDisposable _notificationTrigger;

        public int UnreadItemCount { get; private set; }
        public NotificationStream UnreadedStream { get; private set; }
        public NotificationStream ReadedStream { get; private set; }

        public async Task Activate()
        {
            await Deactivate();
            try
            {
                await _syncer.WaitAsync();
                UnreadedStream = new NotificationStream(_account.PlusClient.Notification.GetNotifications(true), _account);
                ReadedStream = new NotificationStream(_account.PlusClient.Notification.GetNotifications(false), _account);
                _timer = Observable.Timer(DateTimeOffset.MinValue, TimeSpan.FromMinutes(5))
                    .Subscribe(async obj => await Connect().ConfigureAwait(false));
            }
            finally { _syncer.Release(); }
        }
        public async Task Deactivate()
        {
            try
            {
                await _syncer.WaitAsync();
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
                if (_notificationTrigger != null)
                {
                    _notificationTrigger.Dispose();
                    _notificationTrigger = null;
                }
                UnreadedStream = null;
                ReadedStream = null;
            }
            finally { _syncer.Release(); }
        }
        public async Task<bool> AllMarkAsRead()
        {
            UnreadItemCount = 0;
            return await UnreadedStream.AllMarkAsRead();
        }
        public async void Dispose()
        {
            try
            {
                await _syncer.WaitAsync().ConfigureAwait(false);
                if (_notificationTrigger != null)
                    _notificationTrigger.Dispose();
            }
            finally { _syncer.Release(); }
        }
        async Task Connect()
        {
            try
            {
                await _syncer.WaitAsync();
                //未読通知数取得
                UnreadItemCount = await _account.PlusClient.Notification
                    .GetUnreadCount() - UnreadedStream.IgnoreItemCount;
                OnRecievedSignal(new EventArgs());

                //通知の監視をする
                if (_notificationTrigger != null)
                    return;
                _notificationTrigger = _account.PlusClient.Activity.GetStream()
                    .OfType<NotificationSignal>()
                    .Throttle(TimeSpan.FromMilliseconds(3000))
                    .Subscribe(
                        async signal =>
                        {
                            UnreadItemCount = await _account.PlusClient.Notification
                                .GetUnreadCount() - UnreadedStream.IgnoreItemCount;
                            OnRecievedSignal(new EventArgs());
                        },
                        exp => _notificationTrigger = null);
            }
            catch (FailToOperationException) { System.Diagnostics.Debug.Fail("通知APIとの通信でエラー発生。"); }
            finally { _syncer.Release(); }
        }

        public event EventHandler RecievedSignal;
        protected virtual void OnRecievedSignal(EventArgs e)
        {
            if (RecievedSignal != null)
                RecievedSignal(this, e);
        }
    }
    public class NotificationStream
    {
        public NotificationStream(NotificationInfoContainer notificationModel, Account account)
        {
            _notificationModel = notificationModel;
            _account = account;
            Status = StreamStateType.UnLoaded;
            Items = new ObservableCollection<NotificationInfo>();
        }
        readonly int _minItemCount = 10;
        readonly SemaphoreSlim _syncer = new SemaphoreSlim(1, 1);
        readonly TimeSpan _updateIntervalRegulation = TimeSpan.FromSeconds(15);
        readonly Account _account;
        readonly NotificationInfoContainer _notificationModel;
        DateTime _latestUpdateDate = DateTime.MinValue;
        public int IgnoreItemCount { get; private set; }
        public StreamStateType Status { get; private set; }
        public ObservableCollection<NotificationInfo> Items { get; private set; }

        public async Task<bool> Update()
        {
            try
            {
                await _syncer.WaitAsync();
                if (DateTime.UtcNow - _latestUpdateDate < _updateIntervalRegulation)
                {
                    Status = StreamStateType.Connected;
                    OnChangedStatus(new EventArgs());
                    return true;
                }

                _latestUpdateDate = DateTime.UtcNow;
                Status = StreamStateType.Loading;
                OnChangedStatus(new EventArgs());
                await _notificationModel.UpdateAsync(_minItemCount);
                
                //新しい通知を追加し、古い通知を削除する。
                //入れなおす手法ではなく部分的編集で済ますのは通知更新時にItemsを監視するVM側で
                //保持していた通知VMが消えないようにする事を目的としている。VMはこうしないと書いて
                //いた返信文などが定時更新で消し飛んでしまったりする
                var itemsIdx = 0;
                foreach (var srcItem in _notificationModel.Notifications)
                {
                    var srcIdx = Items.IndexOf(srcItem);
                    if (srcIdx > -1)
                        Items.Move(srcIdx, itemsIdx);
                    else
                        Items.Insert(itemsIdx, srcItem);
                    itemsIdx++;
                }
                for (var i = itemsIdx; i < Items.Count; i++)
                    Items.RemoveAt(i);

                IgnoreItemCount = _notificationModel.Notifications.Count - Items.Count;
                Status = StreamStateType.Connected;
                OnChangedStatus(new EventArgs());
                return true;
            }
            catch(FailToOperationException)
            {
                Status = StreamStateType.UnLoaded;
                OnChangedStatus(new EventArgs());

                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                return false;
            }
            finally { _syncer.Release(); }
        }
        public async Task<bool> AllMarkAsRead()
        {
            try
            {
                _latestUpdateDate = DateTime.MinValue;
                foreach (var item in Items.ToArray().Where(inf => (inf.Type & (
                    NotificationFlag.CircleIn | NotificationFlag.CircleAddBack | NotificationFlag.DirectMessage | NotificationFlag.Followup |
                    NotificationFlag.Mension | NotificationFlag.SubscriptionCommunitiy | NotificationFlag.PlusOne | NotificationFlag.Reshare | NotificationFlag.Response)) != 0))
                    await item.MarkAsReadAsync();
                return true;
            }
            catch (FailToOperationException)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                return false;
            }
        }
        public event EventHandler ChangedStatus;
        protected virtual void OnChangedStatus(EventArgs e)
        {
            if (ChangedStatus != null)
                ChangedStatus(this, e);
        }
    }
}
