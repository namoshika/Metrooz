using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SunokoLibrary.GooglePlus;

namespace GPlusBrowser.Model
{
    public class StreamManager
    {
        public StreamManager(Account mainWindow)
        {
            _account = mainWindow;
            _circleStreams = new List<Stream>();
            _displayStreams = new List<Stream>();
            _selectedCircleIndex = -1;
        }
        Account _account;
        int _selectedCircleIndex;
        List<Stream> _circleStreams;
        List<Stream> _displayStreams;

        public bool IsInitialized { get; private set; }
        public int SelectedCircleIndex
        {
            get { return _selectedCircleIndex; }
            set
            {
                if (_selectedCircleIndex == value)
                    return;
                _selectedCircleIndex = value;
                if (_circleStreams.Count >= 0 && _selectedCircleIndex >= 0)
                {
                    var selectedStream = _circleStreams[_selectedCircleIndex];
                    if (!selectedStream.IsRefreshed)
                        selectedStream.Refresh();
                }

                OnChangedSelectedCircleIndex(new EventArgs());
            }
        }
        public ReadOnlyCollection<Stream> CircleStreams
        { get { return _circleStreams.AsReadOnly(); } }
        public ReadOnlyCollection<Stream> DisplayStreams
        { get { return _displayStreams.AsReadOnly(); } }

        public void Initialize()
        {
            try
            {
                lock (_circleStreams)
                {
                    SelectedCircleIndex = -1;
                    foreach (var item in _circleStreams)
                        item.Dispose();
                    _circleStreams.Clear();
                    _circleStreams.Add(new Stream(this,
                        _account.GooglePlusClient.Relation.YourCircle));
                    _circleStreams.AddRange(
                        _account.Circles.Items.Select(info => new Stream(this, info)));
                }
                lock (_displayStreams)
                {
                    foreach (var item in _displayStreams)
                        item.Dispose();
                    _displayStreams.Clear();
                    OnChangedDisplayStreams(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset));
                    foreach (var item in _circleStreams.Skip(1).Take(3))
                    {
                        _displayStreams.Add(item);
                        item.Refresh();
                    }
                }
                IsInitialized = true;
            }
            catch (FailToOperationException)
            { IsInitialized = false; }

            OnInitialized(new EventArgs());
            lock (_displayStreams)
                OnChangedDisplayStreams(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, _displayStreams, 0));
            if (_circleStreams.Count > 0)
                SelectedCircleIndex = 0;
        }
        public void AddDisplayStream(CircleInfo circles)
        {
            var stream = new Stream(this, circles);
            lock (_displayStreams)
            {
                _displayStreams.Add(stream);
                OnChangedDisplayStreams(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, stream, _displayStreams.Count - 1));
            }
        }
        public void RemoveDisplayStream(Stream stream)
        {
            var index = _displayStreams.IndexOf(stream);
            if (index < 0)
                return;
            lock (_displayStreams)
            {
                _displayStreams.Remove(stream);
                OnChangedDisplayStreams(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, stream, index));
            }
        }
        public void MoveDisplayStream(int oldIndex, int newIndex)
        {
            lock (_displayStreams)
            {
                var item = _displayStreams[oldIndex];
                lock (_displayStreams)
                {
                    _displayStreams.RemoveAt(oldIndex);
                    _displayStreams.Insert(newIndex + oldIndex < newIndex ? -1 : 0, item);
                    OnChangedDisplayStreams(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
                }
            }
        }

        public event EventHandler Initialized;
        protected virtual void OnInitialized(EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
        }
        public event EventHandler ChangedSelectedCircleIndex;
        protected virtual void OnChangedSelectedCircleIndex(EventArgs e)
        {
            if (ChangedSelectedCircleIndex != null)
                ChangedSelectedCircleIndex(this, e);
        }
        public event NotifyCollectionChangedEventHandler ChangedDisplayStreams;
        protected virtual void OnChangedDisplayStreams(NotifyCollectionChangedEventArgs e)
        {
            if (ChangedDisplayStreams != null)
                ChangedDisplayStreams(this, e);
        }
    }
}
