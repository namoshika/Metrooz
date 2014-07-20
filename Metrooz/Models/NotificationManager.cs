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

namespace Metrooz.Models
{
    public class NotificationManager : Livet.NotificationObject
    {
        public NotificationManager(Account account) { _account = account; }
        readonly SemaphoreSlim _syncer = new SemaphoreSlim(1);
        int _unreadItemCount;
        Account _account;
        IDisposable _notificationTrigger;

        public int UnreadItemCount
        {
            get { return _unreadItemCount; }
            set
            {
                if (_unreadItemCount == value)
                    return;
                _unreadItemCount = value;
                RaisePropertyChanged();
            }
        }
        public NotificationStream UnreadedStream { get; private set; }
        public NotificationStream ReadedStream { get; private set; }

        public async Task Activate()
        {
            try
            {
                await _syncer.WaitAsync();
                if (_notificationTrigger != null)
                    return;

                //通知の監視をする。
                //  Timerの5分おきの処理とGetStream()の通知更新シグナルの受信をMerge()したものを使う。
                //  GetStream()はDefer().Retry()で例外発生時に再接続させる。間にDelaySubscription()を
                //  挟んで再接続の間に1分間の間隔を開ける。初回接続も1分間遅延するが気にしない。
                _notificationTrigger = Observable.Merge(
                        Observable.Interval(TimeSpan.FromMinutes(5)).Select(num => Unit.Default),
                        Observable.Defer(() => _account.PlusClient.Activity.GetStream().OfType<NotificationSignal>().Select(sig => Unit.Default))
                            .DelaySubscription(TimeSpan.FromMinutes(1))
                            .Retry())
                    .Throttle(TimeSpan.FromMinutes(1))
                    .Subscribe(unit => UpdateUnreadItemCount());
                
                UnreadedStream = new NotificationStream(_account.PlusClient.Notification.GetNotifications(true), _account);
                ReadedStream = new NotificationStream(_account.PlusClient.Notification.GetNotifications(false), _account);
                UpdateUnreadItemCount();
            }
            finally { _syncer.Release(); }
        }
        public async Task Deactivate()
        {
            try
            {
                await _syncer.WaitAsync();
                if (_notificationTrigger != null)
                {
                    _notificationTrigger.Dispose();
                    _notificationTrigger = null;
                }
            }
            finally { _syncer.Release(); }
        }
        public async Task<bool> AllMarkAsRead()
        {
            UnreadItemCount = 0;
            return await UnreadedStream.AllMarkAsRead();
        }
        async void UpdateUnreadItemCount()
        {
            try { UnreadItemCount = await _account.PlusClient.Notification.GetUnreadCount(); }
            catch (FailToOperationException)
            { System.Diagnostics.Debug.Fail("通知APIとの通信でエラー発生。"); }
        }
    }
    public class NotificationStream : Livet.NotificationObject
    {
        public NotificationStream(NotificationInfoContainer notificationModel, Account account)
        {
            _notificationModel = notificationModel;
            _account = account;
            _status = StreamStateType.UnLoaded;
            Items = new ObservableCollection<NotificationInfo>();
        }
        readonly int _minItemCount = 10;
        readonly SemaphoreSlim _syncer = new SemaphoreSlim(1, 1);
        readonly TimeSpan _updateIntervalRegulation = TimeSpan.FromSeconds(15);
        readonly Account _account;
        readonly NotificationInfoContainer _notificationModel;
        DateTime _latestUpdateDate = DateTime.MinValue;
        StreamStateType _status;

        public StreamStateType Status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                    return;
                _status = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<NotificationInfo> Items { get; private set; }

        public async Task<bool> Update()
        {
            try
            {
                await _syncer.WaitAsync();
                if (DateTime.UtcNow - _latestUpdateDate < _updateIntervalRegulation)
                {
                    Status = StreamStateType.Connected;
                    return true;
                }

                _latestUpdateDate = DateTime.UtcNow;
                Status = StreamStateType.Initing;
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
                Status = StreamStateType.Connected;
                return true;
            }
            catch(FailToOperationException)
            {
                Status = StreamStateType.UnLoaded;
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
            catch (FailToOperationException) { return false; }
        }
    }
}
