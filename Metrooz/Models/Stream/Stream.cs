using SunokoLibrary.Web.GooglePlus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrooz.Models
{
    public class Stream : Livet.NotificationObject, IDisposable
    {
        public Stream(CircleInfo source)
        {
            _activities = new ObservableCollection<Activity>();
            _circle = source;
        }
        const int MAX_ACTIVITIES_COUNT = 30;
        readonly System.Threading.SemaphoreSlim _syncer = new System.Threading.SemaphoreSlim(1, 1);
        bool _isPause, _isDisposed;
        int _unreadedActivityCount;
        CircleInfo _circle;
        StreamStateType _status;
        IInfoList<ActivityInfo> _activityGetter;
        ObservableCollection<Activity> _activities, _hiddenActivities;
        IDisposable _streamObj;

        public ObservableCollection<Activity> Activities { get { return _activities; } }
        public string Name { get { return _circle.Name; } }
        public int ChangedActivityCount
        {
            get { return _unreadedActivityCount; }
            set
            {
                if (_unreadedActivityCount == value)
                    return;
                _unreadedActivityCount = value;
                RaisePropertyChanged();
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
                RaisePropertyChanged();
            }
        }

        public async Task<bool> Activate()
        {
            try
            {
                await _syncer.WaitAsync();
                if (_isDisposed)
                    return false;

                //処理の最初に置くことでConnect()後はストリームの休止状態が終了している事を保証させる
                _isPause = false;
                //接続処理
                switch (Status)
                {
                    case StreamStateType.UnLoaded:
                        //読み込み
                        Status = StreamStateType.Initing;
                        _activityGetter = _circle.GetActivities();
                        var activities = await _activityGetter.TakeAsync(20);

                        //更新
                        var outerActiveActivities = _isPause ? _hiddenActivities : _activities;
                        outerActiveActivities.Clear();
                        foreach (var item in activities)
                        {
                            var post = new Activity(item);
                            outerActiveActivities.Add(post);
                            await post.Activate();
                        }

                        //受信開始
                        Status = StreamStateType.Connected;
                        _streamObj = _circle.GetStream().Subscribe(Recieved_Activity,
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
                            {
                                _activities[aIdx].Dispose();
                                _activities.RemoveAt(aIdx);
                            }
                        }
                        //一時停止が終わったらActivityのGC解放のためにも消す
                        _hiddenActivities = null;
                        break;
                }
                return true;
            }
            catch (FailToOperationException)
            {
                Status = StreamStateType.UnLoaded;
                return false;
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
                if (_isDisposed)
                    return;
                _isDisposed = true;
                if (_streamObj != null)
                    _streamObj.Dispose();
                foreach (var item in _activities)
                    item.Dispose();
            }
            finally
            { _syncer.Release(); }
        }
        async void Recieved_Activity(ActivityInfo newInfo)
        {
            try
            {
                await _syncer.WaitAsync().ConfigureAwait(false);
                if (_isDisposed)
                    return;

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
                                var activity = innerActiveActivities[MAX_ACTIVITIES_COUNT];
                                //一時停止モード時の古いActivityのDispose()は表示されているActivityの更新を誤停止する可能性があるため、表示されているActivityと照合してDispose()する。
                                //この時、_isPauseはスレッドロック関係でinnerActiveActivitiesの状態と連動する保証がない。そのため、以下の条件文となっている。
                                if (innerActiveActivities == _hiddenActivities && _activities.Contains(activity) == false)
                                    innerActiveActivities[MAX_ACTIVITIES_COUNT].Dispose();
                                innerActiveActivities.RemoveAt(MAX_ACTIVITIES_COUNT);
                            }
                            //Activate()でも内部的にitem.CoreInfoの更新が発生しうる。
                            //そのため、すべての処理が終わってから呼び出す事で予想外の状態遷移を避ける。
                            await item.Activate();
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
        }
    }
    public enum StreamStateType
    { UnLoaded, Initing, Connected, Paused, }
}