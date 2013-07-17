using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.Model
{
    public class Stream : IDisposable
    {
        public Stream(StreamManager manager, CircleInfo circleInfo)
        {
            _manager = manager;
            _isRefresh = 0;
            _reader = circleInfo;
            _activityGetter = circleInfo.GetActivities();
            _activities = new ObservableCollection<Activity>();
            Name = circleInfo.Name;
            Activities = new ReadOnlyObservableCollection<Activity>(_activities);
        }
        CircleInfo _reader;
        IDisposable _streamObj;
        StreamManager _manager;
        IInfoList<ActivityInfo> _activityGetter;
        ObservableCollection<Activity> _activities;
        int _isRefresh;

        public ReadOnlyObservableCollection<Activity> Activities { get; private set; }
        public bool IsRefreshed { get { return _isRefresh == 1; } }
        public string Name { get; private set; }

        public CircleInfo Circle
        {
            get { return _reader; }
            set
            {
                _reader = value;
                Name = _reader.Name;
            }
        }
        public async void Refresh()
        {
            if (System.Threading.Interlocked.CompareExchange(ref _isRefresh, 1, 0) == 1)
                return;
            var activities = await _activityGetter.TakeAsync(20).ConfigureAwait(false);
            lock (_activities)
            {
                _activities.Clear();
                foreach (var item in activities
                    .Where(info => info != null)
                    .Select(info => new Activity(info)))
                    _activities.Add(item);
            }
        }
        public void Connect()
        {
            if (_streamObj != null)
                return;

            _streamObj = _reader.GetStream().Subscribe(async info =>
                {
                    using (await info.GetParseLocker())
                        lock (_activities)
                            switch (info.PostStatus)
                            {
                                case PostStatusType.First:
                                case PostStatusType.Edited:
                                    var item = Activities.FirstOrDefault(activity => activity.ActivityInfo.Id == info.Id);
                                    //itemがnullの場合は更新する。nullでない場合はすでにある値を更新する。
                                    //しかし更新はActivityオブジェクト自体が行うため、Streamでは行わない
                                    if (item == null)
                                    {
                                        item = new Activity(info);
                                        _activities.Insert(0, item);

                                        if (_activities.Count > 50)
                                        {
                                            _activities[0].Dispose();
                                            _activities.RemoveAt(0);
                                        }
                                    }
                                    break;
                                case PostStatusType.Removed:
                                    item = Activities.FirstOrDefault(activity => activity.ActivityInfo.Id == info.Id);
                                    var idx = Activities.IndexOf(item);
                                    if (idx < 0)
                                        return;
                                    _activities.Remove(item);
                                    break;
                            }
                },
                ex =>
                {
                    _streamObj.Dispose();
                    _streamObj = null;
                });
        }
        public void Dispose()
        {
            lock (_activities)
                foreach (var item in _activities)
                    item.Dispose();
            _streamObj.Dispose();
        }
    }
}