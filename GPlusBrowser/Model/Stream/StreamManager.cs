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
            ((INotifyCollectionChanged)_account.Circles.Items).CollectionChanged += StreamManager_CollectionChanged;
            _circleStreams = new ObservableCollection<Stream>() { new Stream(this,  account.PlusClient.Relation.YourCircle) };
            _selectedCircleIndex = -1;

            CircleStreams = new ReadOnlyObservableCollection<Stream>(_circleStreams);
        }
        int _selectedCircleIndex;
        ObservableCollection<Stream> _circleStreams;
        Account _account;

        public int SelectedCircleIndex
        {
            get { return _selectedCircleIndex; }
            set
            {
                if (_selectedCircleIndex == value)
                    return;
                _selectedCircleIndex = value;
                OnChangedSelectedCircleIndex(new EventArgs());
            }
        }
        public ReadOnlyObservableCollection<Stream> CircleStreams { get; private set; }
        void StreamManager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in _circleStreams)
                        item.Dispose();
                    _circleStreams.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                    foreach(CircleInfo item in e.NewItems)
                        _circleStreams.Insert(e.NewStartingIndex + 1, new Stream(this, item));
                    if (_selectedCircleIndex < 0)
                        SelectedCircleIndex = 0;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (CircleInfo item in e.OldItems)
                    {
                        var target = _circleStreams.First(strm => strm.Circle == item);
                        _circleStreams.Remove(target);
                        target.Dispose();
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var newItem = (CircleInfo)e.NewItems[0];
                    _circleStreams[e.NewStartingIndex + 1].Circle = newItem;
                    break;
                case NotifyCollectionChangedAction.Move:
                    _circleStreams.Move(e.OldStartingIndex + 1, e.NewStartingIndex + 1);
                    break;
            }
            
        }

        public event EventHandler ChangedSelectedCircleIndex;
        protected virtual void OnChangedSelectedCircleIndex(EventArgs e)
        {
            if (_circleStreams.Count >= 0 && _selectedCircleIndex >= 0)
            {
                var selectedStream = _circleStreams[_selectedCircleIndex];
                selectedStream.Connect();
                if (selectedStream.IsRefreshed == false)
                    selectedStream.Refresh();
            }

            if (ChangedSelectedCircleIndex != null)
                ChangedSelectedCircleIndex(this, e);
        }
    }
    public enum StreamUpdateSeqState
    { Unloaded, Loaded, Fail }
}