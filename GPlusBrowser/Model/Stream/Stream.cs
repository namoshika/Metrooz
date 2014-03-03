using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;

namespace GPlusBrowser.Model
{
    public class Stream : IDisposable
    {
        public Stream(StreamManager manager)
        {
            _activities = new ObservableCollection<Activity>();
            _syncer = new System.Threading.SemaphoreSlim(1, 1);
        }
        System.Threading.SemaphoreSlim _syncer;
        int _maxActivityCount = 30;
        CircleInfo _circle;
        IInfoList<ActivityInfo> _activityGetter;
        ObservableCollection<Activity> _activities;
        IDisposable _streamObj;

        public StreamStateType Status { get; private set; }
        public string Name { get { return Circle.Name; } }
        public ObservableCollection<Activity> Activities { get { return _activities; } }
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
            try
            {
                await _syncer.WaitAsync().ConfigureAwait(false);
                if (Status >= StreamStateType.Loading)
                    return;

                Status = StreamStateType.Loading;
                OnChangedStatus(new EventArgs());
                var activities = await _activityGetter.TakeAsync(20).ConfigureAwait(false);

                Status = StreamStateType.Initing;
                OnChangedStatus(new EventArgs());
                _activities.Clear();
                foreach (var item in activities)
                    _activities.Add(new Activity(item));

                Status = StreamStateType.Connecting;
                OnChangedStatus(new EventArgs());
                _streamObj = Circle.GetStream().Subscribe(async newInfo =>
                    {
                        try
                        {
                            await _syncer.WaitAsync().ConfigureAwait(false);
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
                                        if (_activities.Count > _maxActivityCount)
                                        {
                                            _activities[_maxActivityCount].Dispose();
                                            _activities.RemoveAt(_maxActivityCount);
                                        }
                                    }
                                    break;
                                case PostStatusType.Removed:
                                    _activities.Remove(item);
                                    break;
                            }
                        }
                        finally
                        { _syncer.Release(); }
                    },
                    async ex =>
                    {
                        try
                        {
                            await _syncer.WaitAsync().ConfigureAwait(false);
                            _streamObj.Dispose();
                            _streamObj = null;
                            Status = StreamStateType.UnLoaded;
                            OnChangedStatus(new EventArgs());
                        }
                        finally { _syncer.Release(); }
                    },
                    async () =>
                    {
                        try
                        {
                            await _syncer.WaitAsync().ConfigureAwait(false);
                            _streamObj.Dispose();
                            _streamObj = null;
                            Status = StreamStateType.UnLoaded;
                            OnChangedStatus(new EventArgs());
                        }
                        finally { _syncer.Release(); }
                    });
            }
            catch (FailToOperationException)
            {
                Status = StreamStateType.UnLoaded;
                OnChangedStatus(new EventArgs());
            }
            finally { _syncer.Release(); }
        }
        public async void Dispose()
        {
            try
            {
                await _syncer.WaitAsync().ConfigureAwait(false);
                if (_streamObj != null)
                    _streamObj.Dispose();
                foreach (var item in _activities)
                    item.Dispose();
            }
            finally
            { _syncer.Release(); }
        }

        public event EventHandler ChangedStatus;
        protected virtual void OnChangedStatus(EventArgs e)
        {
            if (ChangedStatus != null)
                ChangedStatus(this, e);
        }
    }
    public enum StreamStateType
    { UnLoaded, Loading, Initing, Connecting }
}