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
        }
        Stream _circle;
        DispatchObservableCollection<ActivityViewModel> _activities;

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
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        var viewModel = new ActivityViewModel((Activity)e.NewItems[i], UiThreadDispatcher);
                        if(Activities.Count - (e.NewStartingIndex + i) >= 0)
                        {
                            Activities.Insert(Math.Max(0, Activities.Count - (e.NewStartingIndex + i)), viewModel);
                        }
                        else
                        {
                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debugger.Break();
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                        Activities.RemoveAt(Activities.Count - (e.OldStartingIndex) - 1);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Activities.Clear();
                    break;
            }
        }
    }
}
