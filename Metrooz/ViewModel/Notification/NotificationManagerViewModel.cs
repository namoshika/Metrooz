using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;

namespace Metrooz.ViewModel
{
    using Metrooz.Model;

    public class NotificationManagerViewModel : ViewModelBase
    {
        public NotificationManagerViewModel(NotificationManager model)
        {
            _managerModel = model;
            _managerModel.RecievedSignal += _managerModel_RecievedSignal;
            Items = new ObservableCollection<NotificationStreamViewModel>();
            PropertyChanged += NotificationManagerViewModel_PropertyChanged;
        }
        readonly TimeSpan _markAsReadDelaySpan = TimeSpan.FromSeconds(3);
        NotificationManager _managerModel;
        DateTime _deactiveDate;
        bool _isActive, _existUnreadItem;
        int _unreadItemCount;
        int _selectedIndex;

        public ObservableCollection<NotificationStreamViewModel> Items { get; private set; }
        public bool IsActive
        {
            get { return _isActive; }
            set { Set(() => IsActive, ref _isActive, value); }
        }
        public bool ExistUnreadItem
        {
            get { return _existUnreadItem; }
            set { Set(() => ExistUnreadItem, ref _existUnreadItem, value); }
        }
        public int UnreadItemCount
        {
            get { return _unreadItemCount; }
            set { Set(() => UnreadItemCount, ref _unreadItemCount, value); }
        }
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { Set(() => SelectedIndex, ref _selectedIndex, value); }
        }

        void NotificationManagerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "IsActive":
                    Task.Run(async () =>
                        {
                            if (IsActive)
                            {
                                await Items.AddOnDispatcher(new NotificationStreamViewModel("新着", _managerModel.UnreadedStream)).ConfigureAwait(false);
                                await Items.AddOnDispatcher(new NotificationStreamViewModel("既読通知", _managerModel.ReadedStream)).ConfigureAwait(false);
                                SelectedIndex = 0;
                            }
                            else
                            {
                                SelectedIndex = -1;
                                var tmp = Items.ToArray();
                                await Items.ClearOnDispatcher();
                                foreach (var item in tmp)
                                    item.Cleanup();

                                //非表示化後に一定時間放置されたら既読化処理をする
                                _deactiveDate = DateTime.UtcNow;
                                await Task.Delay(_markAsReadDelaySpan).ConfigureAwait(false);
                                if (IsActive == false && DateTime.UtcNow - _deactiveDate >= _markAsReadDelaySpan)
                                {
                                    await _managerModel.AllMarkAsRead().ConfigureAwait(false);
                                    ExistUnreadItem = false;
                                    UnreadItemCount = 0;
                                }
                            }
                        });
                    break;
            }
        }
        async void _managerModel_RecievedSignal(object sender, EventArgs e)
        {
            UnreadItemCount = _managerModel.UnreadItemCount;
            ExistUnreadItem = _managerModel.UnreadItemCount > 0;
            if (_isActive == false || Items[0].IsActive == false)
                return;
            await Items[0].Update().ConfigureAwait(false);
        }
    }
    public class NotificationStreamViewModel : ViewModelBase
    {
        public NotificationStreamViewModel(string name, NotificationStream model)
        {
            _name = name;
            _streamModel = model;
            Items = new ObservableCollection<NotificationViewModel>();

            model.Items.CollectionChanged += Items_CollectionChanged;
            PropertyChanged += NotificationManagerViewModel_PropertyChanged;
        }
        readonly SemaphoreSlim _syncerItems = new SemaphoreSlim(1, 1);
        NotificationStream _streamModel;
        bool _isActive, _isLoading, _noItem;
        string _name;

        public ObservableCollection<NotificationViewModel> Items { get; private set; }
        public bool IsActive
        {
            get { return _isActive; }
            set { Set(() => IsActive, ref _isActive, value); }
        }
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(() => IsLoading, ref _isLoading, value); }
        }
        public bool NoItem
        {
            get { return _noItem; }
            set { Set(() => NoItem, ref _noItem, value); }
        }
        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }
        public async Task Update()
        {
            if (IsActive == false)
                return;
            try
            {
                IsLoading = true;
                await _streamModel.Update();
            }
            catch (FailToOperationException) { }
            finally
            {
                NoItem = _streamModel.Items.Count == 0;
                IsLoading = false;
            }
        }
        public async override void Cleanup()
        {
            base.Cleanup();
            try
            {
                await _syncerItems.WaitAsync().ConfigureAwait(false);
                _streamModel.Items.CollectionChanged -= Items_CollectionChanged;
                PropertyChanged -= NotificationManagerViewModel_PropertyChanged;

                var tmp = Items.ToArray();
                await Items.ClearOnDispatcher().ConfigureAwait(false);
                foreach (var item in tmp)
                    item.Cleanup();
            }
            finally
            { _syncerItems.Release(); }
        }
        async Task<NotificationViewModel> WrapViewModel(NotificationInfo item)
        {
            NotificationViewModel itemVM = null;
            if ((int)(item.Type & (NotificationFlag.Mension | NotificationFlag.Response | NotificationFlag.Followup | NotificationFlag.PlusOne)) > 0)
                itemVM = await NotificationWithActivityViewModel.Create((NotificationInfoWithActivity)item);
            else if ((int)(item.Type & NotificationFlag.CircleIn) > 0)
                itemVM = new NotificationWithProfileViewModel((NotificationInfoWithActor)item);
            return itemVM;
        }

        void NotificationManagerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsActive":
                    Task.Run(async () =>
                        {
                            try
                            {
                                await _syncerItems.WaitAsync().ConfigureAwait(false);
                                if (_isActive)
                                {
                                    //新しい要素として追加し直す
                                    IsLoading = true;
                                    if (_streamModel.Items.Count > 0)
                                        foreach (var item in await Task.WhenAll(_streamModel.Items.Select(inf => WrapViewModel(inf))))
                                            await Items.AddOnDispatcher(item).ConfigureAwait(false);
                                    await Update().ConfigureAwait(false);
                                }
                                else
                                {
                                    //古い要素を削除
                                    var tmp = Items.ToArray();
                                    await Items.ClearOnDispatcher();
                                    foreach (var item in tmp)
                                        item.Cleanup();
                                }
                            }
                            finally { _syncerItems.Release(); }
                        });
                    break;
            }
        }
        async void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _syncerItems.WaitAsync().ConfigureAwait(false);
                if (IsActive == false)
                    return;
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        for (var i = e.NewItems.Count - 1; i >= 0; i--)
                        {
                            var idx = e.NewStartingIndex + i;
                            var viewModel = await WrapViewModel((NotificationInfo)e.NewItems[i]);
                            await Items.InsertOnDispatcher(idx, viewModel).ConfigureAwait(false);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            var viewModel = Items[Math.Min(e.OldStartingIndex + i, Items.Count - 1)];
                            await Items.RemoveAtOnDispatcher(e.OldStartingIndex + i).ConfigureAwait(false);
                            viewModel.Cleanup();
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        await Items.MoveOnDispatcher(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        for (var i = 0; i < Items.Count; i++)
                            Items[i].Cleanup();
                        await Items.ClearOnDispatcher().ConfigureAwait(false);
                        break;
                }
            }
            finally
            { _syncerItems.Release(); }
        }
    }
}
