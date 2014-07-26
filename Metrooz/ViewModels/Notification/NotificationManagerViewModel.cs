using Livet;
using Livet.EventListeners;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Metrooz.ViewModels
{
    using Metrooz.Models;

    public class NotificationManagerViewModel : ViewModel
    {
        public NotificationManagerViewModel(NotificationManager model)
        {
            _managerModel = model;
            Items = new DispatcherCollection<NotificationStreamViewModel>(App.Current.Dispatcher);
            CompositeDisposable.Add(_modelPropChangedEventListener = new PropertyChangedEventListener(model));
            CompositeDisposable.Add(_thisPropChangedEventListener = new PropertyChangedEventListener(this));

            _modelPropChangedEventListener.Add(() => model.UnreadItemCount, UnreadItemCount_PropertyChanged);
            _thisPropChangedEventListener.Add(() => IsActive, IsActive_PropertyChanged);
        }
        readonly PropertyChangedEventListener _modelPropChangedEventListener;
        readonly PropertyChangedEventListener _thisPropChangedEventListener;
        readonly TimeSpan _markAsReadDelaySpan = TimeSpan.FromSeconds(3);
        NotificationManager _managerModel;
        DateTime _deactiveDate;
        bool _isActive, _existUnreadItem;
        int _unreadItemCount;
        int _selectedIndex;

        public DispatcherCollection<NotificationStreamViewModel> Items { get; private set; }
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                RaisePropertyChanged(() => IsActive);
            }
        }
        public bool ExistUnreadItem
        {
            get { return _existUnreadItem; }
            set
            {
                _existUnreadItem = value;
                RaisePropertyChanged(() => ExistUnreadItem);
            }
        }
        public int UnreadItemCount
        {
            get { return _unreadItemCount; }
            set
            {
                _unreadItemCount = value;
                RaisePropertyChanged(() => UnreadItemCount);
            }
        }
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged(() => SelectedIndex);
            }
        }

        async void IsActive_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (IsActive)
            {
                Items.Add(new NotificationStreamViewModel("新着", _managerModel.UnreadedStream));
                Items.Add(new NotificationStreamViewModel("既読通知", _managerModel.ReadedStream));
                SelectedIndex = 0;
            }
            else
            {
                SelectedIndex = -1;
                foreach (var item in Items)
                    item.Dispose();
                Items.Clear();

                //非表示化後に一定時間放置されたら既読化処理をする
                _deactiveDate = DateTime.UtcNow;
                await Task.Delay(_markAsReadDelaySpan).ConfigureAwait(false);
                if (IsActive == false && DateTime.UtcNow - _deactiveDate >= _markAsReadDelaySpan)
                {
                    //起動時に_managerModel.Activate()されていない状態で呼び出され、
                    //UnreadedStream == nullで例外が発生する。その対策としてif()を挟む。
                    if (_managerModel.UnreadedStream != null)
                        await _managerModel.AllMarkAsRead().ConfigureAwait(false);
                    ExistUnreadItem = false;
                    UnreadItemCount = 0;
                }
            }
        }
        async void UnreadItemCount_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UnreadItemCount = _managerModel.UnreadItemCount;
            ExistUnreadItem = _managerModel.UnreadItemCount > 0;
            if (_isActive == false || Items[0].IsActive == false)
                return;
            await Items[0].Update();
        }
    }
    public class NotificationStreamViewModel : ViewModel
    {
        public NotificationStreamViewModel(string name, NotificationStream model)
        {
            _streamModel = model;
            Name = name;
            CompositeDisposable.Add(_modelPropChangedEventListener = new PropertyChangedEventListener(model));
            CompositeDisposable.Add(_thisPropChangedEventListener = new PropertyChangedEventListener(this));
            CompositeDisposable.Add(Observable.Interval(TimeSpan.FromSeconds(60)).Subscribe(Interval_Fired));
            CompositeDisposable.Add(Items = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                _streamModel.Items, item => WrapViewModel(item, DateTime.UtcNow), App.Current.Dispatcher));

            _modelPropChangedEventListener.Add(() => model.Status, Status_ChangedProperty);
            _thisPropChangedEventListener.Add(() => IsActive, IsActive_PropertyChanged);
        }
        readonly PropertyChangedEventListener _modelPropChangedEventListener;
        readonly PropertyChangedEventListener _thisPropChangedEventListener;
        readonly SemaphoreSlim _syncerItems = new SemaphoreSlim(1);
        NotificationStream _streamModel;
        ReadOnlyDispatcherCollection<NotificationViewModel> _items;
        int _isActive, _isLoading, _noItem;

        public string Name { get; private set; }
        public ReadOnlyDispatcherCollection<NotificationViewModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged(() => Items);
            }
        }
        public bool IsActive
        {
            get { return _isActive == 1; }
            set
            {
                _isActive = value ? 1 : 0;
                RaisePropertyChanged(() => IsActive);
            }
        }
        public bool IsLoading
        {
            get { return _isLoading == 1; }
            set
            {
                _isLoading = value ? 1 : 0;
                RaisePropertyChanged(() => IsLoading);
            }
        }
        public bool NoItem
        {
            get { return _noItem == 1; }
            set
            {
                _noItem = value ? 1 : 0;
                RaisePropertyChanged(() => NoItem);
            }
        }
        public async Task Update()
        {
            if (IsLoading)
                return;
            if (await _streamModel.Update() == false)
                return;
            NoItem = _streamModel.Items.Count == 0;
        }
        NotificationViewModel WrapViewModel(NotificationInfo item, DateTime insertTime)
        {
            if (item is NotificationInfoWithActivity)
            {
                var itemInf = (NotificationInfoWithActivity)item;
                var itemVm = new NotificationWithActivityViewModel(itemInf, new Activity(itemInf.Activity), insertTime);
                itemVm.Activate();
                return itemVm;
            }
            else if (item is NotificationInfoWithActor)
                return new NotificationWithProfileViewModel((NotificationInfoWithActor)item, insertTime);
            else
                return new OtherTypeNotificationViewModel(item, insertTime, "未対応通知。ブラウザで確認して下さい。", false);
            throw new ArgumentException("引数itemに適用できるVMが存在しません。");
        }

        async void IsActive_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (IsActive == false)
                return;
            await Task.Run(async () => await Update());
        }
        async void Interval_Fired(long count)
        {
            if (IsActive == false)
                return;
            await Task.Run(async () => await Update());
        }
        void Status_ChangedProperty(object sender, EventArgs e)
        { IsLoading = _streamModel.Status == StreamStateType.Initing; }
    }
}
