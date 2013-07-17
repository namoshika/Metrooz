using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class StreamManagerViewModel : ViewModelBase, IDisposable
    {
        public StreamManagerViewModel(StreamManager streamManager, AccountViewModel topLevel, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher, topLevel)
        {
            _streamManagerModel = streamManager;
            ((INotifyCollectionChanged)_streamManagerModel.CircleStreams).CollectionChanged += _stream_ChangedDisplayStreams;
            _streamManagerModel.ChangedSelectedCircleIndex += _streamManagerModel_ChangedSelectedCircleIndex;
            _displayStreams = new ObservableCollection<StreamViewModel>(
                _streamManagerModel.CircleStreams.Select(vm => new StreamViewModel(vm, topLevel, uiThreadDispatcher))); ;
            _selectedCircleIndex = 0;
        }
        bool _isError;
        int _selectedCircleIndex;
        StreamManager _streamManagerModel;
        ObservableCollection<StreamViewModel> _displayStreams;

        public bool IsError
        {
            get { return _isError; }
            set
            {
                if (_isError == value)
                    return;
                _isError = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("IsError"));
            }
        }
        public int SelectedCircleIndex
        {
            get { return _selectedCircleIndex; }
            set
            {
                if (_selectedCircleIndex == value)
                    return;
                _selectedCircleIndex = value;
                _streamManagerModel.SelectedCircleIndex = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("SelectedCircleIndex"));
            }
        }
        public ObservableCollection<StreamViewModel> DisplayStreams
        {
            get { return _displayStreams; }
            set
            {
                _displayStreams = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("DisplayStreams"));
            }
        }
        public void Dispose()
        {
            if (_streamManagerModel == null)
                return;

            SelectedCircleIndex = -1;
            ((INotifyCollectionChanged)_streamManagerModel.CircleStreams).CollectionChanged -= _stream_ChangedDisplayStreams;
            _streamManagerModel.ChangedSelectedCircleIndex -= _streamManagerModel_ChangedSelectedCircleIndex;
            _streamManagerModel = null;

            foreach (var item in DisplayStreams)
                item.Dispose();
            DisplayStreams = null;
        }

        void _stream_ChangedDisplayStreams(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            lock(_displayStreams)
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            var circle = (Stream)e.NewItems[i];
                            var circleVm = new StreamViewModel(circle, TopLevel, UiThreadDispatcher);
                            DisplayStreams.InsertAsync(e.NewStartingIndex + i, circleVm, UiThreadDispatcher);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        DisplayStreams.MoveAsync(e.OldStartingIndex, e.NewStartingIndex, UiThreadDispatcher);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; i++)
                            DisplayStreams.RemoveAtAsync(e.OldStartingIndex, UiThreadDispatcher);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        DisplayStreams.ClearAsync(UiThreadDispatcher);
                        break;
                }
        }
        void _streamManagerModel_ChangedSelectedCircleIndex(object sender, EventArgs e)
        { UiThreadDispatcher.Invoke((Action)delegate() { SelectedCircleIndex = _streamManagerModel.SelectedCircleIndex; }); }
    }
}