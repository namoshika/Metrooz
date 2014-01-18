using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SunokoLibrary.Web.GooglePlus;

namespace GPlusBrowser.Model
{
    public class StreamManager
    {
        public StreamManager(Account account)
        {
            _accountModel = account;
            Streams = new ObservableCollection<Stream>();
            UpdateStatus = CircleUpdateLevel.Unloaded;
        }
        bool _isAddedYourCircle;
        Account _accountModel;

        public CircleUpdateLevel UpdateStatus { get; private set; }
        public ObservableCollection<Stream> Streams { get; private set; }
        public async Task Initialize(CircleUpdateLevel loadMode)
        {
            await _accountModel.PlusClient.Relation.UpdateCirclesAndBlockAsync(false, loadMode);
            if (_isAddedYourCircle == false)
            {
                _isAddedYourCircle = true;
                Streams.Add(new Stream(this) { Circle = _accountModel.PlusClient.Relation.YourCircle });
            }
            lock (Streams)
            {
                var i = 0;
                for (; i < _accountModel.PlusClient.Relation.Circles.Count; i++)
                {
                    //PlatformClientはサークル情報更新時にCircles.CircleInfoの同一性を保持しない
                    //そのため、ストリームの遅延読み込みでCircleInfoの新旧の扱いに面倒な部分がある。
                    //ここの処理でストリームの遅延読み込みに必要なCircleInfoの新旧の追跡を実現する
                    var circleInf = _accountModel.PlusClient.Relation.Circles[i];
                    var item = Streams.FirstOrDefault(info => info.Circle.Id == circleInf.Id);
                    if (item != null)
                    {
                        item.Circle = circleInf;
                        Streams.Move(Streams.IndexOf(item), i + 1);
                    }
                    else
                        Streams.Insert(i + 1, new Stream(this) { Circle = circleInf });
                }
                for (i += 1; i < Streams.Count; i++)
                {
                    var rmItem = Streams[i];
                    Streams.RemoveAt(i);
                    rmItem.Dispose();
                }
                UpdateStatus = _accountModel.PlusClient.Relation.CirclesAndBlockStatus;
            }
            OnUpdated(new EventArgs());
        }
        public void Connect()
        {
            foreach (var item in Streams)
                item.Connect();
        }

        public event EventHandler Updated;
        protected virtual void OnUpdated(EventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
        }
    }
    public enum StreamUpdateSeqState
    { Unloaded, Loaded, Fail }
}