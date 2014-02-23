﻿using GalaSoft.MvvmLight;
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
            _circleName = circle.Name;
            _circleManagerModel = manager;
            _accountModel = account;

            _activities = new ObservableCollection<ActivityViewModel>();
            _connectStatus = ConnectStateType.UnInitialized;
            ReconnectCommand = new RelayCommand(ReconnectCommand_Executed);

            _circleModel.ChangedIsConnected += _circleModel_ChangedIsConnected;
            PropertyChanged += StreamViewModel_PropertyChanged;
        }
        bool _isActive, isLoading;
        string _circleName;
        ConnectStateType _connectStatus;
        Stream _circleModel;
        Account _accountModel;
        StreamManager _circleManagerModel;
        ObservableCollection<ActivityViewModel> _activities;
        readonly System.Threading.SemaphoreSlim _syncerActivities = new System.Threading.SemaphoreSlim(1, 1);

        public bool IsLoading
        {
            get { return isLoading; }
            set { Set(() => IsLoading, ref isLoading, value); }
        }
        public bool IsActive
        {
            get { return _isActive; }
            set { Set(() => IsActive, ref _isActive, value); }
        }
        public string CircleName
        {
            get { return _circleName; }
            set { Set(() => CircleName, ref _circleName, value); }
        }
        public ConnectStateType ConnectStatus
        {
            get { return _connectStatus; }
            set { Set(() => ConnectStatus, ref _connectStatus, value); }
        }
        public ObservableCollection<ActivityViewModel> Activities
        {
            get { return _activities; }
            set { Set(() => Activities, ref _activities, value); }
        }
        public ICommand ReconnectCommand { get; private set; }

        public override void Cleanup()
        {
            base.Cleanup();
            PropertyChanged -= StreamViewModel_PropertyChanged;
            foreach (var item in _activities)
                item.Cleanup();
        }
        protected async void OnActivitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await _syncerActivities.WaitAsync();
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
                            Activities.InsertOnDispatcher(idx, viewModel);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            var viewModel = Activities[Math.Min(e.OldStartingIndex + i, Activities.Count - 1)];
                            Activities.RemoveAtOnDispatcher(e.OldStartingIndex + i);
                            viewModel.Cleanup();
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        for (var i = 0; i < Activities.Count; i++)
                            Activities[i].Cleanup();
                        Activities.ClearOnDispatcher();
                        break;
                }
            }
            finally
            { _syncerActivities.Release(); }
        }
        async void StreamViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsActive":
                    try
                    {
                        await _syncerActivities.WaitAsync();
                        //残留しているゴミを捨てる
                        _circleModel.Activities.CollectionChanged -= OnActivitiesCollectionChanged;
                        foreach (var item in _activities)
                            item.Cleanup();
                        _activities.Clear();

                        //アクティブ時は新たにmodelから読み込む
                        if (_isActive)
                        {
                            foreach (var item in _circleModel.Activities.ToArray())
                            {
                                var viewModel = new ActivityViewModel(item);
                                _activities.Add(viewModel);
                            }
                            _circleModel.Activities.CollectionChanged += OnActivitiesCollectionChanged;
                            if (_circleModel.IsConnected == false)
                            {
                                IsLoading = true;
                                await _circleModel.Connect();
                                IsLoading = false;
                            }
                        }
                        break;
                    }
                    finally
                    { _syncerActivities.Release(); }
            }
        }
        void _circleModel_ChangedIsConnected(object sender, EventArgs e)
        { ConnectStatus = _circleModel.IsConnected ? ConnectStateType.Connected : ConnectStateType.Disconnected; }
        async void ReconnectCommand_Executed()
        {
            if (IsActive == false)
                return;

            IsLoading = true;
            await _circleModel.Connect();
            IsLoading = false;
        }
    }
    public enum ConnectStateType
    { UnInitialized, Connected, Disconnected }
}