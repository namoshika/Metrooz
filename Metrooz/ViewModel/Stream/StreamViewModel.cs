using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SunokoLibrary.Collections.ObjectModel;
using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Metrooz.ViewModel
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
            _activities = new ObservableCollection<ViewModelBase>();
            ReconnectCommand = new RelayCommand(ReconnectCommand_Executed);
            ResumeCommand = new RelayCommand(ReconnectCommand_Executed);

            _resumeButton = new ResumeButtonViewModel(ResumeCommand);
            _circleModel.ChangedStatus += _circleModel_ChangedStatus;
            _circleModel.ChangedChangedActivityCount += _circleModel_ChangedChangedActivityCount;
            PropertyChanged += StreamViewModel_PropertyChanged;
        }
        readonly System.Threading.SemaphoreSlim _activitiesSemaph = new System.Threading.SemaphoreSlim(1, 1);
        readonly Stream _circleModel;
        readonly Account _accountModel;
        readonly StreamManager _circleManagerModel;
        readonly ResumeButtonViewModel _resumeButton;
        readonly ObservableCollection<ViewModelBase> _activities;
        bool _isActive, _isLoading, _isIniting, _isConnected;
        double _scrollOffset, _scrollOffsetPauseStream = 150;
        string _name;
        IDisposable _activitiesSyncer;

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
        public double ScrollOffset
        {
            get { return _scrollOffset; }
            set { Set(() => ScrollOffset, ref _scrollOffset, value); }
        }
        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }
        public ObservableCollection<ViewModelBase> Activities { get { return _activities; } }
        public ICommand ReconnectCommand { get; private set; }
        public ICommand ResumeCommand { get; private set; }

        public async Task Activate()
        {
            switch(_circleModel.Status)
            {
                case StreamStateType.UnLoaded:
                case StreamStateType.Paused:
                    await _circleModel.Connect();
                    break;
            }
        }
        public override void Cleanup()
        {
            base.Cleanup();
            _circleModel.ChangedStatus -= _circleModel_ChangedStatus;
            _circleModel.ChangedChangedActivityCount -= _circleModel_ChangedChangedActivityCount;
            PropertyChanged -= StreamViewModel_PropertyChanged;
            foreach (var item in _activities)
                item.Cleanup();
        }
        async Task Refresh()
        {
            try
            {
                await _activitiesSemaph.WaitAsync().ConfigureAwait(false);
                //古い要素を削除
                if(_activitiesSyncer != null)
                    _activitiesSyncer.Dispose();
                foreach (var item in _activities)
                    item.Cleanup();
                await _activities.ClearOnDispatcher(App.Current.Dispatcher).ConfigureAwait(false);

                //新しい要素を追加
                if (_isActive == false)
                    return;
                _resumeButton.NewItemCount = 0;
                _activitiesSyncer = _circleModel.Activities.SyncWith(
                    _activities, item => new ActivityViewModel(item),
                    _circleModel_ActivitiesCollectionChanged, item => item.Cleanup(), App.Current.Dispatcher);
            }
            finally
            { _activitiesSemaph.Release(); }
            await Activate().ConfigureAwait(false);
        }

        void StreamViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsActive":
                    Task.Run(async () => await Refresh());
                    break;
                case "ScrollOffset":
                    if (ScrollOffset > _scrollOffsetPauseStream)
                        Task.Run(async () => await _circleModel.Pause());
                    else if (_circleModel.Status == StreamStateType.Connected)
                        Task.Run(async () => await _circleModel.Connect());
                    break;
            }
        }
        async void _circleModel_ActivitiesCollectionChanged(Func<Task> syncProc, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _activitiesSemaph.WaitAsync().ConfigureAwait(false);
                if (IsActive == false)
                    return;
                await syncProc();
            }
            finally
            { _activitiesSemaph.Release(); }
        }
        async void _circleModel_ChangedStatus(object sender, EventArgs e)
        {
            try
            {
                var status = _circleModel.Status;
                await _activitiesSemaph.WaitAsync().ConfigureAwait(false);
                switch (status)
                {
                    case StreamStateType.Loading:
                        IsConnected = true;
                        IsLoading = true;
                        break;
                    case StreamStateType.Initing:
                        IsIniting = true;
                        await Task.Delay(150);
                        await Activities.RemoveOnDispatcher(_resumeButton, App.Current.Dispatcher);
                        break;
                    case StreamStateType.Connected:
                        IsIniting = false;
                        IsLoading = false;
                        await Activities.RemoveOnDispatcher(_resumeButton, App.Current.Dispatcher);
                        break;
                    case StreamStateType.Paused:
                        await Activities.InsertOnDispatcher(0, _resumeButton, App.Current.Dispatcher);
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
            { _activitiesSemaph.Release(); }
        }
        void _circleModel_ChangedChangedActivityCount(object sender, EventArgs e)
        { _resumeButton.NewItemCount = _circleModel.ChangedActivityCount; }
        void ReconnectCommand_Executed() { Task.Run(async () => await Activate()); }
    }
    public class ResumeButtonViewModel : ViewModelBase
    {
        public ResumeButtonViewModel(ICommand resumeCommand) { ResumeCommand = resumeCommand; }
        int _newItemCount;
        public int NewItemCount
        {
            get { return _newItemCount; }
            set { Set(() => NewItemCount, ref _newItemCount, value); }
        }
        public ICommand ResumeCommand { get; private set; }
    }
}