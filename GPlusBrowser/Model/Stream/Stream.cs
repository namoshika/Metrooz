using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;

namespace GPlusBrowser.Model
{
    public class Stream : IDisposable
    {
        public Stream(StreamManager manager)
        {
            _manager = manager;
            _activities = new ObservableCollection<Activity>();
            _syncer = new System.Threading.SemaphoreSlim(1, 1);
            Activities = new ReadOnlyObservableCollection<Activity>(_activities);
        }
        System.Threading.SemaphoreSlim _syncer;
        CircleInfo _circle;
        IDisposable _streamObj;
        StreamManager _manager;
        IInfoList<ActivityInfo> _activityGetter;
        ObservableCollection<Activity> _activities;
        bool _isUpdated;

        public string Name { get { return Circle.Name; } }
        public ReadOnlyObservableCollection<Activity> Activities { get; private set; }
        public CircleInfo Circle
        {
            get { return _circle; }
            set
            {
                _circle = value;
                _activityGetter = Circle.GetActivities();
            }
        }

        public async void Connect()
        {
            if (_isUpdated == false)
                try
                {
                    await _syncer.WaitAsync();
                    _isUpdated = true;
                    _activities.Clear();

                    foreach (var item in await _activityGetter.TakeAsync(20).ConfigureAwait(false))
                        _activities.Add(new Activity(item));
                }
                finally
                { _syncer.Release(); }
            if (_streamObj == null && _isUpdated)
                _streamObj = Circle.GetStream().Subscribe(async newInfo =>
                    {
                        try
                        {
                            await _syncer.WaitAsync();
                            var item = Activities.FirstOrDefault(activity => activity.CoreInfo.Id == newInfo.Id);
                            switch (newInfo.PostStatus)
                            {
                                case PostStatusType.First:
                                case PostStatusType.Edited:
                                    //itemがnullの場合は更新する。nullでない場合はすでにある値を更新する。
                                    //しかし更新はActivityオブジェクト自体が行うため、Streamでは行わない
                                    if (item == null)
                                    {
                                        item = new Activity(newInfo);
                                        _activities.Insert(0, item);
                                        if (_activities.Count > 50)
                                        {
                                            _activities[0].Dispose();
                                            _activities.RemoveAt(0);
                                        }
                                    }
                                    break;
                                case PostStatusType.Removed:
                                    var idx = Activities.IndexOf(item);
                                    if (idx >= 0)
                                        _activities.Remove(item);
                                    break;
                            }
                        }
                        finally
                        { _syncer.Release(); }
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
            _streamObj = null;
        }
    }
}