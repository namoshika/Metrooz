﻿using GalaSoft.MvvmLight;
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
            _activities = new ObservableCollection<ActivityViewModel>();
            _circle = circle;
            _circleName = circle.Name;
        }
        bool _isActive;
        string _circleName;
        Stream _circle;
        ObservableCollection<ActivityViewModel> _activities;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                Set(() => IsActive, ref _isActive, value);
                if (value)
                {
                    foreach (var item in _circle.Activities.ToArray())
                    {
                        var viewModel = new ActivityViewModel(item);
                        _activities.Add(viewModel);
                    }
                    _circle.Activities.CollectionChanged += OnActivitiesCollectionChanged;
                    _circle.Connect();
                }
                else
                {
                    _circle.Activities.CollectionChanged -= OnActivitiesCollectionChanged;
                    foreach (var item in _activities)
                        item.Cleanup();
                    _activities.Clear();
                }
            }
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

        public override void Cleanup()
        {
            base.Cleanup();
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
                            var viewModel = new ActivityViewModel((Activity)e.NewItems[i]);
                            if (Activities.Any(vm => vm.ActivityUrl == viewModel.ActivityUrl))
                                continue;
                            Activities.InsertAsync(idx, viewModel, App.Current.Dispatcher);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            var viewModel = Activities[Math.Min(e.OldStartingIndex + i, Activities.Count - 1)];
                            Activities.RemoveAtAsync(e.OldStartingIndex + i, App.Current.Dispatcher);
                            viewModel.Cleanup();
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        for (var i = 0; i < Activities.Count; i++)
                            Activities[i].Cleanup();
                        Activities.ClearAsync(App.Current.Dispatcher);
                        break;
                }
            }
        }
    }
}