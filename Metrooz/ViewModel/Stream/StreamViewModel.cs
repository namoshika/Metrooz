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
            ResumeCommand = new RelayCommand(ResumeCommand_Executed);

            _resumeButton = new ResumeButtonViewModel(ResumeCommand);
            _circleModel.ChangedStatus += _circleModel_ChangedStatus;
            PropertyChanged += StreamViewModel_PropertyChanged;
        }
        readonly System.Threading.SemaphoreSlim _syncerActivities = new System.Threading.SemaphoreSlim(1, 1);
        bool _isPaused, _isActive, _isLoading, _isIniting, _isConnected;
        double _scrollOffset, _scrollOffsetPauseStream = 500;
        string _name;
        Stream _circleModel;
        Account _accountModel;
        StreamManager _circleManagerModel;
        ObservableCollection<ViewModelBase> _activities;
        ResumeButtonViewModel _resumeButton;

        public bool IsPaused
        {
            get { return _isPaused; }
            set { Set(() => IsPaused, ref _isPaused, value); }
        }
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
        public ObservableCollection<ViewModelBase> Activities
        {
            get { return _activities; }
            set { Set(() => Activities, ref _activities, value); }
        }
        public ICommand ReconnectCommand { get; private set; }
        public ICommand ResumeCommand { get; private set; }

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
        async Task Refresh()
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
                _isPaused = false;
                _resumeButton.NewItemCount = 0;
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
        }

        void StreamViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsPaused":
                    Task.Run(async () =>
                        {
                            if (IsPaused == false)
                            {
                                IsIniting = true;
                                await Task.Delay(150);
                                await Refresh();
                                IsIniting = false;
                            }
                            else if (Activities.FirstOrDefault() is ResumeButtonViewModel == false)
                                await Activities.InsertOnDispatcher(0, _resumeButton);
                        });
                    break;
                case "IsActive":
                    Task.Run(async () => await Refresh());
                    break;
            }
        }
        async void _circleModel_ActivitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _syncerActivities.WaitAsync().ConfigureAwait(false);
                //アクティブ化されていない場合はスルー
                //休止状態の場合もスルー。先頭に再始動用VMが挿入されていない場合は挿入
                if (IsActive == false)
                    return;
                if (IsPaused || ScrollOffset > _scrollOffsetPauseStream)
                {
                    IsPaused = true;
                    if (e.Action == NotifyCollectionChangedAction.Add)
                        _resumeButton.NewItemCount++;
                    return;
                }
                //Model同期
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
                var status = _circleModel.Status;
                await _syncerActivities.WaitAsync().ConfigureAwait(false);
                switch (status)
                {
                    case StreamStateType.Loading:
                        IsConnected = true;
                        IsLoading = true;
                        break;
                    case StreamStateType.Initing:
                        IsIniting = true;
                        await Task.Delay(150);
                        if (IsPaused)
                        {
                            await Activities.RemoveOnDispatcher(_resumeButton);
                            _isPaused = false;
                            _resumeButton.NewItemCount = 0;
                        }
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
        void ResumeCommand_Executed() { IsPaused = false; }
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