﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.Model
{
    public class Stream : IDisposable
    {
        public Stream(StreamManager manager, CircleInfo circleInfo)
        {
            _manager = manager;
            _activities = new List<Activity>();
            Poster = circleInfo;
            Reader = circleInfo;
        }
        IPostRange _poster;
        IReadRange _reader;
        IDisposable _streamObj;
        StreamManager _manager;
        List<Activity> _activities;
        public ReadOnlyCollection<Activity> Activities
        { get { return _activities.AsReadOnly(); } }
        public bool Postable { get; private set; }
        public bool Readable { get; private set; }
        public bool IsRefreshed { get; private set; }
        public string Name { get; private set; }

        public IPostRange Poster
        {
            get { return _poster; }
            set
            {
                Postable = value != null;
                _poster = value;
            }
        }
        public IReadRange Reader
        {
            get { return _reader; }
            set
            {
                if (_reader != value && _reader != null || _reader != null)
                {
                    _streamObj.Dispose();
                    _activities.Clear();
                }
                Readable = value != null;
                Name = value.Name;
                _reader = value;
                _streamObj = _reader.GetStream().Subscribe(activity_OnNext);
            }
        }
        public void Refresh()
        {
            IsRefreshed = true;
            _reader.GetActivitiesAsync(20, null)
                .ContinueWith(tsk =>
                    {
                        var infos = tsk.Result;
                        _activities.Clear();
                        OnUpdatedActivities(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                        var activities = infos.Items.Where(info => info != null)
                            .Select(info => new Activity(info)).Reverse().ToArray();
                        _activities.InsertRange(0, activities);
                        OnUpdatedActivities(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, activities, 0));
                    });
        }
        public void Post(string content)
        {
            if (Postable)
                Poster.Post(content);
        }
        public void Dispose()
        {
            lock (_activities)
                foreach (var item in _activities)
                    item.Dispose();
            _streamObj.Dispose();
        }
        void activity_OnNext(ActivityInfo info)
        {
            using (info.GetParseLocker())
                switch (info.PostStatus)
                {
                    case PostStatusType.First:
                        var item = new Activity(info);
                        lock (_activities)
                            _activities.Add(item);
                        OnUpdatedActivities(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                            System.Collections.Specialized.NotifyCollectionChangedAction.Add,
                            item, Activities.Count - 1));
                        break;
                    case PostStatusType.Edited:
                        item = Activities.FirstOrDefault(activity => activity.ActivityInfo.Id == info.Id);
                        //itemがnullの場合は更新する。nullでない場合はすでにある値を更新する。
                        //しかし更新はActivityオブジェクト自体が行うため、Streamでは行わない
                        if (item == null)
                        {
                            item = new Activity(info);
                            lock (_activities)
                                _activities.Add(item);
                            OnUpdatedActivities(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                                System.Collections.Specialized.NotifyCollectionChangedAction.Add,
                                item, Activities.Count - 1));
                        }
                        break;
                    case PostStatusType.Removed:
                        item = Activities.FirstOrDefault(activity => activity.ActivityInfo.Id == info.Id);
                        var idx = Activities.IndexOf(item);
                        if (idx < 0)
                            return;
                        lock (_activities)
                            _activities.Remove(item);
                        OnUpdatedActivities(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                            System.Collections.Specialized.NotifyCollectionChangedAction.Remove, item, idx));
                        break;
                }
        }

        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler UpdatedActivities;
        protected virtual void OnUpdatedActivities(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (UpdatedActivities != null)
                UpdatedActivities(this, e);
        }
    }
}