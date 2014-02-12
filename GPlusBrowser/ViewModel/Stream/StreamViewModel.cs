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
        public StreamViewModel(Stream circle)
        {
            _activities = new ObservableCollection<ViewModelBase>();
            _circleModel = circle;
            _circleName = circle.Name;
            PropertyChanged += StreamViewModel_PropertyChanged;
        }

        bool _isActive;
        string _circleName;
        Stream _circleModel;
        ObservableCollection<ViewModelBase> _activities;
        readonly System.Threading.SemaphoreSlim _syncerActivities = new System.Threading.SemaphoreSlim(1, 1);

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
        public ObservableCollection<ViewModelBase> Activities
        {
            get { return _activities; }
            set { Set(() => Activities, ref _activities, value); }
        }

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
                            _circleModel.Connect();
                        }
                        break;
                    }
                    finally
                    { _syncerActivities.Release(); }
            }
        }
    }
}