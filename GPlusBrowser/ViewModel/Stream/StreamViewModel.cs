using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class StreamViewModel : ViewModelBase
    {
        public StreamViewModel(Stream circle, Account account, StreamManager manager)
        {
            _circleModel = circle;
            _name = circle.Name;
            _circleManagerModel = manager;
            _accountModel = account;
            _isConnected = true;
            _lastConnectDate = DateTime.MaxValue;
            _activities = new ObservableCollection<ActivityViewModel>();
            ReconnectCommand = new RelayCommand(ReconnectCommand_Executed);

            _circleModel.ChangedStatus += _circleModel_ChangedStatus;
            PropertyChanged += StreamViewModel_PropertyChanged;
        }
        readonly System.Threading.SemaphoreSlim _syncerActivities = new System.Threading.SemaphoreSlim(1, 1);
        bool _isActive, _isLoading, _isIniting, _isConnected;
        string _name;
        Stream _circleModel;
        Account _accountModel;
        StreamManager _circleManagerModel;
        ObservableCollection<ActivityViewModel> _activities;
        DateTime _lastConnectDate;

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
        public bool IsIniting
        {
            get { return _isIniting; }
            set { Set(() => IsIniting, ref _isIniting, value); }
        }
        public bool IsConnected
        {
            get { return _isConnected; }
            set { Set(() => IsConnected, ref _isConnected, value); }
        }
        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }
        public ObservableCollection<ActivityViewModel> Activities
        {
            get { return _activities; }
            set { Set(() => Activities, ref _activities, value); }
        }
        public ICommand ReconnectCommand { get; private set; }

        public async Task Activate()
        {
            if (_circleModel.Status < StreamStateType.Loading)
                try { await _circleModel.Connect(); }
                catch (FailToOperationException) { }
        }
        public override void Cleanup()
        {
            base.Cleanup();
            PropertyChanged -= StreamViewModel_PropertyChanged;
            foreach (var item in _activities)
                item.Cleanup();
        }

        void StreamViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsActive":
                    Task.Run(async () =>
                        {
                            try
                            {
                                await _syncerActivities.WaitAsync().ConfigureAwait(false);
                                //古い要素を削除
                                _circleModel.Activities.CollectionChanged -= _circleModel_ActivitiesCollectionChanged;
                                foreach (var item in _activities)
                                    item.Cleanup();
                                await _activities.ClearOnDispatcher().ConfigureAwait(false);

                                //新しい要素を追加
                                if (_isActive == false)
                                    return;
                                _circleModel.Activities.CollectionChanged += _circleModel_ActivitiesCollectionChanged;
                                foreach (var item in _circleModel.Activities)
                                {
                                    var activity = new ActivityViewModel(item);
                                    await _activities.AddOnDispatcher(activity).ConfigureAwait(false);
                                }
                            }
                            finally
                            { _syncerActivities.Release(); }
                            await Activate().ConfigureAwait(false);
                        });
                    break;
            }
        }
        async void _circleModel_ActivitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _syncerActivities.WaitAsync().ConfigureAwait(false);
                if (IsActive == false)
                    return;
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        for (var i = e.NewItems.Count - 1; i >= 0; i--)
                        {
                            var idx = e.NewStartingIndex + i;
                            var viewModel = new ActivityViewModel((Activity)e.NewItems[i]);
                            if (Activities.Any(vm => vm is ActivityViewModel ? ((ActivityViewModel)vm).ActivityUrl == viewModel.ActivityUrl : false))
                                continue;
                            await Activities.InsertOnDispatcher(idx, viewModel).ConfigureAwait(false);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            var viewModel = Activities[Math.Min(e.OldStartingIndex + i, Activities.Count - 1)];
                            await Activities.RemoveAtOnDispatcher(e.OldStartingIndex + i).ConfigureAwait(false);
                            viewModel.Cleanup();
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        for (var i = 0; i < Activities.Count; i++)
                            Activities[i].Cleanup();
                        await Activities.ClearOnDispatcher().ConfigureAwait(false);
                        break;
                }
            }
            finally
            { _syncerActivities.Release(); }
        }
        async void _circleModel_ChangedStatus(object sender, EventArgs e)
        {
            try
            {
                await _syncerActivities.WaitAsync().ConfigureAwait(false);
                switch (_circleModel.Status)
                {
                    case StreamStateType.Loading:
                        IsConnected = true;
                        IsLoading = true;
                        _lastConnectDate = DateTime.UtcNow;
                        break;
                    case StreamStateType.Initing:
                        IsIniting = true;
                        await Task.Delay(300);
                        break;
                    case StreamStateType.Successful:
                        IsIniting = false;
                        IsLoading = false;
                        break;
                    case StreamStateType.UnLoaded:
                        await Task.Delay(1000);
                        IsIniting = false;
                        IsLoading = false;
                        IsConnected = false;
                        break;
                }
            }
            finally
            { _syncerActivities.Release(); }
        }
        void ReconnectCommand_Executed() { Task.Run(async () => await Activate()); }
    }
}