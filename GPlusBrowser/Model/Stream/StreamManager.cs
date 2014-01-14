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
            _account = account;
            _account.Circles.Items.CollectionChanged += StreamManager_CollectionChanged;
            CircleStreams = new ObservableCollection<Stream>();
        }
        Account _account;

        public ObservableCollection<Stream> CircleStreams { get; private set; }
        public async Task Initialize()
        {
            await _account.PlusClient.Relation.UpdateCirclesAndBlockAsync(false, CircleUpdateLevel.Loaded);
            CircleStreams.Add(new Stream(this) { Circle = _account.PlusClient.Relation.YourCircle });
        }
        public void Connect()
        {
            foreach (var item in CircleStreams)
                item.Connect();
        }
        void StreamManager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in CircleStreams)
                        item.Dispose();
                    CircleStreams.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                    foreach (CircleInfo item in e.NewItems)
                        CircleStreams.Insert(e.NewStartingIndex + 1, new Stream(this) { Circle = item });
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (CircleInfo item in e.OldItems)
                    {
                        var target = CircleStreams.First(strm => strm.Circle == item);
                        CircleStreams.Remove(target);
                        target.Dispose();
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var newItem = (CircleInfo)e.NewItems[0];
                    CircleStreams[e.NewStartingIndex + 1].Circle = newItem;
                    break;
                case NotifyCollectionChangedAction.Move:
                    CircleStreams.Move(e.OldStartingIndex + 1, e.NewStartingIndex + 1);
                    break;
            }
            
        }
    }
    public enum StreamUpdateSeqState
    { Unloaded, Loaded, Fail }
}