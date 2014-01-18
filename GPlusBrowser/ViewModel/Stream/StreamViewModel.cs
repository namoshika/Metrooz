using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class StreamViewModel : ViewModelBase
    {
        public StreamViewModel(Stream circle)
        {
            Activities = new ObservableCollection<ActivityViewModel>();
            Circle = circle;
            MaxActivitiesCount = 30;
            _activityCount = 0;
        }
        int _maxActivityCount;
        int _activityCount;
        string _name;
        Stream _circle;
        ObservableCollection<ActivityViewModel> _activities;

        public string CircleName
        {
            get { return _name; }
            set { Set(() => CircleName, ref _name, value); }
        }
        public int MaxActivitiesCount
        {
            get { return _maxActivityCount; }
            set { Set(() => MaxActivitiesCount, ref _maxActivityCount, value); }
        }
        public Stream Circle
        {
            get { return _circle; }
            set
            {
                if (_circle != null)
                {
                    Activities.Clear();
                    ((INotifyCollectionChanged)_circle.Activities).CollectionChanged -= OnActivitiesCollectionChanged;
                }
                if (value != null)
                {
                    ((INotifyCollectionChanged)value.Activities).CollectionChanged += OnActivitiesCollectionChanged;
                    Set(() => Circle, ref _circle, value);
                    CircleName = _circle.Name;

                    foreach (var item in _circle.Activities.ToArray())
                    {
                        var viewModel = new ActivityViewModel(item);
                        Activities.Add(viewModel);
                    }
                }
            }
        }
        public ObservableCollection<ActivityViewModel> Activities
        {
            get { return _activities; }
            set { Set(() => Activities, ref _activities, value); }
        }
        public void Connect()
        { _circle.Connect(); }
        public override void Cleanup()
        {
            base.Cleanup();

            _activityCount = 0;
            Circle = null;
            foreach (var item in _activities)
                item.Cleanup();
            _activities.ClearAsync(App.Current.Dispatcher);
        }
        protected void OnActivitiesCollectionChanged(
            object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock (Activities)
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        for (var i = e.NewItems.Count - 1; i >= 0; i--)
                        {
                            var idx = e.NewStartingIndex + i;
                            if (idx >= 0 && idx < MaxActivitiesCount - 1)
                            {
                                var viewModel = new ActivityViewModel((Activity)e.NewItems[i]);
                                if (Activities.Any(vm => vm.ActivityUrl == viewModel.ActivityUrl))
                                    continue;
                                Activities.InsertAsync(idx, viewModel, App.Current.Dispatcher);
                                _activityCount++;
                            }
                        }
                        //while (_activityCount > _maxActivityCount)
                        //{
                        //    _activityCount--;
                        //    _activities.GetFromIndex(_activityCount, UiThreadDispatcher)
                        //        .ContinueWith(tsk =>
                        //            {
                        //                _activities.RemoveAsync(tsk.Result, UiThreadDispatcher);
                        //                tsk.Result.Dispose();
                        //            });

                        //}
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            var idx = _circle.Activities.Count - e.OldStartingIndex;
                            if (idx >= 0 && idx < MaxActivitiesCount - 1)
                            {
                                var tmp = Activities[idx];
                                Activities.RemoveAtAsync(idx, App.Current.Dispatcher);
                                tmp.Cleanup();
                                _activityCount--;
                            }
                        }
                        if (_activityCount < _maxActivityCount && _circle.Activities.Count >= _activityCount)
                        {
                            for (var i = _circle.Activities.Count - _activityCount - 1; i >= 0 && _activityCount < _maxActivityCount; i--)
                            {
                                var viewModel = new ActivityViewModel(_circle.Activities[i]);
                                Activities.AddAsync(viewModel, App.Current.Dispatcher);
                                _activityCount++;
                            }
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        for (var i = 0; i < Activities.Count; i++)
                            Activities[i].Cleanup();
                        Activities.ClearAsync(App.Current.Dispatcher);
                        _activityCount = 0;
                        break;
                }
            }
        }
    }
}