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
            _circleName = circle.Name;
            _circleManagerModel = manager;
            _accountModel = account;
            _isConnected = true;
            _activities = new ObservableCollection<ActivityViewModel>();
            ReconnectCommand = new RelayCommand(ReconnectCommand_Executed);

            PropertyChanged += StreamViewModel_PropertyChanged;
        }
        bool _isActive, _isLoading, _isIniting, _isConnected;
        string _circleName;
        Stream _circleModel;
        Account _accountModel;
        StreamManager _circleManagerModel;
        ObservableCollection<ActivityViewModel> _activities;
        readonly System.Threading.SemaphoreSlim _syncerActivities = new System.Threading.SemaphoreSlim(1, 1);

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
        public string CircleName
        {
            get { return _circleName; }
            set { Set(() => CircleName, ref _circleName, value); }
        }
        public ObservableCollection<ActivityViewModel> Activities
        {
            get { return _activities; }
            set { Set(() => Activities, ref _activities, value); }
        }
        public ICommand ReconnectCommand { get; private set; }

        public async Task Activate()
        {
            try
            {
                await _syncerActivities.WaitAsync();

                //_circleModelの初期化
                //接続に失敗しても連打防止として1秒間は通信中の表示をする。これの
                //実装用に_circleModel.ChangedStatusの購読を切っておく必要があった。
                _circleModel.ChangedStatus -= _circleModel_ChangedStatus;
                if (_circleModel.Status < StreamStateType.Initializing)
                {
                    IsConnected = true;
                    IsLoading = true;
                    await _circleModel.Initialize();
                    if (_circleModel.Status < StreamStateType.Initializing)
                    {
                        await Task.Delay(1000);
                        IsConnected = false;
                    }
                    IsIniting = true;
                }
                //初期化に成功したら_activitiesの更新を行う
                _circleModel.ChangedStatus += _circleModel_ChangedStatus;
                if (_circleModel.Status >= StreamStateType.Initializing)
                {
                    //古い要素を削除
                    _circleModel.Activities.CollectionChanged -= _circleModel_ActivitiesCollectionChanged;
                    foreach (var item in _activities)
                        item.Cleanup();
                    _activities.Clear();
                    //新しい要素を追加
                    foreach (var item in _circleModel.Activities)
                    {
                        var activity = new ActivityViewModel(item);
                        var tsk = activity.Refresh();
                        _activities.Add(activity);
                    }

                    //ストリーミングapiへ接続
                    _circleModel.Activities.CollectionChanged += _circleModel_ActivitiesCollectionChanged;
                    if (_circleModel.Status < StreamStateType.Connecting)
                        await _circleModel.Connect();
                }
                IsIniting = false;
                IsLoading = false;
            }
            finally { _syncerActivities.Release(); }
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
                    if (_isActive)
                    {
                        var tsk = Activate();
                    }
                    else
                    {
                        //古い要素を削除
                        _circleModel.Activities.CollectionChanged -= _circleModel_ActivitiesCollectionChanged;
                        foreach (var item in _activities)
                            item.Cleanup();
                        _activities.Clear();
                    }
                    break;
            }
        }
        async void _circleModel_ActivitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
        void _circleModel_ChangedStatus(object sender, EventArgs e)
        {
            if (_circleModel.Status == StreamStateType.UnInitialized)
                IsConnected = false;
        }
        void ReconnectCommand_Executed()
        { var tsk = Activate(); }
    }
}