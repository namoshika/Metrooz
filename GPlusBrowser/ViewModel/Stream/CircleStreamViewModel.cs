using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class CircleStreamViewModel : StreamViewModel
    {
        public CircleStreamViewModel(Stream circle, int order, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            Activities = new DispatchObservableCollection<ActivityViewModel>(uiThreadDispatcher);
            Order = order;
            Circle = circle;
            MaxActivitiesCount = 40;
        }
        int _maxActivityCount;
        Stream _circle;
        DispatchObservableCollection<ActivityViewModel> _activities;

        public int MaxActivitiesCount
        {
            get { return _maxActivityCount; }
            set
            {
                _maxActivityCount = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("MaxActivitiesCount"));
                lock (_activities)
                    ActivitiesCompaction(value);
            }
        }
        public Stream Circle
        {
            get { return _circle; }
            set
            {
                if (_circle != null)
                    _circle.UpdatedActivities -= OnActivitiesCollectionChanged;
                if (value != null)
                {
                    value.UpdatedActivities += OnActivitiesCollectionChanged;
                    _circle = value;
                    CircleName = _circle.Name;
                }
                Activities.Clear();
                if (value != null)
                    foreach (var item in _circle.Activities.ToArray())
                        Activities.Add(new ActivityViewModel(item, UiThreadDispatcher));
            }
        }
        public DispatchObservableCollection<ActivityViewModel> Activities
        {
            get { return _activities; }
            set
            {
                _activities = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Activities"));
            }
        }
        public override void Dispose()
        {
            Circle = null;
            foreach (var item in _activities)
                item.Dispose();
            _activities.Clear();
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
                            var viewModel = new ActivityViewModel((Activity)e.NewItems[i], UiThreadDispatcher);
                            var idx = _circle.Activities.Count - (e.NewStartingIndex + i) - 1;
                            if (idx >= 0 && idx < MaxActivitiesCount - 1)
                                Activities.Insert(idx, viewModel);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            var idx = _circle.Activities.Count - (e.OldStartingIndex) - 1;
                            if (idx >= 0 && idx < MaxActivitiesCount - 1)
                            {
                                Activities[idx].Dispose();
                                Activities.RemoveAt(idx);
                            }
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        for (var i = 0; i < Activities.Count; i++)
                            Activities[i].Dispose();
                        Activities.Clear();
                        break;
                }
                ActivitiesCompaction(MaxActivitiesCount);
            }
        }
        void ActivitiesCompaction(int ActivitiesCapacity)
        {
            if (Activities.Count > ActivitiesCapacity)
            {
                for (var i = ActivitiesCapacity; i < Activities.Count; i++)
                {
                    Activities[i].Dispose();
                    Activities.RemoveAt(i);
                }
            }
            else
            {
                for (var i = _circle.Activities.Count - Activities.Count - 1; i >= 0 && i < _circle.Activities.Count; i++)
                    Activities.Add(new ActivityViewModel(_circle.Activities[i], UiThreadDispatcher));
            }
        }
    }
}
