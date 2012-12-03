using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace GPlusBrowser.ViewModel
{
    using Model;

    public class StreamManagerViewModel : ViewModelBase, IDisposable
    {
        public StreamManagerViewModel(StreamManager streamManager, Dispatcher uiThreadDispatcher)
            : base(uiThreadDispatcher)
        {
            _streamManagerModel = streamManager;
            _streamManagerModel.ChangedDisplayStreams += _stream_ChangedDisplayStreams;
            _streamManagerModel.ChangedSelectedCircleIndex += _streamManagerModel_ChangedSelectedCircleIndex;
            _displayStreams = new ObservableCollection<StreamViewModel>();
            _selectedCircleIndex = -1;
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
            _streamManagerModel.ChangedDisplayStreams -= _stream_ChangedDisplayStreams;
            _streamManagerModel.ChangedSelectedCircleIndex -= _streamManagerModel_ChangedSelectedCircleIndex;
            _streamManagerModel = null;

            foreach (var item in DisplayStreams)
                item.Dispose();
            DisplayStreams = null;
        }

        void _stream_ChangedDisplayStreams(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in _displayStreams
                        .Where(streamVm => streamVm.Order >= e.NewStartingIndex + 1))
                        item.Order += e.NewItems.Count;
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        var circle = (Stream)e.NewItems[i];
                        var circleVm = new StreamViewModel(circle, e.NewStartingIndex + i, UiThreadDispatcher);
                        circleVm.Order = e.NewStartingIndex + i + 1;
                        DisplayStreams.AddAsync(circleVm, UiThreadDispatcher);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    var max = Math.Max(e.NewStartingIndex, e.OldStartingIndex);
                    var min = Math.Min(e.NewStartingIndex, e.OldStartingIndex);
                    foreach (var item in _displayStreams.Where(strmVm => strmVm.Order >= min && strmVm.Order < max))
                        item.Order++;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                        DisplayStreams.RemoveAtAsync(e.OldStartingIndex, UiThreadDispatcher);
                    foreach (var item in _displayStreams.Where(strmVm => strmVm.Order >= e.OldStartingIndex))
                        item.Order -= e.NewItems.Count;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    DisplayStreams.ClearAsync(UiThreadDispatcher);
                    break;
            }
        }
        void _streamManagerModel_ChangedSelectedCircleIndex(object sender, EventArgs e)
        {
            UiThreadDispatcher.Invoke(
                (Action)delegate() { SelectedCircleIndex = _streamManagerModel.SelectedCircleIndex; },
                _streamManagerModel.SelectedCircleIndex < DisplayStreams.Count
                    ? DispatcherPriority.DataBind : DispatcherPriority.ContextIdle);
        }
    }
}