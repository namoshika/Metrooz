using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using SunokoLibrary.Web.GooglePlus;
using SunokoLibrary.Web.GooglePlus.Primitive;

namespace Metrooz.Model
{
    public class Stream : IDisposable
    {
        public Stream(CircleInfo source, StreamManager manager)
        {
            _activities = new ObservableCollection<Activity>();
            _syncer = new System.Threading.SemaphoreSlim(1, 1);
            Circle = source;
        }
        const int MAX_ACTIVITIES_COUNT = 30;
        readonly System.Threading.SemaphoreSlim _syncer;
        bool _isPause;
        int _unreadedActivityCount;
        StreamStateType _status;
        CircleInfo _circle;
        IInfoList<ActivityInfo> _activityGetter;
        ObservableCollection<Activity> _activities, _hiddenActivities;
        IDisposable _streamObj;

        public ObservableCollection<Activity> Activities { get { return _activities; } }
        public string Name { get { return Circle.Name; } }
        public int ChangedActivityCount
        {
            get { return _unreadedActivityCount; }
            set
            {
                if (_unreadedActivityCount == value)
                    return;
                _unreadedActivityCount = value;
                OnChangedChangedActivityCount(new EventArgs());
            }
        }
        public StreamStateType Status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                    return;
                _status = value;
                OnChangedStatus(new EventArgs());
            }
        }
        public CircleInfo Circle
        {
            get { return _circle; }
            private set
            {
                _circle = value;
                _activityGetter = Circle.GetActivities();
            }
        }

        public async Task Connect()
        {
            try
            {
                await _syncer.WaitAsync();
                //処理の最初に置くことでConnect()後はストリームの休止状態が終了している事を保証させる
                _isPause = false;

                //接続処理
                switch (Status)
                {
                    case StreamStateType.UnLoaded:
                        //読み込み
                        Status = StreamStateType.Loading;
                        var activities = await _activityGetter.TakeAsync(20);

                        //更新
                        Status = StreamStateType.Initing;
                        var outerActiveActivities = _isPause ? _hiddenActivities : _activities;
                        outerActiveActivities.Clear();
                        foreach (var item in activities)
                            outerActiveActivities.Add(new Activity(item));

                        //受信開始
                        Status = StreamStateType.Connected;
                        _streamObj = Circle.GetStream().Subscribe(async newInfo =>
                            {
                                try
                                {
                                    await _syncer.WaitAsync().ConfigureAwait(false);
                                    //一時停止している場合は外から見えない領域でストリームを更新
                                    //するために_activitiesではなく_tmpActivitiesが更新される
                                    var innerActiveActivities = _isPause ? _hiddenActivities : _activities;
                                    var item = innerActiveActivities.FirstOrDefault(activity => activity.CoreInfo.Id == newInfo.Id);
                                    var existUpdate = false;
                                    switch (newInfo.PostStatus)
                                    {
                                        case PostStatusType.First:
                                        case PostStatusType.Edited:
                                            //itemがnullの場合は更新する。nullでない場合はすでにある値を更新する。
                                            //しかし更新はActivityオブジェクト自体が行うため、Streamでは行わない
                                            if (item == null)
                                            {
                                                existUpdate = true;
                                                item = new Activity(newInfo);
                                                innerActiveActivities.Insert(0, item);
                                                if (innerActiveActivities.Count > MAX_ACTIVITIES_COUNT)
                                                {
                                                    innerActiveActivities[MAX_ACTIVITIES_COUNT].Dispose();
                                                    innerActiveActivities.RemoveAt(MAX_ACTIVITIES_COUNT);
                                                }
                                            }
                                            break;
                                        case PostStatusType.Removed:
                                            innerActiveActivities.Remove(item);
                                            existUpdate = true;
                                            break;
                                    }
                                    if (_isPause && existUpdate)
                                    {
                                        Status = StreamStateType.Paused;
                                        ChangedActivityCount++;
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
                                }
                                finally { _syncer.Release(); }
                            });
                        break;
                    case StreamStateType.Paused:
                        Status = StreamStateType.Connected;
                        if (ChangedActivityCount > 0)
                        {
                            int hIdx, aIdx;
                            for (hIdx = 0; hIdx < _hiddenActivities.Count; hIdx++)
                                if ((aIdx = _activities.IndexOf(_hiddenActivities[hIdx])) < 0)
                                    _activities.Insert(hIdx, _hiddenActivities[hIdx]);
                                else if (aIdx != hIdx)
                                    _activities.Move(aIdx, hIdx);
                            for (aIdx = _activities.Count - 1; aIdx >= hIdx; aIdx--)
                                _activities.RemoveAt(aIdx);
                        }
                        //一時停止が終わったらActivityのGC解放のためにも消す
                        _hiddenActivities = null;
                        break;
                }
            }
            catch (FailToOperationException)
            {
                Status = StreamStateType.UnLoaded;
                throw;
            }
            finally { _syncer.Release(); }
        }
        public async Task Pause()
        {
            try
            {
                await _syncer.WaitAsync();
                if (_isPause || Status == StreamStateType.Paused)
                    return;

                _isPause = true;
                _hiddenActivities = new ObservableCollection<Activity>(_activities);
                ChangedActivityCount = 0;
            }
            finally
            { _syncer.Release(); }
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
        public event EventHandler ChangedChangedActivityCount;
        protected virtual void OnChangedChangedActivityCount(EventArgs e)
        {
            if (ChangedChangedActivityCount != null)
                ChangedChangedActivityCount(this, e);
        }
    }
    public enum StreamStateType
    { UnLoaded, Loading, Initing, Connected, Paused, }
}