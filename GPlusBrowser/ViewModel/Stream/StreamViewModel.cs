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

    public class StreamViewModel : ViewModelBase, IDisposable
    {
        public StreamViewModel(Stream circle, AccountViewModel topLevel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, topLevel)
        {
            Activities = new ObservableCollection<ActivityViewModel>();
            Circle = circle;
            MaxActivitiesCount = 30;
            _sourceAccount = topLevel;
            _activityCount = 0;
        }
        int _maxActivityCount;
        int _activityCount;
        string _name;
        Stream _circle;
        AccountViewModel _sourceAccount;
        ObservableCollection<ActivityViewModel> _activities;

        public string CircleName
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("CircleName"));
            }
        }
        public int MaxActivitiesCount
        {
            get { return _maxActivityCount; }
            set
            {
                _maxActivityCount = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MaxActivitiesCount"));
            }
        }
        public Stream Circle
        {
            get { return _circle; }
            set
            {
                if (_circle != null)
                    ((INotifyCollectionChanged)_circle.Activities).CollectionChanged -= OnActivitiesCollectionChanged;
                if (value != null)
                {
                    ((INotifyCollectionChanged)value.Activities).CollectionChanged += OnActivitiesCollectionChanged;
                    _circle = value;
                    CircleName = _circle.Name;
                }
                Activities.Clear();
                if (value != null)
                    foreach (var item in _circle.Activities.ToArray())
                        Activities.Add(new ActivityViewModel(item, _sourceAccount, UiThreadDispatcher));
            }
        }
        public ObservableCollection<ActivityViewModel> Activities
        {
            get { return _activities; }
            set
            {
                _activities = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Activities"));
            }
        }
        public void Connect()
        { _circle.Connect(); }
        public void Dispose()
        {
            _activityCount = 0;
            Circle = null;
            foreach (var item in _activities)
                item.Dispose();
            _activities.ClearAsync(UiThreadDispatcher);
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
                            var viewModel = new ActivityViewModel((Activity)e.NewItems[i], _sourceAccount, UiThreadDispatcher);
                            var idx = e.NewStartingIndex + i;
                            if (idx >= 0 && idx < MaxActivitiesCount - 1)
                            {
                                Activities.InsertAsync(idx, viewModel, UiThreadDispatcher);
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
                                Activities.RemoveAtAsync(idx, UiThreadDispatcher);
                                tmp.Dispose();
                                _activityCount--;
                            }
                        }
                        if (_activityCount < _maxActivityCount && _circle.Activities.Count >= _activityCount)
                        {
                            for (var i = _circle.Activities.Count - _activityCount - 1; i >= 0 && _activityCount < _maxActivityCount; i--)
                            {
                                Activities.AddAsync(new ActivityViewModel(
                                    _circle.Activities[i], _sourceAccount, UiThreadDispatcher), UiThreadDispatcher);
                                _activityCount++;
                            }
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        for (var i = 0; i < Activities.Count; i++)
                            Activities[i].Dispose();
                        Activities.ClearAsync(UiThreadDispatcher);
                        _activityCount = 0;
                        break;
                }
            }
        }
    }
}